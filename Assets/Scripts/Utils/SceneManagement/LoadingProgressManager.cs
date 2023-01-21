namespace SpaceRpg.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;

    public class LoadingProgressManager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject m_ProgressTrackerPrefab;

        private AsyncOperation m_LocalLoadOperation;
        private float m_LocalProgress;

        public event Action OnTrackersUpdated;

        public Dictionary<ulong, NetworkedLoadingProgressTracker> ProgressTrackers { get; } = new Dictionary<ulong, NetworkedLoadingProgressTracker>();

        public float LocalProgress
        {
            get => this.IsSpawned && this.ProgressTrackers.ContainsKey(NetworkManager.LocalClientId) ? this.ProgressTrackers[NetworkManager.LocalClientId].Progress.Value : this.m_LocalProgress;
            set
            {
                if (this.IsSpawned && this.ProgressTrackers.ContainsKey(NetworkManager.LocalClientId))
                {
                    this.ProgressTrackers[NetworkManager.LocalClientId].Progress.Value = value;
                }
                else
                {
                    this.m_LocalProgress = value;
                }
            }
        }

        public AsyncOperation LocalLoadOperation
        {
            set
            {
                this.LocalProgress = 0;
                this.m_LocalLoadOperation = value;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (this.IsServer)
            {
                this.NetworkManager.OnClientConnectedCallback += this.AddTracker;
                this.NetworkManager.OnClientDisconnectCallback += this.RemoveTracker;
                this.AddTracker(this.NetworkManager.LocalClientId);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (this.IsServer)
            {
                this.NetworkManager.OnClientConnectedCallback -= this.AddTracker;
                this.NetworkManager.OnClientDisconnectCallback -= this.RemoveTracker;
            }

            this.ProgressTrackers.Clear();
            this.OnTrackersUpdated?.Invoke();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.m_LocalLoadOperation != null)
            {
                this.LocalProgress = this.m_LocalLoadOperation.isDone ? 1 : this.m_LocalLoadOperation.progress;
            }
        }

        [ClientRpc]
        void UpdateTrackersClientRpc()
        {
            if (!this.IsHost)
            {
                this.ProgressTrackers.Clear();
                foreach(var tracker in FindObjectsOfType<NetworkedLoadingProgressTracker>())
                {
                    if (tracker.IsSpawned)
                    {
                        this.ProgressTrackers[tracker.OwnerClientId] = tracker;

                        if (tracker.OwnerClientId == this.NetworkManager.LocalClientId)
                        {
                            this.LocalProgress = Mathf.Max(this.m_LocalProgress, this.LocalProgress);
                        }
                    }
                }
            }

            this.OnTrackersUpdated?.Invoke();
        }

        void AddTracker(ulong clientId)
        {
            if (this.IsServer)
            {
                var tracker = Instantiate(this.m_ProgressTrackerPrefab);
                var networkObject = tracker.GetComponent<NetworkObject>();
                networkObject.SpawnWithOwnership(clientId);
                this.ProgressTrackers[clientId] = tracker.GetComponent<NetworkedLoadingProgressTracker>();
                this.UpdateTrackersClientRpc();
            }
        }

        void RemoveTracker(ulong clientId)
        {
            if (this.IsServer)
            {
                if(this.ProgressTrackers.ContainsKey(clientId))
                {
                    var tracker = this.ProgressTrackers[clientId];
                    this.ProgressTrackers.Remove(clientId);
                    tracker.NetworkObject.Despawn();
                    this.UpdateTrackersClientRpc();
                }
            }
        }

        public void ResetLocalProgress()
        {
            this.LocalProgress = 0;
        }
    }
}