namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.ConnectionManagement;
    using SpaceRpg.Infrastructure;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;

    public class UIMessageFeed : MonoBehaviour
    {
        [SerializeField] List<UIMessageSlot> m_MessageSlots;

        [SerializeField] GameObject m_MessageSlotPrefab;

        [SerializeField] VerticalLayoutGroup m_VerticalLayoutGroup;

        DisposableGroup m_Subscriptions;

        [Inject]
        void InjectDependencies(
            ISubscriber<ConnectionEventMessage> connectionEventSubscriber)
        {
            this.m_Subscriptions = new DisposableGroup();
            this.m_Subscriptions.Add(connectionEventSubscriber.Subscribe(this.OnConnectionEvent));
            
        }

        void OnConnectionEvent(ConnectionEventMessage message)
        {
            switch (message.ConnectStatus)
            {
                case ConnectStatus.Success:
                    this.DisplayMessage($"{message.PlayerName} has joined the game!");
                    break;
                case ConnectStatus.ServerFull:
                case ConnectStatus.LoggedInAgain:
                case ConnectStatus.UserRequestedDisconnect:
                case ConnectStatus.GenericDisconnect:
                case ConnectStatus.IncompatibleBuildType:
                case ConnectStatus.HostEndedSession:
                    this.DisplayMessage($"{message.PlayerName} has left the game!");
                    break;
            }
        }

        void DisplayMessage(string text)
        {
            var messageSlot = this.GetAvailableSlot();
            messageSlot.Display(text);
        }

        UIMessageSlot GetAvailableSlot()
        {
            foreach (var slot in this.m_MessageSlots)
            {
                if (!slot.IsDisplaying)
                {
                    return slot;
                }
            }

            var go = Instantiate(this.m_MessageSlotPrefab, this.m_VerticalLayoutGroup.transform);
            var messageSlot = go.GetComponentInChildren<UIMessageSlot>();
            this.m_MessageSlots.Add(messageSlot);
            return messageSlot;
        }
        // Start is called before the first frame update
        void OnDestroy()
        {
            this.m_Subscriptions.Dispose();
        }
    }
}