namespace SpaceRpg.Gameplay.GameplayObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;


    [RequireComponent(typeof(NetworkObject))]
    public class Character : MonoBehaviour
    {
        public event Action CharacteristicChange;
        public event Action HealthChange;

        public string Name;

        [Header("Characteristics", order = 1)]
        public Characteristic Strength;

        [Header("Characteristics", order = 2)]
        public Characteristic Dexterity;

        [Header("Characteristics", order = 3)]
        public Characteristic Endurance;

        [Header("Characteristics", order = 4)]
        public Characteristic Intellect;

        [Header("Characteristics", order = 5)]
        public Characteristic Education;

        [Header("Characteristics", order = 6)]
        public Characteristic Charm;

        [Header("Characteristics", order = 7)]
        public Characteristic PsionicStrength;

        [Header("Characteristics", order = 8)]
        public Characteristic Luck;

        [Header("Characteristics", order = 9)]
        public Characteristic Morale;

        [SerializeField]
        private int m_HealthCurrent;

        /// <summary>
        /// Gets the maximum health for this character as a sum of their natural physical characteristics.
        /// </summary>
        public int HealthMax
        {
            get
            {
                return this.Endurance.Natural + this.Dexterity.Natural + this.Strength.Natural;
            }
        }

        /// <summary>
        /// Gets or sets the current health level and fires HealthChange if the value changes.
        /// </summary>
        public int HealthCurrent 
        { 
            get => this.m_HealthCurrent;
            private set
            {
                int previous = this.m_HealthCurrent;
                this.m_HealthCurrent = Math.Min(value, this.HealthMax);

                if (this.m_HealthCurrent != previous)
                {
                    this.HealthChange?.Invoke();
                }
            }
        }

        private void Awake()
        {
            this.AddCharacteristicListeners();
        }

        private void OnDestroy()
        {
            this.RemoveCharacteristicListeners();
        }

        /// <summary>
        /// Adds the listeners to the characteristics events.
        /// </summary>
        private void AddCharacteristicListeners()
        {
            this.Strength.Change += this.OnCharacteristicChange;
            this.Dexterity.Change += this.OnCharacteristicChange;
            this.Endurance.Change += this.OnCharacteristicChange;
            this.Education.Change += this.OnCharacteristicChange;
            this.Intellect.Change += this.OnCharacteristicChange;
            this.Charm.Change += this.OnCharacteristicChange;
            this.Luck.Change += this.OnCharacteristicChange;
            this.PsionicStrength.Change += this.OnCharacteristicChange;
            this.Morale.Change += this.OnCharacteristicChange;
        }

        /// <summary>
        /// Removes the listeners to the characteristics events.
        /// </summary>
        private void RemoveCharacteristicListeners()
        {
            this.Strength.Change -= this.OnCharacteristicChange;
            this.Dexterity.Change -= this.OnCharacteristicChange;
            this.Endurance.Change -= this.OnCharacteristicChange;
            this.Education.Change -= this.OnCharacteristicChange;
            this.Intellect.Change -= this.OnCharacteristicChange;
            this.Charm.Change -= this.OnCharacteristicChange;
            this.Luck.Change -= this.OnCharacteristicChange;
            this.PsionicStrength.Change -= this.OnCharacteristicChange;
            this.Morale.Change -= this.OnCharacteristicChange;
        }

        /// <summary>
        /// Fires CharacteristicChange event
        /// </summary>
        private void OnCharacteristicChange()
        {
            this.CharacteristicChange?.Invoke();
        }

        /// <summary>
        /// Character Characteristic Class
        /// </summary>
        [Serializable]
        public class Characteristic
        {
            /// <summary>
            /// Event fired when there is a change to this characteristic.
            /// </summary>
            public event Action Change;

            /// <summary>
            /// Natural level of this characteristic before any modifiers or damage.
            /// </summary>
            [SerializeField] private int m_Natural;

            /// <summary>
            /// Current level of this characteristic before any modifiers but after damage.
            /// </summary>
            [SerializeField] private int m_Current;

            /// <summary>
            /// Current level of modifiers applied to this characteristic
            /// </summary>
            [SerializeField] private int m_Modifiers;

            /// <summary>
            /// Gets or sets the natural level of this characteristic.
            /// </summary>
            public int Natural
            {
                get => this.m_Natural;
                set
                {
                    if (this.m_Natural != value)
                    {
                        this.m_Natural = value;
                        this.Change?.Invoke();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the current level of this characteristic.
            /// </summary>
            public int Current
            {
                get => this.m_Current;
                set
                {
                    if (this.m_Current != value)
                    {
                        this.m_Current = value;
                        this.Change?.Invoke();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the current modifiers on this characteristic
            /// </summary>
            public int Modifiers
            {
                get => this.m_Modifiers;
                set
                {
                    if (this.m_Modifiers != value)
                    {
                        this.m_Modifiers = value;
                        this.Change?.Invoke();
                    }
                }
            }

            /// <summary>
            /// Gets the current total value of this characteristic
            /// </summary>
            public int Total
            {
                get => this.Current + this.Modifiers;
            }

            /// <summary>
            /// Gets the current dice modifier of this characteristic
            /// </summary>
            public int DiceModifier
            {
                get => (this.Total / 3) - 2;
            }
        }
    }
}