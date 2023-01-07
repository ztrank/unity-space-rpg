using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditProfileController : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField bioField;
    private UserProfile userProfile;


    // Start is called before the first frame update
    void Start()
    {
        if (AppState.UserProfileManager.Current == null)
        {
            AppState.UserProfileManager.CurrentUserChange += this.SetUser;
        }
        else
        {
            this.SetUser(this, AppState.UserProfileManager.Current);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        this.RemoveListeners();
    }

    public void Save()
    {
        this.StartCoroutine(this.SaveProfile(() => this.Return()));
    }

    public void Cancel()
    {
        this.Return();
    }

    private void SetUser(object sender, UserProfile profile)
    {
        Debug.Log("Setting Profile");
        this.nameField.text = profile.Name;
        this.bioField.text = profile.Bio;
        this.userProfile = profile;
        this.RemoveListeners();
        
    }

    private IEnumerator SaveProfile(Action callback)
    {
        if (this.userProfile != null)
        {
            Debug.Log("Saving Profile");
            this.userProfile.Name = this.nameField.text;
            this.userProfile.Bio = this.bioField.text;
            AppState.UserProfileManager.SaveUserProfile(this.userProfile);
            yield return null;

            callback.Invoke();
        }
        else
        {
            Debug.Log("Profile Not Set");
            yield return null;
        }
    }

    private void RemoveListeners()
    {
        try
        {
            AppState.UserProfileManager.CurrentUserChange -= this.SetUser;
        }
        catch (Exception)
        {

        }
    }

    private void Return()
    {
        this.RemoveListeners();
        SceneManager.LoadScene(0);
    }
}
