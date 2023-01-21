namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class ProfileListItemUI : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI m_ProfileNameText;

        [Inject] ProfileManager m_ProfileManager;

        public void SetProfileName(string profileName)
        {
            this.m_ProfileNameText.text = profileName;
        }

        public void OnSelectClick()
        {
            this.m_ProfileManager.Profile = this.m_ProfileNameText.text;
        }

        public void OnDeleteClick()
        {
            this.m_ProfileManager.DeleteProfile(this.m_ProfileNameText.text);
        }
    }
}