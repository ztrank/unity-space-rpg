namespace SpaceRpg.UnityServices.Lobbies
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Services.Lobbies.Models;
    using UnityEngine;

    [Serializable]
    public class LocalLobbyUser
    {
        public event Action<LocalLobbyUser> changed;

        UserData m_UserData;
        UserMembers m_LastChanged;

        public LocalLobbyUser()
        {
            this.m_UserData = new UserData(isHost: false, displayName: null, id: null);
        }

        public bool IsHost
        {
            get { return this.m_UserData.IsHost; }
            set
            {
                if (this.m_UserData.IsHost != value)
                {
                    this.m_UserData.IsHost = value;
                    this.m_LastChanged = UserMembers.IsHost;
                    this.OnChanged();
                }
            }
        }

        public string DisplayName
        {
            get => this.m_UserData.DisplayName;
            set
            {
                if (this.m_UserData.DisplayName != value)
                {
                    this.m_UserData.DisplayName = value;
                    this.m_LastChanged = UserMembers.DisplayName;
                    this.OnChanged();
                }
            }
        }

        public string ID
        {
            get => this.m_UserData.ID;
            set
            {
                if (this.m_UserData.ID != value)
                {
                    this.m_UserData.ID = value;
                    this.m_LastChanged = UserMembers.ID;
                    this.OnChanged();
                }
            }
        }

        public void CopyDataFrom(LocalLobbyUser lobby)
        {
            var data = lobby.m_UserData;
            int lastChanged = // Set flags just for the members that will be changed.
                (this.m_UserData.IsHost == data.IsHost ? 0 : (int)UserMembers.IsHost) |
                (this.m_UserData.DisplayName == data.DisplayName ? 0 : (int)UserMembers.DisplayName) |
                (this.m_UserData.ID == data.ID ? 0 : (int)UserMembers.ID);

            if (lastChanged == 0) // Ensure something actually changed.
            {
                return;
            }

            this.m_UserData = data;
            this.m_LastChanged = (UserMembers)lastChanged;

            this.OnChanged();
        }

        void OnChanged()
        {
            this.changed?.Invoke(this);
        }

        public Dictionary<string, PlayerDataObject> GetDataForUnityServices() =>
            new Dictionary<string, PlayerDataObject>()
            {
                {"DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, this.DisplayName)},
            };

        public void ResetState()
        {
            this.m_UserData = new UserData(false, this.m_UserData.DisplayName, this.m_UserData.ID);
        }

        [Flags]
        public enum UserMembers
        {
            IsHost = 1,
            DisplayName = 2,
            ID = 4
        }

        public struct UserData
        {
            public bool IsHost { get; set; }
            public string DisplayName { get; set; }
            public string ID { get; set; }

            public UserData(bool isHost, string displayName, string id)
            {
                this.IsHost = isHost;
                this.DisplayName = displayName;
                this.ID = id;
            }
        }
    }
}