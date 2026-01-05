namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for a fully evaluated chess game
/// </summary>
public sealed class EvaluatedGameDto
{
    public string WhitePlayer { get; init; } = string.Empty;
    public string BlackPlayer { get; init; } = string.Empty;
    public int? WhiteElo { get; init; }
    public int? BlackElo { get; init; }
    public string? Event { get; init; }
    public DateOnly? Date { get; init; }
    public string? Result { get; init; }
    public IReadOnlyList<EvaluatedMoveDto> EvaluatedMoves { get; init; } = Array.Empty<EvaluatedMoveDto>();
}