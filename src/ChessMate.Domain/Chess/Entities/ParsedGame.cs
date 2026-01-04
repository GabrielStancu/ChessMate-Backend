using ChessMate.Domain.Chess.ValueObjects;

namespace ChessMate.Domain.Chess.Entities;

/// <summary>
/// Represents a fully parsed chess game with moves and metadata
/// </summary>
public sealed class ParsedGame
{
    public string WhitePlayer { get; private set; }
    public string BlackPlayer { get; private set; }
    public string? Event { get; private set; }
    public string? Site { get; private set; }
    public DateOnly? Date { get; private set; }
    public string? Result { get; private set; }
    public string? TimeControl { get; private set; }
    public int? WhiteElo { get; private set; }
    public int? BlackElo { get; private set; }
    public FenPosition StartingPosition { get; private set; }
    public IReadOnlyList<ParsedMove> Moves { get; private set; }

    private ParsedGame() 
    {
        WhitePlayer = string.Empty;
        BlackPlayer = string.Empty;
        StartingPosition = new FenPosition(FenPosition.StartingPosition);
        Moves = new List<ParsedMove>();
    }

    public ParsedGame(
        string whitePlayer,
        string blackPlayer,
        IEnumerable<ParsedMove> moves,
        string? @event = null,
        string? site = null,
        DateOnly? date = null,
        string? result = null,
        string? timeControl = null,
        int? whiteElo = null,
        int? blackElo = null,
        FenPosition? startingPosition = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(whitePlayer);
        ArgumentException.ThrowIfNullOrWhiteSpace(blackPlayer);

        WhitePlayer = whitePlayer;
        BlackPlayer = blackPlayer;
        Event = @event;
        Site = site;
        Date = date;
        Result = result;
        TimeControl = timeControl;
        WhiteElo = whiteElo;
        BlackElo = blackElo;
        StartingPosition = startingPosition ?? new FenPosition(FenPosition.StartingPosition);
        Moves = moves.ToList();
    }

    public ParsedMove? GetMoveAt(int moveNumber, PieceColor color)
    {
        return Moves.FirstOrDefault(m => m.MoveNumber == moveNumber && m.Color == color);
    }

    public FenPosition GetPositionAfterMove(int moveNumber, PieceColor color)
    {
        var move = GetMoveAt(moveNumber, color);
        return move?.PositionAfterMove ?? StartingPosition;
    }
}