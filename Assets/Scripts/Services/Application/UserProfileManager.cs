using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserProfileManager
{
    private static UserProfileManager instance;

    private readonly Dictionary<string, UserProfile> cache = new Dictionary<string, UserProfile>();

    public static UserProfileManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UserProfileManager();
            }

            return instance;
        }
    }

    public event EventHandler<UserProfile> CurrentUserChange;
    public UserProfile Current { get; private set; }

    public UserProfile GetUserProfile(string userId)
    {
        if (this.cache.ContainsKey(userId))
        {
            return this.cache[userId];
        }

        if (FileService.Instance.FileExists(Path.Combine(Application.persistentDataPath, "profiles", userId)))
        {
            string strData = FileService.Instance.FromBytes(FileService.Instance.ReadAllBytes(Path.Combine(Application.persistentDataPath, "profiles", userId)));
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
        FileService.Instance.WriteAllBytes(Path.Combine(Application.persistentDataPath, "profiles", profile.UserId), FileService.Instance.ToBytes(JsonUtility.ToJson(profile.ToData())));
    }

    public void SetCurrentUser(UserProfile profile)
    {
        this.Current = profile;
        this.CurrentUserChange?.Invoke(this, this.Current);
    }
}
