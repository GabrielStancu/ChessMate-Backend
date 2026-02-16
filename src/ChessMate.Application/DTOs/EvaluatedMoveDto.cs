using System.Text.Json.Serialization;

namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for a chess move with position evaluations
/// </summary>
public sealed class EvaluatedMoveDto
{
    [JsonPropertyName("moveNumber")]
    public int MoveNumber { get; init; }

    [JsonPropertyName("color")]
    public string Color { get; init; } = string.Empty;

    [JsonPropertyName("moveNotation")]
    public string MoveNotation { get; init; } = string.Empty;

    [JsonPropertyName("positionBefore")]
    public PositionEvaluationDto PositionBefore { get; init; } = null!;

    [JsonPropertyName("positionAfter")]
    public PositionEvaluationDto PositionAfter { get; init; } = null!;

    [JsonPropertyName("evaluationChange")]
    public int EvaluationChange { get; init; }

    [JsonPropertyName("centipawnLoss")]
    public int CentipawnLoss { get; init; }

    [JsonPropertyName("gamePhase")]
    public string GamePhase { get; init; } = string.Empty;

    [JsonPropertyName("classification")]
    public string Classification { get; init; } = string.Empty;

    [JsonPropertyName("classificationSymbol")]
    public string ClassificationSymbol { get; init; } = string.Empty;

    [JsonPropertyName("isMistake")]
    public bool IsMistake { get; init; }

    [JsonPropertyName("isGreatOrBrilliant")]
    public bool IsGreatOrBrilliant { get; init; }

    [JsonPropertyName("intent")]
    public string Intent { get; private set; } = string.Empty;

    [JsonPropertyName("opponentIntent")]
    public string OpponentIntent { get; private set; } = string.Empty;

    [JsonPropertyName("suggestedPlan")]
    public string SuggestedPlan { get; private set; } = string.Empty;

    public void SetExplanations(string intent, string opponentIntent, string suggestedPlan)
    {
        Intent = intent;
        OpponentIntent = opponentIntent;
        SuggestedPlan = suggestedPlan;
    }
}