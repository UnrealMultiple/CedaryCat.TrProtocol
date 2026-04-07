namespace Dimensions.Core
{
    public class PacketReceiveArgs
    {
        public readonly object Packet;
        public bool Handled;

        public PacketReceiveArgs(object packet)
        {
            Packet = packet;
        }
    }
}
