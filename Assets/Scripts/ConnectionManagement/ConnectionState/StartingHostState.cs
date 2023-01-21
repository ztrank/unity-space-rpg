namespace SpaceRpg.ConnectionManagement
{
    using global::SpaceRpg.Infrastructure;
    using global::SpaceRpg.UnityServices.Lobbies;
    using global::SpaceRpg.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Unity.Netcode;
    using UnityEngine;
    using VContainer;

    public class StartingHostState : OnlineState
    {
        [Inject]
        LobbyServiceFacade m_LobbyServiceFacade;

        [Inject]
        LocalLobby m_LocalLobby;

        ConnectionMethodBase m_ConnectionMethod;

        public StartingHostState Configure(ConnectionMethodBase connectionMethod)
        {
            this.m_ConnectionMethod = connectionMethod;
            return this;
        }

        public override void Enter()
        {
            this.StartHost();
        }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == this.m_ConnectionManager.NetworkManager.LocalClientId)
            {
                this.StartHostFailed();
            }
        }

        void StartHostFailed()
        {
            this.m_ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
        }

        public override void OnServerStarted()
        {
            this.m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Hosting);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;

            if (clientId == this.m_ConnectionManager.NetworkManager.LocalClientId)
            {
                var payload = Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId, new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        async void StartHost()
        {
            try
            {
                await this.m_ConnectionMethod.SetupHostConnectionAsync();
                Debug.Log($"Created relay allocation with join code {m_LocalLobby.RelayJoinCode}");

                if (!this.m_ConnectionManager.NetworkManager.StartHost())
                {
                    this.OnClientConnected(this.m_ConnectionManager.NetworkManager.LocalClientId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                this.StartHostFailed();
                throw;
            }
        }
    }
}
