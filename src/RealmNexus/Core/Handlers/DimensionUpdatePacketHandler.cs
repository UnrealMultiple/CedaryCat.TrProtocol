using RealmNexus.Logging;
using RealmNexus.Models;
using RealmNexus.Packets;

namespace RealmNexus.Core.Handlers;

public class DimensionUpdatePacketHandler(RealmClient client, ILogger logger) : CustomPacketHandlerBase<DimensionUpdate>(client, logger)
{
    protected override void HandleC2S(DimensionUpdate packet, PacketInterceptArgs args)
    {
        args.Handled = true;
    }

    protected override void HandleS2C(DimensionUpdate packet, PacketInterceptArgs args)
    {

        switch (packet.SubType)
        {
            case SubMessageID.OnlineInfoRequest:
                _ = Client.SendCustomPacketAsync(new DimensionUpdate
                {
                    SubType = SubMessageID.OnlineInfoResponse,
                    Content = "RealmNexus Proxy"
                });
                args.Handled = true;
                break;

            case SubMessageID.ChangeServer:
                var server = Config.Instance.GetServer(packet.Content);
                if (server != null)
                {
                    Logger.LogInfo("DimensionUpdate", $"服务端请求切换到服务器: {server.Name}");
                    _ = Client.ChangeServerAsync(server);
                }
                else
                {
                    Logger.LogWarning("DimensionUpdate", $"[DimensionUpdate] 未找到服务器: {packet.Content}");
                }
                args.Handled = true;
                break;

            case SubMessageID.ChangeCustomizedServer:
                Logger.LogInfo("DimensionUpdate", $"服务端请求切换到自定义服务器: {packet.Content}:{packet.Port}");
                _ = Client.ChangeServerAsync(new Server
                {
                    Name = "Customized Server",
                    Host = packet.Content,
                    Port = packet.Port
                });
                args.Handled = true;
                break;

            case SubMessageID.ClientAddress:
                Logger.LogDebug("DimensionUpdate", $" 客户端地址: {packet.Content}:{packet.Port}");
                break;
        }
    }
}
