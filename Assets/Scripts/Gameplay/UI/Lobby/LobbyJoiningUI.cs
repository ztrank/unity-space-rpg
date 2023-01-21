namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.Infrastructure;
    using SpaceRpg.UnityServices.Lobbies;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;

    public class LobbyJoiningUI : MonoBehaviour
    {
        [SerializeField] LobbyListItemUI m_LobbyListItemPrototype;
        [SerializeField] InputField m_JoinCodeField;
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] Graphic m_EmptyLobbyListLabel;
        [SerializeField] Button m_JoinLobbyButton;

        IObjectResolver m_Container;
        LobbyUIMediator m_LobbyUIMediator;
        UpdateRunner m_UpdateRunner;
        ISubscriber<LobbyListFetchedMessage> m_LocalLobbiesRefreshedSub;

        List<LobbyListItemUI> m_LobbyListItems = new List<LobbyListItemUI>();

        private void Awake()
        {
            this.m_LobbyListItemPrototype.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (this.m_UpdateRunner != null)
            {
                this.m_UpdateRunner?.Unsubscribe(this.PeriodicRefresh);
            }
        }

        private void OnDestroy()
        {
            this.m_LocalLobbiesRefreshedSub?.Unsubscribe(this.UpdateUI);
        }

        [Inject]
        void InjectDependenciesAndInitialize(
            IObjectResolver container,
            LobbyUIMediator lobbyUiMediator,
            UpdateRunner updateRunner,
            ISubscriber<LobbyListFetchedMessage> refreshSub)
        {
            this.m_Container = container;
            this.m_LobbyUIMediator = lobbyUiMediator;
            this.m_UpdateRunner = updateRunner;
            this.m_LocalLobbiesRefreshedSub = refreshSub;
            this.m_LocalLobbiesRefreshedSub.Subscribe(this.UpdateUI);
        }

        public void OnJoinCodeInputTextChanged()
        {
            this.m_JoinCodeField.text = this.SanitizeJoinCode(this.m_JoinCodeField.text);
            this.m_JoinLobbyButton.interactable = this.m_JoinCodeField.text.Length > 0;
        }

        string SanitizeJoinCode(string dirtyString)
        {
            return Regex.Replace(dirtyString.ToUpper(), "[^A-Z0-9]", "");
        }

        public void OnJoinButtonPressed()
        {
            this.m_LobbyUIMediator.JoinLobbyWithCodeRequest(this.SanitizeJoinCode(this.m_JoinCodeField.text));
        }

        public void OnRefresh()
        {
            this.m_LobbyUIMediator.QueryLobbiesRequest(true);
        }

        private void UpdateUI(LobbyListFetchedMessage message)
        {
            this.EnsureNumberOfActiveUISlots(message.LocalLobbies.Count);

            for (var i = 0; i < message.LocalLobbies.Count; i++)
            {
                var localLobby = message.LocalLobbies[i];
                this.m_LobbyListItems[i].SetData(localLobby);
            }

            if (message.LocalLobbies.Count == 0)
            {
                this.m_EmptyLobbyListLabel.enabled = true;
            }
            else
            {
                this.m_EmptyLobbyListLabel.enabled = false;
            }
        }

        void EnsureNumberOfActiveUISlots(int requiredNumber)
        {
            int delta = requiredNumber - this.m_LobbyListItems.Count;

            for (int i = 0; i < delta; i++)
            {
                this.m_LobbyListItems.Add(this.CreateLobbyListItem());
            }
        }

        LobbyListItemUI CreateLobbyListItem()
        {
            var listItem = Instantiate(this.m_LobbyListItemPrototype.gameObject, this.m_LobbyListItemPrototype.transform.parent).GetComponent<LobbyListItemUI>();
            listItem.gameObject.SetActive(true);
            this.m_Container.Inject(listItem);
            return listItem;
        }

        public void Show()
        {
            this.m_CanvasGroup.alpha = 1f;
            this.m_CanvasGroup.blocksRaycasts = true;
            this.m_JoinCodeField.text = "";
            this.m_UpdateRunner.Subscribe(this.PeriodicRefresh, 10f);
        }

        public void Hide()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.blocksRaycasts = false;
            this.m_UpdateRunner.Unsubscribe(this.PeriodicRefresh);
        }

        void PeriodicRefresh(float _)
        {
            this.m_LobbyUIMediator.QueryLobbiesRequest(false);
        }
    }
}