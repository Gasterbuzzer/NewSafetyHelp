using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignPatches.Modifier.Data;
using NewSafetyHelp.CustomCampaignPatches.Themes;
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
            
            CustomCampaign customCampaign = ParseCampaignFile(ref jObjectParsed, ref usermodFolderPath,
                    ref jsonFolderPath, ref customCampaignName);
            
            // Check if any callers have to be added to this campaign.
            if (GlobalParsingVariables.PendingCustomCampaignCustomCallers.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CallerPatches.CallerModel.CustomCCaller> tempList =
                    new List<CallerPatches.CallerModel.CustomCCaller>(GlobalParsingVariables.PendingCustomCampaignCustomCallers);

                foreach (CallerPatches.CallerModel.CustomCCaller customCallerCC in tempList)
                {
                    if (customCallerCC.CustomCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing custom caller {customCallerCC.CallerName} to the custom campaign: {customCampaignName}.");
                        #endif

                        if (customCallerCC.IsGameOverCaller)
                        {
                            customCampaign.CustomGameOverCallersInCampaign.Add(customCallerCC);
                        }
                        else if (customCallerCC.IsWarningCaller)
                        {
                            customCampaign.CustomWarningCallersInCampaign.Add(customCallerCC);
                        }
                        else
                        {
                            customCampaign.CustomCallersInCampaign.Add(customCallerCC);
                        }

                        GlobalParsingVariables.PendingCustomCampaignCustomCallers.Remove(customCallerCC);
                    }
                }
            }
            

            // Check if any entries have to be added to this campaign.
            ParsingHelper.AddPendingElementsToCampaign(ref GlobalParsingVariables.PendingCustomCampaignEntries,
                ref customCampaign.EntriesOnlyInCampaign, customCampaignName, "entries");

            // Check if any entries have to be added to this campaign.
            ParsingHelper.AddPendingElementsToCampaign(ref GlobalParsingVariables.PendingCustomCampaignReplaceEntries,
                ref customCampaign.EntryReplaceOnlyInCampaign, customCampaignName, "replace-entries");

            // Check if any emails have to be added to a custom campaign.
            ParsingHelper.AddPendingElementsToCampaign(ref GlobalParsingVariables.PendingCustomCampaignEmails,
                ref customCampaign.Emails, customCampaignName, "emails");
            
            // Sort by unlock day. This is to prevent some oddities where some emails that appear later, appear in the list sooner.
            customCampaign.Emails.Sort((emailOne, emailTwo) => emailOne.UnlockDay.CompareTo(emailTwo.UnlockDay));

            // Check if any videos have to be added to a custom campaign.
            ParsingHelper.AddPendingElementsToCampaign(ref GlobalParsingVariables.PendingCustomCampaignVideos,
                ref customCampaign.AllDesktopVideos, customCampaignName, "videos");
            
            // Check if any music has to be added to a custom campaign.
            if (GlobalParsingVariables.PendingCustomCampaignMusic.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomMusic> tempList = new List<CustomMusic>(GlobalParsingVariables.PendingCustomCampaignMusic);

                foreach (CustomMusic missingMusic in tempList)
                {
                    if (missingMusic.CustomCampaignName == customCampaignName)
                    {
                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: Adding missing music to the custom campaign: {customCampaignName}.");
                        #endif

                        if (missingMusic.IsIntermissionMusic)
                        {
                            customCampaign.CustomIntermissionMusic.Add(missingMusic);
                        }
                        else
                        {
                            customCampaign.CustomMusic.Add(missingMusic);
                        }
                        
                        GlobalParsingVariables.PendingCustomCampaignMusic.Remove(missingMusic);
                    }
                }
            }
            
            // Check if any modifier has to be added to a custom campaign.
            if (GlobalParsingVariables.PendingCustomCampaignModifiers.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomModifier> tempList = new List<CustomModifier>(GlobalParsingVariables.PendingCustomCampaignModifiers);

                foreach (CustomModifier missingModifier in tempList)
                {
                    if (missingModifier.CustomCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing modifier to the custom campaign: {customCampaignName}.");
                        #endif

                        if (missingModifier.UnlockDays == null)
                        {
                            customCampaign.CustomModifiersGeneral.Add(missingModifier);
                        }
                        else
                        {
                            customCampaign.CustomModifiersDays.Add(missingModifier);
                        }
                        
                        GlobalParsingVariables.PendingCustomCampaignModifiers.Remove(missingModifier);
                    }
                }
            }
            
            // Check if any theme has to be added to a custom campaign.
            if (GlobalParsingVariables.PendingCustomCampaignThemes.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomTheme> tempList = new List<CustomTheme>(GlobalParsingVariables.PendingCustomCampaignThemes);

                foreach (CustomTheme missingTheme in tempList)
                {
                    if (missingTheme.CustomCampaignName == customCampaignName)
                    {
                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: Adding missing theme to the custom campaign: {customCampaignName}.");
                        #endif

                        if (missingTheme.UnlockDays == null)
                        {
                            customCampaign.CustomThemesGeneral.Add(missingTheme);
                        }
                        else
                        {
                            customCampaign.CustomThemesDays.Add(missingTheme);
                        }
                        
                        GlobalParsingVariables.PendingCustomCampaignThemes.Remove(missingTheme);
                    }
                }
            }
            
            // Check if any ringtone has to be added to a custom campaign.
            ParsingHelper.AddPendingElementsToCampaign(ref GlobalParsingVariables.PendingCustomCampaignRingtones,
                ref customCampaign.CustomRingtones, customCampaignName, "ringtone");
            
            // We finished adding all missing values and now add the campaign as available.
            CustomCampaignGlobal.CustomCampaignsAvailable.Add(customCampaign);
        }
        
        private static CustomCampaign ParseCampaignFile(ref JObject jObjectParsed,
            ref string usermodFolderPath, ref string jsonFolderPath, ref string customCampaignName)
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
            
            // Wait Time between callers
            List<float> waitBetweenCallers = new List<float>();
            bool enableCustomWaitBetweenCallers = false;

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
            
            // Ringtone

            bool doNotAccountDefaultRingtone = true;
            
            /*
             * Parsing the JSON File
             */
            
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_name", ref customCampaignName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_desktop_name", ref customCampaignDesktopName);
            ParsingHelper.TryAssign(jObjectParsed, "desktop_username_text", ref desktopUsernameText);
            ParsingHelper.TryAssign(jObjectParsed, "start_year", ref desktopDateStartYear);
            ParsingHelper.TryAssign(jObjectParsed, "start_month", ref desktopDateStartMonth);
            ParsingHelper.TryAssign(jObjectParsed, "start_day", ref desktopDateStartDay);
            ParsingHelper.TryAssign(jObjectParsed, "use_europe_date_format", ref useEuropeDateFormat);
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_days", ref customCampaignDays);
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_remove_main_entries", ref removeAllExistingEntries);
            
            if (jObjectParsed.TryGetValue("custom_campaign_empty_main_entries_permission", out var customCampaignEmptyMainEntriesPermissionValue))
            {
                resetDefaultEntriesPermission = customCampaignEmptyMainEntriesPermissionValue.Value<bool>();

                ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_show_new_tag_for_main_entries", ref doShowNewTagForMainGameEntries);
            }
            
            // Sanity check in case it was passed but no entries have been reset to 0th permission.
            if (jObjectParsed.TryGetValue("custom_campaign_show_new_tag_for_main_entries", out _) && !resetDefaultEntriesPermission)
            {
                MelonLogger.Warning("WARNING: Provided option to show 'NEW' tag for main game entries but main game entries are not being reset?");
            }

            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_gameover_threshold", ref gameOverThreshold);
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_warning_threshold", ref warningThreshold);
            
            if (jObjectParsed.TryGetValue("custom_campaign_days_names", out var customCampaignDaysNamesValue))
            {
                JArray _customCampaignDays = (JArray) customCampaignDaysNamesValue;

                foreach (JToken campaignDay in _customCampaignDays)
                {
                    customCampaignDaysNames.Add((string) campaignDay);
                }
            }

            ParsingHelper.TryAssignSprite(jObjectParsed, "custom_campaign_icon_image_name", ref customCampaignSprite, jsonFolderPath,
                usermodFolderPath, customCampaignName);
            
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
            
            ParsingHelper.TryAssignVideoPath(jObjectParsed, "custom_campaign_end_cutscene_video_name",
                ref endCutscenePath, jsonFolderPath, usermodFolderPath);
            
            ParsingHelper.TryAssignVideoPath(jObjectParsed, "custom_campaign_gameover_cutscene_video_name",
                ref gameOverCutscenePath, jsonFolderPath, usermodFolderPath);
            
            ParsingHelper.TryAssign(jObjectParsed, "always_randomize_music", ref useRandomMusic);
            ParsingHelper.TryAssign(jObjectParsed, "remove_default_music", ref removeDefaultMusic);
            ParsingHelper.TryAssign(jObjectParsed, "entry_browser_always_active", ref entryBrowserAlwaysActive);
            ParsingHelper.TryAssign(jObjectParsed, "scorecard_always_active", ref scorecardAlwaysActive);
            ParsingHelper.TryAssign(jObjectParsed, "artbook_always_active", ref artbookAlwaysActive);
            ParsingHelper.TryAssign(jObjectParsed, "arcade_always_active", ref arcadeAlwaysActive);
            ParsingHelper.TryAssign(jObjectParsed, "always_show_skip_call_wait_time", ref alwaysSkipCallButton);
            ParsingHelper.TryAssign(jObjectParsed, "rename_main_game_desktop_icon", ref renameMainProgram);
            ParsingHelper.TryAssign(jObjectParsed, "disable_main_campaign_videos", ref disableDefaultVideos);
            ParsingHelper.TryAssign(jObjectParsed, "remove_default_emails", ref removeAllDefaultEmails);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "main_game_desktop_icon_path", ref changeMainProgramSprite,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
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

            ParsingHelper.TryAssignSprite(jObjectParsed, "custom_campaign_desktop_game_finished_background", ref backgroundFinishedGameSprite,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "disable_desktop_logo", ref disableDesktopLogo);
            ParsingHelper.TryAssign(jObjectParsed, "disable_green_color_on_desktop", ref disableGreenColorBackground);
            ParsingHelper.TryAssign(jObjectParsed, "custom_desktop_logo_transparency", ref customDesktopLogoTransparency);
            ParsingHelper.TryAssign(jObjectParsed, "skip_callers_correctly", ref skipCallersCorrectly);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "custom_desktop_logo_name", ref customDesktopLogo,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssign(jObjectParsed, "defaultTheme", ref defaultTheme);
            ParsingHelper.TryAssign(jObjectParsed, "disable_theme_dropdown", ref disablePickingThemeOption);
            ParsingHelper.TryAssign(jObjectParsed, "do_not_account_default_ringtone", ref doNotAccountDefaultRingtone);

            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, "waiting_time", ref waitBetweenCallers);
            if (waitBetweenCallers != null && waitBetweenCallers.Count > 0)
            {
                enableCustomWaitBetweenCallers = true;
            }
            
            return new CustomCampaign
            {
                CampaignName = customCampaignName,
                CampaignDays = customCampaignDays,
                CampaignIcon = customCampaignSprite,
                CampaignDayStrings = customCampaignDaysNames,
                CampaignDesktopName = customCampaignDesktopName,

                RemoveExistingEntries = removeAllExistingEntries,
                ResetDefaultEntriesPermission = resetDefaultEntriesPermission,
                DoShowNewTagForMainGameEntries = doShowNewTagForMainGameEntries,
                
                SkipCallersCorrectly = skipCallersCorrectly,

                LoadingTexts = loadingTexts,

                DesktopUsernameText = desktopUsernameText,
                DesktopDateStartYear = desktopDateStartYear,
                DesktopDateStartMonth = desktopDateStartMonth,
                DesktopDateStartDay = desktopDateStartDay,
                UseEuropeDateFormat = useEuropeDateFormat,

                GameOverThreshold = gameOverThreshold,
                WarningThreshold = warningThreshold,
                WarningCallThresholdCallerAmounts = warningCallThresholdCallerAmounts,

                EndCutsceneVideoName = endCutscenePath,
                GameOverCutsceneVideoName = gameOverCutscenePath,
                
                AlwaysRandomMusic = useRandomMusic,
                RemoveDefaultMusic = removeDefaultMusic,

                EntryBrowserAlwaysActive = entryBrowserAlwaysActive,
                ScorecardAlwaysActive = scorecardAlwaysActive,
                ArtbookAlwaysActive = artbookAlwaysActive,
                ArcadeAlwaysActive = arcadeAlwaysActive,

                DisableAllDefaultVideos = disableDefaultVideos,

                RenameMainGameDesktopIcon = renameMainProgram,
                ChangeMainGameDesktopIcon = changeMainProgramSprite,

                AlwaysSkipCallButton = alwaysSkipCallButton,

                RemoveDefaultEmails = removeAllDefaultEmails,

                BackgroundSprites = backgroundSprites,
                GameFinishedBackground = backgroundFinishedGameSprite,

                DisableDesktopLogo = disableDesktopLogo,
                CustomDesktopLogo = customDesktopLogo,
                CustomDesktopLogoTransparency = customDesktopLogoTransparency,

                DisableGreenColorBackground = disableGreenColorBackground,
                
                DefaultTheme = defaultTheme,
                DisablePickingThemeOption = disablePickingThemeOption,
                
                DoNotAccountDefaultRingtone = doNotAccountDefaultRingtone,
                
                WaitBetweenCallers = waitBetweenCallers,
                EnableCustomWaitBetweenCallers = enableCustomWaitBetweenCallers
            };
        }
    }
}