using System;
using Game.UnityClient.Contracts;
using UnityEngine;

namespace Game.UnityClient.UI
{
    public sealed class RealtimeGameplayPublisher : MonoBehaviour
    {
        [SerializeField] private GameClientBootstrap bootstrap;
        [SerializeField] private string activeLobbyId;
        [SerializeField] private string activeGameId;

        public void SetContext(string lobbyId, string gameId)
        {
            activeLobbyId = lobbyId;
            activeGameId = gameId;
        }

        public async void PublishPlayerMoved(Vector3 position, float rotationY)
        {
            await PublishAsync(RealtimeEventTypes.PlayerMoved, new { position = new { x = position.x, y = position.y, z = position.z }, rotationY });
        }

        public async void PublishShotFired(Vector3 origin, Vector3 direction)
        {
            bootstrap?.MatchResultSyncService.RegisterRealtimeEvent(RealtimeEventTypes.ShotFired);
            await PublishAsync(RealtimeEventTypes.ShotFired, new
            {
                origin = new { x = origin.x, y = origin.y, z = origin.z },
                direction = new { x = direction.x, y = direction.y, z = direction.z }
            });
        }

        public async void PublishCollision(string targetId, string collisionType)
        {
            bootstrap?.MatchResultSyncService.RegisterRealtimeEvent(RealtimeEventTypes.Collision);
            await PublishAsync(RealtimeEventTypes.Collision, new { targetId, collisionType });
        }

        public async void PublishPowerupSpawned(string powerupType, Vector3 position)
        {
            bootstrap?.MatchResultSyncService.RegisterRealtimeEvent(RealtimeEventTypes.PowerupSpawned);
            await PublishAsync(RealtimeEventTypes.PowerupSpawned, new
            {
                powerupType,
                position = new { x = position.x, y = position.y, z = position.z }
            });
        }

        public void RegisterKill()
        {
            bootstrap?.MatchResultSyncService.RegisterKill();
        }

        public void RegisterDeath()
        {
            bootstrap?.MatchResultSyncService.RegisterDeath();
        }

        private async System.Threading.Tasks.Task PublishAsync(string eventType, object payload)
        {
            if (bootstrap == null || bootstrap.WebSocketClient == null || !bootstrap.WebSocketClient.IsConnected)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(activeLobbyId))
            {
                throw new InvalidOperationException("Active lobby is required before publishing realtime gameplay events.");
            }

            await bootstrap.WebSocketClient.SendEventAsync(eventType, activeLobbyId, activeGameId, payload);
        }
    }
}
