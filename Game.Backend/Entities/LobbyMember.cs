namespace Game.Backend.Entities;

public sealed class LobbyMember
{
    public Guid LobbyId { get; set; }

    public Lobby Lobby { get; set; } = null!;

    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}