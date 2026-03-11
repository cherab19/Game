using Game.UnityClient.Contracts;

namespace Game.UnityClient.Services
{
    public sealed class PlayerSession
    {
        public string JwtToken { get; private set; }
        public PlayerProfile Profile { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(JwtToken);

        public void Set(AuthResponse auth)
        {
            JwtToken = auth.Token;
            Profile = auth.Player;
        }

        public void Clear()
        {
            JwtToken = null;
            Profile = null;
        }
    }
}
