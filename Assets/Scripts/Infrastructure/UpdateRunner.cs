namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UpdateRunner : MonoBehaviour
    {
        class SubscriberData
        {
            public float Period;
            public float NextCallTime;
        }

        readonly Queue<Action> m_PendingHandlers = new Queue<Action>();
        readonly HashSet<Action<float>> m_Subscribers = new HashSet<Action<float>>();
        readonly Dictionary<Action<float>, SubscriberData> m_SubscriberData = new Dictionary<Action<float>, SubscriberData>();

        public void OnDestroy()
        {
            this.m_PendingHandlers.Clear();
            this.m_SubscriberData.Clear();
            this.m_Subscribers.Clear();
        }

        public void Subscribe(Action<float> onUpdate, float updatePeriod)
        {
            if (onUpdate == null)
            {
                return;
            }

            if (onUpdate.Target == null)
            {
                Debug.LogError("Can't subscribe to a local function that can go out of scope and can't be unsubscrbied from");
                return;
            }

            if (onUpdate.Method.ToString().Contains("<"))
            {
                Debug.LogError("Can't subscribe with an anonymous function that cnanot be Unsubscribed");
                return;
            }

            if (!this.m_Subscribers.Contains(onUpdate))
            {
                this.m_PendingHandlers.Enqueue(() =>
                {
                    if (this.m_Subscribers.Add(onUpdate))
                    {
                        this.m_SubscriberData.Add(onUpdate, new SubscriberData()
                        {
                            Period = updatePeriod,
                            NextCallTime = 0
                        });
                    }
                });
            }
        }

        public void Unsubscribe(Action<float> onUpdate)
        {
            this.m_PendingHandlers.Enqueue(() =>
            {
                this.m_Subscribers.Remove(onUpdate);
                this.m_SubscriberData.Remove(onUpdate);
            });
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            while (this.m_PendingHandlers.Count > 0)
            {
                this.m_PendingHandlers.Dequeue()?.Invoke();
            }

            foreach(var subscriber in this.m_Subscribers)
            {
                var subscriberData = this.m_SubscriberData[subscriber];

                if (Time.time >= subscriberData.NextCallTime)
                {
                    subscriber.Invoke(Time.deltaTime);
                    subscriberData.NextCallTime = Time.time + subscriberData.Period;
                }
            }
        }
    }

}
