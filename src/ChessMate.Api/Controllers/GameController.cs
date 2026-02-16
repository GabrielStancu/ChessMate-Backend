using ChessMate.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChessMate.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameEvaluationService _gameEvaluationService;
    private readonly IChessGameClient _chessGameClient;
    private readonly IPgnParser _pgnParser;

    public GameController(IGameEvaluationService gameEvaluationService,
        IChessGameClient chessGameClient,
        IPgnParser pgnParser)
    {
        _gameEvaluationService = gameEvaluationService;
        _chessGameClient = chessGameClient;
        _pgnParser = pgnParser;
    }

    /// <summary>
    /// Analyze a game from PGN with Stockfish evaluation
    /// </summary>
    /// <param name="request">The PGN content and analysis options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Evaluated game with position analysis</returns>
    [HttpPost("analyze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeGame([FromBody] AnalyzeGameRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Pgn))
            {
                return BadRequest(new { message = "PGN content is required" });
            }

            var depth = request.Depth ?? 15;
            if (depth < 1 || depth > 30)
            {
                return BadRequest(new { message = "Depth must be between 1 and 30" });
            }

            var parsedGame = _pgnParser.ParseGame(request.Pgn);
            var evaluatedGame = await _gameEvaluationService.EvaluateGameAsync(parsedGame, depth, cancellationToken);

            return Ok(evaluatedGame);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Evaluate a specific game with Stockfish analysis for each move
    /// </summary>
    /// <param name="username">Chess.com username</param>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="gameIndex">Index of the game in the month (0-based)</param>
    /// <param name="depth">Stockfish evaluation depth (1-30, default: 15)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Evaluated game with position analysis</returns>
    [HttpGet("player/{username}/games/{year:int}/{month:int}/evaluate/{gameIndex:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateGame(string username, int year, int month, int gameIndex,
        [FromQuery] int depth = 15, CancellationToken cancellationToken = default)
    {
        try
        {
            if (depth < 1 || depth > 30)
            {
                return BadRequest(new { message = "Depth must be between 1 and 30" });
            }

            var pgn = await _chessGameClient.GetPgnForMonthAsync(username, year, month, cancellationToken);
            if (string.IsNullOrWhiteSpace(pgn))
            {
                return NotFound(new { message = "No games found for the specified period" });
            }

            var parsedGames = _pgnParser.ParseGames(pgn).ToList();
            if (gameIndex < 0 || gameIndex >= parsedGames.Count)
            {
                return NotFound(new { message = $"Game index {gameIndex} out of range. Found {parsedGames.Count} games." });
            }

            var game = parsedGames[gameIndex];
            var evaluatedGame = await _gameEvaluationService.EvaluateGameAsync(game, depth, cancellationToken);

            return Ok(evaluatedGame);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Request model for analyzing a game from PGN
/// </summary>
public sealed class AnalyzeGameRequest
{
    /// <summary>
    /// The PGN content of the game to analyze
    /// </summary>
    public required string Pgn { get; init; }

    /// <summary>
    /// Stockfish evaluation depth (1-30, default: 15)
    /// </summary>
    public int? Depth { get; init; }
}
