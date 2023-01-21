namespace SpaceRpg.Gameplay.UI
{
    using SpaceRpg.ApplicationLifecycle.Messages;
    using SpaceRpg.ConnectionManagement;
    using SpaceRpg.Infrastructure;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VContainer;

    public class UIQuitPanel : MonoBehaviour
    {
        enum QuitMode
        {
            ReturnToMenu,
            QuitApplication
        }

        [SerializeField] QuitMode m_QuitMode = QuitMode.ReturnToMenu;

        [Inject] ConnectionManager m_ConnectionManager;
        [Inject] IPublisher<QuitApplicationMessage> m_QuitApplicationPub;

        public void Quit()
        {
            switch (this.m_QuitMode)
            {
                case QuitMode.ReturnToMenu:
                    this.m_ConnectionManager.RequestShutdown();
                    break;
                case QuitMode.QuitApplication:
                    this.m_QuitApplicationPub.Publish(new QuitApplicationMessage());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.gameObject.SetActive(false);
        }
    }
}