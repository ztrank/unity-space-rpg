namespace SpaceRpg.Gameplay.GameplayObjects
{
    using SpaceRpg.Infrastructure;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu]
    public class PersistentPlayerRuntimeCollection : RuntimeCollection<PersistentPlayer>
    {
        public bool TryGetPlayer(ulong clientId, out PersistentPlayer persistentPlayer)
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (clientId == this.Items[i].OwnerClientId)
                {
                    persistentPlayer = this.Items[i];
                    return true;
                }
            }

            persistentPlayer = null;
            return false;
        }
    }
}