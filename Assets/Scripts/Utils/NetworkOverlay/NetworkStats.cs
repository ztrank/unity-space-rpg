namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.Assertions;

    [RequireComponent(typeof(NetworkObject))]
    public class NetworkStats : NetworkBehaviour
    {
        const int k_MaxWindowSizeSeconds = 3;
        const float k_PingIntervalSeconds = 0.1f;
        const float k_MaxWindowSize = k_MaxWindowSizeSeconds / k_PingIntervalSeconds;

        // Since the game is asynchronous, turn based, higher ping times are tolerable. The following thresholds are in milliseconds.
        const float k_StrugglingNetworkConditionsRTTThreshold = 500;
        const float k_BadNetworkConditionsRTTThreshold = 1000;

        ExponentialMovingAverageCalculator m_SpaceRpgRTT = new ExponentialMovingAverageCalculator(0);
        ExponentialMovingAverageCalculator m_UtpRTT = new ExponentialMovingAverageCalculator(0);

        float m_LastPingTime;
        TextMeshProUGUI m_TextStat;
        TextMeshProUGUI m_TextHostType;
        TextMeshProUGUI m_TextBadNetworkConditions;

        int m_CurrentRTTPingId;
        Dictionary<int, float> m_PingHistoryStartTimes = new Dictionary<int, float>();

        ClientRpcParams m_PongClientParams;

        bool m_IsServer;
        string m_TextToDisplay;

        public override void OnNetworkSpawn()
        {
            this.m_IsServer = this.IsServer;
            bool isClientOnly = this.IsClient && !this.IsServer;

            if (!this.IsOwner && isClientOnly)
            {
                this.enabled = false;
                return;
            }

            if (this.IsOwner)
            {
                this.CreateNetworkStatsText();
            }

            this.m_PongClientParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams()
                {
                    TargetClientIds = new[] { this.OwnerClientId }
                }
            };
        }

        private void CreateNetworkStatsText()
        {
            Assert.IsNotNull(Editor.NetworkOverlay.Instance,
                "No NetworkOverlay object part of scene. Add NetworkOverlay prefab to bootstrap scene!");

            string hostType = IsHost ? "Host" : IsClient ? "Client" : "Unknown";
            Editor.NetworkOverlay.Instance.AddTextToUI("UI Host Type Text", $"Type: {hostType}", out m_TextHostType);
            Editor.NetworkOverlay.Instance.AddTextToUI("UI Stat Text", "No Stat", out m_TextStat);
            Editor.NetworkOverlay.Instance.AddTextToUI("UI Bad Conditions Text", "", out m_TextBadNetworkConditions);
        }

        private void FixedUpdate()
        {
            if (!this.m_IsServer)
            {
                if (Time.realtimeSinceStartup - m_LastPingTime > k_PingIntervalSeconds)
                {
                    this.PingServerRPC(m_CurrentRTTPingId);
                    this.m_PingHistoryStartTimes[this.m_CurrentRTTPingId] = Time.realtimeSinceStartup;
                    this.m_CurrentRTTPingId++;
                    this.m_LastPingTime = Time.realtimeSinceStartup;

                    this.m_UtpRTT.NextValue(NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId));
                }

                if (this.m_TextStat != null)
                {
                    this.m_TextToDisplay = $"RTT: {(this.m_SpaceRpgRTT.Average * 1000).ToString("0")} ms; \nUTP RTT {this.m_UtpRTT.Average.ToString("0")} ms";

                    if (this.m_UtpRTT.Average > k_BadNetworkConditionsRTTThreshold)
                    {
                        this.m_TextStat.color = Color.red;
                    }
                    else if (this.m_UtpRTT.Average > k_StrugglingNetworkConditionsRTTThreshold)
                    {
                        this.m_TextStat.color = Color.yellow;
                    }
                    else
                    {
                        this.m_TextStat.color = Color.white;
                    }
                }

                if (this.m_TextBadNetworkConditions != null)
                {
                    this.m_TextBadNetworkConditions.text = this.m_UtpRTT.Average > k_BadNetworkConditionsRTTThreshold ? "Bad Network Conditions Detected!" : "";
                    var color = Color.red;
                    color.a = Mathf.PingPong(Time.time, 1f);
                    this.m_TextBadNetworkConditions.color = color;
                }
            }
            else
            {
                this.m_TextToDisplay = $"Connected players: {NetworkManager.Singleton.ConnectedClients.Count}";
            }
            
            if (this.m_TextStat)
            {
                this.m_TextStat.text = this.m_TextToDisplay;
            }
        }

        [ServerRpc]
        void PingServerRPC(int pingId, ServerRpcParams serverParams = default)
        {
            this.PongClientRPC(pingId, this.m_PongClientParams);
        }

        [ClientRpc]
        void PongClientRPC(int pingId, ClientRpcParams clientParams = default)
        {
            var startTime = this.m_PingHistoryStartTimes[pingId];
            this.m_PingHistoryStartTimes.Remove(pingId);
            this.m_SpaceRpgRTT.NextValue(Time.realtimeSinceStartup - startTime);
        }

        public override void OnNetworkDespawn()
        {
            if (this.m_TextStat != null)
            {
                Destroy(this.m_TextStat.gameObject);
            } 

            if (this.m_TextHostType != null)
            {
                Destroy(this.m_TextHostType.gameObject);
            }

            if (this.m_TextBadNetworkConditions != null)
            {
                Destroy(this.m_TextBadNetworkConditions.gameObject);
            }
        }

        struct ExponentialMovingAverageCalculator
        {
            readonly float m_Alpha;
            float m_Average;

            public float Average => this.m_Average;

            public ExponentialMovingAverageCalculator(float average)
            {
                this.m_Alpha = 2f / (k_MaxWindowSize + 1);
                this.m_Average = average;
            }

            public float NextValue(float value) => this.m_Average = (value - this.m_Average) * this.m_Alpha + this.m_Average;
        }
    }

}
