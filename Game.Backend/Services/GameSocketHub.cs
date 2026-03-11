using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Game.Backend.Contracts;
using Game.Backend.Data;
using Game.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Services;

public sealed class GameSocketHub(IServiceScopeFactory scopeFactory)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, ClientConnection> _connections = new();
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task RunSessionAsync(
        Guid playerId,
        string username,
        Guid? lobbyId,
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        var connection = new ClientConnection(Guid.NewGuid().ToString("N"), lobbyId, socket);
        _connections.TryAdd(connection.ConnectionId, connection);

        try
        {
            if (lobbyId.HasValue)
            {
                await PersistEventAsync(playerId, lobbyId, null, RealtimeEventTypes.PlayerJoined, new { playerId, username }, cancellationToken);
                await BroadcastToLobbyAsync(
                    lobbyId.Value,
                    new OutgoingRealtimeMessage(
                        RealtimeEventTypes.PlayerJoined,
                        playerId,
                        username,
                        lobbyId,
                        null,
                        new { playerId, username },
                        DateTime.UtcNow),
                    cancellationToken);
            }

            var buffer = new byte[16 * 1024];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var messageText = await ReceiveMessageAsync(socket, buffer, cancellationToken);
                if (messageText is null)
                {
                    break;
                }

                IncomingRealtimeMessage? incoming;

                try
                {
                    incoming = JsonSerializer.Deserialize<IncomingRealtimeMessage>(messageText, SerializerOptions);
                }
                catch (JsonException)
                {
                    await SendToConnectionAsync(connection, ApiResponse<object>.Fail("invalid_payload", "Realtime message must be valid JSON."), cancellationToken);
                    continue;
                }

                if (incoming is null || string.IsNullOrWhiteSpace(incoming.EventType))
                {
                    await SendToConnectionAsync(connection, ApiResponse<object>.Fail("invalid_event", "Realtime eventType is required."), cancellationToken);
                    continue;
                }

                if (!RealtimeEventTypes.ClientPublishable.Contains(incoming.EventType))
                {
                    await SendToConnectionAsync(connection, ApiResponse<object>.Fail("unsupported_event", $"Event '{incoming.EventType}' is not supported."), cancellationToken);
                    continue;
                }

                var targetLobbyId = incoming.LobbyId ?? lobbyId;
                if (!targetLobbyId.HasValue)
                {
                    await SendToConnectionAsync(connection, ApiResponse<object>.Fail("lobby_required", "A lobbyId is required for realtime events."), cancellationToken);
                    continue;
                }

                var payload = incoming.Payload.ValueKind == JsonValueKind.Undefined
                    ? new { }
                    : JsonSerializer.Deserialize<object>(incoming.Payload.GetRawText(), SerializerOptions);

                var persisted = await PersistEventAsync(playerId, targetLobbyId, incoming.GameId, incoming.EventType, payload, cancellationToken);
                if (!persisted)
                {
                    await SendToConnectionAsync(connection, ApiResponse<object>.Fail("lobby_forbidden", "Player cannot publish events into the requested lobby or game."), cancellationToken);
                    continue;
                }

                var outbound = new OutgoingRealtimeMessage(
                    incoming.EventType,
                    playerId,
                    username,
                    targetLobbyId,
                    incoming.GameId,
                    payload,
                    DateTime.UtcNow);

                await BroadcastToLobbyAsync(targetLobbyId.Value, outbound, cancellationToken);
            }
        }
        finally
        {
            _connections.TryRemove(connection.ConnectionId, out _);

            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }
    }

    public async Task BroadcastToLobbyAsync(Guid lobbyId, OutgoingRealtimeMessage message, CancellationToken cancellationToken)
    {
        var recipients = _connections.Values.Where(connection => connection.LobbyId == lobbyId).ToArray();

        foreach (var recipient in recipients)
        {
            await SendToConnectionAsync(recipient, message, cancellationToken);
        }
    }

    private async Task<bool> PersistEventAsync(
        Guid playerId,
        Guid? lobbyId,
        Guid? gameId,
        string eventType,
        object? payload,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        if (lobbyId.HasValue)
        {
            var membershipExists = await dbContext.LobbyMembers.AnyAsync(
                member => member.LobbyId == lobbyId.Value && member.PlayerId == playerId,
                cancellationToken);

            if (!membershipExists)
            {
                return false;
            }
        }

        if (gameId.HasValue)
        {
            var gameExists = await dbContext.Games.AnyAsync(
                game => game.Id == gameId.Value && (!lobbyId.HasValue || game.LobbyId == lobbyId.Value),
                cancellationToken);

            if (!gameExists)
            {
                return false;
            }
        }

        dbContext.Events.Add(new GameEvent
        {
            LobbyId = lobbyId,
            GameId = gameId,
            PlayerId = playerId,
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(payload ?? new { }, SerializerOptions),
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static async Task<string?> ReceiveMessageAsync(WebSocket socket, byte[] buffer, CancellationToken cancellationToken)
    {
        var segment = new ArraySegment<byte>(buffer);
        using var stream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(segment, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            await stream.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken);

            if (result.EndOfMessage)
            {
                break;
            }
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static async Task SendToConnectionAsync(ClientConnection connection, object payload, CancellationToken cancellationToken)
    {
        if (connection.Socket.State != WebSocketState.Open)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, SerializerOptions));
        await connection.SendLock.WaitAsync(cancellationToken);

        try
        {
            await connection.Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            connection.SendLock.Release();
        }
    }

    private sealed class ClientConnection(string connectionId, Guid? lobbyId, WebSocket socket)
    {
        public string ConnectionId { get; } = connectionId;

        public Guid? LobbyId { get; } = lobbyId;

        public WebSocket Socket { get; } = socket;

        public SemaphoreSlim SendLock { get; } = new(1, 1);
    }
}