using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CallerPatches;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign
{
    public class CustomCampaignExtraInfo
    {
        public string campaignName = "NO_CAMPAIGN_NAME_PROVIDED";

        public int campaignDays = 7;
        
        // Desktop
        public Sprite campaignIcon = null;
        public string campaignDesktopName = "NO_CAMPAIGN_DESKTOP_NAME";
        
        // In Game
        public List<CustomCallerExtraInfo> customCallersInCampaign = new List<CustomCallerExtraInfo>();
        
        public List<string> campaignDayStrings = new List<string>();
        
        // Saving
        public MelonPreferences_Category campaignSaveCategory = null;

        public int currentDay = 1;

        public int savedCurrentCaller = 0;

        public int currentPermissionTier = 1;

        public int savedCallerArrayLength = 0;

        public List<bool> savedCallersCorrectAnswer = new List<bool>();
        
        
        // Special Saves
        public int savedGameFinished = 0;
        public int savedGameFinishedDisplay = 0;
        
        // Saved scores for the day. (Used for unlocking emails or icons)
        public List<float> savedDayScores = new List<float>();
    }
}