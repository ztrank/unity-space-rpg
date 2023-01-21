namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class UITooltipDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        [Tooltip("The actual Tooltip that should be trigger")]
        private UITooltipPopup m_TooltipPopup;

        [SerializeField]
        [Multiline]
        [Tooltip("The text of the tooltip (this is the default text; it can also be changed in code)")]
        private string m_TooltipText;

        [SerializeField]
        [Tooltip("Should the tooltip appear instantly if the player clicks this ui element?")]
        private bool m_ActivateOnClick = true;

        [SerializeField]
        [Tooltip("The length of time the mouse needs to hover over this element before the tooltip appears (in seconds)")]
        private float m_TooltipDelay = 0.5f;

        private float m_PointerEnterTime = 0;
        private bool m_IsShowingTooltip;

        public void SetText(string text)
        {
            bool wasChanged = text != this.m_TooltipText;
            this.m_TooltipText = text;

            if (wasChanged && this.m_IsShowingTooltip)
            {
                this.HideTooltip();
                this.ShowTooltip();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.m_PointerEnterTime = Time.time;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.m_PointerEnterTime = 0;
            this.HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (this.m_ActivateOnClick)
            {
                this.ShowTooltip();
            }
        }

        private void Update()
        {
            if (this.m_PointerEnterTime != 0 && (Time.time - this.m_PointerEnterTime) > this.m_TooltipDelay)
            {
                this.ShowTooltip();
            }
        }

        private void ShowTooltip()
        {
            if (!this.m_IsShowingTooltip)
            {
                this.m_TooltipPopup.ShowTooltip(this.m_TooltipText, Input.mousePosition);
                this.m_IsShowingTooltip = true;
            }
        }

        private void HideTooltip()
        {
            if (this.m_IsShowingTooltip)
            {
                this.m_TooltipPopup.HideTooltip();
                this.m_IsShowingTooltip = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.gameObject.scene.rootCount > 1)
            {
                if (!this.m_TooltipPopup)
                {
                    this.m_TooltipPopup = FindObjectOfType<UITooltipPopup>();
                }
            }
        }
#endif
    }
}