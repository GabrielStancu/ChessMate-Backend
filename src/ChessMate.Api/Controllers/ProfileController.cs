using ChessMate.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChessMate.Api.Controllers;

/// <summary>
/// Controller for Chess.com API integration endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IChessGameClient _chessGameClient;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IChessGameClient chessGameClient, 
        ILogger<ProfileController> logger)
    {
        _chessGameClient = chessGameClient;
        _logger = logger;
    }

    /// <summary>
    /// Get player profile information
    /// </summary>
    /// <param name="username">Chess.com username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player profile data</returns>
    [HttpGet("player/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerProfile(string username, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _chessGameClient.GetPlayerProfileAsync(username, cancellationToken);
            return Ok(profile);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Player not found: {Username}", username);
            return NotFound(new { message = $"Player '{username}' not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get games for a specific month
    /// </summary>
    /// <param name="username">Chess.com username</param>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of games</returns>
    [HttpGet("player/{username}/games/{year:int}/{month:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyGames(string username, int year, int month, CancellationToken cancellationToken)
    {
        try
        {
            var games = await _chessGameClient.GetMonthlyGamesAsync(username, year, month, cancellationToken);
            return Ok(new { username, year, month, games, count = games.Count() });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}