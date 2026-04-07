using TrProtocol;
using TrProtocol.Attributes;
using TrProtocol.Interfaces;

namespace Dimensions.Packets
{
    public enum SubMessageID : short
    {
        ClientAddress = 1,
        ChangeSever = 2,
        ChangeCustomizedServer = 3,
        OnlineInfoRequest = 4,
        OnlineInfoResponse = 5
    }

    public struct DimensionUpdate : IBinarySerializable
    {
        public readonly MessageID Type => MessageID.Unused67;
        public SubMessageID SubType { get; set; }
        public string Content { get; set; }
        private bool ShouldHasPort => SubType == SubMessageID.ChangeCustomizedServer || SubType == SubMessageID.ClientAddress;
        [Condition(nameof(ShouldHasPort))]
        public ushort Port { get; set; }

        public unsafe void ReadContent(ref void* ptr, void* end_ptr)
        {
            SubType = (SubMessageID)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<short>(ptr);
            ptr = (byte*)ptr + sizeof(short);

            int contentLength = CommonCode.Read7BitEncodedInt(ref ptr, end_ptr);
            Content = System.Text.Encoding.UTF8.GetString(new ReadOnlySpan<byte>(ptr, contentLength));
            ptr = (byte*)ptr + contentLength;

            if (ShouldHasPort)
            {
                Port = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ushort>(ptr);
                ptr = (byte*)ptr + sizeof(ushort);
            }
        }

        public unsafe void WriteContent(ref void* ptr)
        {
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptr, (short)SubType);
            ptr = (byte*)ptr + sizeof(short);

            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(Content ?? string.Empty);
            CommonCode.Write7BitEncodedInt(ref ptr, contentBytes.Length);
            fixed (byte* p = contentBytes)
            {
                System.Buffer.MemoryCopy(p, ptr, contentBytes.Length, contentBytes.Length);
            }
            ptr = (byte*)ptr + contentBytes.Length;

            if (ShouldHasPort)
            {
                System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptr, Port);
                ptr = (byte*)ptr + sizeof(ushort);
            }
        }
    }
}
