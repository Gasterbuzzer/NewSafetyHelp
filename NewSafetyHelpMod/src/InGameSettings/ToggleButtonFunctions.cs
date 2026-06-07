using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.InGameSettings
{
    public static class ToggleButtonFunctions
    {
        public static bool OnDebugLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowDebugLogs.Value = toggleValue;

            LoggingHelper.InfoLog($"Debug Log Toggle changed to '{toggleValue}'.");

            return toggleValue;
        }

        public static bool OnSkipComputerSceneToggle(bool toggleValue)
        {
            GlobalPreferences.SkipComputerScene.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnShowSkippedCallerLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowSkippedCallerDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnThemeLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowThemeDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnRingtoneLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowRingtoneDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnEmailLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowEmailDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnVideoLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowVideoDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnSkipLoadingScreenToggle(bool toggleValue)
        {
            GlobalPreferences.SkipLoadingScreen.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnEntryLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowEntryDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnTextFileLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowTextFileDebugLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnCutsceneLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowCutsceneLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnMemoryLogToggle(bool toggleValue)
        {
            GlobalPreferences.ShowMemoryLog.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnSkipDayClockInToggle(bool toggleValue)
        {
            GlobalPreferences.SkipDayClockIn.Value = toggleValue;

            return toggleValue;
        }

        public static bool OnVsyncToggle(bool toggleValue)
        {
            GlobalPreferences.Vsync.Value = toggleValue;

            if (GlobalPreferences.Vsync.Value)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }

            return toggleValue;
        }
    }
}