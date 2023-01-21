namespace SpaceRpg.Utils.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class NetworkOverlay : MonoBehaviour
    {
        public static NetworkOverlay Instance { get; private set; }

        [SerializeField]
        GameObject m_DebugCanvasPrefab;

        Transform m_VerticalLayoutTransform;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public void AddTextToUI(string gameObjectName, string defaultText, out TextMeshProUGUI textComponent)
        {
            var rootGo = new GameObject(gameObjectName);
            textComponent = rootGo.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 28;
            textComponent.text = defaultText;
            textComponent.horizontalAlignment = HorizontalAlignmentOptions.Left;
            textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
            textComponent.raycastTarget = false;
            textComponent.autoSizeTextContainer = true;

            var rectTransform = rootGo.GetComponent<RectTransform>();
            AddToUI(rectTransform);
        }

        public void AddToUI(RectTransform displayTransform)
        {
            if (this.m_VerticalLayoutTransform == null)
            {
                this.CreateDebugCanvas();
            }

            displayTransform.sizeDelta = new Vector2(100f, 24f);
            displayTransform.SetParent(this.m_VerticalLayoutTransform);
            displayTransform.SetAsFirstSibling();
            displayTransform.localScale = Vector3.one;
        }

        private void CreateDebugCanvas()
        {
            var canvas = Instantiate(this.m_DebugCanvasPrefab, this.transform);
            this.m_VerticalLayoutTransform = canvas.GetComponentInChildren<VerticalLayoutGroup>().transform;
        }
    }

}
