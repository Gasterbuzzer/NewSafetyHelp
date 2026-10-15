using NewSafetyHelp.ARG.Achievements;
using NewSafetyHelp.InGameSettings;
using UnityEngine;

namespace NewSafetyHelp.ARG.ARGLogic
{
    public static class ARGSettings
    {
        /// <summary>
        /// Creates the custom settings for the ARG.
        /// </summary>
        public static void CreateCustomInGameSettings()
        {
            GameObject customCampaignSection = InGameSettingHelper.CreateNewSettingsSection("Custom Campaign Settings",
                "Settings about the custom campaign.");

            InGameSettingHelper.CreateNewToggle(customCampaignSection, OnDisableDaveToggle,
                "Disable Dave in Arcade Mode", GlobalPreferences.DisableDave.Value);

            InGameSettingHelper.CreateButton(customCampaignSection, ResetAchievements,
                "Reset Achievements", "Resets all the achievements.");
        }

        /// <summary>
        /// Event that gets triggered when the disable dave toggle is changed.
        /// </summary>
        /// <param name="toggleValue">Value of the toggle.</param>
        /// <returns>Newly updated value.</returns>
        private static bool OnDisableDaveToggle(bool toggleValue)
        {
            GlobalPreferences.DisableDave.Value = toggleValue;

            return toggleValue;
        }

        /// <summary>
        /// Resets all the achievements.
        /// </summary>
        /// <param name="resetButton">Button that is the reset button.</param>
        /// <returns>GameObjects to be reset.</returns>
        private static GameObject ResetAchievements(GameObject resetButton)
        {
            ARGAchievementSave.ResetAllAchievements();

            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");

            ARGHelper.CreateErrorMessage("All achievements were reset.", mainMenuCanvas.transform,
                "STATUS INFO");

            return resetButton;
        }
    }
}