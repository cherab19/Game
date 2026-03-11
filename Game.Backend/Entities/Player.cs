namespace Game.Backend.Entities;

public sealed class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = string.Empty;

    public string UsernameNormalized { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string EmailNormalized { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<Lobby> HostedLobbies { get; set; } = new List<Lobby>();

    public ICollection<LobbyMember> LobbyMemberships { get; set; } = new List<LobbyMember>();

    public ICollection<GameEvent> Events { get; set; } = new List<GameEvent>();

    public LeaderboardEntry? Leaderboard { get; set; }
}