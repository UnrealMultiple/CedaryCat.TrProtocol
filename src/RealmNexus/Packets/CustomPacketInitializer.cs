using TrProtocol;

namespace RealmNexus.Packets;

public static class CustomPacketInitializer
{
    public static void RegisterAll()
    {
        CustomPacketRegistry.Register<DimensionUpdate>(MessageID.Unused67);
    }
}
