namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class PopupPanel : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI m_TitleText;

        [SerializeField]
        TextMeshProUGUI m_MainText;

        [SerializeField]
        GameObject m_ConfirmButton;

        [SerializeField]
        GameObject m_LoadingSpinner;

        [SerializeField]
        CanvasGroup m_CanvasGroup;

        private bool m_IsDisplaying;

        private bool m_ClosableByUser;

        public bool IsDisplaying => this.m_IsDisplaying;

        void Awake()
        {
            this.Hide();
        }

        public void OnConfirmClick()
        {
            if (this.m_ClosableByUser)
            {
                this.Hide();
            }
        }

        public void SetupPopupPanel(string titleText, string mainText, bool closeableByUser = true)
        {
            this.m_TitleText.text = titleText;
            this.m_MainText.text = mainText;
            this.m_ClosableByUser = closeableByUser;
            this.m_ConfirmButton.SetActive(closeableByUser);
            this.m_LoadingSpinner.SetActive(!m_ClosableByUser);
            this.Show();
        }

        public void Show()
        {
            this.m_CanvasGroup.alpha = 1f;
            this.m_CanvasGroup.blocksRaycasts = true;
            this.m_IsDisplaying = true;
        }

        public void Hide()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.blocksRaycasts = false;
            this.m_IsDisplaying = false;
        }
    }
}