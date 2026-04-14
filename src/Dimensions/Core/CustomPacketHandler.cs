using Dimensions.Models;
using Dimensions.Packets;

namespace Dimensions.Core
{
    public class CustomPacketHandler : ClientHandler
    {
        public override void OnC2SPacket(PacketReceiveArgs args)
        {
            if (args.Packet is DimensionUpdate)
                args.Handled = true;
        }

        public override void OnS2CPacket(PacketReceiveArgs args)
        {
            if (args.Packet is not DimensionUpdate update) return;
            Logger.Log("DimensionPackets", LogLevel.INFO, $"收到维度数据包: {update.SubType}, Content: {update.Content}");

            switch (update.SubType)
            {
                case SubMessageID.OnlineInfoRequest:
                    Parent.SendServer(new DimensionUpdate
                    {
                        SubType = SubMessageID.OnlineInfoResponse,
                        Content = string.Join("\n", GlobalTracker.GetClientNames())
                    });
                    break;
                case SubMessageID.ChangeSever:
                    var server = Program.Config.GetServer(update.Content);
                    if (server != null)
                    {
                        Logger.Log("DimensionPackets", LogLevel.INFO, $"服务端请求切换到服务器: {server.Name}");
                        Parent.ChangeServer(server);
                    }
                    else
                    {
                        Logger.Log("DimensionPackets", LogLevel.WARNING, $"未找到服务器: {update.Content}");
                    }
                    break;
                case SubMessageID.ChangeCustomizedServer:
                    Logger.Log("DimensionPackets", LogLevel.INFO, $"服务端请求切换到自定义服务器: {update.Content}:{update.Port}");
                    Parent.ChangeServer(new Server
                    {
                        Name = "Customized Server",
                        ServerIP = update.Content,
                        ServerPort = update.Port
                    });
                    break;
            }
            args.Handled = true;
        }
    }
}
