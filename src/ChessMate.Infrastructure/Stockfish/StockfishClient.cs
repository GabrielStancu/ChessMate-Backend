using System.Net.Http.Json;
using System.Text.Json;
using ChessMate.Application.DTOs;
using ChessMate.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChessMate.Infrastructure.Stockfish;

/// <summary>
/// HTTP client for interacting with Stockfish chess engine API
/// </summary>
public sealed class StockfishClient : IStockfishClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockfishClient> _logger;

    public StockfishClient(HttpClient httpClient, ILogger<StockfishClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PositionEvaluationDto> EvaluatePositionAsync(string fen, int depth = 15, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fen))
        {
            throw new ArgumentException("FEN notation cannot be empty", nameof(fen));
        }

        if (depth < 1 || depth > 30)
        {
            throw new ArgumentException("Depth must be between 1 and 30", nameof(depth));
        }

        try
        {
            _logger.LogInformation("Evaluating position: {Fen} at depth {Depth}", fen, depth);

            var request = new StockfishRequest("evaluate_position", 
                new Dictionary<string, object>
            {
                { "fen", fen },
                { "depth", depth },
                { "moveTimeMs", 500 }
            });

            var response = await _httpClient.PostAsJsonAsync("/call", 
                request, cancellationToken);
            var responseContent = await response.Content
                .ReadFromJsonAsync<StockfishResponse>(cancellationToken);

            return JsonSerializer.Deserialize<PositionEvaluationDto>(
                JsonSerializer.Serialize(responseContent!.Result))!;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to evaluate position with Stockfish");
            throw;
        }
    }
}