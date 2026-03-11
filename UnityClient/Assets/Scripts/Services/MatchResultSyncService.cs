using System;
using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;

namespace Game.UnityClient.Services
{
    public sealed class MatchResultSyncService
    {
        private readonly LeaderboardService _leaderboardService;
        private readonly PlayerSession _session;

        private int _kills;
        private int _deaths;
        private int _shotsFired;
        private int _collisions;
        private int _powerups;
        private bool _inMatch;

        public MatchResultSyncService(LeaderboardService leaderboardService, PlayerSession session)
        {
            _leaderboardService = leaderboardService;
            _session = session;
        }

        public void BeginMatch()
        {
            _kills = 0;
            _deaths = 0;
            _shotsFired = 0;
            _collisions = 0;
            _powerups = 0;
            _inMatch = true;
        }

        public void RegisterRealtimeEvent(string eventType)
        {
            if (!_inMatch)
            {
                return;
            }

            switch (eventType)
            {
                case RealtimeEventTypes.ShotFired:
                    _shotsFired++;
                    break;
                case RealtimeEventTypes.Collision:
                    _collisions++;
                    break;
                case RealtimeEventTypes.PowerupSpawned:
                    _powerups++;
                    break;
            }
        }

        public void RegisterKill()
        {
            if (_inMatch)
            {
                _kills++;
            }
        }

        public void RegisterDeath()
        {
            if (_inMatch)
            {
                _deaths++;
            }
        }

        public async Task<ApiResponse<LeaderboardEntry>> SubmitMatchResultAsync(bool didWin, CancellationToken ct = default)
        {
            if (!_session.IsAuthenticated)
            {
                return new ApiResponse<LeaderboardEntry>
                {
                    Success = false,
                    Error = new ApiError { Code = "not_authenticated", Message = "Player must be authenticated before submitting results." }
                };
            }

            var scoreDelta = Math.Max(0, (_kills * 20) + (_powerups * 10) + (_shotsFired * 2) - (_deaths * 8) - (_collisions * 2));
            var response = await _leaderboardService.UpdateAsync(
                _session.JwtToken,
                scoreDelta: scoreDelta,
                winsDelta: didWin ? 1 : 0,
                lossesDelta: didWin ? 0 : 1,
                killsDelta: _kills,
                deathsDelta: _deaths,
                ct: ct);

            _inMatch = false;
            return response;
        }
    }
}