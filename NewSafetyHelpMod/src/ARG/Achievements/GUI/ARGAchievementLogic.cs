using UnityEngine;

namespace NewSafetyHelp.ARG.Achievements.GUI
{
    public static class ARGAchievementLogic
    {
        // GameObject References
        private static GameObject achievementPopup;

        /// <summary>
        /// Opens the achievement popup prompt.
        /// </summary>
        public static void OpenAchievementPopup()
        {
            if (achievementPopup != null)
            {
                achievementPopup.SetActive(true);
            }
        }

        /// <summary>
        /// Closes the achievement popup prompt.
        /// </summary>
        public static void CloseAchievementPopup()
        {
            if (achievementPopup != null)
            {
                achievementPopup.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the achievement popup correctly.
        /// </summary>
        /// <param name="achievementGameObject"></param>
        public static void SetAchievementPopup(GameObject achievementGameObject)
        {
            achievementPopup = achievementGameObject;
        }
    }
}