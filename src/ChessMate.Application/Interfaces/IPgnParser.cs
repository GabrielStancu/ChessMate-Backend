using ChessMate.Domain.Chess.Entities;

namespace ChessMate.Application.Interfaces;

/// <summary>
/// Service interface for parsing PGN (Portable Game Notation) files
/// </summary>
public interface IPgnParser
{
    /// <summary>
    /// Parses a single PGN string into a structured game object
    /// </summary>
    /// <param name="pgnContent">The PGN content to parse</param>
    /// <returns>Parsed game with moves and positions</returns>
    ParsedGame ParseGame(string pgnContent);

    /// <summary>
    /// Parses multiple games from a PGN string
    /// </summary>
    /// <param name="pgnContent">The PGN content containing multiple games</param>
    /// <returns>Collection of parsed games</returns>
    IEnumerable<ParsedGame> ParseGames(string pgnContent);
}