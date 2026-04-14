using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Channels;
using RealmNexus.Logging;
using RealmNexus.Packets;
using TrProtocol;
using TrProtocol.Interfaces;

namespace RealmNexus.Core;

public sealed class PacketPipe : IDisposable
{
    private readonly PipeReader _reader;
    private readonly Stream _writer;
    private readonly PacketSerializer _serializer;
    private readonly string _flag;
    private readonly string _endpoint;
    private readonly CancellationTokenSource _internalCts = new();
    private readonly Channel<INetPacket> _sendChannel;
    private readonly Channel<ICustomPacket> _customPacketChannel;
    private readonly ILogger _logger;
    private readonly PacketHandlerManager _handlerManager;
    private int _shouldStop;

    public bool IsClient => _serializer.Client;

    public PacketPipe(Stream source, Stream target, bool isClientSide, string flag, string endpoint, PacketHandlerManager handlerManager, ILogger logger = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(flag);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        _reader = PipeReader.Create(source, new StreamPipeReaderOptions(
            bufferSize: 65536,
            minimumReadSize: 1024,
            leaveOpen: true
        ));
        _writer = target ?? throw new ArgumentNullException(nameof(target));
        _serializer = new PacketSerializer(isClientSide);
        _flag = flag;
        _endpoint = endpoint;
        _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
        _logger = logger ?? new ConsoleLogger();

        _sendChannel = Channel.CreateUnbounded<INetPacket>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _customPacketChannel = Channel.CreateUnbounded<ICustomPacket>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _ = ProcessSendQueueAsync();
        _ = ProcessCustomPacketQueueAsync();
    }

    public async Task SendPacketAsync(INetPacket packet, CancellationToken ct)
    {
        if (Interlocked.CompareExchange(ref _shouldStop, 0, 0) != 0)
            return;

        if (packet is ISideSpecific sideSpecific)
        {
            sideSpecific.IsServerSide = _serializer.Client;
        }

        await _sendChannel.Writer.WriteAsync(packet, ct);
    }

    public async Task SendCustomPacketAsync(ICustomPacket packet, CancellationToken ct)
    {
        if (Interlocked.CompareExchange(ref _shouldStop, 0, 0) != 0)
            return;

        await _customPacketChannel.Writer.WriteAsync(packet, ct);
    }

    private async Task ProcessSendQueueAsync()
    {
        await foreach (var packet in _sendChannel.Reader.ReadAllAsync())
        {
            try
            {
                var data = _serializer.Serialize(packet);
                await _writer.WriteAsync(data, CancellationToken.None);
                await _writer.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("PacketPipe", $"[{_flag}] 发送包失败: {ex.Message}");
            }
        }
    }

    private async Task ProcessCustomPacketQueueAsync()
    {
        await foreach (var packet in _customPacketChannel.Reader.ReadAllAsync())
        {
            try
            {
                await SendCustomPacketRawAsync(packet, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError("PacketPipe", $"[{_flag}] 发送自定义包失败: {ex.Message}");
            }
        }
    }

    private unsafe Task SendCustomPacketRawAsync(ICustomPacket packet, CancellationToken ct)
    {
        byte[] buffer = new byte[ushort.MaxValue];
        fixed (byte* pBuffer = buffer)
        {
            void* ptr = pBuffer;
            *(byte*)ptr = (byte)packet.Type;
            ptr = (byte*)ptr + 1;
            packet.WriteContent(ref ptr);
            int contentLen = (int)((byte*)ptr - pBuffer);

            byte[] packetData = new byte[contentLen + 2];
            BinaryPrimitives.WriteUInt16LittleEndian(packetData.AsSpan(0, 2), (ushort)(contentLen + 2));
            Buffer.BlockCopy(buffer, 0, packetData, 2, contentLen);

            return _writer.WriteAsync(packetData, ct).AsTask();
        }
    }

    public void Stop()
    {
        Interlocked.Exchange(ref _shouldStop, 1);
        _internalCts.Cancel();
        _sendChannel.Writer.TryComplete();
        _customPacketChannel.Writer.TryComplete();
    }

    public async Task RunAsync(CancellationToken externalCt)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_internalCts.Token, externalCt);
        var ct = linkedCts.Token;

