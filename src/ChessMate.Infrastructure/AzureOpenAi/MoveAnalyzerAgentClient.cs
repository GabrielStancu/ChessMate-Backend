using Azure.AI.OpenAI;
using ChessMate.Application.DTOs;
using ChessMate.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ChessMate.Infrastructure.AzureOpenAi;

public class MoveAnalyzerAgentClient : IMoveAnalyzerAgentClient
{
    private readonly AzureOpenAIClient _openAiClient;
    private readonly ILogger<MoveAnalyzerAgentClient> _logger;

    public MoveAnalyzerAgentClient(AzureOpenAIClient openAiClient, ILogger<MoveAnalyzerAgentClient> logger)
    {
        _openAiClient = openAiClient;
        _logger = logger;
    }

    public async Task AnalyzeGameAsync(EvaluatedGameDto evaluatedGame, CancellationToken cancellationToken)
    {
        var systemPrompt = @"You are a chess coach explaining individual moves to a 1200–1300 Elo player.

You do NOT evaluate positions or mention engines, evaluations, or centipawns.
You ONLY explain the move itself and its practical meaning.

Rules:
- Be concise and concrete.
- Use at most ONE sentence per section.
- No bullet points.
- No extra commentary.
- No repetition.
- No mention of engines or evaluations.

If a move is marked as ""Best"", explain why it works.
Do not criticize best moves.
Follow the provided structure exactly.

You are going to receive moves from the same game in sequence, with this JSON structure:

{
  ""playerColor"": ""White"",
  ""playerElo"": 1241,

  ""moveNumber"": 5,
  ""movePlayed"": ""Qd2"",

  ""moveQuality"": ""Best"",

  ""positionBefore"": {
    ""fen"": ""rnbqk2r/ppp1ppbp/3p1np1/8/3PPB2/2N5/PPP2PPP/R2QKBNR w KQkq - 2 9""
  },

  ""positionAfter"": {
    ""fen"": ""rnbqk2r/ppp1ppbp/3p1np1/8/3PPB2/2N5/PPPQ1PPP/R3KBNR b KQkq - 3 10""
  }
}

Explain this move using the following structure, with ONE sentence per section. You will return a JSON object with the same structure for each move, with the following sections:

Intent:
<One sentence>

What’s Next for the Opponent:
<One sentence>

Best Move & Plan:
<One sentence>

Expected output:

{
""Intent"": ""You play Qd2 to connect your rooks and prepare flexible king safety, including the option to castle long."",

""OpponentIntent"": ""Black will likely continue normal development, such as castling, since there is no immediate threat."",

""SuggestedPlan"": ""Continue developing smoothly, castle when appropriate, and bring a rook to the center.""
}
";
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };
        var chatClient = _openAiClient.GetChatClient("move-analyzer");

        _logger.LogInformation("Starting game analysis...");

        try
        {
            foreach (var move in evaluatedGame.EvaluatedMoves.Where(m => m.IsMistake || m.IsGreatOrBrilliant))
            {
                _logger.LogInformation("Analyzing move {MoveNumber}: {MovePlayed} ({MoveQuality})", move.MoveNumber, move.MoveNotation, move.Classification);

                var moveMessage = FormatMove(move, move.Color == "White" ? evaluatedGame.WhiteElo ?? 1200 : evaluatedGame.BlackElo ?? 1200);
                var moveChatMessage = new UserChatMessage(moveMessage);
                var response = await chatClient.CompleteChatAsync(messages.Append(moveChatMessage), new ChatCompletionOptions(), cancellationToken);
                var responseText = response.Value.Content.FirstOrDefault()?.Text;
                var moveAnalysis = System.Text.Json.JsonSerializer.Deserialize<MoveAnalysisResult>(responseText!);

                move.SetExplanations(moveAnalysis!.Intent, moveAnalysis.OpponentIntent, moveAnalysis.SuggestedPlan);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing game");
        }
    }

    private string FormatMove(EvaluatedMoveDto move, int elo)
    {
        return $@"{{
            ""playerColor"": ""{move.Color}"",
            ""playerElo"": {elo},

            ""moveNumber"": {move.MoveNumber},
            ""movePlayed"": ""{move.MoveNotation}"",

            ""moveQuality"": ""{move.Classification}"",

            ""positionBefore"": {{
                ""fen"": ""{move.PositionBefore.Fen}""
            }},

            ""positionAfter"": {{
                ""fen"": ""{move.PositionAfter.Fen}""
            }}
        }}";
    }

    private class MoveAnalysisResult
    {
        public string Intent { get; set; } = string.Empty;
        public string OpponentIntent { get; set; } = string.Empty;
        public string SuggestedPlan { get; set; } = string.Empty;
    }
}
