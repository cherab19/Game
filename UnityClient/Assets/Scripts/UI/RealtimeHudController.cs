using Game.UnityClient.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UnityClient.UI
{
    public sealed class RealtimeHudController : MonoBehaviour
    {
        [SerializeField] private GameClientBootstrap bootstrap;
        [SerializeField] private UIDocument uiDocument;

        private Label _realtimeStatus;

        private void Start()
        {
            _realtimeStatus = uiDocument.rootVisualElement.Q<Label>("RealtimeStatus");

            bootstrap.WebSocketClient.ConnectionStateChanged += OnConnectionStateChanged;
            bootstrap.WebSocketClient.EventReceived += OnEventReceived;
        }

        private void OnDestroy()
        {
            if (bootstrap == null || bootstrap.WebSocketClient == null)
            {
                return;
            }

            bootstrap.WebSocketClient.ConnectionStateChanged -= OnConnectionStateChanged;
            bootstrap.WebSocketClient.EventReceived -= OnEventReceived;
        }

        private void OnConnectionStateChanged(string state)
        {
            _realtimeStatus.text = $"Realtime: {state}";
        }

        private void OnEventReceived(RealtimeIncomingEvent incoming)
        {
            if (incoming == null)
            {
                return;
            }

            _realtimeStatus.text = incoming.EventType switch
            {
                RealtimeEventTypes.PlayerJoined => $"{incoming.Username} joined",
                RealtimeEventTypes.PlayerMoved => $"{incoming.Username} moved",
                RealtimeEventTypes.ShotFired => $"{incoming.Username} fired",
                RealtimeEventTypes.Collision => $"Collision detected",
                RealtimeEventTypes.PowerupSpawned => $"Power-up spawned",
                _ => $"Event: {incoming.EventType}"
            };
        }
    }
}
