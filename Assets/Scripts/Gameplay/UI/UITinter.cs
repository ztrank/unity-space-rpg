namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class UITinter : MonoBehaviour
    {
        [SerializeField]
        Color[] m_TintColors;
        Image m_Image;

        void Awake()
        {
            this.m_Image = this.GetComponent<Image>();
        }

        public void SetToColor(int colorIndex)
        {
            if (colorIndex >= this.m_TintColors.Length)
            {
                return;
            }

            this.m_Image.color = m_TintColors[colorIndex];
        }
    }
}