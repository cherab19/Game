using Game.Backend.Entities;

namespace Game.Backend.Contracts;

public sealed record JoinMatchmakingRequest(string? Region, string? GameMode, int? MaxPlayers);

public sealed record JoinMatchmakingResponse(bool Created, bool AlreadyJoined, MatchLobbyResponse Lobby);

public sealed record StartMatchRequest(Guid LobbyId);

public sealed record MatchStartResponse(Guid GameId, GameStatus Status, DateTime StartedAtUtc, MatchLobbyResponse Lobby);

public sealed record MatchLobbyResponse(
    Guid LobbyId,
    string Code,
    string Region,
    string GameMode,
    int MaxPlayers,
    LobbyStatus Status,
    Guid HostPlayerId,
    Guid? ActiveGameId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<LobbyPlayerResponse> Players);

public sealed record LobbyPlayerResponse(Guid PlayerId, string Username, string DisplayName, DateTime JoinedAtUtc, bool IsHost);