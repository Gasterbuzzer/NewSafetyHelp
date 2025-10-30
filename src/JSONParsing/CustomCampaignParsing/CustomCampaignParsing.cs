using System.Collections.Generic;
using System.IO;
using MelonLoader;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.EntryManager;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CustomCampaignParsing
{
    public static class CustomCampaignParsing
    {
        public static CustomCampaignExtraInfo parseCampaignFile(ref JObject jObjectParsed, ref string usermodFolderPath,
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

                foreach (JToken arcadeCustomCall in _customCampaignDays)
                {
                    customCampaignDaysNames.Add((string) arcadeCustomCall);
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

                disableGreenColorBackground = disableGreenColorBackground
            };
        }
    }
}