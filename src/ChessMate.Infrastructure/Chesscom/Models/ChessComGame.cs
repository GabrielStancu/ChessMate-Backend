using ChessMate.Application.DTOs;

namespace ChessMate.Infrastructure.Chesscom.Models;

internal sealed class ChessComGame
{
    public string? Url { get; init; }
    public string? Pgn { get; init; }
    public long EndTime { get; init; }
    public string? TimeControl { get; init; }
    public string? TimeClass { get; init; }
    public string? Rules { get; init; }
    public ChessComPlayer? White { get; init; }
    public ChessComPlayer? Black { get; init; }

    public ChessGameDto ToDto()
    {
        return new ChessGameDto
        {
            Url = Url ?? string.Empty,
            Pgn = Pgn ?? string.Empty,
            EndTime = DateTimeOffset.FromUnixTimeSeconds(EndTime),
            TimeControl = TimeControl ?? string.Empty,
            TimeClass = TimeClass ?? string.Empty,
            Rules = Rules ?? string.Empty,
            White = new PlayerGameInfoDto
            {
                Username = White?.Username ?? string.Empty,
                Rating = White?.Rating ?? 0,
                Result = White?.Result ?? string.Empty
            },
            Black = new PlayerGameInfoDto
            {
                Username = Black?.Username ?? string.Empty,
                Rating = Black?.Rating ?? 0,
                Result = Black?.Result ?? string.Empty
            }
        };
    }
}