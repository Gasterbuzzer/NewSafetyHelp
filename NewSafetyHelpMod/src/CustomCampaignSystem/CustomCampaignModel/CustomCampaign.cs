using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen;
using NewSafetyHelp.CustomCampaignSystem.CustomTextFiles;
using NewSafetyHelp.CustomCampaignSystem.CutsceneLogic;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.CustomCampaignSystem.Themes;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager.EntryData;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel
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
         * Callers
         */

        // Caller in the campaign
        public List<CustomCCaller> CustomCallersInCampaign = new List<CustomCCaller>();

        // Warning Callers in the campaign
        public List<CustomCCaller> CustomWarningCallersInCampaign = new List<CustomCCaller>();

        // Game Over Callers in the campaign
        public List<CustomCCaller> CustomGameOverCallersInCampaign = new List<CustomCCaller>();

        public int GameOverThreshold = 60; // Game Over Threshold
        public int WarningThreshold = 60; // Warning Threshold

        // Amount of calls per day until the warning is allowed to appear.
        public List<int> WarningCallThresholdCallerAmounts = new List<int>();

        /*
         * Entries
         */

        // Entries that exist only in this campaign.  
        public List<EntryMetadata> EntriesOnlyInCampaign = new List<EntryMetadata>();

        // Entries that should only replace in custom campaign.
        public List<EntryMetadata> EntryReplaceOnlyInCampaign = new List<EntryMetadata>();

        // Removes all existing entries and only shows custom entries.
        public bool RemoveExistingEntries = false;

        // If to use the DLCs entries instead of just the main campaign entries.
        public bool UseDLCEntries = false;

        // Resets all default entries to not needing any permission to be viewed. (Like a continuation of the main game)
        public bool ResetDefaultEntriesPermission = false;

        // If main game entries get reset, they do not keep the NEW tag.
        // If one however, does want it to be included, then one can use this option.
        public bool DoShowNewTagForMainGameEntries = false;

        /*
         * Modifications
         */

        public List<string> CampaignDayStrings = new List<string>();

        public bool SkipCallersCorrectly = false; // If all the callers should be marked as correct and skipped.
        public bool GameOverImmunity = false; // If gameover is avoided.

        // Date and Username
        public string DesktopUsernameText = "";

        public int DesktopDateStartYear = -1;
        public int DesktopDateStartMonth = -1;
        public int DesktopDateStartDay = -1;
        public bool UseEuropeDateFormat = false;

        /*
         * Saving
         */

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

        // Saved scores for the day. (Used for unlocking emails or icons)
        public List<float> SavedDayScores = new List<float>();
        
        // The custom campaign logic resets the game beaten booleans to false when a reset happens, this prevents it.
        public VariableChanged<bool> ShouldResetGameBeatenVariableOnReset = new VariableChanged<bool>
        {
            Data = false
        };

        /*
         * Video Cutscenes
         */

        public string EndCutsceneVideoName = ""; // Video shown at the end of the game.
        public string GameOverCutsceneVideoName = ""; // Video shown at game over.

        public List<CustomCutscene> CustomCutscenes = new List<CustomCutscene>();

        /*
         * Music
         */

        public bool AlwaysRandomMusic = true; // If the provided music is to be always randomly chosen. 

        public bool RemoveDefaultMusic = false; // If to remove the default music from the game.

        public List<CustomMusic> CustomMusic = new List<CustomMusic>(); // List of custom music.

        public List<CustomMusic> CustomIntermissionMusic = new List<CustomMusic>(); // List of intermission music.

        /*
         * Wait Time between callers
         */

        // (1 element => Always this wait time;
        // 2 elements => Between those two times;
        // 3+ => Pick any of the ones two chose from)
        public List<float> WaitBetweenCallers = new List<float>();
        public bool EnableCustomWaitBetweenCallers = false;

        /*
         * Always enabled Programs on Desktop
         */

        public bool EntryBrowserAlwaysActive = false;
        public bool ScorecardAlwaysActive = false;
        public bool ArtbookAlwaysActive = false;
        public bool ArcadeAlwaysActive = false;

        // Program rename.
        public string RenameMainGameDesktopIcon = null; // If not empty, it renames the main game desktop icon.

        // Changes the sprite (if not null) of the main game desktop icon.
        public Sprite ChangeMainGameDesktopIcon = null;

        // If to show the "Next Caller" button, which skips the next caller wait time.
        public bool AlwaysSkipCallButton = false;

        /*
         * Emails
         */

        public bool RemoveDefaultEmails = true;
        public List<CustomEmail> Emails = new List<CustomEmail>(); // List of custom emails.

        /*
         * Backgrounds
         */

        public List<Sprite> BackgroundSprites = new List<Sprite>();
        public Sprite GameFinishedBackground = null;

        // If to disable the desktop logo "Home Safety Hotline" (Also disables custom ones)
        public bool DisableDesktopLogo = false;

        public Sprite CustomDesktopLogo = null; // Logo to show in desktop (if not disabled)
        public float CustomDesktopLogoTransparency = 0.2627f;

        // If to disable the color the background green the same as the main game does.
        public bool DisableGreenColorBackground = false;

        /*
         * Video Programs
         */

        public bool DisableAllDefaultVideos = true;

        public List<CustomVideo> CustomVideos = new List<CustomVideo>();

        /*
         * Text Files
         */

        public List<CustomTextFile> CustomTextProgramFiles = new List<CustomTextFile>();

        /*
         * Themes
         */

        public bool DisablePickingThemeOption = false; // If true, it will hide the option to set the theme.

        // If a default theme is given, it will only be applied once, if overwritten.
        // Too bad, we allow our users more freedom.
        public bool DefaultThemeAppliedOnce = false;

        public string DefaultTheme = null; // Default theme to be loaded when doing the campaign for the first time.

        public int ActiveTheme = 0; // 0 is default theme. (0-3 are reserved for the default themes)

        // List of themes for general.
        public List<CustomTheme> CustomThemesGeneral = new List<CustomTheme>();

        // List of (conditional) themes that apply for certain days and apply to a certain theme only.
        public List<CustomTheme> CustomThemesDays = new List<CustomTheme>();

        /*
         * Modifiers: (These work similar to themes, but they modify a specific aspect on a specific day)
         */

        // All the different modifiers.
        public List<ModifierSource> ModifierSources = new List<ModifierSource>();

        /*
         * Ringtones
         */

        public bool DoNotAccountDefaultRingtone = true;
        public List<CustomRingtone.CustomRingtone> CustomRingtones = new List<CustomRingtone.CustomRingtone>();
        
        /*
         * Computer 3D Scenes
         */

        public List<Computer3DScreen> CustomComputer3DScreens = new List<Computer3DScreen>();

        /*
         * Helper functions for custom campaigns.
         */
        
        /// <summary>
        /// Sorts the custom callers to the correct order.
        /// This merely helps with performance.
        /// </summary>
        public void SortCustomCallersInCustomCampaign()
        {
            CustomCallersInCampaign =
                CustomCallersInCampaign.OrderBy(customCCaller => customCCaller.OrderInCampaign).ToList();
        }
        
        /// <summary>
        /// Sorts the emails to the correct priorities and days.
        /// </summary>
        public void SortEmailsInCustomCampaign()
        {
            Emails = Emails.OrderBy(email => email.UnlockDay).ThenByDescending(email => email.EmailPriority).ToList();
        }

        /// <summary>
        /// Sorts the custom cutscenes to the correct priorities.
        /// </summary>
        public void SortCutsceneInCustomCampaign()
        {
            CustomCutscenes = CustomCutscenes.OrderByDescending(cutscene => cutscene.ApplyPriority).ToList();
        }

        /// <summary>
        /// Sorts the custom videos to the correct priorities and unlock days.
        /// </summary>
        public void SortCustomVideoFiles()
        {
            CustomVideos = CustomVideos.OrderByDescending(customVideo => customVideo.UnlockDay)
                .ThenByDescending(customVideo => customVideo.OrderPriority)
                .ThenBy(customVideo => customVideo.DesktopName).ToList();
        }

        /// <summary>
        /// Sorts the text files to the correct priorities and unlock days.
        /// </summary>
        public void SortTextFiles()
        {
            CustomTextProgramFiles = CustomTextProgramFiles
                .OrderByDescending(customTextFile => customTextFile.UnlockDay)
                .ThenByDescending(customTextFile => customTextFile.OrderPriority)
                .ThenBy(customTextFile => customTextFile.FileNameOnDesktop).ToList();
        }
        
        /// <summary>
        /// Sorts the computer 3D screens to the correct priorities.
        /// </summary>
        public void SortComputer3DScreens()
        {
            CustomComputer3DScreens = CustomComputer3DScreens
                .OrderByDescending(computer3DScreen => computer3DScreen.ApplyPriority) .ToList();
        }
    }
}