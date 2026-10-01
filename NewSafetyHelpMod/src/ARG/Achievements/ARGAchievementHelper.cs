using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.ARG.Achievements
{
    public static class ARGAchievementHelper
    {
        private static void UnlockAchievement(CustomAchievement achievement)
        {
            LoggingHelper.TestLog(achievement.AchievementName);
        }

        public static void UnlockDayAchievement()
        {
            switch (GlobalVariables.currentDay)
            {
                case 2:
                    if (!ARGAchievementSave.TheStart.AchievementPreference.Value)
                    {
                        ARGAchievementSave.TheStart.AchievementPreference.Value = true;
                        UnlockAchievement(ARGAchievementSave.TheStart);
                    }

                    break;
                
                case 3:
                    if (!ARGAchievementSave.TheReturn.AchievementPreference.Value)
                    {
                        ARGAchievementSave.TheReturn.AchievementPreference.Value = true;
                        UnlockAchievement(ARGAchievementSave.TheReturn);
                    }

                    break;
                
                case 4:
                    if (!ARGAchievementSave.TheRespite.AchievementPreference.Value)
                    {
                        ARGAchievementSave.TheRespite.AchievementPreference.Value = true;
                        UnlockAchievement(ARGAchievementSave.TheRespite);
                    }

                    break;
                
                case 5:
                    if (!ARGAchievementSave.TheChorus.AchievementPreference.Value)
                    {
                        ARGAchievementSave.TheChorus.AchievementPreference.Value = true;
                        UnlockAchievement(ARGAchievementSave.TheChorus);
                    }
                    
                    if (!ARGAchievementSave.TheClimax.AchievementPreference.Value)
                    {
                        //ARGAchievementSave.TheChorus.AchievementPreference.Value = true;
                        UnlockAchievement(ARGAchievementSave.TheClimax);
                    }

                    break;
            }
        }
    }
}