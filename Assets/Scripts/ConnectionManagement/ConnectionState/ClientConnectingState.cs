using SpaceRpg.UnityServices.Lobbies;
using SpaceRpg.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace SpaceRpg.ConnectionManagement
{
    public class ClientConnectingState : OnlineState
    {
        [Inject]
        protected LobbyServiceFacade m_LobbyServiceFacade;
        [Inject]
        protected LocalLobby m_LocalLobby;
        ConnectionMethodBase m_ConnectionMethod;

        public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            this.m_ConnectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
#pragma warning disable 4014
            this.ConnectClientAsync();
#pragma warning restore 4014
        }

        public override void Exit() { }

        public override void OnClientConnected(ulong _)
        {
            this.m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_ClientConnected);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            this.StartingClientFailedAsync();
        }

        protected void StartingClientFailedAsync()
        {
            var disconnectReason = this.m_ConnectionManager.NetworkManager.DisconnectReason;

            if (string.IsNullOrEmpty(disconnectReason))
            {
                this.m_ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                this.m_ConnectStatusPublisher.Publish(connectStatus);
            }

            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
        }

        internal async Task ConnectClientAsync()
        {
            try
            {
                await this.m_ConnectionMethod.SetupClientConnectionAsync();

                if (!this.m_ConnectionManager.NetworkManager.StartClient())
                {
                    throw new Exception("NetworkManager StartClient failed.");
                }

                SceneLoaderWrapper.Instance.AddOnSceneEventCallback();
            }
            catch (Exception e)
            {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                this.StartingClientFailedAsync();
                throw;
            }
        }
    }
}