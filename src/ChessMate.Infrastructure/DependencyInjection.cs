using Azure;
using Azure.AI.OpenAI;
using ChessMate.Application.Interfaces;
using ChessMate.Infrastructure.AzureOpenAi;
using ChessMate.Infrastructure.Chesscom;
using ChessMate.Infrastructure.Stockfish;
using Microsoft.Extensions.DependencyInjection;

namespace ChessMate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMoveAnalyzerAgentClient, MoveAnalyzerAgentClient>();

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
            client.BaseAddress = new Uri("https://localhost:5001");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register AzureOpenAIClient
        services.AddSingleton(_ => new AzureOpenAIClient(new Uri(""), 
            new AzureKeyCredential("")));

        return services;
    }
}