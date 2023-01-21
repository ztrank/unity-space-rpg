

namespace SpaceRpg.UnityServices.Lobbies
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Services.Lobbies;
    using Unity.Services.Lobbies.Models;
    using System.Threading.Tasks;
    using VContainer;
    using SpaceRpg.Infrastructure;
    using Unity.Services.Authentication;
    using VContainer.Unity;

    public class LobbyServiceFacade : IDisposable, IStartable
    {
        [Inject] LifetimeScope m_ParentContainer;
        [Inject] UpdateRunner m_UpdateRunner;
        [Inject] LocalLobby m_LocalLobby;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] IPublisher<UnityServiceErrorMessage> m_UnityServiceErrorMessagePub;
        [Inject] IPublisher<LobbyListFetchedMessage> m_LobbyListFetchedPub;

        LifetimeScope m_ServiceScope;

        const float k_HeartbeatPeriod = 8;
        float m_HeartbeatTime = 0;

        LobbyAPIInterface m_LobbyAPIInterface;
        JoinedLobbyContentHeartbeat m_JoinedLobbyContentHeartbeat;

        RateLimitCooldown m_RateLimitQuery;
        RateLimitCooldown m_RateLimitJoin;
        RateLimitCooldown m_RateLimitHost;

        public Lobby CurrentUnityLobby { get; private set; }

        bool m_IsTracking = false;

        public void Start()
        {
            this.m_ServiceScope = this.m_ParentContainer.CreateChild(builder =>
            {
                builder.Register<JoinedLobbyContentHeartbeat>(Lifetime.Singleton);
                builder.Register<LobbyAPIInterface>(Lifetime.Singleton);
            });


            this.m_LobbyAPIInterface = this.m_ServiceScope.Container.Resolve<LobbyAPIInterface>();
            this.m_JoinedLobbyContentHeartbeat = this.m_ServiceScope.Container.Resolve<JoinedLobbyContentHeartbeat>();

            this.m_RateLimitQuery = new RateLimitCooldown(1f);
            this.m_RateLimitJoin = new RateLimitCooldown(3f);
            this.m_RateLimitHost = new RateLimitCooldown(3f);
        }

        public void Dispose()
        {
            this.EndTracking();

            if (this.m_ServiceScope != null)
            {
                this.m_ServiceScope = null;
            }
        }

        public void SetRemoteLobby(Lobby lobby)
        {
            this.CurrentUnityLobby = lobby;
            this.m_LocalLobby.ApplyRemoteData(lobby);
        }

        public void BeginTracking()
        {
            if (!this.m_IsTracking)
            {
                this.m_IsTracking = true;
                this.m_UpdateRunner.Subscribe(this.UpdateLobby, 2f);
                this.m_JoinedLobbyContentHeartbeat.BeginTracking();
            }
        }

        public Task EndTracking()
        {
            var task = Task.CompletedTask;

            if (this.CurrentUnityLobby != null)
            {
                this.CurrentUnityLobby = null;
                var lobbyId = this.m_LocalLobby?.LobbyId;

                if (!string.IsNullOrEmpty(lobbyId))
                {
                    if (this.m_LocalUser.IsHost)
                    {
                        task = this.DeleteLobbyAsync(lobbyId);
                    }
                    else
                    {
                        task = this.LeaveLobbyAsync(lobbyId);
                    }
                }

                this.m_LocalUser.ResetState();
                this.m_LocalLobby?.Reset(this.m_LocalUser);
            }

            if (this.m_IsTracking)
            {
                this.m_UpdateRunner.Unsubscribe(this.UpdateLobby);
                this.m_IsTracking = false;
                this.m_HeartbeatTime = 0;
                this.m_JoinedLobbyContentHeartbeat.EndTracking();
            }

            return task;
        }

        private async void UpdateLobby(float unused)
        {
            if (!this.m_RateLimitQuery.CanCall)
            {
                return;
            }

            try
            {
                var lobby = await this.m_LobbyAPIInterface.GetLobby(this.m_LocalLobby.LobbyId);

                this.CurrentUnityLobby = lobby;
                this.m_LocalLobby.ApplyRemoteData(lobby);

                if (!this.m_LocalUser.IsHost)
                {
                    foreach (var lobbyUser in this.m_LocalLobby.LobbyUsers)
                    {
                        if (lobbyUser.Value.IsHost)
                        {
                            return;
                        }

                    }

                    this.m_UnityServiceErrorMessagePub.Publish(new UnityServiceErrorMessage("Host lef the lobby", "Disconnecting", UnityServiceErrorMessage.Service.Lobby));

                    await this.EndTracking();
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitQuery.PutOnCooldown();
                }
                else if (e.Reason != LobbyExceptionReason.LobbyNotFound && !this.m_LocalUser.IsHost)
                {
                    this.PublishError(e);
                }
            }
        }

        public async Task<(bool Success, Lobby lobby)> TryCreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate) 
        {
            if (!this.m_RateLimitHost.CanCall)
            {
                Debug.LogWarning("Create Lobby hit the rate limit.");
                return (false, null);
            }

            try
            {
                var lobby = await this.m_LobbyAPIInterface.CreateLobby(AuthenticationService.Instance.PlayerId, lobbyName, maxPlayers, isPrivate, this.m_LocalUser.GetDataForUnityServices(), null);
                return (true, lobby);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitHost.PutOnCooldown();
                }
                else
                {
                    this.PublishError(e);
                }
            }

            return (false, null);
        }

        public async Task<(bool Success, Lobby lobby)> TryJoinLobbyAsync(string lobbyId, string lobbyCode)
        {
            if (!this.m_RateLimitJoin.CanCall || (lobbyId == null && lobbyCode == null))
            {
                Debug.LogWarning("Join Lobby hit the rate limit.");
                return (false, null);
            }

            try
            {
                if (!string.IsNullOrEmpty(lobbyCode))
                {
                    var lobby = await this.m_LobbyAPIInterface.JoinLobbyByCode(AuthenticationService.Instance.PlayerId, lobbyCode, this.m_LocalUser.GetDataForUnityServices());
                    return (true, lobby);
                }
                else
                {
                    var lobby = await this.m_LobbyAPIInterface.JoinLobbyById(AuthenticationService.Instance.PlayerId, lobbyId, this.m_LocalUser.GetDataForUnityServices());
                    return (true, lobby);
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitJoin.PutOnCooldown();
                }
                else
                {
                    this.PublishError(e);
                }
            }

            return (false, null);
        }

        public async Task RetrieveAndPublishLobbyListAsync()
        {
            if (!this.m_RateLimitQuery.CanCall)
            {
                Debug.LogWarning("Retrieve Lobby list hit the rate limit. Will try again soon...");
                return;
            }

            try
            {
                var response = await this.m_LobbyAPIInterface.QueryAllLobbies();
                this.m_LobbyListFetchedPub.Publish(new LobbyListFetchedMessage(LocalLobby.CreateLocalLobies(response)));
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitQuery.PutOnCooldown();
                }
                else
                {
                    this.PublishError(e);
                }
            }
        }

        public async Task<Lobby> ReconnectToLobbyAsync(string lobbyId)
        {
            try
            {
                return await this.m_LobbyAPIInterface.ReconnectToLobby(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !this.m_LocalUser.IsHost)
                {
                    this.PublishError(e);
                }
            }

            return null;
        }

        /// <summary>
        /// Attempt to leave a lobby
        /// </summary>
        public async Task LeaveLobbyAsync(string lobbyId)
        {
            string uasId = AuthenticationService.Instance.PlayerId;
            try
            {
                await this.m_LobbyAPIInterface.RemovePlayerFromLobby(uasId, lobbyId);
            }
            catch (LobbyServiceException e)
            {
                // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !this.m_LocalUser.IsHost)
                {
                    this.PublishError(e);
                }
            }
        }

        public async void RemovePlayerFromLobbyAsync(string uasId, string lobbyId)
        {
            if (this.m_LocalUser.IsHost)
            {
                try
                {
                    await this.m_LobbyAPIInterface.RemovePlayerFromLobby(uasId, lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    this.PublishError(e);
                }
            }
            else
            {
                Debug.LogError("Only the host can remove other players from the lobby.");
            }
        }

        public async Task DeleteLobbyAsync(string lobbyId)
        {
            if (this.m_LocalUser.IsHost)
            {
                try
                {
                    await this.m_LobbyAPIInterface.DeleteLobby(lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    this.PublishError(e);
                }
            }
            else
            {
                Debug.LogError("Only the host can delete a lobby.");
            }
        }

        /// <summary>
        /// Attempt to push a set of key-value pairs associated with the local player which will overwrite any existing data for these keys.
        /// </summary>
        public async Task UpdatePlayerDataAsync(Dictionary<string, PlayerDataObject> data)
        {
            if (!this.m_RateLimitQuery.CanCall)
            {
                return;
            }

            try
            {
                var result = await this.m_LobbyAPIInterface.UpdatePlayer(this.CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId, data, null, null);

                if (result != null)
                {
                    this.CurrentUnityLobby = result; // Store the most up-to-date lobby now since we have it, instead of waiting for the next heartbeat.
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitQuery.PutOnCooldown();
                }
                else if (e.Reason != LobbyExceptionReason.LobbyNotFound && !this.m_LocalUser.IsHost) // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                {
                    this.PublishError(e);
                }
            }
        }

        /// <summary>
        /// Lobby can be provided info about Relay (or any other remote allocation) so it can add automatic disconnect handling.
        /// </summary>
        public async Task UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo)
        {
            if (!this.m_RateLimitQuery.CanCall)
            {
                return;
            }

            try
            {
                await this.m_LobbyAPIInterface.UpdatePlayer(this.CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId, new Dictionary<string, PlayerDataObject>(), allocationId, connectionInfo);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitQuery.PutOnCooldown();
                }
                else
                {
                    this.PublishError(e);
                }

                //todo - retry logic? SDK is supposed to handle this eventually
            }
        }

        /// <summary>
        /// Attempt to update a set of key-value pairs associated with a given lobby.
        /// </summary>
        public async Task UpdateLobbyDataAsync(Dictionary<string, DataObject> data)
        {
            if (!this.m_RateLimitQuery.CanCall)
            {
                return;
            }

            var dataCurr = this.CurrentUnityLobby.Data ?? new Dictionary<string, DataObject>();

            foreach (var dataNew in data)
            {
                if (dataCurr.ContainsKey(dataNew.Key))
                {
                    dataCurr[dataNew.Key] = dataNew.Value;
                }
                else
                {
                    dataCurr.Add(dataNew.Key, dataNew.Value);
                }
            }

            //we would want to lock lobbies from appearing in queries if we're in relay mode and the relay isn't fully set up yet
            var shouldLock = string.IsNullOrEmpty(this.m_LocalLobby.RelayJoinCode);

            try
            {
                var result = await this.m_LobbyAPIInterface.UpdateLobby(this.CurrentUnityLobby.Id, dataCurr, shouldLock);

                if (result != null)
                {
                    this.CurrentUnityLobby = result;
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    this.m_RateLimitQuery.PutOnCooldown();
                }
                else
                {
                    this.PublishError(e);
                }
            }
        }

        /// <summary>
        /// Lobby requires a periodic ping to detect rooms that are still active, in order to mitigate "zombie" lobbies.
        /// </summary>
        public void DoLobbyHeartbeat(float dt)
        {
            this.m_HeartbeatTime += dt;
            if (this.m_HeartbeatTime > k_HeartbeatPeriod)
            {
                this.m_HeartbeatTime -= k_HeartbeatPeriod;
                try
                {
                    this.m_LobbyAPIInterface.SendHeartbeatPing(this.CurrentUnityLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                    if (e.Reason != LobbyExceptionReason.LobbyNotFound && !this.m_LocalUser.IsHost)
                    {
                        this.PublishError(e);
                    }
                }
            }
        }

        void PublishError(LobbyServiceException e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})"; // Lobby error type, then HTTP error type.
            this.m_UnityServiceErrorMessagePub.Publish(new UnityServiceErrorMessage("Lobby Error", reason, UnityServiceErrorMessage.Service.Lobby, e));
        }
    }

    public interface ILobbyServiceFacade : IDisposable
    {
        Lobby CurrentUnityLobby { get; }
        void Start();
        void SetRemoteLobby(Lobby lobby);
        void BeginTracking();
        Task EndTracking();
        void UpdateLobby(float unused);
        Task<(bool Success, Lobby lobby)> TryCreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate);
        Task<(bool Success, Lobby lobby)> TryJoinLobbyAsync(string lobbyId, string lobbyCode);
        Task<(bool Success, Lobby lobby)> TryQuickJoinLobbyAsync();
        Task RetrieveAndPublishLobbyListAsync();
        Task<Lobby> ReconnectToLobbyAsync(string lobbyId);
        Task LeaveLobbyAsync(string lobbyId);
        void RemovePlayerFromLobbyAsync(string uasId, string lobbyId);
        Task DeleteLobbyAsync(string lobbyId);
        Task UpdatePlayerDataAsync(Dictionary<string, PlayerDataObject> data);
        Task UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo);
        Task UpdateLobbyDataAsync(Dictionary<string, DataObject> data);
        void DoLobbyHeartbeat(float dt);
        void PublishError(LobbyServiceException e);
    }
}
