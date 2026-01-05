using ChessMate.Domain.Chess.ValueObjects;

namespace ChessMate.Domain.Chess.Entities;

/// <summary>
/// Represents a single parsed chess move with its metadata
/// </summary>
public sealed class ParsedMove
{
    public int MoveNumber { get; private set; }
    public PieceColor Color { get; private set; }
    public string San { get; private set; }
    public FenPosition PositionAfterMove { get; private set; }
    public string? Comment { get; private set; }
    public TimeSpan? TimeRemaining { get; private set; }

    private ParsedMove() { }

    public ParsedMove(int moveNumber, PieceColor color, string san,
        FenPosition positionAfterMove, string? comment = null, TimeSpan? timeRemaining = null)
    {
        if (moveNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(moveNumber), "Move number must be positive");

        ArgumentException.ThrowIfNullOrWhiteSpace(san);

        MoveNumber = moveNumber;
        Color = color;
        San = san;
        PositionAfterMove = positionAfterMove;
        Comment = comment;
        TimeRemaining = timeRemaining;
    }
}