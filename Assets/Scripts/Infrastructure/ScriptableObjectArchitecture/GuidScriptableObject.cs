namespace SpaceRpg.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public abstract class GuidScriptableObject : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        byte[] m_Guid;

        public Guid Guid => new Guid(this.m_Guid);

        private void OnValidate()
        {
            if (this.m_Guid.Length == 0)
            {
                this.m_Guid = Guid.NewGuid().ToByteArray();
            }
        }
    }
}