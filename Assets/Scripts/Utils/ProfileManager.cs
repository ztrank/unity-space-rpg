namespace SpaceRpg.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Security.Cryptography;
    using System.Text;
    using UnityEngine;

    public class ProfileManager
    {
        public const string AuthProfileCommandLineArg = "-AuthProfile";

        string m_Profile;

        public string Profile
        {
            get
            {
                return this.m_Profile ??= GetProfile();
            }
            set
            {
                this.m_Profile = value;
                this.OnProfileChanged?.Invoke();
            }
        }

        public event Action OnProfileChanged;

        List<string> m_AvailableProfiles;

        public ReadOnlyCollection<string> AvailableProfiles
        {
            get
            {
                if (this.m_AvailableProfiles == null)
                {
                    this.LoadProfiles();
                }

                return this.m_AvailableProfiles.AsReadOnly();
            }
        }

        static string GetProfile()
        {
            var arguments = Environment.GetCommandLineArgs();
            for(int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == AuthProfileCommandLineArg)
                {
                    var profileId = arguments[i + 1];
                    return profileId;
                }
            }

#if UNITY_EDITOR
            var hashedBytes = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes).ToString("N");
#else
            return "";
#endif
        }

        public void CreateProfile(string profile)
        {
            this.m_AvailableProfiles.Add(profile);
            this.SaveProfiles();
        }

        public void DeleteProfile(string profile)
        {
            this.m_AvailableProfiles.Remove(profile);
            this.SaveProfiles();
        }

        public void LoadProfiles()
        {
            this.m_AvailableProfiles = new List<string>();
            string loadedProfiles = ClientPrefs.GetAvailableProfiles();

            foreach (var profile in loadedProfiles.Split(',')) 
            {
                if (profile.Length > 0)
                {
                    this.m_AvailableProfiles.Add(profile);
                }
            }
        }

        public void SaveProfiles()
        {
            var profilesToSave = string.Join(',', this.m_AvailableProfiles);

            ClientPrefs.SetAvailableProfiles(profilesToSave);
        }
    }
}