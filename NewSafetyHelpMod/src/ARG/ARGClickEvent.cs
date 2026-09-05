using UnityEngine;

namespace NewSafetyHelp.ARG
{
    public class ARGClickEvent : MonoBehaviour
    {
        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;

        /// <summary>
        /// Click Event for opening the keypad popup.
        /// </summary>
        public void OpenKeyPadPopup()
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= DoubleClickThreshold)
            {
                ARGKeypadLogic.OpenKeyPadPopup();
            }

            lastClickTime = Time.time;
        }
    }
}