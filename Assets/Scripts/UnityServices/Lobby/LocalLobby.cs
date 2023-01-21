namespace SpaceRpg.UnityServices.Lobbies
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Services.Lobbies.Models;
    using UnityEngine;

    [Serializable]
    public sealed class LocalLobby
    {
        public event Action<LocalLobby> changed;

        public static List<LocalLobby> CreateLocalLobies(QueryResponse response)
        {
            var retLst = new List<LocalLobby>();
            foreach (var lobby in response.Results)
            {
                retLst.Add(Create(lobby));
            }

            return retLst;
        }

        public static LocalLobby Create(Lobby lobby)
        {
            var data = new LocalLobby();
            data.ApplyRemoteData(lobby);
            return data;
        }

        Dictionary<string, LocalLobbyUser> m_LobbyUsers = new Dictionary<string, LocalLobbyUser>();

        public Dictionary<string, LocalLobbyUser> LobbyUsers => m_LobbyUsers;

        LobbyData m_Data;

        public LobbyData Data => new LobbyData(this.m_Data);

        public void AddUser(LocalLobbyUser user)
        {
            if (this.m_LobbyUsers.ContainsKey(user.ID))
            {
                this.DoAddUser(user);
                this.OnChanged();
            }
        }

        void DoAddUser(LocalLobbyUser user)
        {
            this.m_LobbyUsers.Add(user.ID, user);
            user.changed += this.OnChangedUser;
        }

        public void RemoveUser(LocalLobbyUser user)
        {
            this.DoRemoveUser(user);
            this.OnChanged();
        }

        void DoRemoveUser(LocalLobbyUser user)
        {
            if (!this.m_LobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in lobby: {this.LobbyId}");
                return;
            }

            this.m_LobbyUsers.Remove(user.ID);
            user.changed -= this.OnChangedUser;
        }

        void OnChangedUser(LocalLobbyUser user)
        {
            this.OnChanged();
        }

        void OnChanged()
        {
            this.changed?.Invoke(this);
        }

        public string LobbyId
        {
            get => this.m_Data.LobbyID;
            set
            {
                this.m_Data.LobbyID = value;
                this.OnChanged();
            }
        }

        public string LobbyCode
        {
            get => this.m_Data.LobbyCode;
            set
            {
                this.m_Data.LobbyCode = value;
                this.OnChanged();
            }
        }
        public string RelayJoinCode
        {
            get => this.m_Data.RelayJoinCode;
            set
            {
                this.m_Data.RelayJoinCode = value;
                this.OnChanged();
            }
        }

        public string LobbyName
        {
            get => this.m_Data.LobbyName;
            set
            {
                this.m_Data.LobbyName = value;
                this.OnChanged();
            }
        }

        public bool Private
        {
            get => this.m_Data.Private;
            set
            {
                this.m_Data.Private = value;
                this.OnChanged();
            }
        }

        public int PlayerCount => m_LobbyUsers.Count;

        public int MaxPlayerCount
        {
            get => this.m_Data.MaxPlayerCount;
            set
            {
                this.m_Data.MaxPlayerCount = value;
                this.OnChanged();
            }
        }

        public void CopyDataFrom(LobbyData data, Dictionary<string, LocalLobbyUser> currUsers)
        {
            this.m_Data = data;
            
            if (currUsers == null)
            {
                this.m_LobbyUsers = new Dictionary<string, LocalLobbyUser>();
            }
            else
            {
                List<LocalLobbyUser> toRemove = new List<LocalLobbyUser>();

                foreach (var oldUser in this.m_LobbyUsers)
                {
                    if (currUsers.ContainsKey(oldUser.Key))
                    {
                        oldUser.Value.CopyDataFrom(currUsers[oldUser.Key]);
                    }
                    else
                    {
                        toRemove.Add(oldUser.Value);
                    }
                }

                foreach (var remove in toRemove)
                {
                    this.DoRemoveUser(remove);
                }

                foreach (var currUser in currUsers)
                {
                    if (!this.m_LobbyUsers.ContainsKey(currUser.Key))
                    {
                        this.DoAddUser(currUser.Value);
                    }
                }
            }

            this.OnChanged();
        }

        public Dictionary<string, DataObject> GetDataForUnityServices() => new Dictionary<string, DataObject>()
        {
            { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Private, this.RelayJoinCode) }
        };

        public void ApplyRemoteData(Lobby lobby)
        {
            var info = new LobbyData();
            info.LobbyID = lobby.Id;
            info.LobbyCode = lobby.LobbyCode;
            info.Private = lobby.IsPrivate;
            info.LobbyName = lobby.Name;
            info.MaxPlayerCount = lobby.MaxPlayers;

            if (lobby.Data != null)
            {
                info.RelayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null;
            }
            else
            {
                info.RelayJoinCode = null;
            }

            var lobbyUsers = new Dictionary<string, LocalLobbyUser>();
            foreach (var player in lobby.Players)
            {
                if (player.Data != null)
                {
                    if (this.LobbyUsers.ContainsKey(player.Id))
                    {
                        lobbyUsers.Add(player.Id, this.LobbyUsers[player.Id]);
                        continue;
                    }
                }

                // If the player isn't connected to Relay, get the most recent data that the lobby knows.
                // (If we haven't seen this player yet, a new local representation of the player will have already been added by the LocalLobby.)
                var incomingData = new LocalLobbyUser
                {
                    IsHost = lobby.HostId.Equals(player.Id),
                    DisplayName = player.Data?.ContainsKey("DisplayName") == true ? player.Data["DisplayName"].Value : default,
                    ID = player.Id
                };

                lobbyUsers.Add(incomingData.ID, incomingData);
            }

            this.CopyDataFrom(info, lobbyUsers);
        }

        public void Reset(LocalLobbyUser localUser)
        {
            this.CopyDataFrom(new LobbyData(), new Dictionary<string, LocalLobbyUser>());
            this.AddUser(localUser);
        }

        public struct LobbyData
        {
            public string LobbyID { get; set; }
            public string LobbyCode { get; set; }
            public string RelayJoinCode { get; set; }
            public string LobbyName { get; set; }
            public bool Private { get; set; }
            public int MaxPlayerCount { get; set; }

            public LobbyData(LobbyData existing)
            {
                this.LobbyID = existing.LobbyID;
                this.LobbyCode = existing.LobbyCode;
                this.RelayJoinCode = existing.RelayJoinCode;
                this.LobbyName = existing.LobbyName;
                this.Private = existing.Private;
                this.MaxPlayerCount = existing.MaxPlayerCount;
            }

            public LobbyData(string lobbyCode)
            {
                this.LobbyID = null;
                this.LobbyCode = lobbyCode;
                this.RelayJoinCode = null;
                this.LobbyName = null;
                this.Private = false;
                this.MaxPlayerCount = -1;
            }
        }
    }
}