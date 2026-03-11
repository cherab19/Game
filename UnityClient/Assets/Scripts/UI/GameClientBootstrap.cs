using Game.UnityClient.Config;
using Game.UnityClient.Networking;
using Game.UnityClient.Services;
using UnityEngine;

namespace Game.UnityClient.UI
{
    public sealed class GameClientBootstrap : MonoBehaviour
    {
        public enum MultiplayerStack
        {
            UnityNetcode,
            Mirror,
            Photon
        }

        [Header("References")]
        [SerializeField] private BackendConfig backendConfig;

        [Header("Networking")]
        [SerializeField] private MultiplayerStack multiplayerStack = MultiplayerStack.UnityNetcode;

        public AuthService AuthService { get; private set; }
        public MatchmakingService MatchmakingService { get; private set; }
        public LeaderboardService LeaderboardService { get; private set; }
        public MatchResultSyncService MatchResultSyncService { get; private set; }
        public PlayerSession Session { get; } = new();
        public GameWebSocketClient WebSocketClient { get; private set; }
        public IGameNetworkAdapter NetworkAdapter { get; private set; }

        private void Awake()
        {
            if (backendConfig == null)
            {
                Debug.LogError("BackendConfig is required on GameClientBootstrap.");
                enabled = false;
                return;
            }

            var apiClient = new ApiClient(backendConfig);
            AuthService = new AuthService(apiClient);
            MatchmakingService = new MatchmakingService(apiClient);
            LeaderboardService = new LeaderboardService(apiClient);
            MatchResultSyncService = new MatchResultSyncService(LeaderboardService, Session);
            WebSocketClient = new GameWebSocketClient(backendConfig);
            NetworkAdapter = CreateAdapter(multiplayerStack);
        }

        private static IGameNetworkAdapter CreateAdapter(MultiplayerStack stack)
        {
            return stack switch
            {
                MultiplayerStack.Mirror => new MirrorAdapter(),
                MultiplayerStack.Photon => new PhotonAdapter(),
                _ => new UnityNetcodeAdapter()
            };
        }

        private async void OnDestroy()
        {
            if (WebSocketClient != null)
            {
                await WebSocketClient.CloseAsync();
                WebSocketClient.Dispose();
            }

            if (NetworkAdapter != null)
            {
                await NetworkAdapter.ShutdownAsync();
            }
        }
    }
}
