

namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class EnableOrDisableColliderOnAwake : MonoBehaviour
    {
        [SerializeField]
        Collider m_Collider;

        [SerializeField]
        bool m_EnableStateOnAwake;

        void Awake()
        {
            this.m_Collider.enabled = this.m_EnableStateOnAwake;
        }
    }

}
