using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.InGameSettings
{
    public static class ToggleButtonFunctions
    {
        public static bool OnDebugLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowDebugLogs.Value = toggleValue;
            
            LoggingHelper.InfoLog($"Debug Log Toggle changed to '{toggleValue}'.");
            
            return toggleValue;
        }
        
        public static bool OnSkipComputerSceneToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.SkipComputerScene.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnShowSkippedCallerLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowSkippedCallerDebugLog.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnThemeLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowThemeDebugLog.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnRingtoneLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowRingtoneDebugLog.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnEmailLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowEmailDebugLog.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnVideoLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowVideoDebugLog.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnSkipLoadingScreenToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.SkipLoadingScreen.Value = toggleValue;
            
            return toggleValue;
        }
        
        public static bool OnEntryLogToggle(bool toggleValue)
        {
            NewSafetyHelpMainClass.ShowEntryDebugLog.Value = toggleValue;
            
            return toggleValue;
        }
    }
}