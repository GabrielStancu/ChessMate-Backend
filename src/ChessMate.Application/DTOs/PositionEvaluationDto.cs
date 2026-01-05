using System.Text.Json.Serialization;

namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for Stockfish position evaluation
/// </summary>
public sealed class PositionEvaluationDto
{
    [JsonPropertyName("fen")]
    public string Fen { get; init; } = string.Empty;

    [JsonPropertyName("depth")]
    public int Depth { get; init; }

    [JsonPropertyName("moveTimeMs")]
    public int MoveTimeMs { get; init; }

    [JsonPropertyName("evaluation")]
    public EvaluationDto Evaluation { get; init; } = new();
    
    [JsonPropertyName("bestMove")]
    public string BestMove { get; init; } = string.Empty;
}

public sealed class EvaluationDto
{
    [JsonPropertyName("centipawns")]
    public int Centipawns { get; init; }

    [JsonPropertyName("mateIn")]
    public string? MateIn { get; init; }
}