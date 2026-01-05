using ChessMate.Application.DTOs;

namespace ChessMate.Application.Interfaces;

/// <summary>
/// Service interface for interacting with Stockfish chess engine
/// </summary>
public interface IStockfishClient
{
    /// <summary>
    /// Evaluates a chess position using Stockfish
    /// </summary>
    /// <param name="fen">FEN notation of the position to evaluate</param>
    /// <param name="depth">Search depth for evaluation (default: 15)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Position evaluation result</returns>
    Task<PositionEvaluationDto> EvaluatePositionAsync(
        string fen, 
        int depth = 15, 
        CancellationToken cancellationToken = default);
}