using UnityEngine;

namespace NewSafetyHelp.ARG.ARGLogic
{
    public class ARGAchievementClickEvent : MonoBehaviour
    {
        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;

        /// <summary>
        /// Click Event for opening the keypad popup.
        /// </summary>
        public void OpenAchievementPopup()
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= DoubleClickThreshold)
            {
                ARGAchievementLogic.OpenAchievementPopup();
            }

            lastClickTime = Time.time;
        }
    }
}