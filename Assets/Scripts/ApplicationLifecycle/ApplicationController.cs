namespace SpaceRpg.ApplicationLifecycle
{
    using SpaceRpg.ApplicationLifecycle.Messages;
    using SpaceRpg.ConnectionManagement;
    using SpaceRpg.Gameplay.GameplayState;
    using SpaceRpg.Infrastructure;
    using SpaceRpg.Infrastructure.Auth;
    using SpaceRpg.UnityServices;
    using SpaceRpg.UnityServices.Lobbies;
    using SpaceRpg.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using VContainer;
    using VContainer.Unity;

    public class ApplicationController : LifetimeScope
    {

        [SerializeField] UpdateRunner m_UpdateRunner;
        [SerializeField] ConnectionManager m_ConnectionManager;
        [SerializeField] NetworkManager m_NetworkManager;

        LocalLobby m_LocalLobby;
        LobbyServiceFacade m_LobbyServiceFacade;

        IDisposable m_Subscriptions;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(this.m_UpdateRunner);
            builder.RegisterComponent(this.m_ConnectionManager);
            builder.RegisterComponent(this.m_NetworkManager);
            builder.Register<LocalLobbyUser>(Lifetime.Singleton);
            builder.Register<LocalLobby>(Lifetime.Singleton);
            builder.Register<ProfileManager>(Lifetime.Singleton);
            builder.Register<PersistentGameState>(Lifetime.Singleton);

            builder.RegisterInstance(new MessageChannel<QuitApplicationMessage>()).AsImplementedInterfaces();
            builder.RegisterInstance(new MessageChannel<UnityServiceErrorMessage>()).AsImplementedInterfaces();
            builder.RegisterInstance(new MessageChannel<ConnectStatus>()).AsImplementedInterfaces();
            builder.RegisterComponent(new NetworkedMessageChannel<ConnectionEventMessage>()).AsImplementedInterfaces();

            //this message channel is essential and persists for the lifetime of the lobby and relay services
            builder.RegisterInstance(new MessageChannel<ReconnectMessage>()).AsImplementedInterfaces();

            //buffered message channels hold the latest received message in buffer and pass to any new subscribers
            builder.RegisterInstance(new BufferedMessageChannel<LobbyListFetchedMessage>()).AsImplementedInterfaces();

            //all the lobby service stuff, bound here so that it persists through scene loads
            builder.Register<AuthenticationServiceFacade>(Lifetime.Singleton); //a manager entity that allows us to do anonymous authentication with unity services

            //LobbyServiceFacade is registered as entrypoint because it wants a callback after container is built to do it's initialization
            builder.RegisterEntryPoint<LobbyServiceFacade>(Lifetime.Singleton).AsSelf();
        }
        // Start is called before the first frame update
        void Start()
        {
            this.m_LocalLobby = this.Container.Resolve<LocalLobby>();
            this.m_LobbyServiceFacade = this.Container.Resolve<LobbyServiceFacade>();

            var quitApplicationSub = this.Container.Resolve<ISubscriber<QuitApplicationMessage>>();
            this.m_Subscriptions = new DisposableGroup();
            ((DisposableGroup)this.m_Subscriptions).Add(quitApplicationSub.Subscribe(this.QuitGame));

            Application.wantsToQuit += this.OnWantToQuit;
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this.m_UpdateRunner.gameObject);
            Application.targetFrameRate = 120;
            SceneManager.LoadScene("MainMenu");
        }

        protected override void OnDestroy()
        {
            this.m_Subscriptions?.Dispose();
            this.m_LobbyServiceFacade?.EndTracking();
            base.OnDestroy();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private IEnumerator LeaveBeforeQuit()
        {
            try
            {
                this.m_LobbyServiceFacade.EndTracking();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            yield return null;
            Application.Quit();
        }

        private bool OnWantToQuit()
        {
            var canQuit = string.IsNullOrEmpty(this.m_LocalLobby?.LobbyId);

            if (!canQuit)
            {
                this.StartCoroutine(this.LeaveBeforeQuit());
            }

            return canQuit;
        }

        private void QuitGame(QuitApplicationMessage msg)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}