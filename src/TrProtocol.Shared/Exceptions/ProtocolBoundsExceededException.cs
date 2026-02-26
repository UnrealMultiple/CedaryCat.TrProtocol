namespace TrProtocol.Exceptions;

public sealed class ProtocolBoundsExceededException : ProtocolParseException
{
    public ProtocolBoundsExceededException(
        string typeName,
        string memberPath,
        long offset,
        long endOffset,
        int requiredBytes,
        int? loopIndex = null,
        string? localVariables = null)
        : base(
            "Deserialization tried to read beyond the available boundary.",
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
