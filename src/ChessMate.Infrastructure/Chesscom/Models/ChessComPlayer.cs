namespace ChessMate.Infrastructure.Chesscom.Models;

internal sealed class ChessComPlayer
{
    public string? Username { get; init; }
    public int Rating { get; init; }
    public string? Result { get; init; }
}