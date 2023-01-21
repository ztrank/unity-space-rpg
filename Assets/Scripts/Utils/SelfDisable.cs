namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SelfDisable : MonoBehaviour
    {
        [SerializeField]
        float m_DisableDelay;
        float m_DisableTimestamp;

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= this.m_DisableTimestamp)
            {
                this.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            this.m_DisableTimestamp = Time.time + this.m_DisableDelay;
        }
    }

}
