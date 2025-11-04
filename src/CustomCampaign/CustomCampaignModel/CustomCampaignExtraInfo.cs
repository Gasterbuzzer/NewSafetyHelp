using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager.EntryData;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.CustomCampaignModel
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

        // Caller in the campaign
        public List<CustomCallerExtraInfo> customCallersInCampaign = new List<CustomCallerExtraInfo>();

        // Warning Callers in the campaign
        public List<CustomCallerExtraInfo> customWarningCallersInCampaign = new List<CustomCallerExtraInfo>();

        // Game Over Callers in the campaign
        public List<CustomCallerExtraInfo> customGameOverCallersInCampaign = new List<CustomCallerExtraInfo>();

        // Entries that exist only in this campaign.  
        public List<EntryExtraInfo> entriesOnlyInCampaign = new List<EntryExtraInfo>();

        // Entries that should only replace in custom campaign.
        public List<EntryExtraInfo> entryReplaceOnlyInCampaign = new List<EntryExtraInfo>();

        public List<string> campaignDayStrings = new List<string>();

        public bool removeExistingEntries = false; // Removes all existing entries and only shows custom entries.

        // Resets all default entries to not needing any permission to be viewed. (Like a continuation of the main game)
        public bool resetDefaultEntriesPermission = false;

        // If main game entries get reset, they do not keep the NEW tag.
        // If one however, does want it to be included, then one can use this option.
        public bool doShowNewTagForMainGameEntries = false;

        public int gameOverThreshold = 60; // Game Over Threshold
        public int warningThreshold = 60; // Warning Threshold

        // Amount of calls per day until the warning is allowed to appear.
        public List<int> warningCallThresholdCallerAmounts = new List<int>();

        public bool skipCallersCorrectly = false; // If all the callers should be marked as correct and skipped.

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

        // Music
        public bool alwaysRandomMusic = true; // If the provided music is to be always randomly chosen. 

        public bool removeDefaultMusic = false; // If to remove the default music from the game.

        public List<MusicExtraInfo> customMusic = new List<MusicExtraInfo>(); // List of custom music.

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

        // Changes the sprite (if not null) of the main game desktop icon.
        public Sprite changeMainGameDesktopIcon = null;

        // Always enable in main game

        // If to show the "Next Caller" button, which skips the next caller wait time.
        public bool alwaysSkipCallButton = false;

        // Email
        public bool removeDefaultEmails = true;
        public List<EmailExtraInfo> emails = new List<EmailExtraInfo>(); // List of custom emails.

        // Backgrounds
        public List<Sprite> backgroundSprites = new List<Sprite>();
        public Sprite gameFinishedBackground = null;

        // If to disable the desktop logo "Home Safety Hotline" (Also disables custom ones)
        public bool disableDesktopLogo = false;

        public Sprite customDesktopLogo = null; // Logo to show in desktop (if not disabled)
        public float customDesktopLogoTransparency = 0.2627f;

        // If to disable the color the background green the same as the main game does.
        public bool disableGreenColorBackground = false;

        // Video Programs
        public List<CustomVideoExtraInfo> allDesktopVideos = new List<CustomVideoExtraInfo>();

        // Saved scores for the day. (Used for unlocking emails or icons)
        public List<float> savedDayScores = new List<float>();
    }
}