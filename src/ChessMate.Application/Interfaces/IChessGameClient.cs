using ChessMate.Application.DTOs;

namespace ChessMate.Application.Interfaces;

/// <summary>
/// Service interface for interacting with the Chess.com public API
/// </summary>
public interface IChessGameClient
{
    /// <summary>
    /// Retrieves player profile information from Chess.com
    /// </summary>
    /// <param name="username">The Chess.com username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player profile data</returns>
    Task<PlayerProfileDto> GetPlayerProfileAsync(string username, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves games for a specific month from a player's archive
    /// </summary>
    /// <param name="username">The Chess.com username</param>
    /// <param name="year">Year of the archive</param>
    /// <param name="month">Month of the archive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of games</returns>
    Task<IEnumerable<ChessGameDto>> GetMonthlyGamesAsync(string username, int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads PGN file content for a specific game or set of games
    /// </summary>
    /// <param name="username">The Chess.com username</param>
    /// <param name="year">Year of the archive</param>
    /// <param name="month">Month of the archive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PGN file content as string</returns>
    Task<string> GetPgnForMonthAsync(string username, int year, int month, CancellationToken cancellationToken = default);
}
