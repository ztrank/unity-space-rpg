namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.Gameplay.GameplayObjects;
    using SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ClientSpaceRpgLoadingScreen : ClientLoadingScreen
    {
        [SerializeField]
        PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;

        protected override void AddOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker)
        {
            base.AddOtherPlayerProgressBar(clientId, progressTracker);
            this.m_LoadingProgressBars[clientId].NameText.text = this.GetPlayerName(clientId);
        }

        protected override void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            base.UpdateOtherPlayerProgressBar(clientId, progressBarIndex);
            this.m_LoadingProgressBars[clientId].NameText.text = this.GetPlayerName(clientId);
        }

        private string GetPlayerName(ulong clientId)
        {
            foreach (var player in this.m_PersistentPlayerRuntimeCollection.Items)
            {
                if (clientId == player.OwnerClientId)
                {
                    return player.NetworkNameState.Name.Value;
                }
            }

            return "";
        }
    }
}