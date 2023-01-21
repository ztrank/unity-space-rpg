namespace SpaceRpg.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AutoHide : MonoBehaviour
    {
        [SerializeField]
        float m_TimeToHideSeconds = 5f;

        void Start()
        {
            this.StartCoroutine(this.HideAfterSeconds());
        }

        IEnumerator HideAfterSeconds()
        {
            yield return new WaitForSeconds(this.m_TimeToHideSeconds);
            this.gameObject.SetActive(false);
        }
    }
}