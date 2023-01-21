namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DisposableGroup : IDisposable
    {
        readonly List<IDisposable> m_Disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach(var disposable in m_Disposables)
            {
                disposable.Dispose();
            }

            this.m_Disposables.Clear();
        }

        public void Add(IDisposable disposable)
        {
            this.m_Disposables.Add(disposable);
        }
    }
}