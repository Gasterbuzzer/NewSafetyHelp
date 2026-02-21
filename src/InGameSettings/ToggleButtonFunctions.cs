using MelonLoader;
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
    }
}