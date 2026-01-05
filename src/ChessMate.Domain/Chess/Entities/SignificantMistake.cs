using ChessMate.Domain.Chess.ValueObjects;

namespace ChessMate.Domain.Chess.Entities;

/// <summary>
/// Represents a significant mistake made during a chess game
/// </summary>
public sealed class SignificantMistake
{
    public int MoveNumber { get; }
    public PieceColor Color { get; }
    public string MoveNotation { get; }
    public int CentipawnLoss { get; }
    public GamePhase GamePhase { get; }
    public MoveClassification Classification { get; }
    public string PositionFen { get; }
    public string BestMove { get; }

    public SignificantMistake(
        int moveNumber,
        PieceColor color,
        string moveNotation,
        int centipawnLoss,
        GamePhase gamePhase,
        MoveClassification classification,
        string positionFen,
        string bestMove)
    {
        if (moveNumber <= 0)
            throw new ArgumentException("Move number must be positive", nameof(moveNumber));
        if (string.IsNullOrWhiteSpace(moveNotation))
            throw new ArgumentException("Move notation cannot be empty", nameof(moveNotation));
        if (string.IsNullOrWhiteSpace(positionFen))
            throw new ArgumentException("Position FEN cannot be empty", nameof(positionFen));
        if (string.IsNullOrWhiteSpace(bestMove))
            throw new ArgumentException("Best move cannot be empty", nameof(bestMove));

        MoveNumber = moveNumber;
        Color = color;
        MoveNotation = moveNotation;
        CentipawnLoss = centipawnLoss;
        GamePhase = gamePhase;
        Classification = classification;
        PositionFen = positionFen;
        BestMove = bestMove;
    }

    public string GetPlayerDescription() => Color == PieceColor.White ? "White" : "Black";

    public override string ToString() => 
        $"Move {MoveNumber} ({GetPlayerDescription()}, {GamePhase.Description}): {MoveNotation} - {Classification.Description} ({CentipawnLoss:+0;-0} cp)";
}