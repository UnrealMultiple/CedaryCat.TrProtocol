namespace TrProtocol.Exceptions;

public class ProtocolParseException : Exception
{
    public string TypeName { get; }
    public string MemberPath { get; }
    public long Offset { get; }
    public long EndOffset { get; }
    public int RequiredBytes { get; }
    public int? LoopIndex { get; }
    public string? LocalVariables { get; }

    public ProtocolParseException(
        string message,
        string typeName,
        string memberPath,
        long offset,
        long endOffset,
        int requiredBytes,
        int? loopIndex = null,
        string? localVariables = null,
        Exception? innerException = null)
        : base(BuildMessage(message, typeName, memberPath, offset, endOffset, requiredBytes, loopIndex, localVariables), innerException)
    {
        TypeName = typeName;
        MemberPath = memberPath;
        Offset = offset;
        EndOffset = endOffset;
        RequiredBytes = requiredBytes;
        LoopIndex = loopIndex;
        LocalVariables = localVariables;
    }

    private static string BuildMessage(
        string message,
        string typeName,
        string memberPath,
        long offset,
        long endOffset,
        int requiredBytes,
        int? loopIndex,
        string? localVariables)
    {
        var loopText = loopIndex.HasValue ? $" LoopIndex={loopIndex.Value}." : string.Empty;
        var localsText = string.IsNullOrWhiteSpace(localVariables) ? string.Empty : $" Locals={localVariables}.";
        return $"{message} Type={typeName}, Member={memberPath}, Offset={offset}, EndOffset={endOffset}, RequiredBytes={requiredBytes}.{loopText}{localsText}";
    }
}
