using MelonLoader;
using NewSafetyHelp.ImportFiles;
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

            // Days
            TheStart = new CustomAchievement("The Start", "Complete Monday.", null,
                ARGAchievementCategory);

            TheReturn = new CustomAchievement("The Return", "Complete Tuesday.", null,
                ARGAchievementCategory);

            TheRespite = new CustomAchievement("The Respite", "Complete Wednesday.", null,
                ARGAchievementCategory);

            TheChorus = new CustomAchievement("The Chorus", "Complete Thursday.", null,
                ARGAchievementCategory);

            TheClimax = new CustomAchievement("The Climax", "Complete Friday.", EmbedLoader.AdminIcon,
                ARGAchievementCategory);

            // Endings
            TheEndOfEverything = new CustomAchievement("The End Of Everything",
                "Get the bad ending for Summer Support.", null, ARGAchievementCategory);

            TheDeathOfDave = new CustomAchievement("The Death Of Dave",
                "Get the good ending for Summer Support.", null, ARGAchievementCategory);

            // Performance in the Game
            TheGoldStandard = new CustomAchievement("The Gold Standard",
                "Earn a gold star of safety.", null, ARGAchievementCategory);

            TheWorstOfTheWorst = new CustomAchievement("The Worst Of The Worst", "Die.",
                null, ARGAchievementCategory);

            TheBestOfTheBest = new CustomAchievement("The Best Of The Best",
                "Complete Summer Support with flawless accuracy.", null,
                ARGAchievementCategory);

            // Arcade Calls
            Critter101 = new CustomAchievement("Critter 101", "Complete X calls in Call Training",
                null, ARGAchievementCategory);

            ClockedIn = new CustomAchievement("Clocked In", "Complete X calls in Call Training.",
                null, ARGAchievementCategory);

            SafetyScholar = new CustomAchievement("Safety Scholar",
                "Complete X calls in Call Training.", null, ARGAchievementCategory);

            DrSafety = new CustomAchievement("Dr. Safety", "Complete X calls in Call Training.",
                null, ARGAchievementCategory);

            // Completionist
            EmployeeOfTheMonth = new CustomAchievement("Employee Of The Month",
                "Unlock every standard achievement.", null, ARGAchievementCategory);

            Archivist = new CustomAchievement("Archivist", "Read every email.", null,
                ARGAchievementCategory);

            // In game events
            FMinus = new CustomAchievement("F-", "Upset Detective Horus.", null,
                ARGAchievementCategory);

            GoAway = new CustomAchievement("Go Away", "Hang up on Supervisor Dave.", null,
                ARGAchievementCategory);

            GoodRiddance = new CustomAchievement("Good Riddance", "Get David Baldina killed.",
                null, ARGAchievementCategory);

            // ARG
            TWFuIG9uIHRoZSBTY3JlZW4 = new CustomAchievement("TWFuIG9uIHRoZSBTY3JlZW4",
                "SGVhciB0aGUgY2FsbCBvZiBhIGdvZA==", null,
                ARGAchievementCategory, true);

            ArcanaMachinae = new CustomAchievement("Arcana Machinae",
                "Gain entry to the ADMIN console.", null,
                ARGAchievementCategory, true);

            // All achievements
            EmployeeOfTheCentury = new CustomAchievement("Employee Of The Century",
                "Unlock every achievement.", null, ARGAchievementCategory);
        }
    }
}