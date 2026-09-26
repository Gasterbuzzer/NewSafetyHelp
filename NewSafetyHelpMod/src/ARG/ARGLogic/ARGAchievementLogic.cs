using UnityEngine;

namespace NewSafetyHelp.ARG.ARGLogic
{
    public static class ARGAchievementLogic
    {
        // GameObject References
        private static GameObject achievementPopup;

        public static bool AchievementIsOpen;

        /// <summary>
        /// Opens the achievement popup prompt.
        /// </summary>
        public static void OpenAchievementPopup()
        {
            if (achievementPopup != null)
            {
                achievementPopup.SetActive(true);
                AchievementIsOpen = true;
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
                AchievementIsOpen = false;
            }
        }

        /// <summary>
        /// Sets the achievement popup correctly.
        /// </summary>
        /// <param name="achievementGameObject"></param>
        public static void SetKeypadPopup(GameObject achievementGameObject)
        {
            achievementPopup = achievementGameObject;
        }
    }
}