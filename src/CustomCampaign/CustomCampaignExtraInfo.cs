using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CallerPatches;
using NewSafetyHelp.EntryManager;
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

        public List<List<string>> loadingTexts = new List<List<string>>();
        
        // In Game
        public List<CustomCallerExtraInfo> customCallersInCampaign = new List<CustomCallerExtraInfo>(); // Caller in the campaign
        
        public List<EntryExtraInfo> entriesOnlyInCampaign = new List<EntryExtraInfo>(); // Entries that exist only in this campaign.  
        public List<EntryExtraInfo> entryReplaceOnlyInCampaign = new List<EntryExtraInfo>(); // Entries that should only replace in custom campaign.
        
        public List<string> campaignDayStrings = new List<string>();
        
        public bool removeExistingEntries = false; // Removes all existing entries and only shows custom entries.
        
        public int gameOverThreshold = 60;  // Game Over Threshold
        public int warningThreshold = 60;  // Warning Threshold
        public List<int> warningCallThresholdCallerAmounts  = new List<int>(); // Amount of calls per day until the warning is allowed to appear.
        
        // Date and Username
        public string desktopUsernameText = "";
        public int desktopDateStartYear = -1;
        public int desktopDateStartMonth = -1;
        public int desktopDateStartDay = -1;
        public bool useEuropeDateFormat = false;
        
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