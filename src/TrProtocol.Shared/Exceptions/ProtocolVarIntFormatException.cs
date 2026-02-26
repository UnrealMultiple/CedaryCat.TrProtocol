namespace TrProtocol.Exceptions;

public sealed class ProtocolVarIntFormatException : ProtocolParseException
{
    public ProtocolVarIntFormatException(
        string typeName,
        string memberPath,
        long offset,
        long endOffset,
        int requiredBytes,
        int? loopIndex = null,
        string? localVariables = null)
        : base(
            "Invalid 7-bit encoded integer format encountered during deserialization.",
            typeName,
            memberPath,
            offset,
            endOffset,
            requiredBytes,
            loopIndex,
            localVariables)
    {
    }
}
