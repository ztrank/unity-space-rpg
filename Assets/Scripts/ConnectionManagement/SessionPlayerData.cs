namespace SpaceRpg.ConnectionManagement
{
    using global::SpaceRpg.Infrastructure;
    using global::SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;

        public SessionPlayerData(ulong clientId, string name, NetworkGuid avatarNetworkGuid, int currentHitpoints = 0, bool isConnected = false, bool hasCharacterSpawned = false)
        {
            this.ClientID = clientId;
            this.PlayerName = name;
            this.PlayerNumber = -1;
            this.PlayerPosition = Vector3.zero;
            this.PlayerRotation = Quaternion.identity;
            this.AvatarNetworkGuid = avatarNetworkGuid;
            this.CurrentHitPoints = currentHitpoints;
            this.IsConnected = isConnected;
            this.HasCharacterSpawned = hasCharacterSpawned;
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public void Reinitialize()
        {
            this.HasCharacterSpawned = false;
        }
    }
}