using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AppAuthentication
{
    private static AppAuthentication instance;

    public static AppAuthentication Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppAuthentication();
                instance.SetListeners();
            }

            return instance;
        }
    }

    public event EventHandler<string> AuthStateChange;

    public bool IsAuthenticated
    {
        get
        {
            return AuthenticationService.Instance.IsAuthorized;
        }
    }

    private void SetListeners()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Sign In Annonymously Succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            this.AuthStateChange?.Invoke(this, AuthenticationService.Instance.PlayerId);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Signed Out Successfully");
            this.AuthStateChange?.Invoke(this, null);
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogException(err);
            this.AuthStateChange?.Invoke(this, null);
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Session Expired and unable to reconnect.");
        };

        Debug.Log("AppAuth Listeners Set");
    }

    public async Task SignOn()
    {
        if (!this.IsAuthenticated)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
