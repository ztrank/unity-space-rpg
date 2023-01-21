using SpaceRpg.UnityServices.Lobbies;
using SpaceRpg.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace SpaceRpg.ConnectionManagement
{
    public abstract class ConnectionMethodBase
    {
        protected ConnectionManager m_ConnectionManager;
        protected ProfileManager m_ProfileManager;
        protected readonly string m_PlayerName;
        public ConnectionMethodBase(ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
        {
            this.m_ConnectionManager = connectionManager;
            this.m_ProfileManager = profileManager;
            this.m_PlayerName = playerName;
        }
            
        public abstract Task SetupHostConnectionAsync();
        public abstract Task SetupClientConnectionAsync();

        protected void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });

            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            this.m_ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        protected string GetPlayerId()
        {
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
            {
                return ClientPrefs.GetGuid() + m_ProfileManager.Profile;
            }

            return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid() + this.m_ProfileManager.Profile;
        }
    }

    class ConnectionMethodIP : ConnectionMethodBase
    {
        string m_IpAddress;
        ushort m_Port;

        public ConnectionMethodIP(string ip, ushort port, ConnectionManager connectionManager, ProfileManager profileManager, string playerName) 
            : base(connectionManager, profileManager, playerName)
        {
            this.m_IpAddress = ip;
            this.m_Port = port;
        }

#pragma warning disable 1998
        public override async Task SetupClientConnectionAsync()
        {
            this.SetConnectionPayload(this.GetPlayerId(), this.m_PlayerName);
            var utp = (UnityTransport)this.m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(this.m_IpAddress, this.m_Port);
        }

        public override async Task SetupHostConnectionAsync()
        {
            this.SetConnectionPayload(this.GetPlayerId(), this.m_PlayerName);
            var utp = (UnityTransport)this.m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(this.m_IpAddress, this.m_Port);
        }
    }
#pragma warning restore 1998

    class ConnectionMethodRelay : ConnectionMethodBase
    {
        private LobbyServiceFacade m_lobbyServiceFacade;
        private LocalLobby m_LocalLobby;

        public ConnectionMethodRelay(LobbyServiceFacade lobbyService, LocalLobby lobby, ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
            : base(connectionManager, profileManager, playerName)
        {
            this.m_LocalLobby = lobby;
            this.m_lobbyServiceFacade = lobbyService;
            this.m_ConnectionManager = connectionManager;
        }

        public override async Task SetupClientConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay Client");

            this.SetConnectionPayload(this.GetPlayerId(), this.m_PlayerName);

            if (this.m_lobbyServiceFacade.CurrentUnityLobby == null)
            {
                throw new Exception("Trying to start relay while Lobby isn't set");
            }

            var joinedAllocation = await RelayService.Instance.JoinAllocationAsync(this.m_LocalLobby.RelayJoinCode);
            Debug.Log($"client: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
                $"host: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
                $"client: {joinedAllocation.AllocationId}");

            await this.m_lobbyServiceFacade.UpdatePlayerRelayInfoAsync(joinedAllocation.AllocationId.ToString(), this.m_LocalLobby.RelayJoinCode);

            var utp = (UnityTransport)this.m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;

            utp.SetRelayServerData(new RelayServerData(joinedAllocation, OnlineState.k_DtlsConnType));
        }

        public override async Task SetupHostConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay Host");

            this.SetConnectionPayload(this.GetPlayerId(), this.m_PlayerName);

            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(this.m_ConnectionManager.MaxConnectedPlayers, region: null);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            Debug.Log($"server: connection data: {hostAllocation.ConnectionData[0]} {hostAllocation.ConnectionData[1]}, " +
                $"allocation ID:{hostAllocation.AllocationId}, region:{hostAllocation.Region}");

            this.m_LocalLobby.RelayJoinCode = joinCode;

            await this.m_lobbyServiceFacade.UpdateLobbyDataAsync(this.m_LocalLobby.GetDataForUnityServices());
            await this.m_lobbyServiceFacade.UpdatePlayerRelayInfoAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

            var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(hostAllocation, OnlineState.k_DtlsConnType));
        }
        
    }
}
