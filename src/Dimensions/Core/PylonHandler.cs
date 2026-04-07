using Terraria.DataStructures;
using Terraria.GameContent;
using TrProtocol.NetPackets.Modules;
using static Terraria.GameContent.NetModules.NetTeleportPylonModule;

namespace Dimensions.Core;

public class PylonHandler : ClientHandler
{
    private const byte maxPylon = 9;
    private readonly Point16[] activePylon = new Point16[maxPylon];

    public PylonHandler()
    {
        for (short i = 0; i < maxPylon; ++i)
            activePylon[i] = new Point16(-1, -1);
    }

    public override void OnCommonPacket(PacketReceiveArgs args)
    {
        if (args.Packet is NetTeleportPylonModule pylon)
            if (pylon.PylonPacketType == SubPacketType.PylonWasAdded)
                activePylon[(int)pylon.PylonType] = pylon.Position;
            else
                activePylon[(int)pylon.PylonType] = new Point16(-1, -1);
    }

    public override void OnCleaning()
    {
        for (short i = 0; i < maxPylon; ++i)
        {
            if (activePylon[i].X >= 0 && activePylon[i].Y >= 0)
            {
                Parent.SendClient(new NetTeleportPylonModule
                {
                    PylonPacketType = SubPacketType.PylonWasRemoved,
                    PylonType = (TeleportPylonType)i,
                    Position = activePylon[i]
                });
                activePylon[i] = new Point16(-1, -1);
            }
        }
    }
}