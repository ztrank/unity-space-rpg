using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserProfile
{
    public string UserId { get; private set; }
    public string Name { get; set; }
    public string Bio { get; set; }

    public UserProfile(string userId)
    {
        this.UserId = userId;
    }

    public UserProfile(UserProfileData data)
    {
        this.UserId = data.UserId;
        this.Name = data.Name;
        this.Bio = data.Bio;
    }

    public UserProfileData ToData()
    {
        return new UserProfileData()
        {
            UserId = this.UserId,
            Name = this.Name,
            Bio = this.Bio
        };
    }

    [Serializable]
    public class UserProfileData
    {
        public string UserId;
        public string Name;
        public string Bio;
    }
}
