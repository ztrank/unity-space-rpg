namespace SpaceRpg.Gameplay.GameplayObjects
{
    using SpaceRpg.ConnectionManagement;
    using SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;

    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        [SerializeField]
        PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;

        [SerializeField]
        NetworkNameState m_NetworkNameState;


        public NetworkNameState NetworkNameState => this.m_NetworkNameState;

        public override void OnNetworkSpawn()
        {
            this.gameObject.name = "PersistentPlayer" + this.OwnerClientId;

            this.m_PersistentPlayerRuntimeCollection.Add(this);

            if (this.IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(this.OwnerClientId);

                if (sessionPlayerData.HasValue)
                {
                    var playerdata = sessionPlayerData.Value;

                    this.m_NetworkNameState.Name.Value = playerdata.PlayerName;

                    if (playerdata.HasCharacterSpawned)
                    {

                    }
                    else
                    {
                        SessionManager<SessionPlayerData>.Instance.SetPlayerData(this.OwnerClientId, playerdata);
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            this.RemovePersistentPlayer();
        }

        public override void OnNetworkDespawn()
        {
            this.RemovePersistentPlayer();
        }

        void RemovePersistentPlayer()
        {
            this.m_PersistentPlayerRuntimeCollection.Remove(this);

            if (this.IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(this.OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerdata = sessionPlayerData.Value;
                    playerdata.PlayerName = this.m_NetworkNameState.Name.Value;

                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(this.OwnerClientId, playerdata);
                }
            }
        }
    }
}