namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class RuntimeCollection<T> : ScriptableObject
    {
        public List<T> Items = new List<T>();
        public event Action<T> ItemAdded;
        public event Action<T> ItemRemoved;

        public void Add(T item)
        {
            if (!this.Items.Contains(item))
            {
                this.Items.Add(item);
                this.ItemAdded?.Invoke(item);
            }
        }

        public void Remove(T item)
        {
            if (this.Items.Contains(item))
            {
                this.Items.Remove(item);
                this.ItemRemoved?.Invoke(item);
            }
        }
    }
}