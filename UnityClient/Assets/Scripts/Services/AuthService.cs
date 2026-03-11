using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Contracts;

namespace Game.UnityClient.Services
{
    public sealed class AuthService
    {
        private readonly ApiClient _apiClient;

        public AuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<ApiResponse<AuthResponse>> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        {
            var request = new AuthRequest
            {
                UsernameOrEmail = usernameOrEmail,
                Password = password
            };

            return _apiClient.PostAsync<AuthRequest, AuthResponse>("/auth/login", request, ct: ct);
        }

        public Task<ApiResponse<AuthResponse>> RegisterAsync(string username, string email, string password, string displayName, CancellationToken ct = default)
        {
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                DisplayName = displayName
            };

            return _apiClient.PostAsync<RegisterRequest, AuthResponse>("/auth/register", request, ct: ct);
        }
    }
}
