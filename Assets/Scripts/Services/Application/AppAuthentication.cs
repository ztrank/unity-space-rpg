using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public interface IAuthenticationService
{
    event EventHandler<string> AuthStateChange;
    bool IsAuthenticated { get; }
    Task SignInAnonymouslyAsync();
    void Init();
}

/// <summary>
/// Authentication service for the application
/// </summary>
/// <remarks>
/// Handles authenticating the user using the Unity AuthenticationService.
/// </remarks>
public class AppAuthentication : IAuthenticationService
{
    private bool isInitialized = false;

    /// <summary>
    /// Event that fires when the authentication state changes. If the argument is null, the user is unauthenticated. Otherwise it is the profile Id.
    /// </summary>
    /// <remarks>
    /// The Profile Id is unique and unchanging per player. However it can change if the user doesn't use an external provider and switches devices.
    /// </remarks>
    public event EventHandler<string> AuthStateChange;

    /// <summary>
    /// True if the user is authenticated.
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            return AuthenticationService.Instance.IsAuthorized;
        }
    }

    /// <summary>
    /// Signs the user in using the annonymous sign in.
    /// </summary>
    /// <remarks>
    /// This will log them in no matter who their provider is as long as there is a cached token. This can be used immediately, then combined with linking to a provider when those are implemented.
    /// </remarks>
    /// <returns>
    /// Task when complete.
    /// </returns>
    public async Task SignInAnonymouslyAsync()
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

    /// <summary>
    /// Sets the listeners to the unity AuthenticationService.
    /// </summary>
    public void Init()
    {
        if (!this.isInitialized)
        {
            this.isInitialized = true;
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
    }
}
