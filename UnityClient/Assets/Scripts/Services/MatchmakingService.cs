using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;

namespace Game.UnityClient.Services
{
    public sealed class MatchmakingService
    {
        private readonly ApiClient _apiClient;

        public MatchmakingService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<ApiResponse<JoinMatchmakingResponse>> JoinAsync(string jwtToken, string region, string gameMode, int maxPlayers, CancellationToken ct = default)
        {
            var request = new JoinMatchmakingRequest
            {
                Region = region,
                GameMode = gameMode,
                MaxPlayers = maxPlayers
            };

            return _apiClient.PostAsync<JoinMatchmakingRequest, JoinMatchmakingResponse>("/matchmaking/join", request, jwtToken, ct);
        }

        public Task<ApiResponse<MatchStartResponse>> StartAsync(string jwtToken, string lobbyId, CancellationToken ct = default)
        {
            var request = new StartMatchRequest
            {
                LobbyId = lobbyId
            };

            return _apiClient.PostAsync<StartMatchRequest, MatchStartResponse>("/matchmaking/start", request, jwtToken, ct);
        }
    }
}
