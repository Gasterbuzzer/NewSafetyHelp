using MelonLoader;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.ARG.Achievements
{
    public static class ARGAchievementSave
    {
        private static MelonPreferences_Category ARGAchievementCategory;

        public static CustomAchievement TheStart;
        public static CustomAchievement TheReturn;
        public static CustomAchievement TheRespite;
        public static CustomAchievement TheChorus;
        public static CustomAchievement TheClimax;
        public static CustomAchievement TheEndOfEverything;
        public static CustomAchievement TheDeathOfDave;
        public static CustomAchievement TheGoldStandard;
        public static CustomAchievement TheWorstOfTheWorst;
        public static CustomAchievement TheBestOfTheBest;
        public static CustomAchievement Critter101;
        public static CustomAchievement ClockedIn;
        public static CustomAchievement SafetyScholar;
        public static CustomAchievement DrSafety;
        public static CustomAchievement EmployeeOfTheMonth;
        public static CustomAchievement Archivist;
        public static CustomAchievement FMinus;
        public static CustomAchievement GoAway;
        public static CustomAchievement GoodRiddance;
        public static CustomAchievement TWFuIG9uIHRoZSBTY3JlZW4;
        public static CustomAchievement ArcanaMachinae;
        public static CustomAchievement EmployeeOfTheCentury;

        public static void InitializeARGAchievements()
        {
            LoggingHelper.DebugLog("Initializing ARG Achievements...");

            ARGAchievementCategory = MelonPreferences.CreateCategory("ARG_ACHIEVEMENTS");

            /*
             * Initialize all achievements.
             */

            TheStart = new CustomAchievement("The Start", "Complete Monday.", ARGAchievementCategory);

            TheReturn = new CustomAchievement("The Return", "Complete Tuesday.",
                ARGAchievementCategory);

            TheRespite = new CustomAchievement("The Respite", "Complete Wednesday.",
                ARGAchievementCategory);

            TheChorus = new CustomAchievement("The Chorus", "Complete Thursday.",
                ARGAchievementCategory);

            TheClimax = new CustomAchievement("The Climax", "Complete Friday.",
                ARGAchievementCategory);

            TheEndOfEverything = new CustomAchievement("The End Of Everything",
                "Get the bad ending for Summer Support.", ARGAchievementCategory);

            TheDeathOfDave = new CustomAchievement("The Death Of Dave",
                "Get the good ending for Summer Support.", ARGAchievementCategory);

            TheGoldStandard = new CustomAchievement("The Gold Standard",
                "Earn a gold star of safety.", ARGAchievementCategory);

            TheWorstOfTheWorst = new CustomAchievement("The Worst Of The Worst", "Die.",
                ARGAchievementCategory);

            TheBestOfTheBest = new CustomAchievement("The Best Of The Best",
                "Complete Summer Support with flawless accuracy.", ARGAchievementCategory);

            Critter101 = new CustomAchievement("Critter 101", "Complete X calls in Call Training",
                ARGAchievementCategory);

            ClockedIn = new CustomAchievement("Clocked In", "Complete X calls in Call Training.",
                ARGAchievementCategory);

            SafetyScholar = new CustomAchievement("Safety Scholar",
                "Complete X calls in Call Training.", ARGAchievementCategory);

            DrSafety = new CustomAchievement("Dr. Safety", "Complete X calls in Call Training.",
                ARGAchievementCategory);

            EmployeeOfTheMonth = new CustomAchievement("Employee Of The Month",
                "Unlock every standard achievement.", ARGAchievementCategory);

            Archivist = new CustomAchievement("Archivist", "Read every email.",
                ARGAchievementCategory);

            FMinus = new CustomAchievement("F-", "Upset Detective Horus.",
                ARGAchievementCategory);

            GoAway = new CustomAchievement("Go Away", "Hang up on Supervisor Dave.",
                ARGAchievementCategory);

            GoodRiddance = new CustomAchievement("Good Riddance", "Get David Baldina killed.",
                ARGAchievementCategory);

            TWFuIG9uIHRoZSBTY3JlZW4 = new CustomAchievement("TWFuIG9uIHRoZSBTY3JlZW4",
                "SGVhciB0aGUgY2FsbCBvZiBhIGdvZA==",
                ARGAchievementCategory, true);

            ArcanaMachinae = new CustomAchievement("Arcana Machinae",
                "Gain entry to the ADMIN console.", ARGAchievementCategory, true);

            EmployeeOfTheCentury = new CustomAchievement("Employee Of The Century",
                "Unlock every achievement.", ARGAchievementCategory);
        }
    }
}