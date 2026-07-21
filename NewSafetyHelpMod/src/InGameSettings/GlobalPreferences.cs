using MelonLoader;

namespace NewSafetyHelp.InGameSettings
{
    public static class GlobalPreferences
    {
        // Category for Entries (So that they can be saved upon quitting the game)
        public static MelonPreferences_Category PersistantEntrySave;

        private static MelonPreferences_Category mainModSettings;

        public static MelonPreferences_Entry<bool> Vsync;

        public static MelonPreferences_Entry<bool> SkipComputerScene; // If to skip the initial computer scene.

        public static MelonPreferences_Entry<bool> SkipLoadingScreen; // If to skip the loading texts part.

        public static MelonPreferences_Entry<bool> SkipDayClockIn; // If to skip the clock in part.

        public static MelonPreferences_Entry<bool> ShowDebugLogs; // If to show the debug logs at all.

        // If to show the skipped callers debug log.
        public static MelonPreferences_Entry<bool> ShowSkippedCallerDebugLog;

        public static MelonPreferences_Entry<bool> ShowThemeDebugLog; // If to show the logs for theme info.
        public static MelonPreferences_Entry<bool> ShowRingtoneDebugLog; // If to show the logs for ringtone info.
        public static MelonPreferences_Entry<bool> ShowEmailDebugLog; // If to show the logs for email info.
        public static MelonPreferences_Entry<bool> ShowVideoDebugLog; // If to show the logs for video info.
        public static MelonPreferences_Entry<bool> ShowEntryDebugLog; // If to show the logs for entry info.
        public static MelonPreferences_Entry<bool> ShowTextFileDebugLog; // If to show the logs for text file info.
        public static MelonPreferences_Entry<bool> ShowCutsceneLog; // If to show the logs for cutscenes info.
        public static MelonPreferences_Entry<bool> ShowMemoryLog; // If to show the logs for memory usage info.
        public static MelonPreferences_Entry<bool> ShowLinkAppLog; // If to show the logs for link apps.

        public static void InitializeMelonPreferences()
        {
            // Entries are created when needed.
            PersistantEntrySave = MelonPreferences.CreateCategory("EntryAlreadyCalled");

            // Settings
            mainModSettings = MelonPreferences.CreateCategory("MainModSettings");

            Vsync = mainModSettings.CreateEntry("Vsync", false);

            SkipComputerScene = mainModSettings.CreateEntry("SkipComputerScene", false);

            SkipLoadingScreen = mainModSettings.CreateEntry("SkipLoadingScreen", false);

            SkipDayClockIn = mainModSettings.CreateEntry("SkipDayClockIn", false);

            ShowDebugLogs = mainModSettings.CreateEntry("ShowDebugLogs", false);
            ShowSkippedCallerDebugLog = mainModSettings.CreateEntry("ShowSkippedCallerDebugLog", false);
            ShowThemeDebugLog = mainModSettings.CreateEntry("ShowThemeDebugLog", false);
            ShowRingtoneDebugLog = mainModSettings.CreateEntry("ShowRingtoneDebugLog", false);
            ShowEmailDebugLog = mainModSettings.CreateEntry("ShowEmailDebugLog", false);
            ShowVideoDebugLog = mainModSettings.CreateEntry("ShowVideoDebugLog", false);
            ShowEntryDebugLog = mainModSettings.CreateEntry("ShowEntryDebugLog", false);
            ShowTextFileDebugLog = mainModSettings.CreateEntry("ShowTextFileDebugLog", false);
            ShowCutsceneLog = mainModSettings.CreateEntry("ShowCutsceneLog", false);
            ShowMemoryLog = mainModSettings.CreateEntry("ShowMemoryLog", false);
            ShowLinkAppLog = mainModSettings.CreateEntry("ShowLinkAppLog", false);
        }
    }
}