using System.Text.Json.Serialization;

namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for a fully evaluated chess game
/// </summary>
public sealed class EvaluatedGameDto
{
    [JsonPropertyName("whitePlayer")]
    public string WhitePlayer { get; init; } = string.Empty;

    [JsonPropertyName("blackPlayer")]
    public string BlackPlayer { get; init; } = string.Empty;

    [JsonPropertyName("whiteElo")]
    public int? WhiteElo { get; init; }

    [JsonPropertyName("blackElo")]
    public int? BlackElo { get; init; }

    [JsonPropertyName("event")]
    public string? Event { get; init; }

    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("result")]
    public string? Result { get; init; }

    [JsonPropertyName("evaluatedMoves")]
    public IReadOnlyList<EvaluatedMoveDto> EvaluatedMoves { get; init; } = Array.Empty<EvaluatedMoveDto>();

    [JsonPropertyName("significantMistakes")]
    public IReadOnlyList<SignificantMistakeDto> SignificantMistakes { get; init; } = Array.Empty<SignificantMistakeDto>();

    [JsonPropertyName("whiteInaccuracies")]
    public int WhiteInaccuracies { get; init; }

    [JsonPropertyName("whiteMistakes")]
    public int WhiteMistakes { get; init; }

    [JsonPropertyName("whiteBlunders")]
    public int WhiteBlunders { get; init; }

    [JsonPropertyName("blackInaccuracies")]
    public int BlackInaccuracies { get; init; }

    [JsonPropertyName("blackMistakes")]
    public int BlackMistakes { get; init; }

    [JsonPropertyName("blackBlunders")]
    public int BlackBlunders { get; init; }
}