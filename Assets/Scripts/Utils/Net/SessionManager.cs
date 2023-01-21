namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public interface ISessionPlayerData
    {
        bool IsConnected { get; set; }
        ulong ClientID { get; set; }
        void Reinitialize();
    }

    public class SessionManager<T> where T : struct, ISessionPlayerData
    {
        private static SessionManager<T> s_Instance;

        private Dictionary<string, T> m_ClientData;
        private Dictionary<ulong, string> m_ClientIDToPlayerId;
        private bool m_HasSessionStarted;

        SessionManager()
        {
            this.m_ClientData = new Dictionary<string, T>();
            this.m_ClientIDToPlayerId = new Dictionary<ulong, string>();
        }

        public static SessionManager<T> Instance => s_Instance ??= new SessionManager<T>();

        public void DisconnectClient(ulong clientID)
        {
            if (this.m_HasSessionStarted)
            {
                if (this.m_ClientIDToPlayerId.TryGetValue(clientID, out var playerId))
                {
                    if (this.GetPlayerData(playerId)?.ClientID == clientID)
                    {
                        var clientData = this.m_ClientData[playerId];
                        clientData.IsConnected = false;
                        this.m_ClientData[playerId] = clientData;
                    }
                }
            }
            else
            {
                if (this.m_ClientIDToPlayerId.TryGetValue(clientID, out var playerId))
                {
                    this.m_ClientIDToPlayerId.Remove(clientID);
                    if (this.GetPlayerData(playerId)?.ClientID == clientID)
                    {
                        this.m_ClientData.Remove(playerId);
                    }
                }
            }
        }

        public bool IsDuplicateConnection(string playerId)
        {
            return this.m_ClientData.ContainsKey(playerId) && this.m_ClientData[playerId].IsConnected;
        }

        public void SetupConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData)
        {
            var isReconnecting = false;

            if (this.IsDuplicateConnection(playerId))
            {
                Debug.LogError($"Player ID {playerId} already exists. This is a duplicate connection. Rejecting this session data.");
                return;
            }

            if (this.m_ClientData.ContainsKey(playerId) && !this.m_ClientData[playerId].IsConnected)
            {
                isReconnecting = true;
            }

            if (isReconnecting)
            {
                sessionPlayerData = this.m_ClientData[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            this.m_ClientData[playerId] = sessionPlayerData;
            this.m_ClientIDToPlayerId[clientId] = playerId;
        }

        public string GetPlayerId(ulong clientId)
        {
            if (this.m_ClientIDToPlayerId.TryGetValue(clientId, out string playerId))
            {
                return playerId;
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        public T? GetPlayerData(ulong clientId)
        {
            //First see if we have a playerId matching the clientID given.
            var playerId = this.GetPlayerId(clientId);
            if (playerId != null)
            {
                return this.GetPlayerData(playerId);
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        public T? GetPlayerData(string playerId)
        {
            if (this.m_ClientData.TryGetValue(playerId, out T data))
            {
                return data;
            }

            Debug.Log($"No PlayerData of matching player ID found: {playerId}");
            return null;
        }

        public void SetPlayerData(ulong clientId, T sessionPlayerData)
        {
            if (this.m_ClientIDToPlayerId.TryGetValue(clientId, out string playerId))
            {
                this.m_ClientData[playerId] = sessionPlayerData;
            }
            else
            {
                Debug.LogError($"No client player ID found mapped to the given client ID: {clientId}");
            }
        }

        public void OnSessionStarted()
        {
            this.m_HasSessionStarted = true;
        }

        public void OnSessionEnded()
        {
            this.ClearDisconnectedPlayersData();
            this.ReinitializePlayersData();
            this.m_HasSessionStarted = false;
        }

        public void OnServerEnded()
        {
            this.m_ClientData.Clear();
            this.m_ClientIDToPlayerId.Clear();
            this.m_HasSessionStarted = false;
        }

        void ReinitializePlayersData()
        {
            foreach (var id in this.m_ClientIDToPlayerId.Keys)
            {
                string playerId = this.m_ClientIDToPlayerId[id];
                T sessionPlayerData = this.m_ClientData[playerId];
                sessionPlayerData.Reinitialize();
                this.m_ClientData[playerId] = sessionPlayerData;
            }
        }

        void ClearDisconnectedPlayersData()
        {
            List<ulong> idsToClear = new List<ulong>();
            foreach (var id in this.m_ClientIDToPlayerId.Keys)
            {
                var data = this.GetPlayerData(id);
                if (data is { IsConnected: false })
                {
                    idsToClear.Add(id);
                }
            }

            foreach (var id in idsToClear)
            {
                string playerId = this.m_ClientIDToPlayerId[id];
                if (this.GetPlayerData(playerId)?.ClientID == id)
                {
                    this.m_ClientData.Remove(playerId);
                }

                this.m_ClientIDToPlayerId.Remove(id);
            }
        }
    }
}