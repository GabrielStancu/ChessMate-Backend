using ChessMate.Application.Interfaces;
using ChessMate.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChessMate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IChessPositionService, ChessPositionService>();
        services.AddScoped<IPgnParser, PgnParser>();
        services.AddScoped<IGameEvaluationService, GameEvaluationService>();

        return services;
    }
}
