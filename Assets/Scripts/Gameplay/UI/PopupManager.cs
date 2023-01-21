namespace SpaceRpg.Gameplay.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PopupManager : MonoBehaviour
    {
        [SerializeField]
        GameObject m_PopupPanelPrefab;

        [SerializeField]
        GameObject m_Canvas;

        List<PopupPanel> m_PopupPanels = new List<PopupPanel>();

        static PopupManager s_instance;

        const float k_Offset = 30;
        const float k_MaxOffset = 200;

        void Awake()
        {
            if (s_instance != null)
            {
                throw new Exception("Invalid state, instance is not null");
            }

            s_instance = this;
            DontDestroyOnLoad(this.m_Canvas);
        }

        private void OnDestroy()
        {
            s_instance = null;
        }

        public static PopupPanel ShowPopupPanel(string titleText, string mainText, bool closableByPlayer = true)
        {
            if (s_instance != null)
            {
                return s_instance.DisplayPopupPanel(titleText, mainText, closableByPlayer);
            }


            Debug.LogError($"No PopupPanel instance found. Cannot display message: {titleText}: {mainText}");
            return null;
        }

        PopupPanel DisplayPopupPanel(string titleText, string mainText, bool closableByPlayer)
        {
            PopupPanel popup = this.GetNextAvailablePopupPanel();
            if (popup != null)
            {
                popup.SetupPopupPanel(titleText, mainText, closableByPlayer);
            }

            return popup;
        }

        PopupPanel GetNextAvailablePopupPanel()
        {
            int nextAvailablePopupIndex = 0;

            for(int i = 0; i <this.m_PopupPanels.Count; i++)
            {
                if (this.m_PopupPanels[i].IsDisplaying)
                {
                    nextAvailablePopupIndex = i + 1;
                }
            }

            if (nextAvailablePopupIndex < this.m_PopupPanels.Count)
            {
                return m_PopupPanels[nextAvailablePopupIndex];
            }

            var popupGameObject = Instantiate(this.m_PopupPanelPrefab, this.gameObject.transform);
            popupGameObject.transform.position += new Vector3(1, -1) * (k_Offset * this.m_PopupPanels.Count % k_MaxOffset);
            var popupPanel = popupGameObject.GetComponent<PopupPanel>();

            if (popupPanel != null)
            {
                this.m_PopupPanels.Add(popupPanel);
            }
            else
            {
                Debug.LogError("PopupPanel prefab does not have a PopupPanel component!");
            }

            return popupPanel;
        }
    }
}