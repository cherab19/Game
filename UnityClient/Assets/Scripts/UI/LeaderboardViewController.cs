using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UnityClient.UI
{
    public sealed class LeaderboardViewController : MonoBehaviour
    {
        [SerializeField] private GameClientBootstrap bootstrap;
        [SerializeField] private UIDocument uiDocument;

        private Label _leaderboardBody;
        private Label _status;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            _leaderboardBody = root.Q<Label>("LeaderboardBody");
            _status = root.Q<Label>("LeaderboardStatus");

            root.Q<Button>("RefreshLeaderboardButton").clicked += RefreshLeaderboard;
            root.Q<Button>("PostMatchResultButton").clicked += PostMatchResult;
        }

        private async void RefreshLeaderboard()
        {
            try
            {
                var response = await bootstrap.LeaderboardService.GetTopAsync(10);
                if (!response.Success)
                {
                    _status.text = $"Leaderboard fetch failed: {response.Error?.Message}";
                    return;
                }

                var sb = new StringBuilder();
                for (var i = 0; i < response.Data.Count; i++)
                {
                    var entry = response.Data[i];
                    sb.AppendLine($"#{i + 1} {entry.DisplayName} | Score {entry.Score} | W {entry.Wins} L {entry.Losses}");
                }

                _leaderboardBody.text = sb.ToString();
                _status.text = "Leaderboard updated.";
            }
            catch (Exception exception)
            {
                _status.text = $"Leaderboard exception: {exception.Message}";
            }
        }

        private async void PostMatchResult()
        {
            if (!bootstrap.Session.IsAuthenticated)
            {
                _status.text = "Authenticate first.";
                return;
            }

            try
            {
                var response = await bootstrap.MatchResultSyncService.SubmitMatchResultAsync(didWin: true);

                if (!response.Success)
                {
                    _status.text = $"Update failed: {response.Error?.Message}";
                    return;
                }

                _status.text = "Match result posted.";
                RefreshLeaderboard();
            }
            catch (Exception exception)
            {
                _status.text = $"Update exception: {exception.Message}";
            }
        }
    }
}
