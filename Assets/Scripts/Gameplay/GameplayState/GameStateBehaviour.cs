namespace SpaceRpg.Gameplay.GameplayState
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VContainer.Unity;

    public enum GameState
    {
        MainMenu,
        Lobby
    }

    public abstract class GameStateBehaviour : LifetimeScope
    { 
    
        public virtual bool Persists
        {
            get
            {
                return false;
            }
        }

        public abstract GameState ActiveState { get; }

        private static GameObject s_ActiveStateGO;

        protected override void Awake()
        {
            base.Awake();

            if (this.Parent != null)
            {
                this.Parent.Container.Inject(this);
            }
        }

        protected virtual void Start()
        {
            if (s_ActiveStateGO != null)
            {
                if (s_ActiveStateGO == this.gameObject)
                {
                    return;
                }

                var previousState = s_ActiveStateGO.GetComponent<GameStateBehaviour>();

                if (previousState.Persists && previousState.ActiveState == this.ActiveState)
                {
                    Destroy(this.gameObject);
                    return;
                }

                Destroy(s_ActiveStateGO);
            }

            s_ActiveStateGO = this.gameObject;

            if (this.Persists)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }

        protected override void OnDestroy()
        {
            if (!this.Persists)
            {
                s_ActiveStateGO = null;
            }
        }
    }
}