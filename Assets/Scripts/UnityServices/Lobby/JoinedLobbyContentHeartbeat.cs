namespace SpaceRpg.UnityServices.Lobbies
{
    using SpaceRpg.Infrastructure;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VContainer;

    public class JoinedLobbyContentHeartbeat
    {
        [Inject] LocalLobby m_LocalLobby;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] UpdateRunner m_UpdateRunner;
        [Inject] LobbyServiceFacade m_LobbyServiceFacade;

        int m_AwaitingQueryCount = 0;
        bool m_ShouldPushData = false;

        public void BeginTracking()
        {
            this.m_UpdateRunner.Subscribe(this.OnUpdate, 1.5f);
            this.m_LocalLobby.changed += this.OnLocalLobbyChanged;
            this.m_ShouldPushData = true;
        }

        public void EndTracking()
        {
            this.m_ShouldPushData = false;
            this.m_UpdateRunner.Unsubscribe(this.OnUpdate);
            this.m_LocalLobby.changed -= this.OnLocalLobbyChanged;
        }

        void OnLocalLobbyChanged(LocalLobby lobby)
        {
            if (string.IsNullOrEmpty(lobby.LobbyId))
            {
                this.EndTracking();
            }

            this.m_ShouldPushData = false;
        }

        async void OnUpdate(float dt)
        {
            if (this.m_AwaitingQueryCount > 0)
            {
                return;
            }

            if (this.m_LocalUser.IsHost)
            {
                this.m_LobbyServiceFacade.DoLobbyHeartbeat(dt);
            }

            if (this.m_ShouldPushData)
            {
                this.m_ShouldPushData = false;

                if (this.m_LocalUser.IsHost)
                {
                    this.m_AwaitingQueryCount++;
                    await this.m_LobbyServiceFacade.UpdateLobbyDataAsync(this.m_LocalLobby.GetDataForUnityServices());
                    this.m_AwaitingQueryCount--;
                }

                this.m_AwaitingQueryCount++;
                await this.m_LobbyServiceFacade.UpdatePlayerDataAsync(this.m_LocalUser.GetDataForUnityServices());
                this.m_AwaitingQueryCount--;
            }
        }
    }
}