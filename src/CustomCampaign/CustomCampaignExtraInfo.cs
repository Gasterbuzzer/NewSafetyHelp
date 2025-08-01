using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CallerPatches;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
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
        
        public List<CustomCallerExtraInfo> customWarningCallersInCampaign = new List<CustomCallerExtraInfo>(); // Warning Callers in the campaign
        
        public List<CustomCallerExtraInfo> customGameOverCallersInCampaign = new List<CustomCallerExtraInfo>(); // Game Over Callers in the campaign
        
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
        
        // Video Cutscenes
        public string endCutsceneVideoName = ""; // Video shown at the end of the game.
        public string gameOverCutsceneVideoName = ""; // Video shown at game over.
        
        // Special Saves
        public int savedGameFinished = 0;
        public int savedGameFinishedDisplay = 0;
        
        // Always enabled Programs on Desktop
        public bool entryBrowserAlwaysActive = false;
        public bool scorecardAlwaysActive = false;
        public bool artbookAlwaysActive = false;
        public bool arcadeAlwaysActive = false;
        
        public bool disableAllDefaultVideos = true;
        
        // Program rename.
        public string renameMainGameDesktopIcon = null; // If not empty, it renames the main game desktop icon.
        public Sprite changeMainGameDesktopIcon = null; // Changes the sprite (if not null) of the main game desktop icon.
        
        // Always enable in main game
        public bool alwaysSkipCallButton = false; // If to show the "Next Caller" button, which skips the next caller wait time.
        
        // Email
        public bool removeDefaultEmails = true;
        public List<EmailExtraInfo> emails = new List<EmailExtraInfo>(); // List of custom emails.
        
        // Backgrounds
        public List<Sprite> backgroundSprites = new List<Sprite>();
        public Sprite gameFinishedBackground = null;

        public bool disableDesktopLogo = false; // If to disable the desktop logo "Home Safety Hotline" (Also disables custom ones)
        public Sprite customDesktopLogo = null; // Logo to show in desktop (if not disabled)
        public float customDesktopLogoTransparency = 0.2627f;

        public bool disableGreenColorBackground = false; // If to disable the color the background green the same as the main game does.
        
        // Video Programs
        public List<CustomVideoExtraInfo> allDesktopVideos = new List<CustomVideoExtraInfo>();
        
        // Saved scores for the day. (Used for unlocking emails or icons)
        public List<float> savedDayScores = new List<float>();
    }
}