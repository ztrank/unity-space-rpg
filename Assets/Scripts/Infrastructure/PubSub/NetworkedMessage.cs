namespace SpaceRpg.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Netcode;
    using UnityEngine;
    using VContainer;

    public class NetworkedMessageChannel<T> : MessageChannel<T> where T : unmanaged, INetworkSerializeByMemcpy
    {
        NetworkManager m_NetworkManager;

        string m_Name;

        public NetworkedMessageChannel()
        {
            this.m_Name = $"{typeof(T).FullName}NetworkMessageChannel";
        }

        [Inject]
        void InjectDependencies(NetworkManager networkManager)
        {
            this.m_NetworkManager = networkManager;
            this.m_NetworkManager.OnClientConnectedCallback += this.OnClientConnected;

            if (this.m_NetworkManager.IsListening)
            {
                this.RegisterHandler();
            }
        }

        public override void Dispose()
        {
            if (!this.IsDisposed)
            {
                if (this.m_NetworkManager != null && this.m_NetworkManager.CustomMessagingManager != null)
                {
                    this.m_NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(this.m_Name);
                }
            }
            base.Dispose();
        }

        private void OnClientConnected(ulong clientId)
        {
            this.RegisterHandler();
        }

        public void RegisterHandler()
        {
            if (!this.m_NetworkManager.IsServer)
            {
                this.m_NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(this.m_Name, this.ReceiveMessageThroughNetwork);
            }
        }

        public override void Publish(T message)
        {
            if (this.m_NetworkManager.IsServer)
            {
                this.SendMessageThroughNetwork(message);
                base.Publish(message);
            }
            else
            {
                Debug.LogError("Only a server can publish in a NetworkedMessageChannel");
            }
        }

        private void SendMessageThroughNetwork(T message)
        {
            FastBufferWriter writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<T>(), Allocator.Temp);
            writer.WriteValueSafe(message);

            this.m_NetworkManager.CustomMessagingManager.SendNamedMessageToAll(this.m_Name, writer);
        }

        private void ReceiveMessageThroughNetwork(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out T message);
            base.Publish(message);
        }
    }
}
