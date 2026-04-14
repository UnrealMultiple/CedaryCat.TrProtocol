using RealmNexus.Core;
using RealmNexus.Logging;
using RealmNexus.Models;
using RealmNexus.Packets;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace RealmNexus;

public sealed class ProxyServer : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Channel<RealmClient> _clientChannel;
    private readonly ILogger _logger;

    public ProxyServer(int port, ILogger logger = null)
    {
        CustomPacketInitializer.RegisterAll();
        _listener = new TcpListener(IPAddress.Any, port);
        _logger = logger ?? new ConsoleLogger();
        _clientChannel = Channel.CreateBounded<RealmClient>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
        CancellationToken token = linkedCts.Token;

        _listener.Start();
        _logger.LogInfo("ProxyServer", $"[Server] 启动成功，监听端口: {((IPEndPoint)_listener.LocalEndpoint).Port}");
        _logger.LogInfo("ProxyServer", $"[Server] 目标服务器数量: {Config.Instance.Servers.Length}");
        foreach (Server server in Config.Instance.Servers)
        {
            _logger.LogInfo("ProxyServer", $"  - {server.Name}: {server.Host}:{server.Port}");
        }

        Task[] workers = new Task[Environment.ProcessorCount];
        for (int i = 0; i < workers.Length; i++)
        {
            workers[i] = ClientWorkerAsync(token);
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(token);
                var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";

                client.NoDelay = true;
                client.ReceiveBufferSize = 65536;
                client.SendBufferSize = 65536;

                var realmClient = new RealmClient(client, Config.Instance.Servers[0], _logger);

                if (!_clientChannel.Writer.TryWrite(realmClient))
                {
                    _logger.LogWarning("ProxyServer", $"[{endpoint}] 客户端队列已满，拒绝连接");
                    realmClient.Dispose();
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            _logger.LogInfo("ProxyServer", "[Server] 正在关闭...");
        }
        finally
        {
            _clientChannel.Writer.Complete();
            await Task.WhenAll(workers);
        }
    }

    private async Task ClientWorkerAsync(CancellationToken ct)
    {
        await foreach (RealmClient client in _clientChannel.Reader.ReadAllAsync(ct))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await client.RunAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInfo("ProxyServer", $"[{client.Endpoint}] 连接被取消");
                }
                catch (Exception ex)
                {
                    _logger.LogError("ProxyServer", $"[{client.Endpoint}] 错误: {ex.Message}");
                }
                finally
                {
                    client.Dispose();
                }
            }, ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _listener.Stop();
        await Task.CompletedTask;
    }
}
