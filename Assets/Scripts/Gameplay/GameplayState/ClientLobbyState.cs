namespace SpaceRpg.Gameplay.GameplayState
{
    using SpaceRpg.ConnectionManagement;
    using SpaceRpg.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using VContainer;

    [RequireComponent(typeof(NetcodeHooks))]
    public class ClientLobbyState : GameStateBehaviour
    {
        public static ClientLobbyState Instance { get; private set; }

        [SerializeField]
        NetcodeHooks m_NetcodeHooks;

        public override GameState ActiveState => GameState.Lobby;

        [SerializeField]
        [Tooltip("This is triggered when the player presses the \"Ready\" button")]
        string m_AnimationTriggerOnReady = "Ready";

        [SerializeField]
        TextMeshProUGUI m_NumPlayersText;

        [SerializeField]
        TextMeshProUGUI m_ReadyButtonText;

        [SerializeField]
        [Tooltip("UI Elements to turn on when the player hasn't selected their character (and the game is about to start0. Turned off otherwise!")]
        List<GameObject> m_UIElementsForCharacterSelect;

        [SerializeField]
        [Tooltip("UI Elements to turn on when the player has locked in their character (and the game is about to start0. Turned off otherwise!")]
        List<GameObject> m_UIElementsForCharacterSelected;

        [SerializeField]
        [Tooltip("UI Elements to turn on when the lobby is closed (and the game is about to start0. Turned off otherwise!")]
        List<GameObject> m_UIElementsForLobbyEnding;

        [SerializeField]
        [Tooltip("UI Elements to turn on when there's been a fatal error (and the client cannot proceed). Turned off otherwise!")]
        List<GameObject> m_UIElementsForFatalError;

        [SerializeField]
        Transform m_CharacterGraphicsParent;

        [Header("Lobby Seats")]
        [SerializeField]
        [Tooltip("Collection of 8 portrait-boxes, one for each potential lobby member")]
        //List<UICharSelectPlayerSeat> m_PlayerSeats;
        List<GameObject> m_PlayerSeats;

        int m_LastSeatSelected = -1;
        bool m_HasLocalPlayerLockedIn = false;

        GameObject m_CurrentCharacterGraphics;
        Animator m_CurrentCharacterGraphicsAnimator;

        readonly Dictionary<Guid, GameObject> m_SpawnedCharacterGraphics = new Dictionary<Guid, GameObject>();

        enum LobbyMode
        {
            CharacterSelect,
            CharacterSelected,
            LobbyEnding,
            FatalError
        }

        Dictionary<LobbyMode, List<GameObject>> m_LobbyUiElementsByMode;

        [Inject]
        ConnectionManager m_ConnectionManager;

        protected override void Awake()
        {
            base.Awake();

            Instance = this;

            this.m_NetcodeHooks.OnNetworkDespawnHook += this.OnNetworkDespawn;
            this.m_NetcodeHooks.OnNetworkSpawnHook += this.OnNetworkSpawn;

            this.m_LobbyUiElementsByMode = new Dictionary<LobbyMode, List<GameObject>>()
            {
                { LobbyMode.CharacterSelect, this.m_UIElementsForCharacterSelect },
                { LobbyMode.CharacterSelected, this.m_UIElementsForCharacterSelected },
                { LobbyMode.LobbyEnding, this.m_UIElementsForLobbyEnding },
                { LobbyMode.FatalError, this.m_UIElementsForFatalError },
            };
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnDestroy();
        }

        protected override void Start()
        {
            Debug.LogWarning(nameof(ClientLobbyState) + " Is not complete");
        }

        void OnNetworkDespawn()
        {

        }

        void OnNetworkSpawn()
        {

        }
    }
}