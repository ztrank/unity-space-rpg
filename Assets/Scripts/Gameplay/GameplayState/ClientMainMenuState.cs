namespace SpaceRpg.Gameplay.GameplayState
{
    using SpaceRpg.Gameplay.UI;
    using SpaceRpg.Infrastructure.Auth;
    using SpaceRpg.UnityServices.Lobbies;
    using SpaceRpg.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;
    using VContainer.Unity;

    public class ClientMainMenuState : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.MainMenu;

        [SerializeField] LobbyUIMediator m_LobbyUIMediator;
        // [SerializeField] IPUIMediator m_IPUIMediator;
        [SerializeField] Button m_LobbyButton;
        [SerializeField] GameObject m_SignInSpinner;
        [SerializeField] UIProfileSelector m_UIProfileSelector;
        [SerializeField] UITooltipDetector m_UGSSetupTooltipDetector;

        [Inject] AuthenticationServiceFacade m_AuthServiceFacade;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] LocalLobby m_LocalLobby;
        [Inject] ProfileManager m_ProfileManager;

        protected override void Awake()
        {
            base.Awake();

            this.m_LobbyButton.interactable = false;
            this.m_LobbyUIMediator.Hide();

            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                this.OnSignInFailed();
                return;
            }

            this.TrySignIn();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(this.m_LobbyUIMediator);
            //builder.RegisterComponent(this.m_IPUIMediator);
        }

        private async void TrySignIn()
        {
            try
            {
                var unityAuthenticationInitOptions = new InitializationOptions();

                var profile = this.m_ProfileManager.Profile;

                if (profile.Length > 0)
                {
                    unityAuthenticationInitOptions.SetProfile(profile);
                }

                await this.m_AuthServiceFacade.InitializeAndSignInAsync(unityAuthenticationInitOptions);
                this.OnAuthSignIn();

                this.m_ProfileManager.OnProfileChanged += this.OnProfileChanged;
            }
            catch (Exception)
            {
                this.OnSignInFailed();
            }
        }

        private void OnAuthSignIn()
        {
            this.m_LobbyButton.interactable = true;
            this.m_UGSSetupTooltipDetector.enabled = false;
            this.m_SignInSpinner.SetActive(false);

            Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");

            this.m_LocalUser.ID = AuthenticationService.Instance.PlayerId;
            this.m_LocalLobby.AddUser(this.m_LocalUser);
        }

        private void OnSignInFailed()
        {
            if (this.m_LobbyButton)
            {
                this.m_LobbyButton.interactable = false;
                this.m_UGSSetupTooltipDetector.enabled = false;
            }

            if (this.m_SignInSpinner)
            {
                this.m_SignInSpinner.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            this.m_ProfileManager.OnProfileChanged -= this.OnProfileChanged;
            base.OnDestroy();
        }

        async void OnProfileChanged()
        {
            this.m_LobbyButton.interactable = false;
            this.m_SignInSpinner.SetActive(true);
            await this.m_AuthServiceFacade.SwitchProfileAndReSignInAsync(this.m_ProfileManager.Profile);

            this.m_LobbyButton.interactable = true;
            this.m_SignInSpinner.SetActive(false);

            Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");

            // Updating LocalUser and LocalLobby
            this.m_LocalLobby.RemoveUser(this.m_LocalUser);
            this.m_LocalUser.ID = AuthenticationService.Instance.PlayerId;
            this.m_LocalLobby.AddUser(this.m_LocalUser);
        }

        public void OnStartClicked()
        {
            this.m_LobbyUIMediator.ToggleJoinLobbyUI();
            this.m_LobbyUIMediator.Show();
        }
        /*
        public void OnDirectIPClicked()
        {
            this.m_LobbyUIMediator.Hide();
            this.m_IPUIMediator.Show();
        }
        */

        public void OnChangeProfileClicked()
        {
            this.m_UIProfileSelector.Show();
        }
    }
}