namespace SpaceRpg.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Events;

    public class UnityEventGameEventListener : MonoBehaviour, IGameEventListenable
    {
        [SerializeField]
        GameEvent m_GameEvent;

        [SerializeField]
        UnityEvent m_Response;

        public GameEvent GameEvent
        {
            get => this.m_GameEvent;
            set => this.m_GameEvent = value;
        }

        void OnEnable()
        {
            Assert.IsNotNull(this.GameEvent, "Assign this GameEvent within the Editor!");
            this.GameEvent.RegisterListener(this);
        }

        void OnDisable()
        {
            this.GameEvent.DeregisterListener(this);
        }

        public void EventRaised()
        {
            this.m_Response.Invoke();
        }
    }
}