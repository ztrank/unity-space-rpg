namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class UIMessageSlot : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        [SerializeField] TextMeshProUGUI m_TextLabel;
        [SerializeField] float m_HideDelay = 10f;

        public bool IsDisplaying { get; private set; }
        public void Display(string text)
        {
            if (!this.IsDisplaying)
            {
                this.IsDisplaying = true;
                this.m_Animator.SetTrigger("Display");
                this.StartCoroutine(this.HideCoroutine());
                this.m_TextLabel.text = text;
                this.transform.parent.SetAsLastSibling();
            }
        }
        
        IEnumerator HideCoroutine()
        {
            yield return new WaitForSeconds(this.m_HideDelay);
            this.m_Animator.SetTrigger("Hide");
        }

        public void Hide()
        {
            if (!this.IsDisplaying)
            {
                this.IsDisplaying = false;
            }
        }
    }
}