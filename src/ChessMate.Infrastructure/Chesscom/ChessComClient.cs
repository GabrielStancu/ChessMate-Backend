using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChessMate.Application.DTOs;
using ChessMate.Application.Interfaces;
using ChessMate.Infrastructure.Chesscom.Models;
using Microsoft.Extensions.Logging;

namespace ChessMate.Infrastructure.Chesscom;

/// <summary>
/// Implementation of Chess.com API service
/// </summary>
public sealed class ChessComClient : IChessGameClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChessComClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChessComClient(HttpClient httpClient, ILogger<ChessComClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc />
    public async Task<PlayerProfileDto> GetPlayerProfileAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        try
        {
            _logger.LogInformation("Fetching profile for player: {Username}", username);

            var response = await _httpClient.GetFromJsonAsync<ChessComPlayerProfile>(
                $"player/{username}", _jsonOptions, cancellationToken);

            if (response is null)
            {
                throw new InvalidOperationException($"Failed to retrieve profile for user: {username}");
            }

            return response.ToDto();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching profile for {Username}", username);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetGameArchivesAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        try
        {
            _logger.LogInformation("Fetching game archives for player: {Username}", username);

            var response = await _httpClient.GetFromJsonAsync<ChessComArchivesResponse>(
                $"player/{username}/games/archives", _jsonOptions, cancellationToken);

            return response?.Archives ?? Enumerable.Empty<string>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching archives for {Username}", username);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChessGameDto>> GetMonthlyGamesAsync(string username, int year, int month, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        if (year < 2000 || year > DateTime.UtcNow.Year)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 2000 and current year");

        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");

        try
        {
            _logger.LogInformation("Fetching games for {Username} - {Year}/{Month:D2}", username, year, month);

            var response = await _httpClient.GetFromJsonAsync<ChessComGamesResponse>(
                $"player/{username}/games/{year}/{month:D2}", _jsonOptions, cancellationToken);

            if (response?.Games is null)
            {
                return Enumerable.Empty<ChessGameDto>();
            }

            return response.Games.Select(g => g.ToDto());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching games for {Username} {Year}/{Month}", username, year, month);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetPgnForMonthAsync(string username, int year, int month, CancellationToken cancellationToken = default)
    {
        var games = await GetMonthlyGamesAsync(username, year, month, cancellationToken);
        var pgnBuilder = new System.Text.StringBuilder();

        foreach (var game in games)
        {
            if (!string.IsNullOrWhiteSpace(game.Pgn))
            {
                pgnBuilder.AppendLine(game.Pgn);
                pgnBuilder.AppendLine();
            }
        }

        return pgnBuilder.ToString();
    }
}