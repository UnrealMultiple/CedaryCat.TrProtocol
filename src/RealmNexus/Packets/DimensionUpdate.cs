using System.Runtime.CompilerServices;
using System.Text;
using TrProtocol;

namespace RealmNexus.Packets;

public enum SubMessageID : short
{
    ClientAddress = 1,
    ChangeServer = 2,
    ChangeCustomizedServer = 3,
    OnlineInfoRequest = 4,
    OnlineInfoResponse = 5
}

public struct DimensionUpdate : ICustomPacket
{
    public readonly MessageID Type => MessageID.Unused67;
    public SubMessageID SubType { get; set; }
    public string Content { get; set; }
    public ushort Port { get; set; }

    public unsafe void ReadContent(ref void* ptr, void* end_ptr)
    {
        SubType = (SubMessageID)Unsafe.ReadUnaligned<short>(ptr);
        ptr = (byte*)ptr + sizeof(short);

        int contentLength = CommonCode.Read7BitEncodedInt(ref ptr, end_ptr);
        Content = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(ptr, contentLength));
        ptr = (byte*)ptr + contentLength;

        // 只有特定子类型才有 Port
        if (SubType == SubMessageID.ChangeCustomizedServer || SubType == SubMessageID.ClientAddress)
        {
            Port = Unsafe.ReadUnaligned<ushort>(ptr);
            ptr = (byte*)ptr + sizeof(ushort);
        }
    }

    public unsafe void WriteContent(ref void* ptr)
    {
        Unsafe.WriteUnaligned(ptr, (short)SubType);
        ptr = (byte*)ptr + sizeof(short);

        byte[] contentBytes = Encoding.UTF8.GetBytes(Content ?? string.Empty);
        CommonCode.Write7BitEncodedInt(ref ptr, contentBytes.Length);
        fixed (byte* p = contentBytes)
        {
            Buffer.MemoryCopy(p, ptr, contentBytes.Length, contentBytes.Length);
        }
        ptr = (byte*)ptr + contentBytes.Length;

        // 只有特定子类型才有 Port
        if (SubType == SubMessageID.ChangeCustomizedServer || SubType == SubMessageID.ClientAddress)
        {
            Unsafe.WriteUnaligned(ptr, Port);
            ptr = (byte*)ptr + sizeof(ushort);
        }
    }
}
