using System.Security.Cryptography;
using System.Text.Json;
using Game.Backend.Contracts;
using Game.Backend.Data;
using Game.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Services;

public sealed class MatchmakingService(GameDbContext dbContext, GameSocketHub socketHub)
{
    private readonly GameDbContext _dbContext = dbContext;
    private readonly GameSocketHub _socketHub = socketHub;

    public async Task<JoinMatchResult> JoinAsync(Guid playerId, JoinMatchmakingRequest request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players.FirstOrDefaultAsync(entry => entry.Id == playerId, cancellationToken)
            ?? throw new InvalidOperationException("Player profile was not found.");

        var existingLobbyMembership = await _dbContext.LobbyMembers
            .Include(member => member.Lobby)
                .ThenInclude(lobby => lobby.Members)
                    .ThenInclude(member => member.Player)
            .Include(member => member.Lobby)
                .ThenInclude(lobby => lobby.Games)
            .Where(member => member.PlayerId == playerId && member.Lobby.Status != LobbyStatus.Completed)
            .OrderByDescending(member => member.Lobby.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var existingLobby = existingLobbyMembership?.Lobby;

        if (existingLobby is not null)
        {
            return new JoinMatchResult(existingLobby, false, true);
        }

        var region = NormalizeOrDefault(request.Region, "global");
        var gameMode = NormalizeOrDefault(request.GameMode, "standard");
        var maxPlayers = Math.Clamp(request.MaxPlayers ?? 4, 2, 16);

        var candidateLobbies = await _dbContext.Lobbies
            .Include(lobby => lobby.Members)
                .ThenInclude(member => member.Player)
            .Include(lobby => lobby.Games)
            .Where(lobby =>
                lobby.Status == LobbyStatus.Waiting &&
                lobby.Region == region &&
                lobby.GameMode == gameMode &&
                lobby.MaxPlayers == maxPlayers)
            .OrderBy(lobby => lobby.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var lobby = candidateLobbies.FirstOrDefault(candidate => candidate.Members.Count < candidate.MaxPlayers);
        var created = false;
        var timestamp = DateTime.UtcNow;

        if (lobby is null)
        {
            lobby = new Lobby
            {
                Code = await GenerateLobbyCodeAsync(cancellationToken),
                Region = region,
                GameMode = gameMode,
                MaxPlayers = maxPlayers,
                HostPlayerId = playerId,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            };

            _dbContext.Lobbies.Add(lobby);
            created = true;
        }

        lobby.Members.Add(new LobbyMember
        {
            Lobby = lobby,
            PlayerId = playerId,
            Player = player,
            JoinedAtUtc = timestamp
        });
        lobby.UpdatedAtUtc = timestamp;

        _dbContext.Events.Add(new GameEvent
        {
            Lobby = lobby,
            PlayerId = playerId,
            EventType = RealtimeEventTypes.PlayerJoined,
            PayloadJson = JsonSerializer.Serialize(new
            {
                playerId = player.Id,
                username = player.Username,
                displayName = player.DisplayName
            }),
            CreatedAtUtc = timestamp
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _socketHub.BroadcastToLobbyAsync(
            lobby.Id,
            new OutgoingRealtimeMessage(
                RealtimeEventTypes.PlayerJoined,
                player.Id,
                player.Username,
                lobby.Id,
                null,
                new { playerId = player.Id, username = player.Username, displayName = player.DisplayName },
                timestamp),
            cancellationToken);

        return new JoinMatchResult(lobby, created, false);
    }

    public async Task<StartMatchResult> StartAsync(Guid playerId, Guid lobbyId, CancellationToken cancellationToken)
    {
        var lobby = await _dbContext.Lobbies
            .Include(existingLobby => existingLobby.Members)
                .ThenInclude(member => member.Player)
            .Include(existingLobby => existingLobby.Games)
            .FirstOrDefaultAsync(existingLobby => existingLobby.Id == lobbyId, cancellationToken)
            ?? throw new InvalidOperationException("Lobby was not found.");

        if (lobby.HostPlayerId != playerId)
        {
            throw new UnauthorizedAccessException("Only the lobby host can start the match.");
        }

        if (lobby.Members.Count < 2)
        {
            throw new InvalidOperationException("At least two players are required to start a match.");
        }

        var existingGame = lobby.Games.FirstOrDefault(game => game.Status == GameStatus.Active);
        if (existingGame is not null)
        {
            return new StartMatchResult(existingGame, lobby);
        }

        var game = new GameSession
        {
            LobbyId = lobby.Id,
            Status = GameStatus.Active,
            StartedAtUtc = DateTime.UtcNow
        };

        lobby.Status = LobbyStatus.InProgress;
        lobby.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync(cancellationToken);

        lobby.Games.Add(game);
        return new StartMatchResult(game, lobby);
    }

    private async Task<string> GenerateLobbyCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var code = string.Create(6, 0, static (span, _) =>
            {
                const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                for (var index = 0; index < span.Length; index++)
                {
                    span[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
                }
            });

            var exists = await _dbContext.Lobbies.AnyAsync(lobby => lobby.Code == code, cancellationToken);
            if (!exists)
            {
                return code;
            }
        }
    }

    private static string NormalizeOrDefault(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToLowerInvariant();
}

public sealed record JoinMatchResult(Lobby Lobby, bool Created, bool AlreadyJoined);

public sealed record StartMatchResult(GameSession Game, Lobby Lobby);