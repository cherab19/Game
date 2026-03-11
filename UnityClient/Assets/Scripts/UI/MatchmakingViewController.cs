using System;
using Game.UnityClient.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UnityClient.UI
{
    public sealed class MatchmakingViewController : MonoBehaviour
    {
        [SerializeField] private GameClientBootstrap bootstrap;
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private RealtimeGameplayPublisher realtimePublisher;

        private TextField _region;
        private TextField _gameMode;
        private IntegerField _maxPlayers;
        private Label _status;

        private MatchLobby _currentLobby;
        private string _currentGameId;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            _region = root.Q<TextField>("Region");
            _gameMode = root.Q<TextField>("GameMode");
            _maxPlayers = root.Q<IntegerField>("MaxPlayers");
            _status = root.Q<Label>("MatchStatus");

            root.Q<Button>("JoinMatchButton").clicked += OnJoinClicked;
            root.Q<Button>("StartMatchButton").clicked += OnStartClicked;
        }

        private async void OnJoinClicked()
        {
            if (!bootstrap.Session.IsAuthenticated)
            {
                _status.text = "Authenticate first.";
                return;
            }

            try
            {
                var join = await bootstrap.MatchmakingService.JoinAsync(
                    bootstrap.Session.JwtToken,
                    _region.value,
                    _gameMode.value,
                    Mathf.Max(2, _maxPlayers.value));

                if (!join.Success)
                {
                    _status.text = $"Matchmaking failed: {join.Error?.Message}";
                    return;
                }

                _currentLobby = join.Data.Lobby;
                realtimePublisher?.SetContext(_currentLobby.LobbyId, _currentLobby.ActiveGameId);
                var localPlayerId = bootstrap.Session.Profile.Id;
                var isHost = string.Equals(_currentLobby.HostPlayerId, localPlayerId, StringComparison.OrdinalIgnoreCase);

                await bootstrap.NetworkAdapter.InitializeForLobbyAsync(_currentLobby, localPlayerId, isHost);
                await bootstrap.WebSocketClient.ConnectAsync(bootstrap.Session.JwtToken, _currentLobby.LobbyId);

                _status.text = $"Joined lobby {_currentLobby.Code} ({_currentLobby.Players.Count}/{_currentLobby.MaxPlayers})";
            }
            catch (Exception exception)
            {
                _status.text = $"Join exception: {exception.Message}";
            }
        }

        private async void OnStartClicked()
        {
            if (_currentLobby == null)
            {
                _status.text = "Join a lobby first.";
                return;
            }

            try
            {
                var start = await bootstrap.MatchmakingService.StartAsync(bootstrap.Session.JwtToken, _currentLobby.LobbyId);
                if (!start.Success)
                {
                    _status.text = $"Start failed: {start.Error?.Message}";
                    return;
                }

                _currentGameId = start.Data.GameId;
                realtimePublisher?.SetContext(_currentLobby.LobbyId, _currentGameId);
                bootstrap.MatchResultSyncService.BeginMatch();
                await bootstrap.NetworkAdapter.OnMatchStartedAsync(start.Data);
                _status.text = $"Match started: {_currentGameId}";
            }
            catch (Exception exception)
            {
                _status.text = $"Start exception: {exception.Message}";
            }
        }
    }
}
