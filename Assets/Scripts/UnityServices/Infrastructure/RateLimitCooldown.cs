namespace SpaceRpg.UnityServices
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class RateLimitCooldown
    {
        public float CooldownTimeLength => this.m_CooldownTimeLength;

        readonly float m_CooldownTimeLength;
        private float m_CooldownFinishedTime;
        

        public RateLimitCooldown(float cooldownTimeLength)
        {
            this.m_CooldownTimeLength = cooldownTimeLength;
            this.m_CooldownFinishedTime = -1f;
        }

        public bool CanCall => Time.unscaledTime > this.m_CooldownFinishedTime;

        public void PutOnCooldown()
        {
            this.m_CooldownFinishedTime = Time.unscaledTime + this.m_CooldownTimeLength;
        }
    }
}