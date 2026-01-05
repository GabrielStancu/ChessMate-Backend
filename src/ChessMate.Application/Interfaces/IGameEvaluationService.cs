using ChessMate.Application.DTOs;
using ChessMate.Domain.Chess.Entities;

namespace ChessMate.Application.Interfaces;

public interface IGameEvaluationService
{
    /// <summary>
    /// Evaluates all moves in a parsed game
    /// </summary>
    /// <param name="game">The parsed game to evaluate</param>
    /// <param name="depth">Stockfish search depth</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Evaluated game with position analysis for each move</returns>
    Task<EvaluatedGameDto> EvaluateGameAsync(ParsedGame game, int depth = 15, CancellationToken cancellationToken = default);
}