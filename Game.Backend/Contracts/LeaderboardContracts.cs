namespace Game.Backend.Contracts;

public sealed record LeaderboardUpdateRequest(int ScoreDelta, int WinsDelta, int LossesDelta, int KillsDelta, int DeathsDelta);

public sealed record LeaderboardEntryResponse(
    Guid PlayerId,
    string Username,
    string DisplayName,
    int Score,
    int Wins,
    int Losses,
    int Kills,
    int Deaths,
    DateTime UpdatedAtUtc);