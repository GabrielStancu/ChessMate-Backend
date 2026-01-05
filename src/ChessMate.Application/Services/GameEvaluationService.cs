using ChessMate.Application.DTOs;
using ChessMate.Application.Interfaces;
using ChessMate.Domain.Chess.Entities;
using ChessMate.Domain.Chess.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ChessMate.Application.Services;

/// <summary>
/// Service for evaluating chess games using Stockfish engine
/// </summary>
public sealed class GameEvaluationService : IGameEvaluationService
{
    private readonly IStockfishClient _stockfishClient;
    private readonly ILogger<GameEvaluationService> _logger;

    public GameEvaluationService(
        IStockfishClient stockfishClient, 
        ILogger<GameEvaluationService> logger)
    {
        _stockfishClient = stockfishClient;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates all moves in a parsed game
    /// </summary>
    /// <param name="game">The parsed game to evaluate</param>
    /// <param name="depth">Stockfish search depth</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Evaluated game with position analysis for each move</returns>
    public async Task<EvaluatedGameDto> EvaluateGameAsync(ParsedGame game, int depth = 15, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting evaluation of game: {White} vs {Black}", 
            game.WhitePlayer, game.BlackPlayer);

        var evaluatedMoves = new List<EvaluatedMoveDto>();
        var moves = game.Moves.ToList();

        // Evaluate starting position
        var startingFen = game.StartingPosition?.Value ?? FenPosition.StartingPosition;
        var previousEvaluation = await _stockfishClient.EvaluatePositionAsync(
            startingFen, depth, cancellationToken);

        foreach (var move in moves)
        {
            // Evaluate position after this move
            var currentEvaluation = await _stockfishClient.EvaluatePositionAsync(
                move.PositionAfterMove.Value, depth, cancellationToken);

            // Calculate evaluation change from the perspective of the player who moved
            var evaluationChange = move.Color == PieceColor.White 
                ? currentEvaluation.Evaluation.Centipawns - previousEvaluation.Evaluation.Centipawns
                : previousEvaluation.Evaluation.Centipawns - currentEvaluation.Evaluation.Centipawns;

            evaluatedMoves.Add(new EvaluatedMoveDto
            {
                MoveNumber = move.MoveNumber,
                Color = move.Color.ToString(),
                MoveNotation = move.San,
                PositionBefore = previousEvaluation,
                PositionAfter = currentEvaluation,
                EvaluationChange = evaluationChange
            });

            _logger.LogDebug("Evaluated move {MoveNumber}. {Color}: {Move} (? {Change})", 
                move.MoveNumber, move.Color, move.San, evaluationChange);

            previousEvaluation = currentEvaluation;
        }

        return new EvaluatedGameDto
        {
            WhitePlayer = game.WhitePlayer,
            BlackPlayer = game.BlackPlayer,
            WhiteElo = game.WhiteElo,
            BlackElo = game.BlackElo,
            Event = game.Event,
            Date = game.Date,
            Result = game.Result,
            EvaluatedMoves = evaluatedMoves
        };
    }
}