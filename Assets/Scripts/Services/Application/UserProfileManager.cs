using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public interface IUserProfileManager
{
    event EventHandler<UserProfile> CurrentUserChange;
    UserProfile Current { get; }
    UserProfile GetUserProfile(string userId);
    void SaveUserProfile(UserProfile profile);
    void SetCurrentUser(UserProfile profile);

}
public class UserProfileManager : IUserProfileManager
{
    private readonly Dictionary<string, UserProfile> cache = new Dictionary<string, UserProfile>();

    private readonly IFileService fileService;
    private readonly IEncodingService encodingService;

    [Inject]
    public UserProfileManager(IFileService fileService, IEncodingService encodingService)
    {
        this.fileService = fileService;
        this.encodingService = encodingService;
    }



    public event EventHandler<UserProfile> CurrentUserChange;
    public UserProfile Current { get; private set; }

    public UserProfile GetUserProfile(string userId)
    {
        if (this.cache.ContainsKey(userId))
        {
            return this.cache[userId];
        }

        if (this.fileService.Exists(Path.Combine(Application.persistentDataPath, "profiles", userId)))
        {
            string strData = this.encodingService.FromBytes(this.fileService.ReadAllBytes(Path.Combine(Application.persistentDataPath, "profiles", userId)));
            UserProfile.UserProfileData data = JsonUtility.FromJson<UserProfile.UserProfileData>(strData);

            this.cache.Add(userId, new UserProfile(data));
        }
        else
        {
            this.cache.Add(userId, new UserProfile(userId));
        }

        return this.cache[userId];
    }

    public void SaveUserProfile(UserProfile profile)
    {
        this.fileService.WriteAllBytes(Path.Combine(Application.persistentDataPath, "profiles", profile.UserId), this.encodingService.ToBytes(JsonUtility.ToJson(profile.ToData())));
    }

    public void SetCurrentUser(UserProfile profile)
    {
        this.Current = profile;
        this.CurrentUserChange?.Invoke(this, this.Current);
    }
}
