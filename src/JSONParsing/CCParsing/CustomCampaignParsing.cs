using System.Collections.Generic;
using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.CustomCampaign.Themes;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class CustomCampaignParsing
    {
        /// <summary>
        /// Creates a custom campaign from a provided JSON file.
        /// </summary>
        /// <param name="jObjectParsed"></param>
        /// <param name="usermodFolderPath"></param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateCustomCampaign(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object || string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a custom campaign. Possible syntax mistake?");
                return;
            }
            
            string customCampaignName = "NO_CAMPAIGN_NAME_PROVIDED";
            
            CustomCampaignExtraInfo _customCampaign = parseCampaignFile(ref jObjectParsed, ref usermodFolderPath,
                    ref jsonFolderPath, ref customCampaignName);
            
            // Check if any callers have to be added to this campaign.
            if (GlobalParsingVariables.missingCustomCallerCallersCustomCampaign.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomCallerExtraInfo> tempList =
                    new List<CustomCallerExtraInfo>(GlobalParsingVariables.missingCustomCallerCallersCustomCampaign);

                foreach (CustomCallerExtraInfo customCallerCC in tempList)
                {
                    if (customCallerCC.belongsToCustomCampaign == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing custom caller {customCallerCC.callerName} to the custom campaign: {customCampaignName}.");
                        #endif

                        if (customCallerCC.isGameOverCaller)
                        {
                            _customCampaign.customGameOverCallersInCampaign.Add(customCallerCC);
                        }
                        else if (customCallerCC.isWarningCaller)
                        {
                            _customCampaign.customWarningCallersInCampaign.Add(customCallerCC);
                        }
                        else
                        {
                            _customCampaign.customCallersInCampaign.Add(customCallerCC);
                        }

                        GlobalParsingVariables.missingCustomCallerCallersCustomCampaign.Remove(customCallerCC);
                    }
                }
            }

            // Check if any entries have to be added to this campaign.
            if (GlobalParsingVariables.missingEntriesCustomCampaign.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<EntryExtraInfo> tempList = new List<EntryExtraInfo>(GlobalParsingVariables.missingEntriesCustomCampaign);

                foreach (EntryExtraInfo missingEntry in tempList)
                {
                    if (missingEntry.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing entry to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.entriesOnlyInCampaign.Add(missingEntry);

                        GlobalParsingVariables.missingEntriesCustomCampaign.Remove(missingEntry);
                    }
                }
            }

            // Check if any entries have to be added to this campaign.
            if (GlobalParsingVariables.missingReplaceEntriesCustomCampaign.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<EntryExtraInfo> tempList = new List<EntryExtraInfo>(GlobalParsingVariables.missingReplaceEntriesCustomCampaign);

                foreach (EntryExtraInfo missingEntry in tempList)
                {
                    if (missingEntry.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding 'replace' missing entry to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.entryReplaceOnlyInCampaign.Add(missingEntry);
                        GlobalParsingVariables.missingReplaceEntriesCustomCampaign.Remove(missingEntry);
                    }
                }
            }

            // Check if any emails have to be added to a custom campaign.
            if (GlobalParsingVariables.missingCustomCampaignEmails.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<EmailExtraInfo> tempList = new List<EmailExtraInfo>(GlobalParsingVariables.missingCustomCampaignEmails);

                foreach (EmailExtraInfo missingEmail in tempList)
                {
                    if (missingEmail.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing email to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.emails.Add(missingEmail);
                        GlobalParsingVariables.missingCustomCampaignEmails.Remove(missingEmail);
                    }
                }
            }

            // Check if any videos have to be added to a custom campaign.
            if (GlobalParsingVariables.missingCustomCampaignVideo.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomVideoExtraInfo> tempList = new List<CustomVideoExtraInfo>(GlobalParsingVariables.missingCustomCampaignVideo);

                foreach (CustomVideoExtraInfo missingVideo in tempList)
                {
                    if (missingVideo.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing video to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.allDesktopVideos.Add(missingVideo);
                        GlobalParsingVariables.missingCustomCampaignVideo.Remove(missingVideo);
                    }
                }
            }
            
            // Check if any music has to be added to a custom campaign.
            if (GlobalParsingVariables.missingCustomCampaignMusic.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<MusicExtraInfo> tempList = new List<MusicExtraInfo>(GlobalParsingVariables.missingCustomCampaignMusic);

                foreach (MusicExtraInfo missingMusic in tempList)
                {
                    if (missingMusic.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing music to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.customMusic.Add(missingMusic);
                        GlobalParsingVariables.missingCustomCampaignMusic.Remove(missingMusic);
                    }
                }
            }
            
            // Check if any modifier has to be added to a custom campaign.
            if (GlobalParsingVariables.missingCustomCampaignModifier.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<ModifierExtraInfo> tempList = new List<ModifierExtraInfo>(GlobalParsingVariables.missingCustomCampaignModifier);

                foreach (ModifierExtraInfo missingModifier in tempList)
                {
                    if (missingModifier.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing modifier to the custom campaign: {customCampaignName}.");
                        #endif

                        if (missingModifier.unlockDays == null)
                        {
                            _customCampaign.customModifiersGeneral.Add(missingModifier);
                        }
                        else
                        {
                            _customCampaign.customModifiersDays.Add(missingModifier);
                        }
                        
                        GlobalParsingVariables.missingCustomCampaignModifier.Remove(missingModifier);
                    }
                }
            }
            
            // Check if any theme has to be added to a custom campaign.
            if (GlobalParsingVariables.missingCustomCampaignTheme.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<ThemesExtraInfo> tempList = new List<ThemesExtraInfo>(GlobalParsingVariables.missingCustomCampaignTheme);

                foreach (ThemesExtraInfo missingTheme in tempList)
                {
                    if (missingTheme.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: Adding missing theme to the custom campaign: {customCampaignName}.");
                        #endif

                        if (missingTheme.unlockDays == null)
                        {
                            _customCampaign.customThemesGeneral.Add(missingTheme);
                        }
                        else
                        {
                            _customCampaign.customThemesDays.Add(missingTheme);
                        }
                        
                        GlobalParsingVariables.missingCustomCampaignTheme.Remove(missingTheme);
                    }
                }
            }
            
            // We finished adding all missing values and now add the campaign as available.
            CustomCampaignGlobal.customCampaignsAvailable.Add(_customCampaign);
        }
        
        private static CustomCampaignExtraInfo parseCampaignFile(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath,
            ref string customCampaignName)
        {
            // Desktop
            string customCampaignDesktopName = "NO_NAME\nPROVIDED";
            Sprite customCampaignSprite = null;
                    
            // Initialize the strings empty.
            List<List<string>> loadingTexts = new List<List<string>>()
            {
                new List<string>() {""}, // First inner list with one empty string
                new List<string>() {""}  // Second inner list with one empty string
            };
            
            // Date and Username
            string desktopUsernameText = "";
            int desktopDateStartYear = -1;
            int desktopDateStartMonth = -1;
            int desktopDateStartDay = -1;
            bool useEuropeDateFormat = false;
                    
            // Campaign Settings
            int customCampaignDays = 7;
                    
            List<string> customCampaignDaysNames = new List<string>();
                    
            bool removeAllExistingEntries = false;

            bool resetDefaultEntriesPermission = false; // If all default entries should have their permission set to 0. (Also hides NEW tag from entry name)

            bool doShowNewTagForMainGameEntries = false; // If to show the NEW in entry names when the permission is set 0 for the first day.

            bool skipCallersCorrectly = false; // If to always skip callers with the answer being marked as "correct".

            // Thresholds
            int gameOverThreshold = 60; // Threshold when to trigger game over.
            int warningThreshold = 60; // Threshold when to trigger a warning call.

            List<int> warningCallThresholdCallerAmounts = new List<int>();

            // Video Cutscenes
            string endCutscenePath = "";
            string gameOverCutscenePath = "";
            
            // Music
            bool useRandomMusic = true;
            
            bool removeDefaultMusic = false; // If to remove the default music from the game.

            // Enable Programs
            bool entryBrowserAlwaysActive = false;
            bool scorecardAlwaysActive = false;
            bool artbookAlwaysActive = false;
            bool arcadeAlwaysActive = false;

            bool disableDefaultVideos = true;

            bool alwaysSkipCallButton = false;

            // Program Settings
            string renameMainProgram = "";
            Sprite changeMainProgramSprite = null;

            // Emails
            bool removeAllDefaultEmails = true;

            // Backgrounds
            List<Sprite> backgroundSprites = new List<Sprite>();
            Sprite backgroundFinishedGameSprite = null;

            bool disableGreenColorBackground = false;

            bool disableDesktopLogo = false;
            Sprite customDesktopLogo = null;
            float customDesktopLogoTransparency = 0.2627f;
            
            // Themes
            
            bool disablePickingThemeOption = false; // If true, it will hide the option to set the theme.

            string defaultTheme = null;
            
            /*
             * Parsing the JSON File
             */
            
            if (jObjectParsed.TryGetValue("custom_campaign_name", out var customCampaignNameValue))
            {
                customCampaignName = (string) customCampaignNameValue;
            }

            if (jObjectParsed.TryGetValue("custom_campaign_desktop_name", out var customCampaignDesktopNameValue))
            {
                customCampaignDesktopName = (string) customCampaignDesktopNameValue;
            }

            if (jObjectParsed.TryGetValue("desktop_username_text", out var desktopUsernameTextValue))
            {
                desktopUsernameText = (string) desktopUsernameTextValue;
            }

            if (jObjectParsed.TryGetValue("start_year", out var startYearValue))
            {
                desktopDateStartYear = (int) startYearValue;
            }

            if (jObjectParsed.TryGetValue("start_month", out var startMonthValue))
            {
                desktopDateStartMonth = (int) startMonthValue;
            }

            if (jObjectParsed.TryGetValue("start_day", out var startDayValue))
            {
                desktopDateStartDay = (int) startDayValue;
            }

            if (jObjectParsed.TryGetValue("use_europe_date_format", out var useEuropeanDateFormatValue))
            {
                useEuropeDateFormat = (bool) useEuropeanDateFormatValue;
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_days", out var customCampaignDaysValue))
            {
                customCampaignDays = (int) customCampaignDaysValue;
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_remove_main_entries", out var customCampaignRemoveMainEntriesValue))
            {
                removeAllExistingEntries = (bool) customCampaignRemoveMainEntriesValue;
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_empty_main_entries_permission", out var customCampaignEmptyMainEntriesPermissionValue))
            {
                resetDefaultEntriesPermission = (bool) customCampaignEmptyMainEntriesPermissionValue;

                if (jObjectParsed.TryGetValue("custom_campaign_show_new_tag_for_main_entries", out JToken resultNewTag))
                {
                    doShowNewTagForMainGameEntries = (bool) resultNewTag;
                }
            }
            
            // Sanity check in case it was passed but no entries have been reset to 0th permission.
            if (jObjectParsed.TryGetValue("custom_campaign_show_new_tag_for_main_entries", out _) && !resetDefaultEntriesPermission)
            {
                MelonLogger.Warning("WARNING: Provided option to show 'NEW' tag for main game entries but main game entries are not being reset?");
            }

            if (jObjectParsed.TryGetValue("custom_campaign_gameover_threshold", out var customCampaignGameoverThresholdValue))
            {
                gameOverThreshold = (int) customCampaignGameoverThresholdValue;
            }

            if (jObjectParsed.TryGetValue("custom_campaign_warning_threshold", out var customCampaignWarningThresholdValue))
            {
                warningThreshold = (int) customCampaignWarningThresholdValue;
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_days_names", out var customCampaignDaysNamesValue))
            {
                JArray _customCampaignDays = (JArray) customCampaignDaysNamesValue;

                foreach (JToken campaignDay in _customCampaignDays)
                {
                    customCampaignDaysNames.Add((string) campaignDay);
                }
            }

            if (jObjectParsed.TryGetValue("custom_campaign_icon_image_name", out var customCampaignIconImageNameValue))
            {
                string customCampaignImagePath = (string) customCampaignIconImageNameValue;

                if (string.IsNullOrEmpty(customCampaignImagePath))
                {
                    MelonLogger.Error($"ERROR: Invalid file name given for '{usermodFolderPath}'. Default icon will be shown.");
                }
                else
                {
                    customCampaignSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + customCampaignImagePath,
                        usermodFolderPath + "\\" + customCampaignImagePath);
                }
            }
            else
            {
                MelonLogger.Warning(
                    $"WARNING: No custom campaign icon given for file in {usermodFolderPath}. Default icon will be shown.");
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_loading_desktop_text1", out var customCampaignLoadingDesktopText1Value))
            {
                JArray _loadingText = (JArray) customCampaignLoadingDesktopText1Value;

                loadingTexts[0] = new List<string>();

                for (int i = 0; i < _loadingText.Count; i++)
                {
                    loadingTexts[0].Add((string) _loadingText[i]);
                }
            }

            if (jObjectParsed.TryGetValue("custom_campaign_loading_desktop_text2", out var customCampaignLoadingDesktopText2Value))
            {
                JArray _loadingText = (JArray)customCampaignLoadingDesktopText2Value;

                loadingTexts[1] = new List<string>();

                for (int i = 0; i < _loadingText.Count; i++)
                {
                    loadingTexts[1].Add((string) _loadingText[i]);
                }
            }

            if (jObjectParsed.TryGetValue("custom_campaign_threshold_amount", out var customCampaignThresholdAmountValue))
            {
                JArray thresholdAmount = (JArray)customCampaignThresholdAmountValue;

                for (int i = 0; i < thresholdAmount.Count; i++)
                {
                    warningCallThresholdCallerAmounts.Add((int) thresholdAmount[i]);
                }
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_end_cutscene_video_name", out var customCampaignEndCutsceneVideoNameValue))
            {
                endCutscenePath = jsonFolderPath + "\\" + (string) customCampaignEndCutsceneVideoNameValue;
                string endCutsceneAlternativePath = usermodFolderPath + "\\" + (string) customCampaignEndCutsceneVideoNameValue;

                #if DEBUG
                    MelonLogger.Msg($"DEBUG: End cutscene video found: '{(string) customCampaignEndCutsceneVideoNameValue}'");
                #endif

                if (string.IsNullOrEmpty((string) customCampaignEndCutsceneVideoNameValue))
                {
                    MelonLogger.Warning(
                        "WARNING: Provided video cutscene name but name is empty. Unable to show custom end cutscene.");
                    endCutscenePath = "";
                }
                else if (!File.Exists(endCutscenePath))
                {
                    if (!File.Exists(endCutsceneAlternativePath))
                    {
                        MelonLogger.Warning($"WARNING: Provided video cutscene {endCutscenePath} does not exist.");
                        endCutscenePath = "";
                    }
                    else
                    {
                        endCutscenePath = endCutsceneAlternativePath;
                    }
                }
            }

            if (jObjectParsed.TryGetValue("custom_campaign_gameover_cutscene_video_name", out var customCampaignGameoverCutsceneVideoNameValue))
            {
                gameOverCutscenePath = jsonFolderPath + "\\" + (string) customCampaignGameoverCutsceneVideoNameValue;
                string gameOverCutsceneAlternativePath = usermodFolderPath + "\\" + (string) customCampaignGameoverCutsceneVideoNameValue;

                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Game Over video found: '{gameOverCutscenePath}'");
                #endif

                if (string.IsNullOrEmpty((string) customCampaignGameoverCutsceneVideoNameValue))
                {
                    MelonLogger.Warning(
                        "WARNING: Provided video cutscene name but name is empty. Unable to show custom game over cutscene.");
                    gameOverCutscenePath = "";
                }
                else if (!File.Exists(gameOverCutscenePath))
                {
                    if (!File.Exists(gameOverCutsceneAlternativePath))
                    {
                        MelonLogger.Warning($"WARNING: Provided video cutscene {gameOverCutscenePath} does not exist.");
                        gameOverCutscenePath = "";
                    }
                    else
                    {
                        gameOverCutscenePath = gameOverCutsceneAlternativePath;
                    }
                }
            }
            
            if (jObjectParsed.TryGetValue("always_randomize_music", out JToken alwaysRandomizeMusicValue))
            {
                useRandomMusic = (bool) alwaysRandomizeMusicValue;
            }
            
            if (jObjectParsed.TryGetValue("remove_default_music", out JToken removeDefaultMusicValue))
            {
                removeDefaultMusic = (bool) removeDefaultMusicValue;
            }

            if (jObjectParsed.TryGetValue("entry_browser_always_active", out var entryBrowserAlwaysActiveValue))
            {
                entryBrowserAlwaysActive = (bool) entryBrowserAlwaysActiveValue;
            }

            if (jObjectParsed.TryGetValue("scorecard_always_active", out var scorecardAlwaysActiveValue))
            {
                scorecardAlwaysActive = (bool) scorecardAlwaysActiveValue;
            }

            if (jObjectParsed.TryGetValue("artbook_always_active", out var artbookAlwaysActiveValue))
            {
                artbookAlwaysActive = (bool) artbookAlwaysActiveValue;
            }

            if (jObjectParsed.TryGetValue("arcade_always_active", out var arcadeAlwaysActiveValue))
            {
                arcadeAlwaysActive = (bool) arcadeAlwaysActiveValue;
            }

            if (jObjectParsed.TryGetValue("always_show_skip_call_wait_time", out var alwaysShowSkipCallWaitTimeValue))
            {
                alwaysSkipCallButton = (bool) alwaysShowSkipCallWaitTimeValue;
            }
            
            if (jObjectParsed.TryGetValue("rename_main_game_desktop_icon", out var renameMainGameDesktopIconValue))
            {
                renameMainProgram = (string) renameMainGameDesktopIconValue;
            }

            if (jObjectParsed.TryGetValue("disable_main_campaign_videos", out var disableMainCampaignVideosValue))
            {
                disableDefaultVideos = (bool) disableMainCampaignVideosValue;
            }

            if (jObjectParsed.TryGetValue("remove_default_emails", out var removeDefaultEmailsValue))
            {
                removeAllDefaultEmails = (bool) removeDefaultEmailsValue;
            }
            
            if (jObjectParsed.TryGetValue("main_game_desktop_icon_path", out var mainGameDesktopIconPathValue))
            {
                string customMainGameDesktopIcon = (string) mainGameDesktopIconPathValue;

                if (string.IsNullOrEmpty(customMainGameDesktopIcon))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given for '{customMainGameDesktopIcon}'. Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
                }
                else
                {
                    changeMainProgramSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + customMainGameDesktopIcon,
                        usermodFolderPath + "\\" + customMainGameDesktopIcon);
                }
            }
            
            if (jObjectParsed.TryGetValue("custom_campaign_desktop_backgrounds", out var customCampaignDesktopBackgrounds))
            {
                JArray backgroundNames = (JArray)customCampaignDesktopBackgrounds;

                for (int i = 0; i < backgroundNames.Count; i++)
                {
                    if (string.IsNullOrEmpty((string) backgroundNames[i]))
                    {
                        MelonLogger.Error($"ERROR: Did not find '{backgroundNames[i]}'. Adding null.");
                        backgroundSprites.Add(null);
                    }
                    else
                    {
                        backgroundSprites.Add(ImageImport.LoadImage(jsonFolderPath + "\\" + backgroundNames[i],
                            usermodFolderPath + "\\" + backgroundNames[i]));
                    }
                }
            }

            if (jObjectParsed.TryGetValue("custom_campaign_desktop_game_finished_background", out var customCampaignDesktopGameFinishedBackground))
            {
                string gameFinishedBackgroundPath = (string) customCampaignDesktopGameFinishedBackground;

                if (string.IsNullOrEmpty(gameFinishedBackgroundPath))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given for '{gameFinishedBackgroundPath}'. Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
                }
                else
                {
                    backgroundFinishedGameSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + gameFinishedBackgroundPath,
                        usermodFolderPath + "\\" + gameFinishedBackgroundPath);
                }
            }

            if (jObjectParsed.TryGetValue("disable_desktop_logo", out var disableDesktopLogoValue))
            {
                disableDesktopLogo = (bool) disableDesktopLogoValue;
            }

            if (jObjectParsed.TryGetValue("disable_green_color_on_desktop", out var disableGreenColorOnDesktopValue))
            {
                disableGreenColorBackground = (bool) disableGreenColorOnDesktopValue;
            }

            if (jObjectParsed.TryGetValue("custom_desktop_logo_transparency", out var customDesktopLogoTransparencyValue))
            {
                customDesktopLogoTransparency = (float) customDesktopLogoTransparencyValue;
            }
            
            if (jObjectParsed.TryGetValue("skip_callers_correctly", out var skipCallersCorrectlyValue))
            {
                skipCallersCorrectly = (bool) skipCallersCorrectlyValue;
            }

            if (jObjectParsed.TryGetValue("custom_desktop_logo_name", out var customDesktopLogoNameValue))
            {
                string customDesktopLogoPath = (string) customDesktopLogoNameValue;

                if (string.IsNullOrEmpty(customDesktopLogoPath))
                {
                    MelonLogger.Error($"ERROR: Invalid file name given for '{customDesktopLogoPath}'. Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
                }
                else
                {
                    customDesktopLogo = ImageImport.LoadImage(jsonFolderPath + "\\" + customDesktopLogoPath,
                        usermodFolderPath + "\\" + customDesktopLogoPath);
                }
            }
            
            if (jObjectParsed.TryGetValue("defaultTheme", out JToken defaultThemeValue))
            {
                defaultTheme = defaultThemeValue.Value<string>();
            }
            
            if (jObjectParsed.TryGetValue("disable_theme_dropdown", out var disableThemeDropdownValue))
            {
                disablePickingThemeOption = (bool) disableThemeDropdownValue;
            }
            
            // Create
            return new CustomCampaignExtraInfo
            {
                campaignName = customCampaignName,
                campaignDays = customCampaignDays,
                campaignIcon = customCampaignSprite,
                campaignDayStrings = customCampaignDaysNames,
                campaignDesktopName = customCampaignDesktopName,

                removeExistingEntries = removeAllExistingEntries,
                resetDefaultEntriesPermission = resetDefaultEntriesPermission,
                doShowNewTagForMainGameEntries = doShowNewTagForMainGameEntries,
                
                skipCallersCorrectly = skipCallersCorrectly,

                loadingTexts = loadingTexts,

                desktopUsernameText = desktopUsernameText,
                desktopDateStartYear = desktopDateStartYear,
                desktopDateStartMonth = desktopDateStartMonth,
                desktopDateStartDay = desktopDateStartDay,
                useEuropeDateFormat = useEuropeDateFormat,

                gameOverThreshold = gameOverThreshold,
                warningThreshold = warningThreshold,
                warningCallThresholdCallerAmounts = warningCallThresholdCallerAmounts,

                endCutsceneVideoName = endCutscenePath,
                gameOverCutsceneVideoName = gameOverCutscenePath,
                
                alwaysRandomMusic = useRandomMusic,
                removeDefaultMusic = removeDefaultMusic,

                entryBrowserAlwaysActive = entryBrowserAlwaysActive,
                scorecardAlwaysActive = scorecardAlwaysActive,
                artbookAlwaysActive = artbookAlwaysActive,
                arcadeAlwaysActive = arcadeAlwaysActive,

                disableAllDefaultVideos = disableDefaultVideos,

                renameMainGameDesktopIcon = renameMainProgram,
                changeMainGameDesktopIcon = changeMainProgramSprite,

                alwaysSkipCallButton = alwaysSkipCallButton,

                removeDefaultEmails = removeAllDefaultEmails,

                backgroundSprites = backgroundSprites,
                gameFinishedBackground = backgroundFinishedGameSprite,

                disableDesktopLogo = disableDesktopLogo,
                customDesktopLogo = customDesktopLogo,
                customDesktopLogoTransparency = customDesktopLogoTransparency,

                disableGreenColorBackground = disableGreenColorBackground,
                
                defaultTheme = defaultTheme,
                disablePickingThemeOption = disablePickingThemeOption
            };
        }
    }
}