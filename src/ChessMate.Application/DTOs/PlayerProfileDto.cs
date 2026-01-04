namespace ChessMate.Application.DTOs;

/// <summary>
/// Data transfer object for Chess.com player profile
/// </summary>
public sealed class PlayerProfileDto
{
    public string Username { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Avatar { get; init; }
    public string? Title { get; init; }
    public int? Followers { get; init; }
    public string Country { get; init; } = string.Empty;
    public DateTimeOffset? LastOnline { get; init; }
    public DateTimeOffset Joined { get; init; }
    public string Status { get; init; } = string.Empty;
}