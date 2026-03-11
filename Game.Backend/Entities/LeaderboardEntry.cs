namespace Game.Backend.Entities;

public sealed class LeaderboardEntry
{
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public int Score { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}