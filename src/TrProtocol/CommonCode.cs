using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TrProtocol.Exceptions;

namespace TrProtocol;

public static unsafe class CommonCode
{
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FastAllocateString")]
    extern static string FastAllocateString(int length);

    #region String

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteString(ref void* current, string value) {
        Write7BitEncodedInt(ref current, Encoding.UTF8.GetByteCount(value));
        current = Unsafe.Add<byte>(current, Encoding.UTF8.GetBytes(value, new Span<byte>(current, short.MaxValue)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Write7BitEncodedInt(ref void* destination, int value) {
        byte* ptr = (byte*)destination;
        uint num = (uint)value;

        while (num >= 0x80) {
            *ptr++ = (byte)(num | 0x80);
            num >>= 7;
        }
        *ptr++ = (byte)num;

        destination = ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureReadable(
        void* ptr,
        void* endPtr,
        int requiredBytes) {
#if DEBUG
        if (requiredBytes < 0) {
            ThrowNegativeRequired(ptr, endPtr, requiredBytes, "", "", null, null);
        }
#endif

        if ((nuint)((byte*)endPtr - (byte*)ptr) < (nuint)requiredBytes) {
            ThrowBoundsExceeded(ptr, endPtr, requiredBytes
#if DEBUG
                , "", "", null, null
#endif
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureReadable(
        void* ptr,
        void* endPtr,
        long requiredBytes) {
#if DEBUG
        if (requiredBytes < 0) {
            ThrowNegativeRequired(ptr, endPtr, SaturatingRequiredBytes(requiredBytes), "", "", null, null);
        }
#endif

        long remaining = (byte*)endPtr - (byte*)ptr;
        if (remaining < 0 || remaining < requiredBytes) {
            ThrowBoundsExceeded(ptr, endPtr, SaturatingRequiredBytes(requiredBytes)
#if DEBUG
                , "", "", null, null
#endif
            );
        }
    }

    public static void EnsureReadable(
        void* ptr,
        void* endPtr,
        int requiredBytes,
        string typeName,
        string memberPath,
        int? loopIndex = null,
        string? localVariables = null) {
#if DEBUG
        if (requiredBytes < 0) {
            ThrowNegativeRequired(ptr, endPtr, requiredBytes, typeName, memberPath, loopIndex, localVariables);
        }

        if ((nuint)((byte*)endPtr - (byte*)ptr) < (nuint)requiredBytes) {
            ThrowBoundsExceeded(ptr, endPtr, requiredBytes, typeName, memberPath, loopIndex, localVariables);
        }
#else
        EnsureReadable(ptr, endPtr, requiredBytes);
#endif
    }

    public static void EnsureReadable(
        void* ptr,
        void* endPtr,
        long requiredBytes,
        string typeName,
        string memberPath,
        int? loopIndex = null,
        string? localVariables = null) {
#if DEBUG
        string? withRequiredBytes = localVariables is null
            ? $"requiredBytes64={requiredBytes}"
            : $"{localVariables};requiredBytes64={requiredBytes}";

        if (requiredBytes < 0) {
            ThrowNegativeRequired(ptr, endPtr, SaturatingRequiredBytes(requiredBytes), typeName, memberPath, loopIndex, withRequiredBytes);
        }

        long remaining = (byte*)endPtr - (byte*)ptr;
        if (remaining < 0 || remaining < requiredBytes) {
            ThrowBoundsExceeded(ptr, endPtr, SaturatingRequiredBytes(requiredBytes), typeName, memberPath, loopIndex, withRequiredBytes);
        }
#else
        EnsureReadable(ptr, endPtr, requiredBytes);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string ReadString(
        ref void* ptr,
        void* endPtr) {
        int byteCount = Read7BitEncodedInt(ref ptr, endPtr);
        if (byteCount < 0) {
            ThrowVarIntFormat(ptr, endPtr, "", "", byteCount, null, null);
        }

        EnsureReadable(ptr, endPtr, byteCount);
        var span = new ReadOnlySpan<byte>(ptr, byteCount);
        string result = Encoding.UTF8.GetString(span);
        ptr = Unsafe.Add<byte>(ptr, byteCount);
        return result;
    }

    public static unsafe string ReadString(
        ref void* ptr,
        void* endPtr,
        string typeName,
        string memberPath,
        int? loopIndex = null,
        string? localVariables = null) {
#if DEBUG
        int byteCount = Read7BitEncodedInt(ref ptr, endPtr, typeName, memberPath, loopIndex, localVariables);
        if (byteCount < 0) {
            throw new ProtocolVarIntFormatException(
                typeName,
                memberPath,
                (long)ptr,
                (long)endPtr,
                byteCount,
                loopIndex,
                localVariables);
        }

        EnsureReadable(ptr, endPtr, byteCount, typeName, memberPath, loopIndex, localVariables);
        var span = new ReadOnlySpan<byte>(ptr, byteCount);
        string result = Encoding.UTF8.GetString(span);
        ptr = Unsafe.Add<byte>(ptr, byteCount);
        return result;
#else
        return ReadString(ref ptr, endPtr);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Read7BitEncodedInt(
        ref void* source,
        void* endPtr) {
        byte* ptr = (byte*)source;
        int result = 0;
        int bits = 0;
        int byteIndex = 0;
        byte b;

        do {
            EnsureReadable(ptr, endPtr, 1);

            b = *ptr++;
            result |= (b & 0x7F) << bits;
            bits += 7;

            if (bits > 35) {
                ThrowVarIntFormat(ptr, endPtr, "", "", 1, null, null);
            }

            byteIndex++;
        } while ((b & 0x80) != 0);

        source = ptr;
        return result;
    }

    public static unsafe int Read7BitEncodedInt(
        ref void* source,
        void* endPtr,
        string typeName,
        string memberPath,
        int? loopIndex = null,
        string? localVariables = null) {
#if DEBUG
        byte* ptr = (byte*)source;
        int result = 0;
        int bits = 0;
        int byteIndex = 0;
        byte b;

        do {
            string? varIntLocalState = null;
#if DEBUG
            varIntLocalState = $"{localVariables};varintByteIndex={byteIndex};bits={bits}";
#endif
            EnsureReadable(
                ptr,
                endPtr,
                1,
                typeName,
                memberPath,
                loopIndex,
                varIntLocalState);

            b = *ptr++;
            result |= (b & 0x7F) << bits;
            bits += 7;

            if (bits > 35) {
                ThrowVarIntFormat(ptr, endPtr, typeName, memberPath, 1, loopIndex, varIntLocalState);
            }

            byteIndex++;
        } while ((b & 0x80) != 0);

        source = ptr;
        return result;
#else
        return Read7BitEncodedInt(ref source, endPtr);
#endif
    }
    #endregion

    #region Compression
    public static unsafe void ReadDecompressedData(
        void* source,
        ref void* destination,
        void* destinationEnd,
        int compressedDataLength) {
        ReadDecompressedData(source, ref destination, destinationEnd, compressedDataLength, "", "");
    }

    public static unsafe void ReadDecompressedData(
        void* source,
        ref void* destination,
        void* destinationEnd,
        int compressedDataLength,
        string typeName,
        string memberPath) {
        using var st = new UnmanagedMemoryStream((byte*)source, compressedDataLength, compressedDataLength, FileAccess.Read);
        using (var dst = new DeflateStream(st, CompressionMode.Decompress, true)) {
            while (true) {
                int remaining = (int)((long)destinationEnd - (long)destination);
                if (remaining < 0) {
                    throw new ProtocolBoundsExceededException(
                        typeName,
                        memberPath,
                        (long)destination,
                        (long)destinationEnd,
                        0);
                }

                if (remaining == 0) {
                    int next = dst.ReadByte();
                    if (next >= 0) {
                        throw new ProtocolBoundsExceededException(
                            typeName,
                            memberPath,
                            (long)destination,
                            (long)destinationEnd,
                            1,
                            localVariables: $"compressedDataLength={compressedDataLength}");
                    }
                    break;
                }

                int readed = dst.Read(new Span<byte>(destination, Math.Min(remaining, 1024 * 32)));
                if (readed == 0) {
                    break;
                }
                destination = Unsafe.Add<byte>(destination, readed);
            }
        }
    }

    public static unsafe void WriteCompressedData(void* source, ref void* destination, int rawDataLength, CompressionLevel level) {
        using var st = new UnmanagedMemoryStream((byte*)destination, 1024 * 64, 1024 * 64, FileAccess.Write);
        using (var dst = new DeflateStream(st, level, true)) {
            dst.Write(new Span<byte>(source, rawDataLength));
        }
        destination = st.PositionPointer;
    }
    #endregion

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNegativeRequired(
        void* ptr,
        void* endPtr,
        int requiredBytes
#if DEBUG
        ,
        string typeName,
        string memberPath,
        int? loopIndex,
        string? localVariables
#endif
        ) {
#if DEBUG
        throw new ProtocolParseException(
            "Negative required byte count encountered during deserialization.",
            typeName,
            memberPath,
            (long)ptr,
            (long)endPtr,
            requiredBytes,
            loopIndex,
            localVariables);
#else
        throw new ProtocolParseException(
            "Negative required byte count encountered during deserialization.",
            "",
            "",
            (long)ptr,
            (long)endPtr,
            requiredBytes);
#endif
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowBoundsExceeded(
        void* ptr,
        void* endPtr,
        int requiredBytes
#if DEBUG
        ,
        string typeName,
        string memberPath,
        int? loopIndex,
        string? localVariables
#endif
        ) {
#if DEBUG
        throw new ProtocolBoundsExceededException(
            typeName,
            memberPath,
            (long)ptr,
            (long)endPtr,
            requiredBytes,
            loopIndex,
            localVariables);
#else
        throw new ProtocolBoundsExceededException(
            "",
            "",
            (long)ptr,
            (long)endPtr,
            requiredBytes);
#endif
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowVarIntFormat(
        void* ptr,
        void* endPtr,
        string typeName,
        string memberPath,
        int requiredBytes,
        int? loopIndex,
        string? localVariables) {
#if DEBUG
        throw new ProtocolVarIntFormatException(
            typeName,
            memberPath,
            (long)ptr,
            (long)endPtr,
            requiredBytes,
            loopIndex,
            localVariables);
#else
        throw new ProtocolVarIntFormatException(
            "",
            "",
            (long)ptr,
            (long)endPtr,
            requiredBytes);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SaturatingRequiredBytes(long requiredBytes) {
        if (requiredBytes > int.MaxValue) {
            return int.MaxValue;
        }
        if (requiredBytes < int.MinValue) {
            return int.MinValue;
        }
        return (int)requiredBytes;
    }
}
