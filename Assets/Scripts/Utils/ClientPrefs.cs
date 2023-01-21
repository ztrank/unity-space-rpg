namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ClientPrefs
    {
        const string k_MasterVolumeKey = "MasterVolume";
        const string k_MusicVolumeKey = "MusicVolume";
        const string k_ClientGUIDKey = "client_guid";
        const string k_AvailableProfilesKey = "AvailableProfiles";
        const float k_DefaultMasterVolume = 0.5f;
        const float k_DefaultMusicVolume = 0.8f;

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(k_MasterVolumeKey, k_DefaultMasterVolume);
        }

        public static void SetMasterVolume(float masterVolume)
        {
            PlayerPrefs.SetFloat(k_MasterVolumeKey, masterVolume);
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(k_MusicVolumeKey, k_DefaultMusicVolume);
        }
        public static void SetMusicVolume(float musicVolume)
        {
            PlayerPrefs.SetFloat(k_MusicVolumeKey, musicVolume);
        }

        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey(k_ClientGUIDKey))
            {
                return PlayerPrefs.GetString(k_ClientGUIDKey);
            }

            var guid = System.Guid.NewGuid();
            var guidString = guid.ToString();

            PlayerPrefs.SetString(k_ClientGUIDKey, guidString);

            return guidString;
        }

        public static string GetAvailableProfiles()
        {
            return PlayerPrefs.GetString(k_AvailableProfilesKey, "");
        }

        public static void SetAvailableProfiles(string availableProfiles)
        {
            PlayerPrefs.SetString(k_AvailableProfilesKey, availableProfiles);
        }
    }
}