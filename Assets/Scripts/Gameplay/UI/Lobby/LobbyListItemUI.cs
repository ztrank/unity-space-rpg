namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.UnityServices.Lobbies;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class LobbyListItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_LobbyNameText;
        [SerializeField] TextMeshProUGUI m_LobbyCountText;
        [Inject] LobbyUIMediator m_LobbyUIMediator;

        LocalLobby m_data;

        public void SetData(LocalLobby data)
        {
            this.m_data = data;
            this.m_LobbyNameText.SetText(data.LobbyName);
            this.m_LobbyCountText.SetText($"{data.PlayerCount}/{data.MaxPlayerCount}");
        }

        public void OnClick()
        {
            this.m_LobbyUIMediator.JoinLobbyRequest(this.m_data);
        }
    }
}