using RealmNexus.Packets;
using TrProtocol;

namespace RealmNexus.Core;

public class PacketInterceptArgs
{
    public PacketPipe Pipe { get; private set; }
    public INetPacket Packet { get; private set; }
    public ICustomPacket CustomPacket { get; private set; }
    public bool Handled { get; set; } = false;

    public PacketInterceptArgs(PacketPipe pipe, INetPacket packet)
    {
        Pipe = pipe;
        Packet = packet;
        CustomPacket = null;
    }

    public PacketInterceptArgs(PacketPipe pipe, ICustomPacket customPacket)
    {
        Pipe = pipe;
        Packet = null;
        CustomPacket = customPacket;
    }

    public void Reset(PacketPipe pipe, INetPacket packet)
    {
        Pipe = pipe;
        Packet = packet;
        CustomPacket = null;
        Handled = false;
    }

    public void Reset(PacketPipe pipe, ICustomPacket customPacket)
    {
        Pipe = pipe;
        Packet = null;
        CustomPacket = customPacket;
        Handled = false;
    }
}
