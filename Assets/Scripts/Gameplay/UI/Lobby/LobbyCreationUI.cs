namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;

    public class LobbyCreationUI : MonoBehaviour
    {
        [SerializeField] InputField m_LobbyNameInputField;
        [SerializeField] GameObject m_LoadingIndicatorObject;
        [SerializeField] Toggle m_IsPrivate;
        [SerializeField] InputField m_NumberOfPlayersField;
        [SerializeField] CanvasGroup m_CanvasGroup;
        [Inject] LobbyUIMediator m_LobbyUIMediator;

        void Awake()
        {
            this.EnableUnityRelayUI();
        }

        void EnableUnityRelayUI()
        {
            this.m_LoadingIndicatorObject.SetActive(false);
        }

        public void OnCreateClick()
        {
            if (int.TryParse(this.m_NumberOfPlayersField.text, out int slots))
            {
                this.m_LobbyUIMediator.CreateLobbyRequest(this.m_LobbyNameInputField.text, this.m_IsPrivate.isOn, slots);
            }
        }

        public void EnsureIntPlayerNumber()
        {
            this.m_NumberOfPlayersField.text = Regex.Replace(this.m_NumberOfPlayersField.text, "[^0-9]", "");
        }

        public void Show()
        {
            this.m_CanvasGroup.alpha = 1f;
            this.m_CanvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.blocksRaycasts = false;
        }
    }
}