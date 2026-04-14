using RealmNexus.Logging;
using RealmNexus.Models;
using TrProtocol.NetPackets.Modules;

namespace RealmNexus.Core.Handlers;

public class ChatCommandHandler(RealmClient client, ILogger logger) : PacketHandlerBase<NetTextModule>(client, logger)
{
    protected override void HandleC2S(NetTextModule packet, PacketInterceptArgs args)
    {
        var text = packet.TextC2S?.Text;
        if (string.IsNullOrEmpty(text)) return;

        if (!text.StartsWith("/server", StringComparison.OrdinalIgnoreCase))
            return;

        var arg = text[7..].Trim();

        if (string.IsNullOrEmpty(arg) || arg.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            _ = Client.SendChatMessageAsync("可用服务器:");
            args.Handled = true;
            return;
        }

        var target = Config.Instance.GetServer(arg);
        if (target == null)
        {
            _ = Client.SendChatMessageAsync($"服务器 '{arg}' 不存在!");
            args.Handled = true;
            return;
        }

        if (target.Name == Client.CurrentServer.Name)
        {
            _ = Client.SendChatMessageAsync("你已连接此服务器");
            args.Handled = true;
            return;
        }

        _ = Client.SendChatMessageAsync($"正在切换到服务器: {target.Name}...");
        
        _ = Client.ChangeServerAsync(target);
        
        args.Handled = true;
    }
}
