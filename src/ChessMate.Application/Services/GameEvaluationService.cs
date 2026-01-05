using ChessMate.Application.DTOs;
using ChessMate.Application.Interfaces;
using ChessMate.Domain.Chess.Entities;
using ChessMate.Domain.Chess.ValueObjects;
using Microsoft.Extensions.Logging;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

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
        var significantMistakes = new List<SignificantMistake>();
        var moves = game.Moves.ToList();

        var startingFen = game.StartingPosition?.Value ?? FenPosition.StartingPosition;
        var previousEvaluation = await _stockfishClient.EvaluatePositionAsync(startingFen, depth, cancellationToken);

        foreach (var move in moves)
        {
            var gamePhase = GamePhase.DeterminePhase(move.MoveNumber, move.PositionAfterMove.Value);
            var currentEvaluation = await _stockfishClient.EvaluatePositionAsync(
                move.PositionAfterMove.Value, depth, cancellationToken);
            var evaluationChange = move.Color == PieceColor.White 
                ? currentEvaluation.Evaluation.Centipawns - previousEvaluation.Evaluation.Centipawns
                : previousEvaluation.Evaluation.Centipawns - currentEvaluation.Evaluation.Centipawns;
            var centipawnLoss = -evaluationChange;
            var classification = MoveClassification.FromCentipawnLoss(centipawnLoss, gamePhase);

            var crtGame = GameFactory.Create(previousEvaluation.Fen);
            var san = new SanNotation(crtGame.Pos);
            var sanMove = san.Convert(previousEvaluation.BestMove);

            var isBestMove = move.San == sanMove;// previousEvaluation.BestMove;

            if (isBestMove && gamePhase != GamePhase.Opening 
                           && classification != MoveClassification.Brilliant
                           && classification != MoveClassification.Great)
            {
                classification = MoveClassification.Best;
            }

            if (classification.IsMistake())
            {
                var mistake = new SignificantMistake(
                    moveNumber: move.MoveNumber,
                    color: move.Color,
                    moveNotation: move.San,
                    centipawnLoss: centipawnLoss,
                    gamePhase: gamePhase,
                    classification: classification,
                    positionFen: previousEvaluation.Fen,
                    bestMove: previousEvaluation.BestMove
                );

                significantMistakes.Add(mistake);

                _logger.LogDebug(
                    "Detected {Classification} in {Phase}: Move {MoveNumber}. {Color}: {Move} (Loss: {Loss} cp, Best: {BestMove})", 
                    classification.Description, gamePhase.Description, move.MoveNumber, 
                    move.Color, move.San, centipawnLoss, previousEvaluation.BestMove);
            }

            evaluatedMoves.Add(new EvaluatedMoveDto
            {
                MoveNumber = move.MoveNumber,
                Color = move.Color.ToString(),
                MoveNotation = move.San,
                PositionBefore = previousEvaluation,
                PositionAfter = currentEvaluation,
                EvaluationChange = evaluationChange,
                CentipawnLoss = centipawnLoss,
                GamePhase = gamePhase.Description,
                Classification = classification.Description,
                ClassificationSymbol = classification.Symbol,
                IsMistake = classification.IsMistake()
            });

            _logger.LogDebug("Evaluated move {MoveNumber}. {Color}: {Move} ({Phase}) - {Classification} (? {Change} cp)", 
                move.MoveNumber, move.Color, move.San, gamePhase.Description, classification.Description, evaluationChange);

            previousEvaluation = currentEvaluation;
        }

        var mistakeStats = CalculateMistakeStatistics(significantMistakes);

        _logger.LogInformation(
            "Evaluation complete. White: {WI} inaccuracies, {WM} mistakes, {WB} blunders. Black: {BI} inaccuracies, {BM} mistakes, {BB} blunders",
            mistakeStats.WhiteInaccuracies, mistakeStats.WhiteMistakes, mistakeStats.WhiteBlunders,
            mistakeStats.BlackInaccuracies, mistakeStats.BlackMistakes, mistakeStats.BlackBlunders);

        return new EvaluatedGameDto
        {
            WhitePlayer = game.WhitePlayer,
            BlackPlayer = game.BlackPlayer,
            WhiteElo = game.WhiteElo,
            BlackElo = game.BlackElo,
            Event = game.Event,
            Date = game.Date,
            Result = game.Result,
            EvaluatedMoves = evaluatedMoves,
            SignificantMistakes = MapSignificantMistakes(significantMistakes),
            WhiteInaccuracies = mistakeStats.WhiteInaccuracies,
            WhiteMistakes = mistakeStats.WhiteMistakes,
            WhiteBlunders = mistakeStats.WhiteBlunders,
            BlackInaccuracies = mistakeStats.BlackInaccuracies,
            BlackMistakes = mistakeStats.BlackMistakes,
            BlackBlunders = mistakeStats.BlackBlunders
        };
    }

    private static MistakeStatistics CalculateMistakeStatistics(List<SignificantMistake> mistakes)
    {
        return new MistakeStatistics
        {
            WhiteInaccuracies = mistakes.Count(m => m.Color == PieceColor.White && m.Classification.Quality == MoveQuality.Inaccuracy),
            WhiteMistakes = mistakes.Count(m => m.Color == PieceColor.White && m.Classification.Quality == MoveQuality.Mistake),
            WhiteBlunders = mistakes.Count(m => m.Color == PieceColor.White && m.Classification.Quality == MoveQuality.Blunder),
            BlackInaccuracies = mistakes.Count(m => m.Color == PieceColor.Black && m.Classification.Quality == MoveQuality.Inaccuracy),
            BlackMistakes = mistakes.Count(m => m.Color == PieceColor.Black && m.Classification.Quality == MoveQuality.Mistake),
            BlackBlunders = mistakes.Count(m => m.Color == PieceColor.Black && m.Classification.Quality == MoveQuality.Blunder)
        };
    }

    private static IReadOnlyList<SignificantMistakeDto> MapSignificantMistakes(List<SignificantMistake> mistakes)
    {
        return mistakes
            .OrderByDescending(m => m.CentipawnLoss)
            .Select(m => new SignificantMistakeDto
            {
                MoveNumber = m.MoveNumber,
                Color = m.Color.ToString(),
                MoveNotation = m.MoveNotation,
                CentipawnLoss = m.CentipawnLoss,
                GamePhase = m.GamePhase.Description,
                Classification = m.Classification.Description,
                ClassificationSymbol = m.Classification.Symbol,
                PositionFen = m.PositionFen,
                BestMove = m.BestMove
            })
            .ToList();
    }

    private sealed record MistakeStatistics
    {
        public int WhiteInaccuracies { get; init; }
        public int WhiteMistakes { get; init; }
        public int WhiteBlunders { get; init; }
        public int BlackInaccuracies { get; init; }
        public int BlackMistakes { get; init; }
        public int BlackBlunders { get; init; }
    }
}