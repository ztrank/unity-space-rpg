namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneLoaderWrapper : NetworkBehaviour
    {
        [SerializeField]
        private ClientLoadingScreen m_ClientLoadingScreen;

        [SerializeField]
        LoadingProgressManager m_LoadingProgressManager;

        bool IsNetworkSceneManagementEnabled => this.NetworkManager != null && this.NetworkManager.SceneManager != null && this.NetworkManager.NetworkConfig.EnableSceneManagement;

        public static SceneLoaderWrapper Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(this);
        }

        public virtual void Start()
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= this.OnSceneLoaded;
        }

        public override void OnNetworkDespawn()
        {
            if (this.NetworkManager != null && this.NetworkManager.SceneManager != null)
            {
                this.NetworkManager.SceneManager.OnSceneEvent -= this.OnSceneEvent;
            }
        }

        public virtual void AddOnSceneEventCallback()
        {
            if (this.IsNetworkSceneManagementEnabled)
            {
                this.NetworkManager.SceneManager.OnSceneEvent += this.OnSceneEvent;
            }
        }

        public virtual void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (useNetworkSceneManager)
            {
                if (this.IsSpawned && this.IsNetworkSceneManagementEnabled && !this.NetworkManager.ShutdownInProgress)
                {
                    if (this.NetworkManager.IsServer)
                    {
                        this.NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                    }
                }
            }
            else
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if (loadSceneMode == LoadSceneMode.Single)
                {
                    this.m_ClientLoadingScreen.StartLoadingScreen(sceneName);
                    this.m_LoadingProgressManager.LocalLoadOperation = loadOperation;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!this.IsSpawned && !this.NetworkManager.ShutdownInProgress)
            {
                this.m_ClientLoadingScreen.StopLoadingScreen();
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    if (this.NetworkManager.IsClient)
                    {
                        if (sceneEvent.LoadSceneMode == LoadSceneMode.Single)
                        {
                            this.m_ClientLoadingScreen.StartLoadingScreen(sceneEvent.SceneName);
                            this.m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                        else
                        {
                            this.m_ClientLoadingScreen.UpdateLoadingScreen(sceneEvent.SceneName);
                            this.m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                    }
                    break;
                case SceneEventType.LoadEventCompleted:
                    if (this.NetworkManager.IsClient)
                    {
                        this.m_ClientLoadingScreen.StopLoadingScreen();
                        this.m_LoadingProgressManager.ResetLocalProgress();
                    }
                    break;
                case SceneEventType.Synchronize:
                    if (this.NetworkManager.IsClient && !this.NetworkManager.IsHost)
                    {
                        this.UnloadAdditiveScenes();
                    }
                    break;
                case SceneEventType.SynchronizeComplete:
                    if (this.NetworkManager.IsServer)
                    {
                        this.StopLoadingScreenClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { sceneEvent.ClientId } } });
                    }
                    break;
            }
        }

        void UnloadAdditiveScenes()
        {
            var activeScene = SceneManager.GetActiveScene();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != activeScene)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [ClientRpc]
        void StopLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
        {
            this.m_ClientLoadingScreen.StopLoadingScreen();
        }
    }
}