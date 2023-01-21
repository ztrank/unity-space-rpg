namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class MessageChannel<T> : IMessageChannel<T>
    {
        readonly List<Action<T>> m_MessageHandlers = new List<Action<T>>();

        readonly Dictionary<Action<T>, bool> m_PendingHandlers = new Dictionary<Action<T>, bool>();

        public bool IsDisposed { get; private set; } = false;

        public virtual void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                this.m_MessageHandlers.Clear();
                this.m_PendingHandlers.Clear();
            }
        }

        public virtual void Publish(T message)
        {
            foreach (var handler in this.m_PendingHandlers.Keys)
            {
                if (this.m_PendingHandlers[handler])
                {
                    this.m_MessageHandlers.Add(handler);
                }
                else
                {
                    this.m_MessageHandlers.Remove(handler);
                }
            }

            this.m_PendingHandlers.Clear();

            foreach(var handler in this.m_MessageHandlers)
            {
                handler?.Invoke(message);
            }
        }

        public virtual IDisposable Subscribe(Action<T> handler)
        {
            Assert.IsTrue(!this.IsSubscribed(handler), "Attempted to subscribe with the same handler more than once");

            if (this.m_PendingHandlers.ContainsKey(handler))
            {
                if (!this.m_PendingHandlers[handler])
                {
                    this.m_PendingHandlers.Remove(handler);
                }
            }
            else
            {
                this.m_PendingHandlers[handler] = true;
            }

            var subscription = new DisposableSubscription<T>(this, handler);
            return subscription;
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (this.IsSubscribed(handler))
            {
                if (this.m_PendingHandlers.ContainsKey(handler))
                {
                    if (this.m_PendingHandlers[handler])
                    {
                        this.m_PendingHandlers.Remove(handler);
                    }
                }
                else
                {
                    this.m_PendingHandlers[handler] = false;
                }
            }
        }

        bool IsSubscribed(Action<T> handler)
        {
            var isPendingRemoval = this.m_PendingHandlers.ContainsKey(handler) && !this.m_PendingHandlers[handler];
            var isPendingAdding = this.m_PendingHandlers.ContainsKey(handler) && this.m_PendingHandlers[handler];
            return this.m_MessageHandlers.Contains(handler) && !isPendingRemoval || isPendingAdding;
        }
    }
}
