using Microsoft.Xna.Framework;
using RealmNexus.Logging;
using RealmNexus.Models;
using RealmNexus.Packets;
using System.Net.Sockets;
using Terraria.Localization;
using TrProtocol;
using TrProtocol.NetPackets;
using TrProtocol.NetPackets.Modules;

namespace RealmNexus.Core;

public sealed class RealmClient : IDisposable
{
    private readonly TcpClient _clientSocket;
    private readonly ILogger _logger;
    private TcpClient _serverConnection;
    private readonly PacketHandlerManager _handlerManager;
    
    private readonly ReaderWriterLockSlim _pipeLock = new(LockRecursionPolicy.NoRecursion);
    private PacketPipe _c2sPipe;
    private PacketPipe _s2cPipe;
    private CancellationTokenSource _cts;

    public string Endpoint { get; }
    public Server CurrentServer { get; private set; }
    
    public string PlayerName { get; set; } = "Unknown";

    public RealmClient(TcpClient clientSocket, Server initialServer, ILogger logger)
    {
        _clientSocket = clientSocket;
        _logger = logger;
        Endpoint = clientSocket.Client.RemoteEndPoint?.ToString() ?? "unknown";
        CurrentServer = initialServer;
        
        _handlerManager = new PacketHandlerManager(this, logger);
        _handlerManager.RegisterHandlersFromCurrentAssembly();
    }

    public T GetHandler<T>() where T : class, IPacketHandler
    {
        return _handlerManager.GetHandler<T>();
    }

    public async Task RunAsync(CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = _cts.Token;

        try
        {
            _handlerManager.OnConnected();
            await ConnectToServerAsync(CurrentServer, token);
            await RunBidirectionalPipesAsync(token);
        }
        finally
        {
            _handlerManager.OnDisconnected();
            _logger.LogInfo("RealmClient", $"[{Endpoint}] 玩家 {PlayerName} 已断开连接");
        }
    }

    private async Task RunBidirectionalPipesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var (c2sPipe, s2cPipe) = GetPipes();
            if (c2sPipe == null || s2cPipe == null)
            {
                continue;
            }

            var c2sTask = c2sPipe.RunAsync(ct);
            var s2cTask = s2cPipe.RunAsync(ct);
            var completed = await Task.WhenAny(c2sTask, s2cTask);

            if (ct.IsCancellationRequested) break;

            try { await completed; }
            catch (Exception ex) when (IsDisconnect(ex))
            {
                var (newC2s, newS2c) = GetPipes();
                if (newC2s != c2sPipe || newS2c != s2cPipe) continue;
                return;
            }
            catch {}

