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
                "Disable Dave", GlobalPreferences.DisableDave.Value);
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
    }
}