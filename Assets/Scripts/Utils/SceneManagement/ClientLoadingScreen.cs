namespace SpaceRpg.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.UI;

    public class ClientLoadingScreen : MonoBehaviour
    {
        protected class LoadingProgressBar
        {
            public Slider ProgressBar { get; set; }
            public Text NameText { get; set; }

            public LoadingProgressBar(Slider progressBar, Text nameText)
            {
                this.ProgressBar = progressBar;
                this.NameText = nameText;
            }

            public void UpdateProgress(float value, float newValue)
            {
                this.ProgressBar.value = newValue;
            }
        }

        [SerializeField]
        private CanvasGroup m_CanvasGroup;

        [SerializeField]
        private float m_DelayBeforeFadeOut = 0.5f;

        [SerializeField]
        float m_FadeOutDuration = 0.1f;

        [SerializeField]
        Slider m_ProgressBar;

        [SerializeField]
        Text m_SceneName;

        [SerializeField]
        List<Slider> m_OtherPlayersProgressBars;

        [SerializeField]
        List<Text> m_OtherPlayerNamesText;

        [SerializeField]
        protected LoadingProgressManager m_LoadingProgressManager;

        protected Dictionary<ulong, LoadingProgressBar> m_LoadingProgressBars = new Dictionary<ulong, LoadingProgressBar>();

        bool m_LoadingScreenRunning;

        Coroutine m_FadeOutCoroutine;

        void Awake()
        {
            DontDestroyOnLoad(this);
            Assert.AreEqual(this.m_OtherPlayerNamesText.Count, this.m_OtherPlayersProgressBars.Count, "There should be the same number of progress bars and name labels");
        }

        // Start is called before the first frame update
        void Start()
        {
            this.SetCanvasVisibility(false);
            this.m_LoadingProgressManager.OnTrackersUpdated += this.OnProgressTrackersUpdated;
        }

        private void OnDestroy()
        {
            this.m_LoadingProgressManager.OnTrackersUpdated -= this.OnProgressTrackersUpdated;
        }

        // Update is called once per frame
        void Update()
        {
            if (this.m_LoadingScreenRunning)
            {
                this.m_ProgressBar.value = this.m_LoadingProgressManager.LocalProgress;
            }
        }

        void OnProgressTrackersUpdated()
        {

            foreach(var clientId in this.m_LoadingProgressBars.Keys)
            {
                if (this.m_LoadingProgressManager.ProgressTrackers.ContainsKey(clientId))
                {
                    this.RemoveOtherPlayerProgressBar(clientId);
                }
            }

            for (var i = 0; i < this.m_OtherPlayersProgressBars.Count; i++)
            {
                this.m_OtherPlayersProgressBars[i].gameObject.SetActive(false);
                this.m_OtherPlayerNamesText[i].gameObject.SetActive(false);
            }

            foreach( var progressTracker in this.m_LoadingProgressManager.ProgressTrackers)
            {
                var clientId = progressTracker.Key;
                if (clientId != NetworkManager.Singleton.LocalClientId && !m_LoadingProgressBars.ContainsKey(clientId))
                {
                    this.AddOtherPlayerProgressBar(clientId, progressTracker.Value);
                }
            }
        }

        public void StopLoadingScreen()
        {
            if (this.m_LoadingScreenRunning)
            {
                if (this.m_FadeOutCoroutine != null)
                {
                    this.StopCoroutine(this.m_FadeOutCoroutine);
                }
                this.m_FadeOutCoroutine = StartCoroutine(this.FadeOutCoroutine());
            }
        }

        public void StartLoadingScreen(string sceneName)
        {
            this.SetCanvasVisibility(true);
            this.m_LoadingScreenRunning = true;
            this.UpdateLoadingScreen(sceneName);
            this.ReinitializeProgressBars();
        }
        void ReinitializeProgressBars()
        {
            // deactivate progress bars of clients that are no longer tracked
            foreach (var clientId in this.m_LoadingProgressBars.Keys)
            {
                if (!this.m_LoadingProgressManager.ProgressTrackers.ContainsKey(clientId))
                {
                    this.RemoveOtherPlayerProgressBar(clientId);
                }
            }

            for (var i = 0; i < this.m_OtherPlayersProgressBars.Count; i++)
            {
                this.m_OtherPlayersProgressBars[i].gameObject.SetActive(false);
                this.m_OtherPlayerNamesText[i].gameObject.SetActive(false);
            }

            var index = 0;

            foreach (var progressTracker in this.m_LoadingProgressManager.ProgressTrackers)
            {
                var clientId = progressTracker.Key;
                if (clientId != NetworkManager.Singleton.LocalClientId)
                {
                    this.UpdateOtherPlayerProgressBar(clientId, index++);
                }
            }
        }

        protected virtual void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            this.m_LoadingProgressBars[clientId].ProgressBar = this.m_OtherPlayersProgressBars[progressBarIndex];
            this.m_LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
            this.m_LoadingProgressBars[clientId].NameText = this.m_OtherPlayerNamesText[progressBarIndex];
            this.m_LoadingProgressBars[clientId].NameText.gameObject.SetActive(true);
        }


        protected virtual void AddOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker)
        {
            if (this.m_LoadingProgressBars.Count < this.m_OtherPlayersProgressBars.Count && this.m_LoadingProgressBars.Count < this.m_OtherPlayerNamesText.Count)
            {
                var index = this.m_LoadingProgressBars.Count;
                this.m_LoadingProgressBars[clientId] = new LoadingProgressBar(this.m_OtherPlayersProgressBars[index], this.m_OtherPlayerNamesText[index]);
                progressTracker.Progress.OnValueChanged += this.m_LoadingProgressBars[clientId].UpdateProgress;
                this.m_LoadingProgressBars[clientId].ProgressBar.value = progressTracker.Progress.Value;
                this.m_LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
                this.m_LoadingProgressBars[clientId].NameText.gameObject.SetActive(true);
                this.m_LoadingProgressBars[clientId].NameText.text = $"Client {clientId}";
            }
            else
            {
                throw new Exception("There are not enough progress bars to track the progress of all the players.");
            }
        }

        void RemoveOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker = null)
        {
            if (progressTracker != null)
            {
                progressTracker.Progress.OnValueChanged -= this.m_LoadingProgressBars[clientId].UpdateProgress;
            }
            this.m_LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(false);
            this.m_LoadingProgressBars[clientId].NameText.gameObject.SetActive(false);
            this.m_LoadingProgressBars.Remove(clientId);
        }

        public void UpdateLoadingScreen(string sceneName)
        {
            if (this.m_LoadingScreenRunning)
            {
                this.m_SceneName.text = sceneName;
                if (this.m_FadeOutCoroutine != null)
                {
                    this.StopCoroutine(this.m_FadeOutCoroutine);
                }
            }
        }


        void SetCanvasVisibility(bool visible)
        {
            this.m_CanvasGroup.alpha = visible ? 1 : 0;
            this.m_CanvasGroup.blocksRaycasts = visible;
        }

        IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(this.m_DelayBeforeFadeOut);
            this.m_LoadingScreenRunning = false;

            float currentTime = 0;
            while (currentTime < this.m_FadeOutDuration)
            {
                this.m_CanvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / this.m_FadeOutDuration);
                yield return null;
                currentTime += Time.deltaTime;
            }

            this.SetCanvasVisibility(false);
        }
    }
}