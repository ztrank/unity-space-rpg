namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class NetworkObjectPool : NetworkBehaviour
    {
        private static NetworkObjectPool _instance;

        public static NetworkObjectPool Singleton { get => _instance; }

        [SerializeField]
        List<PoolConfigObject> PooledPrefabsList;

        HashSet<GameObject> prefabs = new HashSet<GameObject>();

        Dictionary<GameObject, Queue<NetworkObject>> pooledObjects = new Dictionary<GameObject, Queue<NetworkObject>>();

        private bool m_HasInitialized = false;

        public void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public override void OnNetworkSpawn()
        {
            this.InitializePool();
        }

        public override void OnNetworkDespawn()
        {
            this.ClearPool();
        }

        public void OnValidate()
        {
            for(int i = 0; i < this.PooledPrefabsList.Count; i++)
            {
                var prefab = this.PooledPrefabsList[i].Prefab;

                if (prefab != null)
                {
                    Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i} has not {nameof(this.NetworkObject)} component.");
                }
            }
        }

        public NetworkObject GetNetworkObject(GameObject prefab)
        {
            return this.GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
        }

        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return this.GetNetworkObjectInternal(prefab, position, rotation);
        }

        public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
        {
            var go = networkObject.gameObject;
            go.SetActive(false);
            this.pooledObjects[prefab].Enqueue(networkObject);
        }

        public void AddPrefab(GameObject prefab, int prewarmCount = 0)
        {
            var networkObject = prefab.GetComponent<NetworkObject>();

            Assert.IsNotNull(networkObject, $"{nameof(prefab)} must have {nameof(networkObject)} component");
            Assert.IsFalse(prefabs.Contains(prefab), $"Prefab {prefab.name} is already registered in the pool.");

            this.RegisterPrefabInternal(prefab, prewarmCount);
        }

        private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            this.prefabs.Add(prefab);

            Queue<NetworkObject> prefabQueue = new Queue<NetworkObject>();

            this.pooledObjects[prefab] = prefabQueue;

            for (int i = 0; i < prewarmCount; i++)
            {
                var go = this.CreateInstance(prefab);
                this.ReturnNetworkObject(go.GetComponent<NetworkObject>(), prefab);
            }

            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameObject CreateInstance(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        private NetworkObject GetNetworkObjectInternal(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var queue = this.pooledObjects[prefab];

            NetworkObject networkObject;
            if (queue.Count > 0)
            {
                networkObject = queue.Dequeue();
            }
            else
            {
                networkObject = this.CreateInstance(prefab).GetComponent<NetworkObject>();
            }

            GameObject go = networkObject.gameObject;
            go.SetActive(true);
            go.transform.position = position;
            go.transform.rotation = rotation;

            return networkObject;
        }

        public void InitializePool()
        {
            if (this.m_HasInitialized)
            {
                return;
            }

            foreach(var configObject in this.PooledPrefabsList)
            {
                this.RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
            }

            this.m_HasInitialized = true;
        }

        public void ClearPool()
        {
            foreach(var prefab in this.prefabs)
            {
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            }

            this.pooledObjects.Clear();
        }
    }

    [Serializable]
    struct PoolConfigObject
    {
        public GameObject Prefab;
        public int PrewarmCount;
    }

    class PooledPrefabInstanceHandler: INetworkPrefabInstanceHandler
    {
        GameObject m_Prefab;
        NetworkObjectPool m_Pool;

        public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
        {
            this.m_Pool = pool;
            this.m_Prefab = prefab;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var netObject = this.m_Pool.GetNetworkObject(this.m_Prefab, position, rotation);
            return netObject;
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            this.m_Pool.ReturnNetworkObject(networkObject, this.m_Prefab);
        }
    }
}