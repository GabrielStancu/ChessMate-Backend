namespace ChessMate.Application.Interfaces;

/// <summary>
/// Service interface for chess position manipulation and FEN generation
/// </summary>
public interface IChessPositionService
{
    /// <summary>
    /// Applies a move in SAN notation to a position and returns the resulting FEN
    /// </summary>
    /// <param name="currentFen">Current position in FEN notation</param>
    /// <param name="sanMove">Move in Standard Algebraic Notation</param>
    /// <returns>FEN position after the move</returns>
    string MakeMove(string currentFen, string sanMove);

    /// <summary>
    /// Validates if a FEN string is well-formed
    /// </summary>
    /// <param name="fen">FEN string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidFen(string fen);
}