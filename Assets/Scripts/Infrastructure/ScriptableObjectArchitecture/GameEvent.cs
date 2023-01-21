namespace SpaceRpg.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu]
    public class GameEvent : ScriptableObject
    {
        List<IGameEventListenable> m_Listeners = new List<IGameEventListenable>();

        public void Raise()
        {
            for(int i = this.m_Listeners.Count - 1; i >= 0; i--)
            {
                if (this.m_Listeners[i] == null)
                {
                    this.m_Listeners.RemoveAt(i);
                    continue;
                }

                this.m_Listeners[i].EventRaised();
            }
        }

        public void RegisterListener(IGameEventListenable listener)
        {
            for (int i = 0; i < this.m_Listeners.Count; i++)
            {
                if (this.m_Listeners[i] == listener)
                {
                    return;
                }
            }

            this.m_Listeners.Add(listener);
        }

        public void DeregisterListener(IGameEventListenable listener)
        {
            this.m_Listeners.Remove(listener);
        }
    }
}