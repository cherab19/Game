using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.UnityClient.Config;
using Game.UnityClient.Contracts;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.UnityClient.Services
{
    public sealed class ApiClient
    {
        private readonly BackendConfig _config;

        public ApiClient(BackendConfig config)
        {
            _config = config;
        }

        public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string path, string jwtToken = null, CancellationToken ct = default)
        {
            EnsureSecureRest();
            var request = UnityWebRequest.Get(BuildUrl(path));
            request.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            }

            return await SendAsync<TResponse>(request, ct);
        }

        public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string path, TRequest payload, string jwtToken = null, CancellationToken ct = default)
        {
            EnsureSecureRest();
            var json = JsonConvert.SerializeObject(payload);
            var request = new UnityWebRequest(BuildUrl(path), UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
            }

            return await SendAsync<TResponse>(request, ct);
        }

        private async Task<ApiResponse<TResponse>> SendAsync<TResponse>(UnityWebRequest request, CancellationToken ct)
        {
            using (request)
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"HTTP error ({request.responseCode}): {request.error}");
                }

                var raw = request.downloadHandler?.text;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return new ApiResponse<TResponse>
                    {
                        Success = false,
                        Error = new ApiError { Code = "empty_response", Message = "Backend returned an empty response." }
                    };
                }

                return JsonConvert.DeserializeObject<ApiResponse<TResponse>>(raw);
            }
        }

        private string BuildUrl(string path)
        {
            var normalized = path.StartsWith("/") ? path : $"/{path}";
            return $"{_config.RestBaseUrl}{normalized}";
        }

        private void EnsureSecureRest()
        {
            if (!_config.IsSecureRest())
            {
                throw new InvalidOperationException("REST base URL must use HTTPS unless localhost insecure mode is enabled.");
            }
        }
    }
}
