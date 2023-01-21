namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Multiplayer.Tools.NetStatsMonitor;
    using UnityEngine;

    public class NetStatsMonitorCustomization : MonoBehaviour
    {
        [SerializeField]
        RuntimeNetStatsMonitor m_Monitor;

        [SerializeField]
        GameObject m_Instructions;

        [SerializeField]
        bool m_Fade = true;

        [SerializeField]
        float m_FadeDelay = 5f;

        const int k_NbTouchesToOpenWindow = 3;

        void Start()
        {
            this.m_Monitor.Visible = false;

            if (this.m_Fade)
            {
                this.StartCoroutine(this.Hide());
            }
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.F2) || Input.touchCount == k_NbTouchesToOpenWindow && AnyTouchDown())
            {
                this.m_Monitor.Visible = !this.m_Monitor.Visible;
            }
        }

        static bool AnyTouchDown()
        {
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }

        IEnumerator Hide()
        {
            yield return new WaitForSeconds(this.m_FadeDelay);
            this.m_Instructions.SetActive(false);
        }
    }
}