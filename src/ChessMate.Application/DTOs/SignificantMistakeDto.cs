using System.Text.Json.Serialization;

namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for a significant mistake in a chess game
/// </summary>
public sealed class SignificantMistakeDto
{
    [JsonPropertyName("moveNumber")]
    public int MoveNumber { get; init; }

    [JsonPropertyName("color")]
    public string Color { get; init; } = string.Empty;

    [JsonPropertyName("moveNotation")]
    public string MoveNotation { get; init; } = string.Empty;

    [JsonPropertyName("centipawnLoss")]
    public int CentipawnLoss { get; init; }

    [JsonPropertyName("gamePhase")]
    public string GamePhase { get; init; } = string.Empty;

    [JsonPropertyName("classification")]
    public string Classification { get; init; } = string.Empty;

    [JsonPropertyName("classificationSymbol")]
    public string ClassificationSymbol { get; init; } = string.Empty;

    [JsonPropertyName("positionFen")]
    public string PositionFen { get; init; } = string.Empty;

    [JsonPropertyName("bestMove")]
    public string BestMove { get; init; } = string.Empty;
}