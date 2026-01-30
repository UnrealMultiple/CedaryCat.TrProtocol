using System.Text;
using TrProtocol;

namespace TrProtocol.TestAgent;

public static class PacketInspector
{
    // Color codes for console
    private const string COLOR_RESET = "\u001b[0m";
    private const string COLOR_GREEN = "\u001b[32m"; // readed
    private const string COLOR_RED = "\u001b[31m";
    private const string COLOR_YELLOW = "\u001b[33m"; // unread
    private const string COLOR_CYAN = "\u001b[36m";

    private const int BYTES_PER_LINE = 16;
    private static PacketInspectorOptions _options = new(
        RoundTripEnabled: false,
        RoundTripDumpMode: "window",
        RoundTripContextLines: 3,
        RoundTripFullDumpThresholdBytes: 256,
        ShowOk: false,
        ShowParseIssues: true,
        ShowRoundTripIssues: true);

    public static void Configure(PacketInspectorOptions options) => _options = options;

    private static unsafe void DumpFull(byte* startPtr, int bufferLen, int expectedLen, int consumedLen) {
        expectedLen = Math.Clamp(expectedLen, 0, bufferLen);
        consumedLen = Math.Clamp(consumedLen, 0, bufferLen);

        for (int i = 0; i < bufferLen; i += BYTES_PER_LINE) {
            Console.Write($"  {i:X4}: ");

            string currentColor = COLOR_RESET;

            for (int j = 0; j < BYTES_PER_LINE && (i + j) < bufferLen; j++) {
                int idx = i + j;

                string nextColor =
                    (idx < consumedLen) ? COLOR_GREEN :
                    (idx < expectedLen) ? COLOR_YELLOW :
                    COLOR_RESET; // The part exceeding expectedLen is not colored

                if (!ReferenceEquals(nextColor, currentColor)) {
                    Console.Write(nextColor);
                    currentColor = nextColor;
                }

                Console.Write($"{startPtr[idx]:X2} ");
            }

            Console.WriteLine(COLOR_RESET);
        }
    }

    private static void WriteByteHex(byte value) => Console.Write($"{value:X2} ");

    private static unsafe void DumpDiffWindow(
        byte* original,
        int originalLen,
        byte* serialized,
        int serializedLen,
        int? focusOffset,
        bool fullDump,
        int contextLines) {
        int maxLen = Math.Max(originalLen, serializedLen);
        if (maxLen <= 0) {
            Console.WriteLine("  (empty)");
            return;
        }

        int totalLines = (maxLen + BYTES_PER_LINE - 1) / BYTES_PER_LINE;
        int startLine = 0;
        int endLine = totalLines - 1;

        if (!fullDump) {
            int focus = Math.Clamp(focusOffset ?? 0, 0, Math.Max(0, maxLen - 1));
            int focusLine = focus / BYTES_PER_LINE;
            startLine = Math.Max(0, focusLine - contextLines);
            endLine = Math.Min(totalLines - 1, focusLine + contextLines);
        }

        for (int line = startLine; line <= endLine; line++) {
            int baseOffset = line * BYTES_PER_LINE;
            Console.Write($"  {baseOffset:X4}: ");

            Console.Write($"{COLOR_CYAN}O{COLOR_RESET} ");
            for (int i = 0; i < BYTES_PER_LINE; i++) {
                int idx = baseOffset + i;
                if (idx >= originalLen) {
                    Console.Write("   ");
                    continue;
                }

                bool hasSer = idx < serializedLen;
                bool equal = hasSer && original[idx] == serialized[idx];

                string color = equal ? COLOR_GREEN : (hasSer ? COLOR_YELLOW : COLOR_RED);
                Console.Write(color);
                WriteByteHex(original[idx]);
                Console.Write(COLOR_RESET);
            }

            Console.Write("| ");
            Console.Write($"{COLOR_CYAN}S{COLOR_RESET} ");
            for (int i = 0; i < BYTES_PER_LINE; i++) {
                int idx = baseOffset + i;
                if (idx >= serializedLen) {
                    Console.Write("   ");
                    continue;
                }

                bool hasOrig = idx < originalLen;
                bool equal = hasOrig && original[idx] == serialized[idx];

                string color = equal ? COLOR_GREEN : (hasOrig ? COLOR_YELLOW : COLOR_RED);
                Console.Write(color);
                WriteByteHex(serialized[idx]);
                Console.Write(COLOR_RESET);
            }

            Console.WriteLine();
        }

        if (!fullDump && totalLines > (endLine - startLine + 1)) {
            Console.WriteLine($"  {COLOR_CYAN}... (set RoundTrip.Dump=\"full\" in testagent.json to dump all){COLOR_RESET}");
        }
    }

