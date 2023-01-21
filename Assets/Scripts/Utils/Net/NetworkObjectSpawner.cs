namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class NetworkObjectSpawner : MonoBehaviour
    {
        public NetworkObject prefabReference;

        public void Awake()
        {
            if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.SceneManager != null) 
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += this.SceneManagerOnOnLoadEventCompleted;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        
        void OnDestroy()
        {
            if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= this.SceneManagerOnOnLoadEventCompleted;
            }
        }

        void SceneManagerOnOnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOUt)
        {
            this.SpawnNetworkObject();
            Destroy(this.gameObject);
        }

        void SpawnNetworkObject()
        {
            var instantiatedNetworkObject = Instantiate(this.prefabReference, this.transform.position, this.transform.rotation, null);

            SceneManager.MoveGameObjectToScene(instantiatedNetworkObject.gameObject, SceneManager.GetSceneByName(this.gameObject.scene.name));

            instantiatedNetworkObject.transform.localScale = this.transform.lossyScale;
            instantiatedNetworkObject.Spawn(destroyWithScene: true);
        }
    }
}