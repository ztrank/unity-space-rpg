namespace SpaceRpg.ConnectionManagement
{
    using global::SpaceRpg.Infrastructure;
    using global::SpaceRpg.UnityServices.Lobbies;
    using global::SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Unity.Netcode;
    using UnityEngine;
    using VContainer;

    public class HostingState : OnlineState
    {
        [Inject]
        LobbyServiceFacade m_LobbyServiceFacade;

        [Inject]
        IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

        const int k_MaxConnectPayload = 1024;

        public override void Enter()
        {
            SceneLoaderWrapper.Instance.AddOnSceneEventCallback();

            //The "BossRoom" server always advances to CharSelect immediately on start. Different games
            //may do this differently.
            SceneLoaderWrapper.Instance.LoadScene("Lobby", useNetworkSceneManager: true);
            if (this.m_LobbyServiceFacade.CurrentUnityLobby != null)
            {
                this.m_LobbyServiceFacade.BeginTracking();
            }
        }

        public override void Exit()
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        }

        public override void OnClientConnected(ulong clientId)
        {
            this.m_ConnectionEventPublisher.Publish(new ConnectionEventMessage()
            {
                ConnectStatus = ConnectStatus.Success,
                PlayerName = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId)?.PlayerName
            });
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == this.m_ConnectionManager.NetworkManager.LocalClientId)
            {
                this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
            }
            else
            {
                var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
                if (playerId != null)
                {
                    var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
                    if (sessionData.HasValue)
                    {
                        this.m_ConnectionEventPublisher.Publish(new ConnectionEventMessage()
                        {
                            ConnectStatus = ConnectStatus.GenericDisconnect,
                            PlayerName = sessionData.Value.PlayerName
                        });

                        SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
                    }
                }
            }
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);

            for (var i = this.m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
            {
                var id = this.m_ConnectionManager.NetworkManager.ConnectedClientsIds[i];
                if (id != this.m_ConnectionManager.NetworkManager.LocalClientId)
                {
                    this.m_ConnectionManager.NetworkManager.DisconnectClient(id, reason);
                }
            }

            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;

            if (connectionData.Length > k_MaxConnectPayload)
            {
                response.Approved = false;
                return;
            }

            var payload  = Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            var gameReturnStatus = this.GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId, new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
            if (m_LobbyServiceFacade.CurrentUnityLobby != null)
            {
                this.m_LobbyServiceFacade.RemovePlayerFromLobbyAsync(connectionPayload.playerId, this.m_LobbyServiceFacade.CurrentUnityLobby.Id);
            }
        }

        ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (this.m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= this.m_ConnectionManager.MaxConnectedPlayers)
            {
                return ConnectStatus.ServerFull;
            }

            if (connectionPayload.isDebug != Debug.isDebugBuild)
            {
                return ConnectStatus.IncompatibleBuildType;
            }

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ? ConnectStatus.LoggedInAgain : ConnectStatus.Success;
        }
    }
}