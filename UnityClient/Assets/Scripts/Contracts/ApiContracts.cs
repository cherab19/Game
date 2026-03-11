using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.UnityClient.Contracts
{
    [Serializable]
    public sealed class ApiResponse<T>
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("data")] public T Data;
        [JsonProperty("error")] public ApiError Error;
    }

    [Serializable]
    public sealed class ApiError
    {
        [JsonProperty("code")] public string Code;
        [JsonProperty("message")] public string Message;
    }

    [Serializable]
    public sealed class AuthRequest
    {
        [JsonProperty("usernameOrEmail")] public string UsernameOrEmail;
        [JsonProperty("password")] public string Password;
    }

    [Serializable]
    public sealed class RegisterRequest
    {
        [JsonProperty("username")] public string Username;
        [JsonProperty("email")] public string Email;
        [JsonProperty("password")] public string Password;
        [JsonProperty("displayName")] public string DisplayName;
    }

    [Serializable]
    public sealed class AuthResponse
    {
        [JsonProperty("token")] public string Token;
        [JsonProperty("expiresAtUtc")] public DateTime ExpiresAtUtc;
        [JsonProperty("player")] public PlayerProfile Player;
    }

    [Serializable]
    public sealed class PlayerProfile
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("username")] public string Username;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("email")] public string Email;
        [JsonProperty("avatarUrl")] public string AvatarUrl;
    }

    [Serializable]
    public sealed class JoinMatchmakingRequest
    {
        [JsonProperty("region")] public string Region;
        [JsonProperty("gameMode")] public string GameMode;
        [JsonProperty("maxPlayers")] public int MaxPlayers;
    }

    [Serializable]
    public sealed class StartMatchRequest
    {
        [JsonProperty("lobbyId")] public string LobbyId;
    }

    [Serializable]
    public sealed class JoinMatchmakingResponse
    {
        [JsonProperty("created")] public bool Created;
        [JsonProperty("alreadyJoined")] public bool AlreadyJoined;
        [JsonProperty("lobby")] public MatchLobby Lobby;
    }

    [Serializable]
    public sealed class MatchStartResponse
    {
        [JsonProperty("gameId")] public string GameId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("startedAtUtc")] public DateTime StartedAtUtc;
        [JsonProperty("lobby")] public MatchLobby Lobby;
    }

    [Serializable]
    public sealed class MatchLobby
    {
        [JsonProperty("lobbyId")] public string LobbyId;
        [JsonProperty("code")] public string Code;
        [JsonProperty("region")] public string Region;
        [JsonProperty("gameMode")] public string GameMode;
        [JsonProperty("maxPlayers")] public int MaxPlayers;
        [JsonProperty("status")] public string Status;
        [JsonProperty("hostPlayerId")] public string HostPlayerId;
        [JsonProperty("activeGameId")] public string ActiveGameId;
        [JsonProperty("players")] public List<LobbyPlayer> Players;
    }

    [Serializable]
    public sealed class LobbyPlayer
    {
        [JsonProperty("playerId")] public string PlayerId;
        [JsonProperty("username")] public string Username;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("isHost")] public bool IsHost;
    }

    [Serializable]
    public sealed class LeaderboardEntry
    {
        [JsonProperty("playerId")] public string PlayerId;
        [JsonProperty("username")] public string Username;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("score")] public int Score;
        [JsonProperty("wins")] public int Wins;
        [JsonProperty("losses")] public int Losses;
        [JsonProperty("kills")] public int Kills;
        [JsonProperty("deaths")] public int Deaths;
    }

    [Serializable]
    public sealed class LeaderboardUpdateRequest
    {
        [JsonProperty("scoreDelta")] public int ScoreDelta;
        [JsonProperty("winsDelta")] public int WinsDelta;
        [JsonProperty("lossesDelta")] public int LossesDelta;
        [JsonProperty("killsDelta")] public int KillsDelta;
        [JsonProperty("deathsDelta")] public int DeathsDelta;
    }

    [Serializable]
    public sealed class RealtimeIncomingEvent
    {
        [JsonProperty("eventType")] public string EventType;
        [JsonProperty("playerId")] public string PlayerId;
        [JsonProperty("username")] public string Username;
        [JsonProperty("lobbyId")] public string LobbyId;
        [JsonProperty("gameId")] public string GameId;
        [JsonProperty("payload")] public object Payload;
    }

    [Serializable]
    public sealed class RealtimeOutgoingEvent
    {
        [JsonProperty("eventType")] public string EventType;
        [JsonProperty("lobbyId")] public string LobbyId;
        [JsonProperty("gameId")] public string GameId;
        [JsonProperty("payload")] public object Payload;
    }
}