            var (checkC2s, checkS2c) = GetPipes();
            if (checkC2s == c2sPipe && checkS2c == s2cPipe) break;
        }
    }

    private (PacketPipe c2s, PacketPipe s2c) GetPipes()
    {
        _pipeLock.EnterReadLock();
        try { return (_c2sPipe, _s2cPipe); }
        finally { _pipeLock.ExitReadLock(); }
    }

    private static bool IsDisconnect(Exception ex) =>
        ex is OperationCanceledException ||
        (ex is IOException);

    private async Task ConnectToServerAsync(Server serverConfig, CancellationToken ct)
    {
        _logger.LogInfo("RealmClient", $"[{Endpoint}] 正在断开旧服务器连接...");

        PacketPipe oldC2SPipe = null;
        PacketPipe oldS2CPipe = null;

        _pipeLock.EnterWriteLock();
        try
        {
            oldC2SPipe = _c2sPipe;
            oldS2CPipe = _s2cPipe;

            _c2sPipe = null;
            _s2cPipe = null;
        }
        finally
        {
            _pipeLock.ExitWriteLock();
        }

        await CloseOldConnectionAsync(oldC2SPipe, oldS2CPipe);

        _logger.LogInfo("RealmClient", $"[{Endpoint}] 正在连接到新服务器: {serverConfig.Host}:{serverConfig.Port}...");

        _serverConnection = new TcpClient
        {
            NoDelay = true,
            ReceiveBufferSize = 65536,
            SendBufferSize = 65536
        };
        
        await _serverConnection.ConnectAsync(serverConfig.Host, serverConfig.Port, ct);

        var clientStream = _clientSocket.GetStream();
        var serverStream = _serverConnection.GetStream();

        _logger.LogInfo("RealmClient", $"[{Endpoint}] 连接成功，创建数据管道...");

        var newC2SPipe = new PacketPipe(clientStream, serverStream, isClientSide: false, "C2S", Endpoint, _handlerManager, _logger);
        var newS2CPipe = new PacketPipe(serverStream, clientStream, isClientSide: true, "S2C", Endpoint, _handlerManager, _logger);

        _pipeLock.EnterWriteLock();
        try
        {
            _c2sPipe = newC2SPipe;
            _s2cPipe = newS2CPipe;
        }
        finally
        {
            _pipeLock.ExitWriteLock();
        }

        CurrentServer = serverConfig;

        _handlerManager.OnServerChanged();

        _logger.LogInfo("RealmClient", $"[{Endpoint}] 已连接到服务器: {serverConfig.Name} ({serverConfig.Host}:{serverConfig.Port})");
    }

    private async Task CloseOldConnectionAsync(PacketPipe oldC2SPipe, PacketPipe oldS2CPipe)
    {
        try { _serverConnection?.Close(); } catch { }
       
        oldC2SPipe?.Stop();
        oldS2CPipe?.Stop();

        oldC2SPipe?.Dispose();
        oldS2CPipe?.Dispose();
        _serverConnection?.Dispose();
    }

    public async Task SendPacketToServerAsync(INetPacket packet, CancellationToken? ct = null)
    {
        _pipeLock.EnterReadLock();
        try
        {
            if (_c2sPipe != null)
            {
                await _c2sPipe.SendPacketAsync(packet, ct ?? CancellationToken.None);
            }
        }
        finally
        {
            _pipeLock.ExitReadLock();
        }
    }

    public async Task SendCustomPacketAsync(ICustomPacket packet, CancellationToken? ct = null)
    {
        _pipeLock.EnterReadLock();
        try
        {
            if (_c2sPipe != null)
            {
                await _c2sPipe.SendCustomPacketAsync(packet, ct ?? CancellationToken.None);
            }
        }
        finally
        {
            _pipeLock.ExitReadLock();
        }
    }

    public async Task SendPacketToClientAsync(INetPacket packet, CancellationToken? ct = null)
    {
        _pipeLock.EnterReadLock();
        try
        {
            if (_s2cPipe != null)
            {
                await _s2cPipe.SendPacketAsync(packet, ct ?? CancellationToken.None);
            }
        }
        finally
        {
            _pipeLock.ExitReadLock();
        }
    }

    public async Task ChangeServerAsync(Server targetServer)
    {
        if (targetServer.Name == CurrentServer.Name)
        {
            await SendChatMessageAsync("你已连接此服务器");
            return;
        }

        _logger.LogInfo("RealmClient", $"[{Endpoint}] 开始切换服务器到: {targetServer.Name}");
        await SendChatMessageAsync($"正在切换到服务器: {targetServer.Name}...");

        try
        {
            _handlerManager.OnServerChanging();
            var newCts = CancellationTokenSource.CreateLinkedTokenSource(_cts!.Token);
            await ConnectToServerAsync(targetServer, newCts.Token);
            await SendChatMessageAsync($"已切换到服务器: {targetServer.Name}");
            _logger.LogInfo("RealmClient", $"[{Endpoint}] 服务器切换完成");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInfo("RealmClient", $"[{Endpoint}] 切换服务器被取消");
        }
        catch (Exception ex)
        {
            _logger.LogError("RealmClient", $"[{Endpoint}] 切换服务器失败: {ex}");
            await SendChatMessageAsync($"切换服务器失败: {ex.Message}");
        }
    }

    public async Task SendChatMessageAsync(string message)
    {
        var packet = new NetTextModule
        {
            TextS2C = new TextS2C
            {
                PlayerSlot = 255,
                Text = new NetworkText(message, NetworkText.Mode.Literal),
                Color = new Color { R = 255, G = 255, B = 255 }
            }
        };

        _pipeLock.EnterReadLock();
        try
        {
            if (_s2cPipe != null)
            {
                await _s2cPipe.SendPacketAsync(packet, CancellationToken.None);
            }
        }
        finally
        {
            _pipeLock.ExitReadLock();
        }
    }

    public async Task DisconnectAsync(string reason)
    {
        try
        {
            var kickPacket = new Kick
            {
                Reason = new NetworkText(reason, NetworkText.Mode.Literal)
            };
            
            _pipeLock.EnterReadLock();
            try
            {
                if (_s2cPipe != null)
                {
                    await _s2cPipe.SendPacketAsync(kickPacket, CancellationToken.None);
                }
            }
            finally
            {
                _pipeLock.ExitReadLock();
            }
            
            _logger.LogInfo("RealmClient", $"[{Endpoint}] 断开连接: {reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError("RealmClient", $"[{Endpoint}] 发送断开消息失败: {ex.Message}");
        }
        finally
        {
            _cts?.Cancel();
            _clientSocket.Close();
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();

        _pipeLock.EnterWriteLock();
        try
        {
            _c2sPipe?.Stop();
            _s2cPipe?.Stop();
            _c2sPipe?.Dispose();
            _s2cPipe?.Dispose();
        }
        finally
        {
            _pipeLock.ExitWriteLock();
        }

        _pipeLock.Dispose();
        _serverConnection?.Dispose();
        _clientSocket.Dispose();
    }
}
