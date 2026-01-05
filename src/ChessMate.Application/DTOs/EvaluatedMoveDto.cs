namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for a chess move with position evaluations
/// </summary>
public sealed class EvaluatedMoveDto
{
    public int MoveNumber { get; init; }
    public string Color { get; init; } = string.Empty;
    public string MoveNotation { get; init; } = string.Empty;
    public PositionEvaluationDto PositionBefore { get; init; } = null!;
    public PositionEvaluationDto PositionAfter { get; init; } = null!;
    public int EvaluationChange { get; init; }
}