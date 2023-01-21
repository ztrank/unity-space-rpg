namespace SpaceRpg.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IGameEventListenable
    {
        GameEvent GameEvent { get; set; }
        void EventRaised();
    }
}
