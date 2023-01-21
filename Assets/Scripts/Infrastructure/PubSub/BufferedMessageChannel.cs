namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BufferedMessageChannel<T> : MessageChannel<T>, IBufferedMessageChannel<T>
    {
        public bool HasBufferedMessage { get; private set; } = false;
        public T BufferedMessage { get; private set; }

        public override void Publish(T message)
        {
            this.HasBufferedMessage = true;
            this.BufferedMessage = message;
            base.Publish(message);
        }

        public override IDisposable Subscribe(Action<T> handler)
        {
            IDisposable subscription = base.Subscribe(handler);
            
            if (this.HasBufferedMessage)
            {
                handler?.Invoke(this.BufferedMessage);
            }

            return subscription;
        }
    }

}
