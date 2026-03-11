using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Config;
using Game.UnityClient.Contracts;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.UnityClient.Services
{
    public sealed class GameWebSocketClient : IDisposable
    {
        private readonly BackendConfig _config;
        private ClientWebSocket _socket;
        private CancellationTokenSource _receiveCts;

        public event Action<RealtimeIncomingEvent> EventReceived;
        public event Action<string> ConnectionStateChanged;

        public bool IsConnected => _socket is { State: WebSocketState.Open };

        public GameWebSocketClient(BackendConfig config)
        {
            _config = config;
        }

        public async Task ConnectAsync(string jwtToken, string lobbyId, CancellationToken ct = default)
        {
            EnsureSecureWebSocket();

            DisposeSocket();
            _socket = new ClientWebSocket();
            _receiveCts = new CancellationTokenSource();

            var uri = BuildUri(jwtToken, lobbyId);
            ConnectionStateChanged?.Invoke("Connecting");
            await _socket.ConnectAsync(uri, ct);
            ConnectionStateChanged?.Invoke("Connected");

            _ = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token));
        }

        public async Task SendEventAsync(string eventType, string lobbyId, string gameId, object payload, CancellationToken ct = default)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("WebSocket is not connected.");
            }

            var message = new RealtimeOutgoingEvent
            {
                EventType = eventType,
                LobbyId = lobbyId,
                GameId = gameId,
                Payload = payload
            };

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, ct);
        }

        public async Task CloseAsync(CancellationToken ct = default)
        {
            if (_socket is { State: WebSocketState.Open })
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", ct);
            }

            DisposeSocket();
            ConnectionStateChanged?.Invoke("Disconnected");
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[16 * 1024];

            try
            {
                while (!ct.IsCancellationRequested && _socket is { State: WebSocketState.Open })
                {
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        ConnectionStateChanged?.Invoke("Closed");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var parsed = JsonConvert.DeserializeObject<RealtimeIncomingEvent>(message);
                    EventReceived?.Invoke(parsed);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"WebSocket receive loop stopped: {exception.Message}");
                ConnectionStateChanged?.Invoke("Error");
            }
        }

        private Uri BuildUri(string jwtToken, string lobbyId)
        {
            var separator = _config.WebSocketUrl.Contains('?') ? "&" : "?";
            var url = $"{_config.WebSocketUrl}{separator}access_token={Uri.EscapeDataString(jwtToken)}&lobbyId={Uri.EscapeDataString(lobbyId)}";
            return new Uri(url);
        }

        private void EnsureSecureWebSocket()
        {
            if (!_config.IsSecureWebSocket())
            {
                throw new InvalidOperationException("WebSocket URL must use WSS unless localhost insecure mode is enabled.");
            }
        }

        private void DisposeSocket()
        {
            if (_receiveCts != null)
            {
                _receiveCts.Cancel();
                _receiveCts.Dispose();
                _receiveCts = null;
            }

            _socket?.Dispose();
            _socket = null;
        }

        public void Dispose()
        {
            DisposeSocket();
        }
    }
}
