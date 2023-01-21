namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.ConnectionManagement;
    using SpaceRpg.Infrastructure;
    using SpaceRpg.Infrastructure.Auth;
    using SpaceRpg.UnityServices.Lobbies;
    using SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class LobbyUIMediator : MonoBehaviour
    {
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] LobbyJoiningUI m_LobbyJoiningUI;
        [SerializeField] LobbyCreationUI m_LobbyCreationUI;
        [SerializeField] UITinter m_JoinToggleHighlight;
        [SerializeField] UITinter m_JoinToggleTabBlocker;
        [SerializeField] UITinter m_CreateToggleHighlight;
        [SerializeField] UITinter m_CreateToggleTabBlocker;
        [SerializeField] TextMeshProUGUI m_PlayerNameLabel;
        [SerializeField] GameObject m_LoadingSpinner;

        AuthenticationServiceFacade m_AuthenticationServiceFacade;
        LobbyServiceFacade m_LobbyServiceFacade;
        LocalLobbyUser m_LocalUser;
        LocalLobby m_LocalLobby;
        ConnectionManager m_ConnectionManager;
        ProfileManager m_ProfileManager;
        ISubscriber<ConnectStatus> m_ConnectStatusSubscriber;

        const string k_DefaultLobbyName = "no-name";

        [Inject]
        void InjectDependenciesAndInitialize(
            AuthenticationServiceFacade authService,
            LobbyServiceFacade lobbyService,
            LocalLobbyUser localUser,
            LocalLobby localLobby,
            ISubscriber<ConnectStatus> connectStatusSub,
            ConnectionManager connectionManager,
            ProfileManager profileManager)
        {
            this.m_AuthenticationServiceFacade = authService;
            this.m_LobbyServiceFacade = lobbyService;
            this.m_LocalUser = localUser;
            this.m_LocalLobby = localLobby;
            this.m_ConnectionManager = connectionManager;
            this.m_ConnectStatusSubscriber = connectStatusSub;
            this.m_ProfileManager = profileManager;

            this.m_ProfileManager.OnProfileChanged += this.OnProfileChanged;
            this.OnProfileChanged();
            this.m_ConnectStatusSubscriber.Subscribe(this.OnConnectStatus);
        }

        void OnConnectStatus(ConnectStatus status)
        {
            if (status is ConnectStatus.GenericDisconnect or ConnectStatus.StartClientFailed)
            {
                this.UnblockUIAfterLoadingIsComplete();
            }
        }

        private void OnProfileChanged()
        {
            if (!string.IsNullOrEmpty(this.m_ProfileManager.Profile))
            {
                this.m_PlayerNameLabel.text = this.m_ProfileManager.Profile;
            }
        }

        private void OnDestroy()
        {
            this.m_ConnectStatusSubscriber?.Unsubscribe(this.OnConnectStatus);
        }

        public async void CreateLobbyRequest(string lobbyName, bool isPrivate, int numberOfPlayers)
        {
            if (string.IsNullOrEmpty(lobbyName))
            {
                lobbyName = k_DefaultLobbyName;
            }

            this.BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                this.UnblockUIAfterLoadingIsComplete();
                return;
            }

            int connections = Mathf.Clamp(numberOfPlayers + 1, 1, this.m_ConnectionManager.MaxConnectedPlayers);
            var lobbyCreationAttempt = await this.m_LobbyServiceFacade.TryCreateLobbyAsync(lobbyName, connections, isPrivate);

            if (lobbyCreationAttempt.Success)
            {
                this.m_LocalUser.IsHost = true;
                this.m_LobbyServiceFacade.SetRemoteLobby(lobbyCreationAttempt.lobby);

                Debug.Log($"Created lobby with ID: {m_LocalLobby.LobbyId} and code {m_LocalLobby.LobbyCode}");
                this.m_ConnectionManager.StartHostLobby(this.m_LocalUser.DisplayName);
            }
            else
            {
                this.UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void QueryLobbiesRequest(bool blockUI)
        {
            if (Unity.Services.Core.UnityServices.State != Unity.Services.Core.ServicesInitializationState.Initialized)
            {
                return;
            }

            if (blockUI)
            {
                this.BlockUIWhileLoadingIsInProgress();
            }

            bool playerIsAuthorized = await this.m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (blockUI && !playerIsAuthorized)
            {
                this.UnblockUIAfterLoadingIsComplete();
                return;
            }

            await this.m_LobbyServiceFacade.RetrieveAndPublishLobbyListAsync();

            if (blockUI)
            {
                this.UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void JoinLobbyWithCodeRequest(string lobbyCode)
        {
            this.BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await this.m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                this.UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await this.m_LobbyServiceFacade.TryJoinLobbyAsync(null, lobbyCode);

            if (result.Success)
            {
                this.OnJoinedLobby(result.lobby);
            }
            else
            {
                this.UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void JoinLobbyRequest(LocalLobby lobby)
        {
            this.BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await this.m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                this.UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await this.m_LobbyServiceFacade.TryJoinLobbyAsync(lobby.LobbyId, lobby.LobbyCode);

            if (result.Success)
            {
                this.OnJoinedLobby(result.lobby);
            }
            else
            {
                this.UnblockUIAfterLoadingIsComplete();
            }
        }

        void OnJoinedLobby(Unity.Services.Lobbies.Models.Lobby remoteLobby)
        {
            this.m_LobbyServiceFacade.SetRemoteLobby(remoteLobby);
            Debug.Log($"Joined lobby with code: {m_LocalLobby.LobbyCode}, Internal Relay Join Code{m_LocalLobby.RelayJoinCode}");
            this.m_ConnectionManager.StartClientLobby(this.m_LocalUser.DisplayName);
        }

        public void Show()
        {
            this.m_CanvasGroup.alpha = 1f;
            this.m_CanvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.blocksRaycasts = false;
            this.m_LobbyCreationUI.Hide();
            this.m_LobbyJoiningUI.Hide();
        }

        public void ToggleJoinLobbyUI()
        {
            this.m_LobbyJoiningUI.Show();
            this.m_LobbyCreationUI.Hide();
            this.m_JoinToggleHighlight.SetToColor(1);
            this.m_JoinToggleTabBlocker.SetToColor(1);
            this.m_CreateToggleHighlight.SetToColor(0);
            this.m_CreateToggleTabBlocker.SetToColor(0);
        }

        public void ToggleCreateLobbyUI()
        {
            this.m_LobbyJoiningUI.Hide();
            this.m_LobbyCreationUI.Show();
            this.m_JoinToggleHighlight.SetToColor(0);
            this.m_JoinToggleTabBlocker.SetToColor(0);
            this.m_CreateToggleHighlight.SetToColor(1);
            this.m_CreateToggleTabBlocker.SetToColor(1);
        }


        void BlockUIWhileLoadingIsInProgress()
        {
            this.m_CanvasGroup.interactable = false;
            this.m_LoadingSpinner.SetActive(true);
        }

        void UnblockUIAfterLoadingIsComplete()
        {
            //this callback can happen after we've already switched to a different scene
            //in that case the canvas group would be null
            if (this.m_CanvasGroup != null)
            {
                this.m_CanvasGroup.interactable = true;
                this.m_LoadingSpinner.SetActive(false);
            }
        }
    }
}