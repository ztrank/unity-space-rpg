using SpaceRpg.UnityServices.Lobbies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace SpaceRpg.ConnectionManagement
{
    public class ClientConnectedState : ConnectionState
    {
        
        [Inject]
        protected LobbyServiceFacade m_LobbyServiceFacade;
        
        public override void Enter()
        {
            if (this.m_LobbyServiceFacade.CurrentUnityLobby != null)
            {
                this.m_LobbyServiceFacade.BeginTracking();
            }
        }

        public override void Exit()
        {
          
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            var disconnectReason =this.m_ConnectionManager.NetworkManager.DisconnectReason;

            if (string.IsNullOrEmpty(disconnectReason))
            {
               this.m_ConnectStatusPublisher.Publish(ConnectStatus.Reconnecting);
               this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_ClientReconnecting);
            }
            else
            {
               var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
               this.m_ConnectStatusPublisher.Publish(connectStatus);
               this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
            }
        }

        public override void OnUserRequestedShutdown()
        {
            this.m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
        }
    }
}
