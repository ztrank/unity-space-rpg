namespace SpaceRpg.ConnectionManagement
{
    using global::SpaceRpg.Infrastructure;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VContainer;

    public class ClientReconnectingState : ClientConnectingState
    {
        [Inject]
        IPublisher<ReconnectMessage> m_ReconnectMessagePublisher;

        Coroutine m_ReconnectCoroutine;
        string m_LobbyCode = "";
        int m_NbAttempts;

        const float k_TimeBetweenAttempts = 5;

        public override void Enter()
        {
            this.m_NbAttempts = 0;
            this.m_LobbyCode = this.m_LobbyServiceFacade != null ? this.m_LobbyServiceFacade.CurrentUnityLobby.LobbyCode : "";
            this.m_ReconnectCoroutine = this.m_ConnectionManager.StartCoroutine(this.ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (this.m_ReconnectCoroutine != null)
            {
                this.m_ConnectionManager.StopCoroutine(this.m_ReconnectCoroutine);
                this.m_ReconnectCoroutine = null;
            }

            this.m_ReconnectMessagePublisher.Publish(new ReconnectMessage(this.m_ConnectionManager.NbReconnectAttempts, this.m_ConnectionManager.NbReconnectAttempts));
        }

        public override void OnClientConnected(ulong _)
        {
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_ClientConnected);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            var disconnectReason = this.m_ConnectionManager.NetworkManager.DisconnectReason;
            if (this.m_NbAttempts < this.m_ConnectionManager.NbReconnectAttempts)
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    this.m_ReconnectCoroutine = this.m_ConnectionManager.StartCoroutine(this.ReconnectCoroutine());
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    this.m_ConnectStatusPublisher.Publish(connectStatus);
                    switch (connectStatus)
                    {
                        case ConnectStatus.UserRequestedDisconnect:
                        case ConnectStatus.HostEndedSession:
                        case ConnectStatus.ServerFull:
                        case ConnectStatus.IncompatibleBuildType:
                            this.m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
                            break;
                        default:
                            this.m_ReconnectCoroutine = this.m_ConnectionManager.StartCoroutine(this.ReconnectCoroutine());
                            break;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    this.m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    this.m_ConnectStatusPublisher.Publish(connectStatus);
                }

                this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
            }
        }

        private IEnumerator ReconnectCoroutine()
        {
            if (this.m_NbAttempts > 0)
            {
                yield return new WaitForSeconds(k_TimeBetweenAttempts);
            }

            Debug.Log("Lost Connection to host, trying to reconnect...");

            this.m_ConnectionManager.NetworkManager.Shutdown();

            yield return new WaitWhile(() => this.m_ConnectionManager.NetworkManager.ShutdownInProgress);

            Debug.Log($"Reconnecting attempt {this.m_NbAttempts + 1}/{this.m_ConnectionManager.NbReconnectAttempts}...");

            this.m_ReconnectMessagePublisher.Publish(new ReconnectMessage(this.m_NbAttempts, this.m_ConnectionManager.NbReconnectAttempts));
            this.m_NbAttempts++;

            if (!string.IsNullOrEmpty(this.m_LobbyCode))
            {
                var reconnectingToLobby = this.m_LobbyServiceFacade.ReconnectToLobbyAsync(this.m_LocalLobby?.LobbyId);
                yield return new WaitUntil(() => reconnectingToLobby.IsCompleted);

                if (!reconnectingToLobby.IsFaulted && reconnectingToLobby.Result != null)
                {
                    var connectingToRelay = this.ConnectClientAsync();
                    yield return new WaitUntil(() => connectingToRelay.IsCompleted);
                }
                else
                {
                    Debug.Log("Failed reconnecting to lobby.");
                    this.OnClientDisconnect(0);
                }
            }
            else
            {
                var connectingClient = this.ConnectClientAsync();
                yield return new WaitUntil(() => connectingClient.IsCompleted);
            }
        }
    }
}