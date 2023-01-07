using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AppState : MonoBehaviour
{
    public static AppState Instance { get; private set; }

    public static AppAuthentication Auth => AppAuthentication.Instance;

    public static UserProfileManager UserProfileManager => UserProfileManager.Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();

        Debug.Log("Unity Services Initialized");

        Auth.AuthStateChange += (object sender, string profileId) =>
        {
            Debug.Log("Auth State Change: " + profileId);
            if (!string.IsNullOrWhiteSpace(profileId))
            {
                UserProfile profile = UserProfileManager.Instance.GetUserProfile(profileId);
                UserProfileManager.Instance.SetCurrentUser(profile);
            }
        };


        await Auth.SignOn();
    }
}
