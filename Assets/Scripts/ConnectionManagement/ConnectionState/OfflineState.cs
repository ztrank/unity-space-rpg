namespace SpaceRpg.ConnectionManagement
{
    using global::SpaceRpg.UnityServices.Lobbies;
    using global::SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using VContainer;

    public class OfflineState : ConnectionState
    {
        [Inject]
        LobbyServiceFacade m_LobbyServiceFacade;

        [Inject]
        ProfileManager m_ProfileManager;

        [Inject]
        LocalLobby m_LocalLobby;

        const string k_MainMenuSceneName = "MainMenu";

        public override void Enter()
        {
            this.m_LobbyServiceFacade.EndTracking();
            this.m_ConnectionManager.NetworkManager.Shutdown();

            if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
            {
                SceneLoaderWrapper.Instance.LoadScene(k_MainMenuSceneName, useNetworkSceneManager: false);
            }
        }

        public override void Exit() { }

        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, this.m_ConnectionManager, this.m_ProfileManager, playerName);
            this.m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
        }

        public override void StartClientLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(this.m_LobbyServiceFacade, this.m_LocalLobby, this.m_ConnectionManager, this.m_ProfileManager, playerName);
            this.m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
        }

        public override void StartHostIP(string playerName, string ipAddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipAddress, (ushort)port, this.m_ConnectionManager, this.m_ProfileManager, playerName);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_StartingHost.Configure(connectionMethod));
        }

        public override void StartHostLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(this.m_LobbyServiceFacade, this.m_LocalLobby, this.m_ConnectionManager, this.m_ProfileManager, playerName);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_StartingHost.Configure(connectionMethod));
        }
    }
}