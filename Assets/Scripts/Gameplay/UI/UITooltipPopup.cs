namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class UITooltipPopup : MonoBehaviour
    {
        [SerializeField] Canvas m_Canvas;
        [SerializeField]
        [Tooltip("This transform is shown/hidden to show/hide the popup box")]
        private GameObject m_WindowRoot;

        [SerializeField]
        private TextMeshProUGUI m_TextField;

        [SerializeField]
        private Vector3 m_CursorOffset;

        private void Awake()
        {
            Assert.IsNotNull(m_Canvas);
        }

        public void ShowTooltip(string text, Vector3 screenXy)
        {
            screenXy += this.m_CursorOffset;
            this.m_WindowRoot.transform.position = this.GetCanvasCoords(screenXy);
            this.m_TextField.text = text;
            this.m_WindowRoot.SetActive(true);
        }

        public void HideTooltip()
        {
            this.m_WindowRoot.SetActive(false);
        }

        private Vector3 GetCanvasCoords(Vector3 screenCoords)
        {
            Vector2 canvasCoords;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.m_Canvas.transform as RectTransform,
                screenCoords,
                this.m_Canvas.worldCamera,
                out canvasCoords);

            return this.m_Canvas.transform.TransformPoint(canvasCoords);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.gameObject.scene.rootCount > 1)
            {
                if (!this.m_Canvas)
                {
                    this.m_Canvas = FindObjectOfType<Canvas>();
                }
            }
        }
#endif
    }
}