using MelonLoader;
using NewSafetyHelp.ARG.Achievements.GUI.AchievementPopup;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.ARG.Achievements
{
    public static class ARGAchievementHelper
    {
        /// <summary>
        /// Unlocks a given achievement with popup.
        /// </summary>
        /// <param name="customAchievement"></param>
        /// <param name="appearDelay"></param>
        private static void UnlockAchievement(CustomAchievement customAchievement, float appearDelay = 1)
        {
            if (!customAchievement.AchievementPreference.Value)
            {
                LoggingHelper.DebugLog($"Unlocking the achievement: '{customAchievement.AchievementName}'.");

                customAchievement.AchievementPreference.Value = true;

                MelonCoroutines.Start(AchievementPopupGUI.TriggerAchievementPopupDesktop(customAchievement,
                    appearDelay));
            }
        }

        /// <summary>
        /// Checks if to unlock a given arcade achievement.
        /// </summary>
        /// <param name="amountOfArcadeCallers"></param>
        public static void UnlockArcadeAchievement(int amountOfArcadeCallers)
        {
            if (amountOfArcadeCallers >= 3
                && !ARGAchievementSave.Critter101.AchievementPreference.Value)
            {
                UnlockAchievement(ARGAchievementSave.Critter101);
            }

            if (amountOfArcadeCallers >= 5
                && !ARGAchievementSave.ClockedIn.AchievementPreference.Value)
            {
                UnlockAchievement(ARGAchievementSave.ClockedIn);
            }

            if (amountOfArcadeCallers >= 8
                && !ARGAchievementSave.SafetyScholar.AchievementPreference.Value)
            {
                UnlockAchievement(ARGAchievementSave.SafetyScholar);
            }

            if (amountOfArcadeCallers >= 10
                && !ARGAchievementSave.DrSafety.AchievementPreference.Value)
            {
                UnlockAchievement(ARGAchievementSave.DrSafety);
            }
        }

        /// <summary>
        /// Unlocks the day achievements.
        /// </summary>
        public static void UnlockDayAchievement()
        {
            if (GlobalVariables.currentDay >= 2)
            {
                UnlockAchievement(ARGAchievementSave.TheStart);
            }

            if (GlobalVariables.currentDay >= 3)
            {
                UnlockAchievement(ARGAchievementSave.TheReturn);
            }

            if (GlobalVariables.currentDay >= 4)
            {
                UnlockAchievement(ARGAchievementSave.TheRespite);
            }

            if (GlobalVariables.currentDay >= 5)
            {
                UnlockAchievement(ARGAchievementSave.TheChorus);

                if (!ARGAchievementSave.TheClimax.AchievementPreference.Value)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return;
                    }

                    if (customCampaign.SavedGameFinished == 1)
                    {
                        UnlockAchievement(ARGAchievementSave.TheClimax);
                    }
                }
            }
        }
    }
}