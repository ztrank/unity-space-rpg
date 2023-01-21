namespace SpaceRpg.ConnectionManagement
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class OnlineState : ConnectionState
    {
        public const string k_DtlsConnType = "dtls";

        public override void OnUserRequestedShutdown()
        {
            this.m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
        }

        public override void OnTransportFailure()
        {
            this.m_ConnectionManager.ChangeState(this.m_ConnectionManager.m_Offline);
        }
    }
}