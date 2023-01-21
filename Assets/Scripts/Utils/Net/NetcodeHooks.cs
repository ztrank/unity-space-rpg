namespace SpaceRpg.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;

    public class NetcodeHooks : NetworkBehaviour
    {
        public event Action OnNetworkSpawnHook;
        public event Action OnNetworkDespawnHook;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            this.OnNetworkSpawnHook?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            this.OnNetworkDespawnHook?.Invoke();
        }
    }
}