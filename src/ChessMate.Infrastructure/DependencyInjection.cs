using ChessMate.Application.Interfaces;
using ChessMate.Infrastructure.Chesscom;
using ChessMate.Infrastructure.Stockfish;
using Microsoft.Extensions.DependencyInjection;

namespace ChessMate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register Chess.com API service
        services.AddHttpClient<IChessGameClient, ChessComClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.chess.com/pub/");
            client.DefaultRequestHeaders.Add("User-Agent", "ChessAnalyzer/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register Chess.com API service
        services.AddHttpClient<IStockfishClient, StockfishClient>(client =>
        {
            //client.BaseAddress = new Uri("https://stockfish-engine-swn-001-fqh7bvbpdnccgxa8.switzerlandnorth-01.azurewebsites.net/");
            client.BaseAddress = new Uri("https://localhost:5001");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}