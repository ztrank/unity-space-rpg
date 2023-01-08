using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class EditProfileController : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField bioField;
    private UserProfile userProfile;
    private IUserProfileManager userProfileManager;

    [Inject]
    public void Inject(IUserProfileManager userProfileManager)
    {
        this.userProfileManager = userProfileManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (this.userProfileManager.Current == null)
        {
            this.userProfileManager.CurrentUserChange += this.SetUser;
        }
        else
        {
            this.SetUser(this, this.userProfileManager.Current);
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
            this.userProfileManager.SaveUserProfile(this.userProfile);
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
            this.userProfileManager.CurrentUserChange -= this.SetUser;
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
