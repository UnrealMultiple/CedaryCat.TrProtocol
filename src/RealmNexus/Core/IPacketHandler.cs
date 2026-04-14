namespace RealmNexus.Core;

public interface IPacketHandler
{
    void OnC2S(PacketInterceptArgs args);
    void OnS2C(PacketInterceptArgs args);

    void OnConnected();

 
    void OnDisconnected();

    void OnServerChanging();

    void OnServerChanged();
}
