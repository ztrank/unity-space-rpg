namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.UnityServices.Lobbies;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;

    public class LobbyNameBox : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI m_JoinCodeText;

        [SerializeField]
        Button m_CopyToClipboardButton;

        LocalLobby m_LocalLobby;
        string m_LobbyCode;

        [Inject]
        private void InjectDependencies(LocalLobby localLobby)
        {
            this.m_LocalLobby = localLobby;
            this.m_LocalLobby.changed += this.UpdateUI;
            this.UpdateUI(localLobby);
        }

        void Awake()
        {
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            this.m_LocalLobby.changed -= this.UpdateUI;
        }

        void UpdateUI(LocalLobby localLobby)
        {
            if (!string.IsNullOrEmpty(localLobby.LobbyCode))
            {
                this.m_LobbyCode = localLobby.LobbyCode;
                this.m_JoinCodeText.text = $"Lobby Code: {this.m_LobbyCode}";
                this.gameObject.SetActive(true);
                this.m_CopyToClipboardButton.gameObject.SetActive(true);
            }
        }

        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = this.m_LobbyCode;
        }
    }
}