namespace ChessMate.Domain.Chess.ValueObjects;

/// <summary>
/// Represents a FEN (Forsyth-Edwards Notation) position
/// </summary>
public sealed record FenPosition
{
    public const string StartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public string Value { get; }

    public FenPosition(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static implicit operator string(FenPosition fen) => fen.Value;
    public override string ToString() => Value;
}