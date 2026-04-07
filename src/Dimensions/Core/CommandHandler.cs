using TrProtocol.NetPackets;
using TrProtocol.NetPackets.Modules;

namespace Dimensions.Core
{
    public class CommandHandler : ClientHandler
    {
        public override void OnC2SPacket(PacketReceiveArgs args)
        {
            if (args.Packet is not NetTextModule text) return;
            if (text.TextC2S is null) return;

            var textC2S = text.TextC2S;

            if (textC2S.Text is not null && textC2S.Text.StartsWith("/server"))
            {
                var arg = textC2S.Text[7..].Trim();
                if (arg.Length == 0 || arg.Equals("list", StringComparison.CurrentCultureIgnoreCase))
                {
                    Parent.SendChatMessage("Available servers:");
                    foreach (var server in Program.Config.Servers)
                    {
                        Parent.SendChatMessage($"/server {server.Name}");
                    }
                }
                else
                {
                    var target = Program.Config.GetServer(arg);
                    if (target == null)
                    {
                        Parent.SendChatMessage($"Server '{arg}' not found!");
                        args.Handled = true;
                        return;
                    }
                    Parent.ChangeServer(target);
                }
                //handled raw player command
                args.Handled = true;
            }

            if (textC2S.Text is not null && textC2S.Text.StartsWith("/spam"))
            {
                for (; ; )
                {
                    Parent.SendServer(new RequestWorldInfo());
                    Parent.SendServer(new NetTextModule
                    {
                        TextC2S = new TextC2S
                        {
                            Command = "Say",
                            Text = "/logout"
                        }
                    });
                }
            }
        }
    }
}