        try
        {
            while (!ct.IsCancellationRequested && IsRunning)
            {
                var result = await _reader.ReadAsync(ct);
                var buffer = result.Buffer;

                if (buffer.IsEmpty && result.IsCompleted)
                    break;

                var position = ProcessBuffer(buffer, out var consumed);
                _reader.AdvanceTo(position, buffer.End);

                if (result.IsCompleted)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private SequencePosition ProcessBuffer(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
    {
        consumed = buffer.Start;

        while (IsRunning && TryReadPacket(ref buffer, out var packetData))
        {
            _ = ProcessPacketAsync(packetData);
            consumed = buffer.Start;
        }

        return consumed;
    }

    private bool IsRunning => Interlocked.CompareExchange(ref _shouldStop, 0, 0) == 0;

    private async Task ProcessPacketAsync(ReadOnlySequence<byte> packetData)
    {
        if (packetData.Length < 2)
            return;

        var length = ReadUInt16(packetData.Slice(0, 2));
        var payloadLength = length - 2;

        if (payloadLength < 0 || packetData.Length < length)
            return;

        var payload = packetData.Slice(2, payloadLength);

        if (payload.Length > 65535)
        {
            _logger.LogDebug("PacketPipe", $"[{_flag}] 包太大，直接转发: {payload.Length} bytes");
            await ForwardRawDataAsync(packetData);
            return;
        }

        var payloadArray = ArrayPool<byte>.Shared.Rent((int)payload.Length);
        try
        {
            payload.CopyTo(payloadArray);
            byte packetId = payloadArray[0];

            if (CustomPacketRegistry.IsCustomPacket(packetId))
            {
                await ProcessCustomPacketAsync(payloadArray, (int)payload.Length, packetId, packetData);
                return;
            }

            await ProcessNetPacketAsync(payloadArray, (int)payload.Length, packetData);
        }
        catch (Exception ex)
        {
            _logger.LogError("PacketPipe", $"[{_flag}] 处理失败: {ex.GetType().Name}: {ex.Message}");
            await ForwardRawDataAsync(packetData);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(payloadArray);
        }
    }

    private async Task ForwardRawDataAsync(ReadOnlySequence<byte> packetData)
    {
        foreach (var segment in packetData)
        {
            await _writer.WriteAsync(segment, CancellationToken.None);
        }
        await _writer.FlushAsync();
    }

    private unsafe Task ProcessCustomPacketAsync(byte[] payload, int length, byte packetId, ReadOnlySequence<byte> originalData)
    {
        if (!CustomPacketRegistry.TryCreate((MessageID)packetId, out var customPacket))
        {
            return ForwardRawDataAsync(originalData);
        }

        fixed (byte* pPayload = payload)
        {
            void* ptr = pPayload + 1;
            byte* end = pPayload + length;
            customPacket.ReadContent(ref ptr, end);
        }

        var isIntercepted = IsClient
            ? _handlerManager.ProcessS2C(this, customPacket)
            : _handlerManager.ProcessC2S(this, customPacket);

        if (isIntercepted)
        {
            _logger.LogDebug("PacketPipe", $"[{_flag}] 自定义包被拦截: {customPacket.GetType().Name}");
            return Task.CompletedTask;
        }

        return SendCustomPacketAsync(customPacket, CancellationToken.None);
    }

    private unsafe Task ProcessNetPacketAsync(byte[] payload, int length, ReadOnlySequence<byte> originalData)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(length + 2);
        try
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(0, 2), (ushort)(length + 2));
            Buffer.BlockCopy(payload, 0, buffer, 2, length);

            INetPacket packet;
            fixed (byte* pBuffer = buffer)
            {
                void* ptr = pBuffer + 2;
                var end = pBuffer + length + 2;
                packet = INetPacket.ReadINetPacket(ref ptr, end, !_serializer.Client);
            }
            _logger.LogDebug(_flag, packet.ToStringInline());
            if (packet is ISideSpecific sideSpecific)
            {
                sideSpecific.IsServerSide = _serializer.Client;
            }

            var isIntercepted = IsClient
                ? _handlerManager.ProcessS2C(this, packet)
                : _handlerManager.ProcessC2S(this, packet);

            if (isIntercepted)
            {
                _logger.LogDebug("PacketPipe", $"[{_flag}] 数据包被拦截: {packet.GetType().Name}");
                return Task.CompletedTask;
            }

            return SendPacketAsync(packet, CancellationToken.None);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        packet = default;

        if (buffer.Length < 2)
            return false;

        var length = ReadUInt16(buffer.Slice(0, 2));
        if (length < 2 || length > 65535 || buffer.Length < length)
            return false;

        packet = buffer.Slice(0, length);
        buffer = buffer.Slice(length);
        return true;
    }

    private static ushort ReadUInt16(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsSingleSegment)
            return BinaryPrimitives.ReadUInt16LittleEndian(sequence.First.Span);

        Span<byte> temp = stackalloc byte[2];
        sequence.CopyTo(temp);
        return BinaryPrimitives.ReadUInt16LittleEndian(temp);
    }

    public void Dispose()
    {
        Stop();
    }
}
