using ChessMate.Application.Interfaces;
using ChessMate.Infrastructure.Chess;
using ChessMate.Infrastructure.Chess.Services;
using ChessMate.Infrastructure.Chesscom;
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

        services.AddScoped<IChessPositionService, ChessPositionService>();
        services.AddScoped<IPgnParser, PgnParser>();

        return services;
    }
}