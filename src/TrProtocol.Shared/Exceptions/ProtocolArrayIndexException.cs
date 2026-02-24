namespace TrProtocol.Exceptions;

public sealed class ProtocolArrayIndexException : ProtocolParseException
{
    public int Index { get; }
    public int ArrayLength { get; }

    public ProtocolArrayIndexException(
        string typeName,
        string memberPath,
        long offset,
        long endOffset,
        int requiredBytes,
        int index,
        int arrayLength,
        int? loopIndex = null,
        string? localVariables = null)
        : base(
            $"Array index {index} is out of bounds for length {arrayLength}.",
            typeName,
            memberPath,
            offset,
            endOffset,
            requiredBytes,
            loopIndex,
            localVariables)
    {
        Index = index;
        ArrayLength = arrayLength;
    }
}