    public static unsafe void Inspect(byte[] data, int length, bool isC2S) {
        // Data format: [ushort TotalLength] [byte PacketID] [Content...]
        if (data == null || length <= 0) {
            if (_options.ShowParseIssues) {
                Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[Error] Null/empty buffer{COLOR_RESET}");
            }
            return;
        }

        if (length > data.Length) length = data.Length;

        if (length < 3) {
            if (_options.ShowParseIssues) {
                Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[Error] Packet too short (len={length}){COLOR_RESET}");
            }
            return;
        }

        fixed (byte* startPtr = data) {
            byte* ptr = startPtr;

            int headerTotalLength = *(ushort*)ptr;
            ptr += 2;

            byte packetId = *ptr;

            int expectedLen = Math.Min(headerTotalLength, length);
            byte* expectedEnd = startPtr + expectedLen;

            int minConsumed = Math.Min(length, 3);

            INetPacket? packet = null;
            Exception? parseEx = null;

            try {
                bool readAsServer = isC2S;

                void* ptrVoid = ptr;
                packet = INetPacket.ReadINetPacket(ref ptrVoid, expectedEnd, readAsServer);
                ptr = (byte*)ptrVoid;
            }
            catch (Exception ex) {
                parseEx = ex;
            }

            int consumed = (int)Math.Clamp((long)(ptr - startPtr), 0, length);
            if (consumed < minConsumed) consumed = minConsumed;

            if (parseEx != null) {
                if (_options.ShowParseIssues) {
                    Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[CRITICAL FAIL] Parsing {packetId}: {parseEx.Message}{COLOR_RESET}");
                    DumpFull(startPtr, length, expectedLen, consumed);
                }
                return;
            }

            if (packet == null) {
                if (_options.ShowParseIssues) {
                    Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[Unknown Packet] ID:{packetId} Direction:{(isC2S ? "C->S" : "S->C")}{COLOR_RESET}");
                    DumpFull(startPtr, length, expectedLen, consumed);
                }
                return;
            }

            if (ptr == expectedEnd) {
                if (_options.ShowOk) {
                    Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_GREEN}[OK] {packet.GetType().Name} (ID:{packetId}) Len:{headerTotalLength}{COLOR_RESET}");
                }

                if (_options.RoundTripEnabled && _options.ShowRoundTripIssues && expectedLen == headerTotalLength) {
                    try {
                        VerifyRoundTrip(data, expectedLen, packet, packetId, isC2S);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[RoundTrip Error] {packet.GetType().Name} (ID:{packetId}): {ex.GetType().Name}: {ex.Message}{COLOR_RESET}");
                        DumpFull(startPtr, length, expectedLen, consumed);
                    }
                }
            }
            else if (ptr < expectedEnd) {
                long unreadBytes = expectedEnd - ptr;
                if (_options.ShowParseIssues) {
                    Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[UNDER-READ] {packet.GetType().Name} (ID:{packetId}){COLOR_RESET}");
                    Console.WriteLine($"  Header TotalLength: {headerTotalLength}");
                    Console.WriteLine($"  Parsed Ends At:     {ptr - startPtr}");
                    Console.WriteLine($"  Expected End:       {expectedEnd - startPtr}");
                    Console.WriteLine($"  Leftover:           {unreadBytes} bytes");
                    DumpFull(startPtr, length, expectedLen, consumed);
                }
            }
            else // ptr > expectedEnd
            {
                long overReadBytes = ptr - expectedEnd;
                if (_options.ShowParseIssues) {
                    Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[OVER-READ] {packet.GetType().Name} (ID:{packetId}){COLOR_RESET}");
                    Console.WriteLine($"  Header TotalLength: {headerTotalLength}");
                    Console.WriteLine($"  Parsed read {overReadBytes} bytes beyond expected end.");
                    DumpFull(startPtr, length, expectedLen, consumed);
                }
            }
        }
    }

    private static unsafe void VerifyRoundTrip(byte[] original, int packetLen, INetPacket packet, byte originalPacketId, bool isC2S) {
        // Serialize back into bytes and compare with original framing: [ushort len][byte id][content...]
        // This is opt-in via env var because buggy serializers can write out-of-bounds and crash a live proxy.
        int maxOutLen = Math.Clamp(Math.Max(packetLen * 2, packetLen + 4096), packetLen, 8 * 1024 * 1024);

        byte[] outBytes = new byte[maxOutLen];
        int written;

        fixed (byte* outStart = outBytes)
        fixed (byte* origStart = original) {
            byte* outPtr = outStart;
            *(ushort*)outPtr = (ushort)packetLen;
            outPtr += 2;
            *outPtr = originalPacketId;
            outPtr += 1;

            void* outVoid = outPtr;
            packet.WriteContent(ref outVoid);
            outPtr = (byte*)outVoid;

            written = (int)(outPtr - outStart);

            if (written != packetLen) {
                string kind = written < packetLen ? "UNDER-WRITE" : "OVER-WRITE";
                Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[{kind}] {packet.GetType().Name} (ID:{originalPacketId}){COLOR_RESET}");
                Console.WriteLine($"  Expected TotalLength: {packetLen}");
                Console.WriteLine($"  Serialized Length:    {written}");
                if (written > maxOutLen) {
                    Console.WriteLine($"  Note: serializer exceeded maxOutLen={maxOutLen} (may have corrupted memory).");
                }

                int safeSerializedLen = Math.Clamp(written, 0, maxOutLen);
                int maxComparable = Math.Min(packetLen, safeSerializedLen);
                int firstDiff = -1;
                for (int i = 0; i < maxComparable; i++) {
                    if (outStart[i] != origStart[i]) {
                        firstDiff = i;
                        break;
                    }
                }
                bool fullDump = _options.RoundTripDumpFull || packetLen <= _options.RoundTripFullDumpThresholdBytes;
                DumpDiffWindow(origStart, packetLen, outStart, safeSerializedLen, firstDiff, fullDump, _options.RoundTripContextLines);
                return;
            }

            // Fast compare: length already matches; compare exact bytes.
            bool equal = true;
            for (int i = 0; i < packetLen; i++) {
                if (outStart[i] != origStart[i]) {
                    equal = false;
                    break;
                }
            }

            if (!equal) {
                Console.WriteLine($"[{(isC2S ? "C2S" : "S2C")}]{COLOR_RED}[RoundTrip MISMATCH] {packet.GetType().Name} (ID:{originalPacketId}){COLOR_RESET}");

                // Show where the first mismatch is, without dumping everything by default.
                int firstDiff = -1;
                for (int i = 0; i < packetLen; i++) {
                    if (outStart[i] != origStart[i]) {
                        firstDiff = i;
                        break;
                    }
                }
                if (firstDiff >= 0) {
                    Console.WriteLine($"  First diff at offset 0x{firstDiff:X} (orig={origStart[firstDiff]:X2}, ser={outStart[firstDiff]:X2})");
                }

                bool fullDump = _options.RoundTripDumpFull || packetLen <= _options.RoundTripFullDumpThresholdBytes;
                DumpDiffWindow(origStart, packetLen, outStart, packetLen, firstDiff, fullDump, _options.RoundTripContextLines);
            }
        }
    }
}
