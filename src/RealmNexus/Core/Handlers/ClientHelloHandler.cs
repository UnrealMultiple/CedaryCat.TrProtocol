using RealmNexus.Logging;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class ClientHelloHandler(RealmClient client, ILogger logger) : PacketHandlerBase<ClientHello>(client, logger)
{
    private ClientHello? _savedHello;

    protected override void HandleC2S(ClientHello packet, PacketInterceptArgs args)
    {
        _savedHello = packet;
    }

    public override void OnServerChanged()
    {
        if (_savedHello != null)
        {
            _ = Client.SendPacketToServerAsync(_savedHello);
        }
    }
}
