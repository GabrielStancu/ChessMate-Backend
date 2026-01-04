namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for Chess.com game data
/// </summary>
public sealed class ChessGameDto
{
    public string Url { get; init; } = string.Empty;
    public string Pgn { get; init; } = string.Empty;
    public DateTimeOffset EndTime { get; init; }
    public string TimeControl { get; init; } = string.Empty;
    public string TimeClass { get; init; } = string.Empty;
    public string Rules { get; init; } = string.Empty;
    public PlayerGameInfoDto White { get; init; } = new();
    public PlayerGameInfoDto Black { get; init; } = new();
}

/// <summary>
/// Player-specific information within a game
/// </summary>
public sealed class PlayerGameInfoDto
{
    public string Username { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Result { get; init; } = string.Empty;
}