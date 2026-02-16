using ChessMate.Application.DTOs;

namespace ChessMate.Application.Interfaces;

public interface IMoveAnalyzerAgentClient
{
    Task AnalyzeGameAsync(EvaluatedGameDto evaluatedGame, CancellationToken cancellationToken);
}