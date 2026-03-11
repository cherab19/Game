using Game.Backend.Entities;

namespace Game.Backend.Contracts;

public static class MappingExtensions
{
    public static PlayerProfileResponse ToProfileResponse(this Player player) =>
        new(
            player.Id,
            player.Username,
            player.DisplayName,
            player.Email,
            player.AvatarUrl,
            player.CreatedAtUtc,
            player.LastLoginAtUtc);

    public static LeaderboardEntryResponse ToResponse(this LeaderboardEntry entry) =>
        new(
            entry.PlayerId,
            entry.Player.Username,
            entry.Player.DisplayName,
            entry.Score,
            entry.Wins,
            entry.Losses,
            entry.Kills,
            entry.Deaths,
            entry.UpdatedAtUtc);

    public static MatchLobbyResponse ToResponse(this Lobby lobby)
    {
        var activeGameId = lobby.Games
            .Where(game => game.Status == GameStatus.Active)
            .OrderByDescending(game => game.StartedAtUtc)
            .Select(game => (Guid?)game.Id)
            .FirstOrDefault();

        var players = lobby.Members
            .OrderBy(member => member.JoinedAtUtc)
            .Select(member => new LobbyPlayerResponse(
                member.PlayerId,
                member.Player.Username,
                member.Player.DisplayName,
                member.JoinedAtUtc,
                member.PlayerId == lobby.HostPlayerId))
            .ToList();

        return new MatchLobbyResponse(
            lobby.Id,
            lobby.Code,
            lobby.Region,
            lobby.GameMode,
            lobby.MaxPlayers,
            lobby.Status,
            lobby.HostPlayerId,
            activeGameId,
            lobby.CreatedAtUtc,
            lobby.UpdatedAtUtc,
            players);
    }
}