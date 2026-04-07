using TrProtocol;

namespace Dimensions
{
    internal static class Serializers
    {
        public static readonly PacketSerializer clientSerializer = new(true);
        public static readonly PacketSerializer serverSerializer = new(false);
    }
}
