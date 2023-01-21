namespace SpaceRpg.UnityServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public struct UnityServiceErrorMessage
    {
        public enum Service
        {
            Authentication,
            Lobby
        }

        public string Title;
        public string Message;
        public Service AffectedService;
        public Exception OriginalException;

        public UnityServiceErrorMessage(string title, string message, Service service, Exception originalException = null)
        {
            this.Title = title;
            this.Message = message;
            this.AffectedService = service;
            this.OriginalException = originalException;
        }
    }
}