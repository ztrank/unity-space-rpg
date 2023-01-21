namespace SpaceRpg.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class EditorChildSceneLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        public List<SceneAsset> ChildScenesToLoadConfig;

        // Update is called once per frame
        void Update()
        {
            // DO NOT DELETE
        }

        public void SaveSceneSetup()
        {
            this.ChildScenesToLoadConfig ??= new List<SceneAsset>();
            this.ChildScenesToLoadConfig.Clear();

            foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
            {
                this.ChildScenesToLoadConfig.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneSetup.path));
            }
        }

        public void ResetSceneSetupToConfig()
        {
            var sceneAssetsToLoad = this.ChildScenesToLoadConfig;

            List<SceneSetup> sceneSetupToLoad = new List<SceneSetup>();

            foreach (var sceneAsset in sceneAssetsToLoad)
            {
                sceneSetupToLoad.Add(new SceneSetup()
                {
                    path = AssetDatabase.GetAssetPath(sceneAsset),
                    isActive = false,
                    isLoaded = true
                });
            }

            sceneSetupToLoad[0].isActive = true;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetupToLoad.ToArray());
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EditorChildSceneLoader))]
    public class ChildSceneLoaderInspectorGUI : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var currentInspectorObject = (EditorChildSceneLoader)this.target;

            if (GUILayout.Button("Save scene setup to config"))
            {
                currentInspectorObject.SaveSceneSetup();
            }

            if (GUILayout.Button("Reset scene setup from config..."))
            {
                currentInspectorObject.ResetSceneSetupToConfig();
            }
        }
    }

    [InitializeOnLoad]
    public class ChildSceneLoader
    {
        static ChildSceneLoader()
        {
            EditorSceneManager.sceneOpened += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene _, OpenSceneMode mode)
        {
            if (mode != OpenSceneMode.Single || BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            var sceneToLoadObjects = GameObject.FindObjectsOfType<EditorChildSceneLoader>();
            if (sceneToLoadObjects.Length > 1)
            {
                throw new Exception("Should only have one root scene at once loaded");
            }

            if (sceneToLoadObjects.Length == 0 || !sceneToLoadObjects[0].enabled)
            {
                return;
            }

            sceneToLoadObjects[0].ResetSceneSetupToConfig();

            Debug.Log("Setup done for root scene and child scenes");
        }
    }
#endif
}