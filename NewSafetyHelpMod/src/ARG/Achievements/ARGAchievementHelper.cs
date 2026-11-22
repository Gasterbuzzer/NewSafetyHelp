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

                // In case we have all the achievements unlocked, we unlock the "base game achievements" achievement.
                UnlockEmployeeOfTheMonthAchievement();

                UnlockEmployeeOfTheCenturyAchievement();
            }
        }

        /// <summary>
        /// Unlocks the "Complete Summer Support with flawless accuracy." (The Best Of The Best) achievement.
        /// </summary>
        public static void UnlockTheBestOfTheBest()
        {
            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign.SavedDayScores.Count >= 6
                && customCampaign.SavedDayScores[1] >= 99.9f
                && customCampaign.SavedDayScores[2] >= 99.9f
                && customCampaign.SavedDayScores[3] >= 99.9f
                && customCampaign.SavedDayScores[4] >= 99.9f
                && customCampaign.SavedDayScores[5] >= 99.9f
               )
            {
                UnlockAchievement(ARGAchievementSave.TheBestOfTheBest);
            }
        }

        /// <summary>
        /// Unlocks the game over achievement.
        /// </summary>
        public static void UnlockGoldStarAchievement(float dayScore)
        {
            if (dayScore >= 99.99f)
            {
                UnlockAchievement(ARGAchievementSave.TheGoldStandard);
            }
        }

        /// <summary>
        /// Unlocks the game over achievement.
        /// </summary>
        public static void UnlockGameOverAchievement()
        {
            UnlockAchievement(ARGAchievementSave.TheWorstOfTheWorst);
        }

        /// <summary>
        /// Checks if to unlock a given arcade achievement.
        /// </summary>
        /// <param name="amountOfArcadeCallers"></param>
        public static void UnlockArcadeAchievement(int amountOfArcadeCallers)
        {
            if (amountOfArcadeCallers >= 10
                && !ARGAchievementSave.Critter101.AchievementPreference.Value)
            {
                UnlockAchievement(ARGAchievementSave.Critter101);
            }

            if (amountOfArcadeCallers >= 20
                && !ARGAchievementSave.ClockedIn.AchievementPreference.Value)
            {
                UnlockAchievement(ARGAchievementSave.ClockedIn);
            }

            if (amountOfArcadeCallers >= 30
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

                    if (customCampaign.SavedGameFinished == 1)
                    {
                        UnlockAchievement(ARGAchievementSave.TheClimax);
                    }
                }
            }
        }

        /// <summary>
        /// Unlocks the bad ending achievement.
        /// </summary>
        public static void UnlockBadEndingAchievement()
        {
            if (!ARGAchievementSave.TheEndOfEverything.AchievementPreference.Value)
            {
                CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (customCampaign.SavedGameFinished == 1)
                {
                    UnlockAchievement(ARGAchievementSave.TheEndOfEverything);
                }
            }
        }

        /// <summary>
        /// Unlocks the achievement for having finished all base game achievements.
        /// </summary>
        private static void UnlockEmployeeOfTheMonthAchievement()
        {
            // If already unlocked.
            if (ARGAchievementSave.EmployeeOfTheMonth.AchievementPreference.Value)
            {
                return;
            }

            if (ARGAchievementSave.TheStart.AchievementPreference.Value
                && ARGAchievementSave.TheReturn.AchievementPreference.Value
                && ARGAchievementSave.TheRespite.AchievementPreference.Value
                && ARGAchievementSave.TheChorus.AchievementPreference.Value
                && ARGAchievementSave.TheClimax.AchievementPreference.Value
                //
                && ARGAchievementSave.TheEndOfEverything.AchievementPreference.Value
                //
                && ARGAchievementSave.TheGoldStandard.AchievementPreference.Value
                && ARGAchievementSave.TheWorstOfTheWorst.AchievementPreference.Value
                && ARGAchievementSave.TheBestOfTheBest.AchievementPreference.Value
                //
                && ARGAchievementSave.Critter101.AchievementPreference.Value
                && ARGAchievementSave.ClockedIn.AchievementPreference.Value
                && ARGAchievementSave.DrSafety.AchievementPreference.Value
                //
                && ARGAchievementSave.FMinus.AchievementPreference.Value
                && ARGAchievementSave.GoAway.AchievementPreference.Value
                && ARGAchievementSave.GoodRiddance.AchievementPreference.Value
               )
            {
                UnlockAchievement(ARGAchievementSave.EmployeeOfTheMonth);
            }
        }

        /// <summary>
        /// Unlocks the achievement for having finished all game achievements.
        /// </summary>
        private static void UnlockEmployeeOfTheCenturyAchievement()
        {
            // If already unlocked.
            if (ARGAchievementSave.EmployeeOfTheCentury.AchievementPreference.Value)
            {
                return;
            }

            if (ARGAchievementSave.TheStart.AchievementPreference.Value
                && ARGAchievementSave.TheReturn.AchievementPreference.Value
                && ARGAchievementSave.TheRespite.AchievementPreference.Value
                && ARGAchievementSave.TheChorus.AchievementPreference.Value
                && ARGAchievementSave.TheClimax.AchievementPreference.Value
                //
                && ARGAchievementSave.TheEndOfEverything.AchievementPreference.Value
                && ARGAchievementSave.TheDeathOfDave.AchievementPreference.Value
                //
                && ARGAchievementSave.TheGoldStandard.AchievementPreference.Value
                && ARGAchievementSave.TheWorstOfTheWorst.AchievementPreference.Value
                && ARGAchievementSave.TheBestOfTheBest.AchievementPreference.Value
                //
                && ARGAchievementSave.Critter101.AchievementPreference.Value
                && ARGAchievementSave.ClockedIn.AchievementPreference.Value
                && ARGAchievementSave.DrSafety.AchievementPreference.Value
                //
                && ARGAchievementSave.FMinus.AchievementPreference.Value
                && ARGAchievementSave.GoAway.AchievementPreference.Value
                && ARGAchievementSave.GoodRiddance.AchievementPreference.Value
                //
                && ARGAchievementSave.EmployeeOfTheMonth.AchievementPreference.Value
                //
                && ARGAchievementSave.TWFuIG9uIHRoZSBTY3JlZW4.AchievementPreference.Value
                && ARGAchievementSave.ArcanaMachinae.AchievementPreference.Value
               )
            {
                UnlockAchievement(ARGAchievementSave.EmployeeOfTheCentury);
            }
        }

        /// <summary>
        /// Unlocks the caller achievements.
        /// </summary>
        /// <param name="callerName">Name of the caller.</param>
        /// <param name="callerID">Order/ID of the caller.</param>
        public static void UnlockCallerAchievement(string callerName, int callerID)
        {
            if (callerID == 96
                && callerName.Equals("Detective Horus"))
            {
                UnlockAchievement(ARGAchievementSave.FMinus);
            }
            else if (callerID == 14 && callerName.Equals("Supervisor Dave"))
            {
                UnlockAchievement(ARGAchievementSave.GoodRiddance);
            }
        }
    }
}