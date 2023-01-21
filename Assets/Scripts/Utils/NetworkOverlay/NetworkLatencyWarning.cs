namespace SpaceRpg.Utils.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using Unity.Netcode;
    using Unity.Netcode.Transports.UTP;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class NetworkLatencyWarning : MonoBehaviour
    {
        TextMeshProUGUI m_LatencyText;
        bool m_LatencyTextCreated;

        Color m_TextColor = Color.red;

        bool m_ArtificalLatencyEnabled;

        // Update is called once per frame
        void Update()
        {
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                var unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

#if UNITY_EDITOR
                var simulatorParameters = unityTransport.DebugSimulator;
                this.m_ArtificalLatencyEnabled = simulatorParameters.PacketDelayMS > 0 || simulatorParameters.PacketJitterMS > 0 || simulatorParameters.PacketDropRate > 0;
#else
                this.m_ArtificalLatencyEnabled = false;
#endif
                if (this.m_ArtificalLatencyEnabled)
                {
                    if (!this.m_LatencyTextCreated)
                    {
                        this.m_LatencyTextCreated = true;
                        this.CreateLatencyText();
                    }

                    this.m_TextColor.a = Mathf.PingPong(Time.time, 1f);
                    this.m_LatencyText.color = this.m_TextColor;
                }
            }
            else
            {
                this.m_ArtificalLatencyEnabled = false;
            }

            if (!this.m_ArtificalLatencyEnabled)
            {
                if (this.m_LatencyTextCreated)
                {
                    this.m_LatencyTextCreated = false;
                    Destroy(this.m_LatencyText);
                }
            }
        }

        void CreateLatencyText()
        {
            Assert.IsNotNull(NetworkOverlay.Instance,
                "No NetworkOverlay object part of scene. Add NetworkOverlay prefab to bootstrap scene!");

            NetworkOverlay.Instance.AddTextToUI("UI Latency Warning Text", "Network Latency Enabled", out this.m_LatencyText);
        }
    }

}
