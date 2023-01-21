namespace SpaceRpg.Gameplay.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class ConnectionAnimation : MonoBehaviour
    {
        [SerializeField]
        private float m_RotationSpeed;

        // Update is called once per frame
        void Update()
        {
            this.transform.Rotate(new Vector3(0, 0, this.m_RotationSpeed * Mathf.PI * Time.deltaTime));
        }
    }
}