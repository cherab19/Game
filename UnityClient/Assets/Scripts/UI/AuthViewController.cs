using System;
using Game.UnityClient.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UnityClient.UI
{
    public sealed class AuthViewController : MonoBehaviour
    {
        [SerializeField] private GameClientBootstrap bootstrap;
        [SerializeField] private UIDocument uiDocument;

        private TextField _username;
        private TextField _email;
        private TextField _password;
        private TextField _displayName;
        private Label _status;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            _username = root.Q<TextField>("Username");
            _email = root.Q<TextField>("Email");
            _password = root.Q<TextField>("Password");
            _displayName = root.Q<TextField>("DisplayName");
            _status = root.Q<Label>("AuthStatus");

            root.Q<Button>("LoginButton").clicked += OnLoginClicked;
            root.Q<Button>("RegisterButton").clicked += OnRegisterClicked;
        }

        private async void OnLoginClicked()
        {
            try
            {
                var response = await bootstrap.AuthService.LoginAsync(_username.value, _password.value);
                if (!response.Success)
                {
                    _status.text = $"Login failed: {response.Error?.Message}";
                    return;
                }

                bootstrap.Session.Set(response.Data);
                _status.text = $"Authenticated as {response.Data.Player.DisplayName}";
            }
            catch (Exception exception)
            {
                _status.text = $"Login exception: {exception.Message}";
            }
        }

        private async void OnRegisterClicked()
        {
            try
            {
                var response = await bootstrap.AuthService.RegisterAsync(_username.value, _email.value, _password.value, _displayName.value);
                if (!response.Success)
                {
                    _status.text = $"Register failed: {response.Error?.Message}";
                    return;
                }

                bootstrap.Session.Set(response.Data);
                _status.text = $"Registered {response.Data.Player.DisplayName}";
            }
            catch (Exception exception)
            {
                _status.text = $"Register exception: {exception.Message}";
            }
        }
    }
}
