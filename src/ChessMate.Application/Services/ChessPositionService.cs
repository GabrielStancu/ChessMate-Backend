using ChessMate.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

namespace ChessMate.Application.Services;

/// <summary>
/// Service for chess position manipulation using Rudzoft.ChessLib
/// </summary>
public sealed class ChessPositionService : IChessPositionService
{
    private readonly ILogger<ChessPositionService> _logger;

    public ChessPositionService(ILogger<ChessPositionService> logger)
    {
        _logger = logger;
    }

    public string MakeMove(string currentFen, string sanMove)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentFen);
        ArgumentException.ThrowIfNullOrWhiteSpace(sanMove);

        try
        {
            var game = GameFactory.Create(currentFen);
            var moves = game.Pos.GenerateMoves();
            
            Move matchedMove = default;
            foreach (var move in moves)
            {
                // Convert move to SAN notation (e.g., "e4", "Nf3", "O-O")
                var sanNotation = new SanNotation(game.Pos);
                var notation = sanNotation.Convert(move);

                if (notation.Equals(sanMove, StringComparison.OrdinalIgnoreCase))
                {
                    matchedMove = move;
                    break;
                }
            }
            
            if (matchedMove.IsNullMove())
            {
                _logger.LogWarning("Invalid move '{SanMove}' for position '{Fen}'", sanMove, currentFen);
                throw new ArgumentException($"Invalid move: {sanMove}", nameof(sanMove));
            }
            
            // Make the move
            var state = new State();
            game.Pos.MakeMove(matchedMove, in state);
            
            // Get the new FEN
            var newFen = game.Pos.GenerateFen().ToString();
            
            _logger.LogInformation("Applied move '{SanMove}' to position. New FEN: '{NewFen}'", sanMove, newFen);
            
            return newFen;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error applying move '{SanMove}' to FEN '{Fen}'", sanMove, currentFen);
            throw new InvalidOperationException($"Failed to apply move '{sanMove}' to position", ex);
        }
    }
}