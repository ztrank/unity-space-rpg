using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Zenject;

public class AppState : MonoBehaviour
{
    private IAuthenticationService authService;
    private IUserProfileManager userProfileManager;

    [Inject]
    public void Inject(IAuthenticationService authService, IUserProfileManager userProfileManager)
    {
        this.authService = authService;
        this.userProfileManager = userProfileManager;
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        this.authService.Init();
        Debug.Log("Unity Services Initialized");

        this.authService.AuthStateChange += (object sender, string profileId) =>
        {
            Debug.Log("Auth State Change: " + profileId);
            if (!string.IsNullOrWhiteSpace(profileId))
            {
                UserProfile profile = this.userProfileManager.GetUserProfile(profileId);
                this.userProfileManager.SetCurrentUser(profile);
            }
        };


        await this.authService.SignInAnonymouslyAsync();
    }
}
