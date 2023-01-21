namespace SpaceRpg.UnityServices.Lobbies
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public struct LobbyListFetchedMessage
    {
        public readonly IReadOnlyList<LocalLobby> LocalLobbies;

        public LobbyListFetchedMessage(List<LocalLobby> localLobbies)
        {
            this.LocalLobbies = localLobbies;
        }
    }
}

