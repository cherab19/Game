using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;
using UnityEngine;

namespace Game.UnityClient.Networking
{
    public sealed class PhotonAdapter : IGameNetworkAdapter
    {
        public string AdapterName => "Photon";

        public Task InitializeForLobbyAsync(MatchLobby lobby, string localPlayerId, bool isHost, CancellationToken ct = default)
        {
#if PHOTON_UNITY_NETWORKING
            Debug.Log($"[{AdapterName}] Initialize room {lobby.Code} as {(isHost ? "MasterClient" : "Client")}");
#else
            Debug.LogWarning($"[{AdapterName}] Package not installed. Define PHOTON_UNITY_NETWORKING and add Photon PUN/Fusion package.");
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
