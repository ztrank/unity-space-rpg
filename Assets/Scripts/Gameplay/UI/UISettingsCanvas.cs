namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UISettingsCanvas : MonoBehaviour
    {
        [SerializeField] GameObject m_QuitPanelRoot;

        void Awake()
        {
            this.DisablePanels();
        }

        void DisablePanels()
        {
            this.m_QuitPanelRoot.SetActive(false);
        }

        public void OnClickQuitButton()
        {
            this.m_QuitPanelRoot.SetActive(!this.m_QuitPanelRoot.activeSelf);
        }
    }
}