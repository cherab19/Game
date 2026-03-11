using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;

namespace Game.UnityClient.Networking
{
    public interface IGameNetworkAdapter
    {
        string AdapterName { get; }

        Task InitializeForLobbyAsync(MatchLobby lobby, string localPlayerId, bool isHost, CancellationToken ct = default);

        Task OnMatchStartedAsync(MatchStartResponse matchStart, CancellationToken ct = default);

        Task ShutdownAsync(CancellationToken ct = default);
    }
}
