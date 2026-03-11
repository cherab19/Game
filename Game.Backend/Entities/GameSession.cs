namespace Game.Backend.Entities;

public sealed class GameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LobbyId { get; set; }

    public Lobby Lobby { get; set; } = null!;

    public GameStatus Status { get; set; } = GameStatus.Active;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAtUtc { get; set; }

    public ICollection<GameEvent> Events { get; set; } = new List<GameEvent>();
}