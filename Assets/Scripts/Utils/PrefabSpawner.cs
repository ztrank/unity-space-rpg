
namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PrefabSpawner : MonoBehaviour
    {
        [SerializeField]
        bool m_UseLocalPosition;

        [SerializeField]
        bool m_UseLocalRotation;

        [SerializeField]
        GameObject m_Prefab;

        [SerializeField]
        Vector3 m_CustomPosition;

        [SerializeField]
        Quaternion m_CustomRotation;

        public void Spawn()
        {
            Instantiate(
                this.m_Prefab,
                this.m_UseLocalPosition ? this.transform.position : this.m_CustomPosition,
                this.m_UseLocalRotation ? this.transform.rotation : this.m_CustomRotation);
        }
    }

}
