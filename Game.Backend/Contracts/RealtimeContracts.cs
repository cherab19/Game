using System.Text.Json;

namespace Game.Backend.Contracts;

public static class RealtimeEventTypes
{
    public const string PlayerJoined = "player_joined";
    public const string PlayerMoved = "player_moved";
    public const string ShotFired = "shot_fired";
    public const string Collision = "collision";
    public const string PowerupSpawned = "powerup_spawned";

    public static readonly ISet<string> ClientPublishable = new HashSet<string>(StringComparer.Ordinal)
    {
        PlayerMoved,
        ShotFired,
        Collision,
        PowerupSpawned
    };
}

public sealed record IncomingRealtimeMessage(string EventType, Guid? LobbyId, Guid? GameId, JsonElement Payload);

public sealed record OutgoingRealtimeMessage(
    string EventType,
    Guid PlayerId,
    string Username,
    Guid? LobbyId,
    Guid? GameId,
    object? Payload,
    DateTime TimestampUtc);