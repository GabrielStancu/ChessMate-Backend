using ChessMate.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

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
            var normalizedSanMove = sanMove.TrimEnd('+', '#').Replace("x", "");
            Move matchedMove = default;

            foreach (var move in moves)
            {
                var sanNotation = new SanNotation(game.Pos);
                var gameSan = sanNotation.Convert(move).TrimEnd('+', '#');

                if (gameSan.Equals(normalizedSanMove, StringComparison.OrdinalIgnoreCase))
                {
                    matchedMove = move;
                    break;
                }
            }

            if (matchedMove.IsNullMove())
            {
                matchedMove = ApplyRegularMoveDisambiguation(normalizedSanMove, moves, game, matchedMove);
            }

            if (matchedMove.IsNullMove())
            {
                matchedMove = ApplyPromotionMoveDisambiguation(normalizedSanMove, moves, game, matchedMove);
            }

            if (matchedMove.IsNullMove())
            {
                _logger.LogWarning("Invalid move '{SanMove}' for position '{Fen}'", sanMove, currentFen);
                throw new ArgumentException($"Invalid move: {sanMove}", nameof(sanMove));
            }

            var state = new State();
            game.Pos.MakeMove(matchedMove, in state);
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

    private static Move ApplyRegularMoveDisambiguation(string normalizedSanMove, MoveList moves, IGame game,
        Move matchedMove)
    {
        // Regex to match moves like "Rae1" (piece + file + square), "Ra2e2" (piece + from square + to square), or "Nd4" (piece + square)
        // Group 1: Piece letter (R, N, B, Q, K)
        // Group 2: Disambiguation (file letter or full square like "a2") - optional
        // Group 3: Destination square
        var disambiguatedMovePattern = @"^([RNBQK]?)([a-h]\d?)?([a-h]\d)$";
        var match = System.Text.RegularExpressions.Regex.Match(normalizedSanMove, disambiguatedMovePattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) 
            return matchedMove;

        var pieceChar = match.Groups[1].Value;
        var disambiguation = match.Groups[2].Success ? match.Groups[2].Value.ToLower() : string.Empty;
        var toSquare = match.Groups[3].Value.ToLower();

        if (string.IsNullOrWhiteSpace(disambiguation) && !"RNBQK".Contains(pieceChar))
        {
            disambiguation = pieceChar;
            pieceChar = "P"; // Default to pawn if no piece specified
        }

        var pieceType = GetPieceType(pieceChar);
        var rank = int.Parse(toSquare[1].ToString()) - 1;
        var file = GetFile(toSquare);
        var destinationSquare = Square.Create(new Rank(rank), file);

        // Find matching move from available moves
        foreach (var move in moves)
        {
            if (TryMatchRegularMove(game, move, destinationSquare, pieceType, disambiguation, out matchedMove))
                return matchedMove;
        }

        return matchedMove;
    }

    private static bool TryMatchRegularMove(IGame game, ExtMove move, Square destinationSquare, PieceTypes pieceType,
        string disambiguation, out Move matchedMove)
    {
        matchedMove = default;

        // Check if move ends at the destination square
        if (move.Move.ToSquare() != destinationSquare)
            return false;

        // Check if the moving piece matches the expected piece type
        var movingPiece = game.Pos.GetPiece(move.Move.FromSquare());
        if (movingPiece.Type() != pieceType)
            return false;

        // Check disambiguation
        var fromSquare = move.Move.FromSquare();
        var fromSquareStr = fromSquare.ToString().ToLower();

        // If no disambiguation provided, match any move of this piece type to the destination
        if (string.IsNullOrEmpty(disambiguation))
        {
            matchedMove = move;
            return true;
        }

        // If disambiguation is a full square (e.g., "a2"), match exactly
        if (disambiguation.Length == 2)
        {
            if (fromSquareStr == disambiguation)
            {
                matchedMove = move;
                return true;
            }
        }
        // If disambiguation is just a file (e.g., "a"), match the file
        else if (disambiguation.Length == 1)
        {
            if (fromSquareStr[0] == disambiguation[0])
            {
                matchedMove = move;
                return true;
            }
        }

        return false;
    }

    private static Move ApplyPromotionMoveDisambiguation(string normalizedSanMove, MoveList moves, IGame game, Move matchedMove)
    {
        // Regex to match pawn promotion moves
        // Group 1: Source file (optional, for captures like "cxb1=Q")
        // Group 2: Destination square
        // Group 3: Promoted piece (Q, R, B, N)
        var promotionPattern = @"^([a-h])?([a-h][18])=([QRBN])$";
        var match = System.Text.RegularExpressions.Regex.Match(normalizedSanMove, promotionPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) 
            return matchedMove;

        var sourceFile = match.Groups[1].Success ? match.Groups[1].Value.ToLower() : string.Empty;
        var toSquare = match.Groups[2].Value.ToLower();
        var promotionPiece = match.Groups[3].Value.ToUpper();
        var promotionPieceType = GetPieceType(promotionPiece);

        var rank = int.Parse(toSquare[1].ToString()) - 1;
        var file = GetFile(toSquare[0].ToString());
        var destinationSquare = Square.Create(new Rank(rank), file);

        // Find matching promotion move
        foreach (var move in moves)
        {
            if (TryMatchPromotionMove(game, move, destinationSquare, promotionPieceType, sourceFile, out matchedMove))
                return matchedMove;
        }

        return matchedMove;
    }

    private static bool TryMatchPromotionMove(IGame game, ExtMove move, Square destinationSquare,
        PieceTypes promotionPieceType, string sourceFile, out Move matchedMove)
    {
        matchedMove = default;

        // Check if move ends at the destination square
        if (move.Move.ToSquare() != destinationSquare)
            return false;

        // Check if it's a pawn move
        var movingPiece = game.Pos.GetPiece(move.Move.FromSquare());
        if (movingPiece.Type() != PieceTypes.Pawn)
            return false;

        // Check if it's a promotion move with the correct promotion piece
        if (!move.Move.IsPromotionMove() || move.Move.PromotedPieceType() != promotionPieceType)
            return false;

        // If source file is specified, check it matches
        if (!string.IsNullOrEmpty(sourceFile))
        {
            var fromSquare = move.Move.FromSquare();
            var fromSquareStr = fromSquare.ToString().ToLower();
            if (fromSquareStr[0] != sourceFile[0])
                return false;
        }

        matchedMove = move;
        return true;
    }

    private static PieceTypes GetPieceType(string pieceChar)
    {
        return pieceChar.ToUpper() switch
        {
            "R" => PieceTypes.Rook,
            "N" => PieceTypes.Knight,
            "B" => PieceTypes.Bishop,
            "Q" => PieceTypes.Queen,
            "K" => PieceTypes.King,
            _ => PieceTypes.Pawn
        };
    }

    private static File GetFile(string toSquare)
    {
        return toSquare[0] switch
        {
            'a' => File.FileA,
            'b' => File.FileB,
            'c' => File.FileC,
            'd' => File.FileD,
            'e' => File.FileE,
            'f' => File.FileF,
            'g' => File.FileG,
            'h' => File.FileH,
            _ => throw new InvalidOperationException("Invalid file in destination square")
        };
    }
}