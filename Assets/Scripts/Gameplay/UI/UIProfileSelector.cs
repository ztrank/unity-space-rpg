namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;

    public class UIProfileSelector : MonoBehaviour
    {
        [SerializeField] ProfileListItemUI m_ProfileListItemPrototype;
        [SerializeField] InputField m_newProfileField;
        [SerializeField] Button m_CreateProfileButton;
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] Graphic m_EmptyProfileListLabel;

        List<ProfileListItemUI> m_ProfileListItems = new List<ProfileListItemUI>();

        [Inject] IObjectResolver m_Resolver;
        [Inject] ProfileManager m_ProfileManager;

        void Awake()
        {
            this.m_ProfileListItemPrototype.gameObject.SetActive(false);
            this.Hide();
            this.m_CreateProfileButton.interactable = false;
        }

        public void SanitizeProfileNameInputText()
        {
            this.m_newProfileField.text = this.SanitizeProfileName(this.m_newProfileField.text);
            this.m_CreateProfileButton.interactable = this.m_newProfileField.text.Length > 0 && !this.m_ProfileManager.AvailableProfiles.Contains(this.m_newProfileField.text);
        }

        public string SanitizeProfileName(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^a-zA-Z0-9]", "");
        }

        public void OnNewProfilebuttonPressed()
        {
            var profile = this.m_newProfileField.text;

            if (!this.m_ProfileManager.AvailableProfiles.Contains(profile)) 
            {
                this.m_ProfileManager.CreateProfile(profile);
                this.m_ProfileManager.Profile = profile;
            }
            else
            {
                PopupManager.ShowPopupPanel("Could not create new Profile", "A profile already exists with this same name. Select one of the already existing profiles or create a new one.");
            }
        }

        public void InitializeUI()
        {
            this.EnsureNumberOfActiveUISlots(this.m_ProfileManager.AvailableProfiles.Count);
            for (var i = 0; i < this.m_ProfileManager.AvailableProfiles.Count; i++)
            {
                var profileName = this.m_ProfileManager.AvailableProfiles[i];
                this.m_ProfileListItems[i].SetProfileName(profileName);
            }

            this.m_EmptyProfileListLabel.enabled = this.m_ProfileManager.AvailableProfiles.Count == 0;
        }

        void EnsureNumberOfActiveUISlots(int requiredNumber)
        {
            int delta = requiredNumber - this.m_ProfileListItems.Count;

            for (int i = 0; i < delta; i++)
            {
                this.CreateProfileListItem();
            }

            for (int i = 0; i < this.m_ProfileListItems.Count; i++)
            {
                this.m_ProfileListItems[i].gameObject.SetActive(i < requiredNumber);
            }
        }

        void CreateProfileListItem()
        {
            var listItem = Instantiate(this.m_ProfileListItemPrototype.gameObject, this.m_ProfileListItemPrototype.transform.parent)
                .GetComponent<ProfileListItemUI>();
            this.m_ProfileListItems.Add(listItem);
            listItem.gameObject.SetActive(true);
            this.m_Resolver.Inject(listItem);
        }

        public void Show()
        {
            this.m_CanvasGroup.alpha = 1f;
            this.m_CanvasGroup.blocksRaycasts = true;
            this.m_newProfileField.text = "";
            this.InitializeUI();
        }

        public void Hide()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.blocksRaycasts = false;
        }
    }
}