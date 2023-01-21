using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using SpaceRpg.Utils;
using System;
using VContainer;

namespace SpaceRpg.ConnectionManagement
{
    public enum ConnectStatus
    {
        Undefined,
        Success,
        ServerFull,
        LoggedInAgain,
        UserRequestedDisconnect,
        GenericDisconnect,
        Reconnecting,
        IncompatibleBuildType,
        HostEndedSession,
        StartHostFailed,
        StartClientFailed
    }

    public struct ReconnectMessage
    {
        public int CurrentAttempt;
        public int MaxAttempt;

        public ReconnectMessage(int currentAttempt, int maxAttempt)
        {
            this.CurrentAttempt = currentAttempt;
            this.MaxAttempt = maxAttempt;
        }
    }


    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ConnectStatus ConnectStatus;
        public FixedPlayerName PlayerName;
    }

    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
    }


    public class ConnectionManager : MonoBehaviour
    {
        private ConnectionState m_CurrentState;

        [Inject]
        private NetworkManager m_NetworkManager;

        [SerializeField]
        int m_NbReconnectAttempts = 2;

        public int NbReconnectAttempts => this.m_NbReconnectAttempts;

        public int MaxConnectedPlayers = 8;

        internal readonly OfflineState m_Offline = new OfflineState();
        internal readonly ClientConnectingState m_ClientConnecting = new ClientConnectingState();
        internal readonly ClientConnectedState m_ClientConnected = new ClientConnectedState();
        internal readonly ClientReconnectingState m_ClientReconnecting = new ClientReconnectingState();
        internal readonly StartingHostState m_StartingHost = new StartingHostState();
        internal readonly HostingState m_Hosting = new HostingState();

        public NetworkManager NetworkManager => this.m_NetworkManager;

        [Inject]
        IObjectResolver m_Resolver;

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            List<ConnectionState> states = new List<ConnectionState>()
            {
                this.m_Offline,
                this.m_ClientConnecting,
                this.m_ClientConnected,
                this.m_ClientReconnecting,
                this.m_StartingHost,
                this.m_Hosting
            };

            foreach(var connectionState in states)
            {
                this.m_Resolver.Inject(connectionState);
            }

            this.m_CurrentState = this.m_Offline;

            this.NetworkManager.OnClientConnectedCallback += this.OnClientConnectedCallback;
            this.NetworkManager.OnClientDisconnectCallback += this.OnClientDisconnectCallback;
            this.NetworkManager.OnServerStarted += this.OnServerStarted;
            this.NetworkManager.ConnectionApprovalCallback += this.ApprovalCheck;
            this.NetworkManager.OnTransportFailure += this.OnTransportFailure;
        }

        private void OnDestroy()
        {
            this.NetworkManager.OnClientConnectedCallback -= this.OnClientConnectedCallback;
            this.NetworkManager.OnClientDisconnectCallback -= this.OnClientDisconnectCallback;
            this.NetworkManager.OnServerStarted -= this.OnServerStarted;
            this.NetworkManager.ConnectionApprovalCallback -= this.ApprovalCheck;
            this.NetworkManager.OnTransportFailure -= this.OnTransportFailure;
        }

        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from {m_CurrentState.GetType().Name} to {nextState.GetType().Name}.");
            if (this.m_CurrentState != null)
            {
                this.m_CurrentState.Exit();
            }

            this.m_CurrentState = nextState;
            this.m_CurrentState.Enter();
        }

        void OnClientDisconnectCallback(ulong clientId)
        {
            this.m_CurrentState.OnClientDisconnect(clientId);
        }

        void OnClientConnectedCallback(ulong clientId)
        {
            this.m_CurrentState.OnClientConnected(clientId);
        }

        void OnServerStarted()
        {
            this.m_CurrentState.OnServerStarted();
        }

        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            this.m_CurrentState.ApprovalCheck(request, response);
        }

        void OnTransportFailure()
        {
            this.m_CurrentState.OnTransportFailure();
        }

        public void StartClientLobby(string playerName)
        {
            this.m_CurrentState.StartClientLobby(playerName);
        }

        public void StartClientIp(string playername, string ipaddress, int port)
        {
            this.m_CurrentState.StartClientIP(playername, ipaddress, port);
        }

        public void StartHostLobby(string playerName)
        {
            this.m_CurrentState.StartHostLobby(playerName);
        }

        public void StartHostIp(string playerName, string ipaddress, int port)
        {
            this.m_CurrentState.StartHostIP(playerName, ipaddress, port);
        }

        public void RequestShutdown()
        {
            this.m_CurrentState.OnUserRequestedShutdown();
        }
    }
}