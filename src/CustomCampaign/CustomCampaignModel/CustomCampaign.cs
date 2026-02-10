using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.CustomCampaign.Themes;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager.EntryData;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.CustomCampaignModel
{
    public class CustomCampaign
    {
        public string CampaignName = "NO_CAMPAIGN_NAME_PROVIDED";

        public int CampaignDays = 7;

        // Desktop
        public Sprite CampaignIcon = null;
        public string CampaignDesktopName = "NO_CAMPAIGN_DESKTOP_NAME";

        public List<List<string>> LoadingTexts = new List<List<string>>();

        /*
         * In Game
         */

        // Caller in the campaign
        public List<CallerPatches.CallerModel.CustomCCaller> CustomCallersInCampaign = new List<CallerPatches.CallerModel.CustomCCaller>();

        // Warning Callers in the campaign
        public List<CallerPatches.CallerModel.CustomCCaller> CustomWarningCallersInCampaign = new List<CallerPatches.CallerModel.CustomCCaller>();

        // Game Over Callers in the campaign
        public List<CallerPatches.CallerModel.CustomCCaller> CustomGameOverCallersInCampaign = new List<CallerPatches.CallerModel.CustomCCaller>();

        // Entries that exist only in this campaign.  
        public List<EntryMetadata> EntriesOnlyInCampaign = new List<EntryMetadata>();

        // Entries that should only replace in custom campaign.
        public List<EntryMetadata> EntryReplaceOnlyInCampaign = new List<EntryMetadata>();

        public List<string> CampaignDayStrings = new List<string>();

        public bool RemoveExistingEntries = false; // Removes all existing entries and only shows custom entries.

        // Resets all default entries to not needing any permission to be viewed. (Like a continuation of the main game)
        public bool ResetDefaultEntriesPermission = false;

        // If main game entries get reset, they do not keep the NEW tag.
        // If one however, does want it to be included, then one can use this option.
        public bool DoShowNewTagForMainGameEntries = false;

        public int GameOverThreshold = 60; // Game Over Threshold
        public int WarningThreshold = 60; // Warning Threshold

        // Amount of calls per day until the warning is allowed to appear.
        public List<int> WarningCallThresholdCallerAmounts = new List<int>();

        public bool SkipCallersCorrectly = false; // If all the callers should be marked as correct and skipped.

        // Date and Username
        public string DesktopUsernameText = "";
        
        public int DesktopDateStartYear = -1;
        public int DesktopDateStartMonth = -1;
        public int DesktopDateStartDay = -1;
        public bool UseEuropeDateFormat = false;

        // Saving
        public MelonPreferences_Category CampaignSaveCategory = null;

        public int CurrentDay = 1;

        public int SavedCurrentCaller = 0;

        public int CurrentPermissionTier = 1;

        public int SavedCallerArrayLength = 0;

        public List<bool> SavedCallersCorrectAnswer = new List<bool>();
            
        // Special Saves
        public int SavedGameFinished = 0;
        public int SavedGameFinishedDisplay = 0;
        
        /*
         * Options Saved
         */

        // Volume
        public float SavedMusicVolume = 1.0f;
        public float SavedSFXVolume = 1.0f;
        public float SavedAmbienceVolume = 1.0f;

        // Screen Effects
        public bool SavedCRTToggle = true; // If to use the CRT Screen effect.
        
        // Screen Options
        public bool SavedFullScreenToggle = true; // If fullscreen is enabled.
        public int SavedScreenHeight = 1080; // Screen Height
        public int SavedScreenWidth = 1920; // Screen Width
        public int SavedRefreshRate = 180; // Screen Refresh Rate
        
        // Text Settings
        public bool SavedDyslexiaToggle = false;
        public float SavedTextSizeMultiplier = 1.0f;
        
        // Phobias
        public bool SavedSpiderToggle = false;
        public bool SavedInsectToggle = false;
        public bool SavedDarkToggle = false;
        public bool SavedHoleToggle = false;
        public bool SavedWatchToggle = false;
        public bool SavedDogToggle = false;
        public bool SavedTightToggle = false;
        
        // Saved Cheat Options
        public bool SavedImmunityToggle = false;
        public bool SavedAccuracyToggle = false;
        public bool SavedCallSkipToggle = false;

        // Video Cutscenes
        public string EndCutsceneVideoName = ""; // Video shown at the end of the game.
        public string GameOverCutsceneVideoName = ""; // Video shown at game over.

        // Music
        public bool AlwaysRandomMusic = true; // If the provided music is to be always randomly chosen. 

        public bool RemoveDefaultMusic = false; // If to remove the default music from the game.

        public List<CustomMusic> CustomMusic = new List<CustomMusic>(); // List of custom music.
        
        public List<CustomMusic> CustomIntermissionMusic = new List<CustomMusic>(); // List of intermission music.

        // Always enabled Programs on Desktop
        public bool EntryBrowserAlwaysActive = false;
        public bool ScorecardAlwaysActive = false;
        public bool ArtbookAlwaysActive = false;
        public bool ArcadeAlwaysActive = false;

        // Program rename.
        public string RenameMainGameDesktopIcon = null; // If not empty, it renames the main game desktop icon.

        // Changes the sprite (if not null) of the main game desktop icon.
        public Sprite ChangeMainGameDesktopIcon = null;

        // Always enable in main game

        // If to show the "Next Caller" button, which skips the next caller wait time.
        public bool AlwaysSkipCallButton = false;

        // Email
        public bool RemoveDefaultEmails = true;
        public List<CustomEmail> Emails = new List<CustomEmail>(); // List of custom emails.

        // Backgrounds
        public List<Sprite> BackgroundSprites = new List<Sprite>();
        public Sprite GameFinishedBackground = null;

        // If to disable the desktop logo "Home Safety Hotline" (Also disables custom ones)
        public bool DisableDesktopLogo = false;

        public Sprite CustomDesktopLogo = null; // Logo to show in desktop (if not disabled)
        public float CustomDesktopLogoTransparency = 0.2627f;

        // If to disable the color the background green the same as the main game does.
        public bool DisableGreenColorBackground = false;

        // Video Programs
        public bool DisableAllDefaultVideos = true;
        
        public List<CustomVideo> AllDesktopVideos = new List<CustomVideo>();

        // Saved scores for the day. (Used for unlocking emails or icons)
        public List<float> SavedDayScores = new List<float>();
        
        // Themes

        public bool DisablePickingThemeOption = false; // If true, it will hide the option to set the theme.
        
        public bool DefaultThemeAppliedOnce = false; // If a default theme is given, it will only be applied once, if overwritten. Too bad, we allow our users more freedom.
        
        public string DefaultTheme = null; // Default theme to be loaded when doing the campaign for the first time.
        
        public int ActiveTheme = 0; // 0 is default theme. (0-3 are reserved for the default themes)
        
        public List<CustomTheme> CustomThemesGeneral = new List<CustomTheme>(); // List of themes for general.
        public List<CustomTheme> CustomThemesDays = new List<CustomTheme>(); // List of (conditional) themes that apply for certain days and apply to a certain theme only.
        
        // Modifiers: (These work similar to themes, but they modify a specific aspect on a specific day)
        
        public List<CustomModifier> CustomModifiersGeneral = new List<CustomModifier>(); // List of modifiers for general.
        public List<CustomModifier> CustomModifiersDays = new List<CustomModifier>(); // List of (conditional) modifiers that apply for certain days.
        
        // Ringtones
        public bool DoNotAccountDefaultRingtone = true;
        public List<CustomRingtone.CustomRingtone> CustomRingtones = new List<CustomRingtone.CustomRingtone>(); // List of custom ringtones.
    }
}