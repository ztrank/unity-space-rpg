namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class ServerAdditiveSceneLoader : NetworkBehaviour
    {
        [SerializeField]
        private float m_DelayBeforeUnload = 5.0f;

        [SerializeField]
        private string m_SceneName;

        [SerializeField]
        private string m_PlayerTag;

        private List<ulong> m_PlayersInTrigger;

        private bool IsActive => this.IsServer && this.IsSpawned;

        private enum SceneState
        {
            Loaded,
            Unloaded,
            Loading,
            Unloading,
            WaitingToUnload
        }

        private SceneState m_SceneState = SceneState.Unloaded;

        private Coroutine m_UnloadCoroutine;

        public override void OnNetworkSpawn()
        {
            if (this.IsServer)
            {
                this.NetworkManager.OnClientDisconnectCallback += this.RemovePlayer;
                this.NetworkManager.SceneManager.OnSceneEvent += this.OnSceneEvent;

                this.m_PlayersInTrigger = new List<ulong>();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (this.IsServer)
            {
                this.NetworkManager.OnClientDisconnectCallback -= this.RemovePlayer;
                this.NetworkManager.SceneManager.OnSceneEvent -= this.OnSceneEvent;
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted && sceneEvent.SceneName == this.m_SceneName)
            {
                this.m_SceneState = SceneState.Loaded;
            }
            else if (sceneEvent.SceneEventType == SceneEventType.UnloadEventCompleted && sceneEvent.SceneName == this.m_SceneName)
            {
                this.m_SceneState = SceneState.Unloaded;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (this.IsActive)
            {
                if (other.CompareTag(this.m_PlayerTag) && other.TryGetComponent(out NetworkObject networkObject))
                {
                    this.m_PlayersInTrigger.Add(networkObject.OwnerClientId);

                    if (this.m_UnloadCoroutine != null)
                    {
                        this.StopCoroutine(this.m_UnloadCoroutine);

                        if (this.m_SceneState == SceneState.WaitingToUnload)
                        {
                            this.m_SceneState = SceneState.Loaded;
                        }
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (this.IsActive)
            {
                if (other.CompareTag(this.m_PlayerTag) && other.TryGetComponent(out NetworkObject networkObject))
                {
                    this.m_PlayersInTrigger.Remove(networkObject.OwnerClientId);
                }
            }
        }

        void FixedUpdate()
        {
            if (this.IsActive)
            {
                if (this.m_SceneState == SceneState.Unloaded && this.m_PlayersInTrigger.Count > 0)
                {
                    var status = this.NetworkManager.SceneManager.LoadScene(this.m_SceneName, LoadSceneMode.Additive);

                    if (status == SceneEventProgressStatus.Started)
                    {
                        this.m_SceneState = SceneState.Loading;
                    }
                }
                else if (this.m_SceneState == SceneState.Loaded && this.m_PlayersInTrigger.Count == 0)
                {
                    this.m_UnloadCoroutine = this.StartCoroutine(this.WaitToUnloadCoroutine());
                    this.m_SceneState = SceneState.WaitingToUnload;
                }
            }
        }

        void RemovePlayer(ulong clientId)
        {
            while (this.m_PlayersInTrigger.Remove(clientId)) { }
        }

        IEnumerator WaitToUnloadCoroutine()
        {
            yield return new WaitForSeconds(this.m_DelayBeforeUnload);
            Scene scene = SceneManager.GetSceneByName(this.m_SceneName);

            if (scene.isLoaded)
            {
                var status = this.NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(this.m_SceneName));

                this.m_SceneState = status == SceneEventProgressStatus.Started ? SceneState.Unloading : SceneState.Loaded;
            }
        }
    }
}