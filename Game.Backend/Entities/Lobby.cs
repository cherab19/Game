namespace Game.Backend.Entities;

public sealed class Lobby
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Region { get; set; } = "global";

    public string GameMode { get; set; } = "standard";

    public int MaxPlayers { get; set; } = 4;

    public LobbyStatus Status { get; set; } = LobbyStatus.Waiting;

    public Guid HostPlayerId { get; set; }

    public Player HostPlayer { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<LobbyMember> Members { get; set; } = new List<LobbyMember>();

    public ICollection<GameSession> Games { get; set; } = new List<GameSession>();

    public ICollection<GameEvent> Events { get; set; } = new List<GameEvent>();
}