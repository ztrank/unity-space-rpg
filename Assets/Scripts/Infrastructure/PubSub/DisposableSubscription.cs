
namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DisposableSubscription<T> : IDisposable
    {
        Action<T> m_Handler;
        bool m_IsDisposed;
        IMessageChannel<T> m_MessageChannel;

        public DisposableSubscription(IMessageChannel<T> messageChannel, Action<T> handler)
        {
            this.m_Handler = handler;
            this.m_MessageChannel = messageChannel;
        }

        public void Dispose()
        {
            if (!this.m_IsDisposed)
            {
                this.m_IsDisposed = true;
                if (!m_MessageChannel.IsDisposed)
                {
                    this.m_MessageChannel.Unsubscribe(this.m_Handler);
                }

                this.m_Handler = null;
                this.m_MessageChannel = null;
            }
        }
    }

}
