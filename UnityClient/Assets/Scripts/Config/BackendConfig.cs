using System;
using UnityEngine;

namespace Game.UnityClient.Config
{
    [CreateAssetMenu(fileName = "BackendConfig", menuName = "Game/Backend Config")]
    public sealed class BackendConfig : ScriptableObject
    {
        [Header("Backend URLs")]
        [SerializeField] private string restBaseUrl = "https://your-lovable-backend.example";
        [SerializeField] private string webSocketUrl = "wss://your-lovable-backend.example/ws/game";

        [Header("Development")]
        [SerializeField] private bool allowInsecureLocalhost;

        public string RestBaseUrl => restBaseUrl.TrimEnd('/');

        public string WebSocketUrl => webSocketUrl;

        public bool AllowInsecureLocalhost => allowInsecureLocalhost;

        public bool IsSecureRest()
        {
            if (!Uri.TryCreate(RestBaseUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttps || (allowInsecureLocalhost && uri.Host == "localhost");
        }

        public bool IsSecureWebSocket()
        {
            if (!Uri.TryCreate(WebSocketUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == "wss" || (allowInsecureLocalhost && uri.Host == "localhost");
        }
    }
}
