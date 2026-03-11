using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;
using UnityEngine;

namespace Game.UnityClient.Networking
{
    public sealed class UnityNetcodeAdapter : IGameNetworkAdapter
    {
        public string AdapterName => "Unity Netcode";

        public Task InitializeForLobbyAsync(MatchLobby lobby, string localPlayerId, bool isHost, CancellationToken ct = default)
        {
#if UNITY_NETCODE_GAMEOBJECTS
            Debug.Log($"[{AdapterName}] Initialize lobby {lobby.Code} as {(isHost ? "Host" : "Client")}");
#else
            Debug.LogWarning($"[{AdapterName}] Package not installed. Define UNITY_NETCODE_GAMEOBJECTS and add Netcode for GameObjects.");
#endif
            return Task.CompletedTask;
        }

        public Task OnMatchStartedAsync(MatchStartResponse matchStart, CancellationToken ct = default)
        {
            Debug.Log($"[{AdapterName}] Match started: {matchStart.GameId}");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(CancellationToken ct = default)
        {
            Debug.Log($"[{AdapterName}] Shutdown requested.");
            return Task.CompletedTask;
        }
    }
}
