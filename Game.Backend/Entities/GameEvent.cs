namespace Game.Backend.Entities;

public sealed class GameEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? LobbyId { get; set; }

    public Lobby? Lobby { get; set; }

    public Guid? GameId { get; set; }

    public GameSession? Game { get; set; }

    public Guid? PlayerId { get; set; }

    public Player? Player { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = "{}";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}