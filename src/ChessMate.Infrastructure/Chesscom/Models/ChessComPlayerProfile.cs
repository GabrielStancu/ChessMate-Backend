using ChessMate.Application.DTOs;

namespace ChessMate.Infrastructure.Chesscom.Models;

internal sealed class ChessComPlayerProfile
{
    public string? Username { get; init; }
    public string? Name { get; init; }
    public string? Avatar { get; init; }
    public string? Title { get; init; }
    public int? Followers { get; init; }
    public string? Country { get; init; }
    public long? LastOnline { get; init; }
    public long Joined { get; init; }
    public string? Status { get; init; }

    internal PlayerProfileDto ToDto()
    {
        return new PlayerProfileDto
        {
            Username = Username ?? string.Empty,
            Name = Name,
            Avatar = Avatar,
            Title = Title,
            Followers = Followers,
            Country = Country ?? string.Empty,
            LastOnline = LastOnline.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(LastOnline.Value)
                : null,
            Joined = DateTimeOffset.FromUnixTimeSeconds(Joined),
            Status = Status ?? string.Empty
        };
    }
}