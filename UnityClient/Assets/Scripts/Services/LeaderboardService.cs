using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;

namespace Game.UnityClient.Services
{
    public sealed class LeaderboardService
    {
        private readonly ApiClient _apiClient;

        public LeaderboardService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<ApiResponse<List<LeaderboardEntry>>> GetTopAsync(int limit, CancellationToken ct = default)
        {
            return _apiClient.GetAsync<List<LeaderboardEntry>>($"/leaderboards/top?limit={limit}", ct: ct);
        }

        public Task<ApiResponse<LeaderboardEntry>> UpdateAsync(string jwtToken, int scoreDelta, int winsDelta, int lossesDelta, int killsDelta, int deathsDelta, CancellationToken ct = default)
        {
            var request = new LeaderboardUpdateRequest
            {
                ScoreDelta = scoreDelta,
                WinsDelta = winsDelta,
                LossesDelta = lossesDelta,
                KillsDelta = killsDelta,
                DeathsDelta = deathsDelta
            };

            return _apiClient.PostAsync<LeaderboardUpdateRequest, LeaderboardEntry>("/leaderboards/update", request, jwtToken, ct);
        }
    }
}
