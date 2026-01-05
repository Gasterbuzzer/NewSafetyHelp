using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static NewSafetyHelp.CustomThemes.ColorHelper;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class ModifierParsing
    {
        /// <summary>
        /// Load a modifier from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateModifier(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a modifier. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            ModifierExtraInfo customModifier = ParseModifier(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaignExtraInfo foundCustomCampaign =
                CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.campaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                if (customModifier.unlockDays == null)
                {
                    foundCustomCampaign.customModifiersGeneral.Add(customModifier);
                }
                else
                {
                    foundCustomCampaign.customModifiersDays.Add(customModifier);
                }
            }
            else
            {
                #if DEBUG
                MelonLogger.Msg("DEBUG: Found modifier file before the custom campaign was found / does not exist.");
                #endif

                GlobalParsingVariables.missingCustomCampaignModifier.Add(customModifier);
            }
        }

        private static ModifierExtraInfo ParseModifier(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            // When the modifier is unlocked. If null, it is a general modifier.
            List<int> unlockDays = null; 

            /*
             * Desktop Settings
             */
            string username = string.Empty;

            // Main Program
            string renameMainGameDesktopIcon = "";
            Sprite mainGameDesktopIconSprite = null;

            // Backgrounds
            List<Sprite> backgroundSprites = new List<Sprite>();
            Sprite gameFinishedBackgroundSprite = null;
            bool disableGreenColorBackground = false;
            Color? desktopBackgroundColor = null;

            Sprite backgroundLogo = null;
            bool disableBackgroundLogo = false;
            float backgroundLogoTransparency = 0.2627f;
            
            // Credits
            string desktopCredits = null;
            
            // Desktop settings
            bool entryBrowserActive = false;
            // If this setting was changed at all. Is used when checking.
            // If this is true and the "active" is false, it will disable the entry browser for example.
            bool entryBrowserChanged = false; 
        
            bool scorecardActive = false;
            bool scorecardChanged = false; // See entryBrowserChanged for explanation.
        
            bool artbookActive = false;
            bool artbookChanged = false; // See entryBrowserChanged for explanation.
        
            bool arcadeActive = false;
            bool arcadeChanged = false; // See entryBrowserChanged for explanation.

            // Day Strings
            List<string> dayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.

            if (jObjectParsed.TryGetValue("modifier_custom_campaign_attached", out JToken customCampaignNameValue))
            {
                customCampaignName = customCampaignNameValue.Value<string>();
            }

            if (jObjectParsed.TryGetValue("unlock_day", out JToken unlockDayValue))
            {
                if (unlockDayValue.Type == JTokenType.Integer)
                {
                    unlockDays = new List<int> { unlockDayValue.Value<int>() };
                }
                else if (unlockDayValue.Type == JTokenType.Array)
                {
                    unlockDays = new List<int>();

                    foreach (JToken unlockDayToken in (JArray) unlockDayValue)
                    {
                        unlockDays.Add(unlockDayToken.Value<int>());
                    }
                }
            }

            if (jObjectParsed.TryGetValue("desktop_username_text", out JToken desktopUsernameTextValue))
            {
                username = desktopUsernameTextValue.Value<string>();
            }

            if (jObjectParsed.TryGetValue("rename_main_game_desktop_icon", out JToken renameMainGameDesktopIconValue))
            {
                renameMainGameDesktopIcon = renameMainGameDesktopIconValue.Value<string>();
            }

            if (jObjectParsed.TryGetValue("main_game_desktop_icon_path", out JToken mainGameDesktopIconPathValue))
            {
                string customMainGameDesktopIcon = mainGameDesktopIconPathValue.Value<string>();

                if (string.IsNullOrEmpty(customMainGameDesktopIcon))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given in theme for '{customMainGameDesktopIcon}'." +
                        $" Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
                }
                else
                {
                    mainGameDesktopIconSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + customMainGameDesktopIcon,
                        usermodFolderPath + "\\" + customMainGameDesktopIcon);
                }
            }

            if (jObjectParsed.TryGetValue("desktop_backgrounds", out JToken customCampaignDesktopBackgrounds))
            {
                JArray backgroundNames = (JArray)customCampaignDesktopBackgrounds;

                foreach (JToken backgroundName in backgroundNames)
                {
                    if (string.IsNullOrEmpty(backgroundName.Value<string>()))
                    {
                        MelonLogger.Error($"ERROR: Did not find '{backgroundName.Value<string>()}'." +
                                          " Adding no background.");
                        backgroundSprites.Add(null);
                    }
                    else
                    {
                        backgroundSprites.Add(
                            ImageImport.LoadImage(jsonFolderPath + "\\" + backgroundName.Value<string>(),
                            usermodFolderPath + "\\" + backgroundName.Value<string>()));
                    }
                }
            }
            
            if (jObjectParsed.TryGetValue("game_finished_desktop_background", out JToken gameFinishedDesktopBackground))
            {
                string gameFinishedBackgroundPath = (string) gameFinishedDesktopBackground;

                if (string.IsNullOrEmpty(gameFinishedBackgroundPath))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given for '{gameFinishedBackgroundPath}'. Not updating modifier for campaign: {(!string.IsNullOrEmpty(customCampaignName) ? $"{customCampaignName}." : ".")}");
                }
                else
                {
                    gameFinishedBackgroundSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + gameFinishedBackgroundPath,
                        usermodFolderPath + "\\" + gameFinishedBackgroundPath);
                }
            }

            if (jObjectParsed.TryGetValue("disable_green_color_on_desktop", out JToken disableGreenColorOnDesktopValue))
            {
                disableGreenColorBackground = disableGreenColorOnDesktopValue.Value<bool>();
            }

            if (jObjectParsed.TryGetValue("desktop_background_color", out var _desktopBackgroundColor))
            {
                if (_desktopBackgroundColor.Type == JTokenType.Array)
                {
                    List<float> desktopBackgroundColorList = new List<float>();

                    foreach (JToken desktopBackgroundColorToken in (JArray) _desktopBackgroundColor)
                    {
                        desktopBackgroundColorList.Add(desktopBackgroundColorToken.Value<float>());
                    }

                    switch (desktopBackgroundColorList.Count)
                    {
                        case 3:
                            desktopBackgroundColor = new Color(GetConvertedColorFloat(desktopBackgroundColorList[0]),
                                GetConvertedColorFloat(desktopBackgroundColorList[1]), GetConvertedColorFloat(desktopBackgroundColorList[2]));
                            break;

                        case 4:
                            desktopBackgroundColor = new Color(GetConvertedColorFloat(desktopBackgroundColorList[0]),
                                GetConvertedColorFloat(desktopBackgroundColorList[1]), GetConvertedColorFloat(desktopBackgroundColorList[2]), 
                                GetConvertedColorFloat(desktopBackgroundColorList[3]));
                            break;

                        default:
                            MelonLogger.Error("ERROR: " +
                                              "Provided color for desktop background is invalid! " +
                                              "Make sure its 3 or 4 values.");
                            break;
                    }
                }
            }

            if (jObjectParsed.TryGetValue("desktop_logo_image_name", out JToken customDesktopLogoNameValue))
            {
                string customDesktopLogoPath = customDesktopLogoNameValue.Value<string>();

                if (string.IsNullOrEmpty(customDesktopLogoPath))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given in theme for '{customDesktopLogoPath}'. Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
                }
                else
                {
                    backgroundLogo = ImageImport.LoadImage(jsonFolderPath + "\\" + customDesktopLogoPath,
                        usermodFolderPath + "\\" + customDesktopLogoPath);
                }
            }

            if (jObjectParsed.TryGetValue("disable_desktop_logo", out JToken disableDesktopLogoValue))
            {
                disableBackgroundLogo = disableDesktopLogoValue.Value<bool>();
            }

            if (jObjectParsed.TryGetValue("desktop_logo_transparency",
                    out JToken customDesktopLogoTransparencyValue))
            {
                backgroundLogoTransparency = customDesktopLogoTransparencyValue.Value<float>();
            }
            
            if (jObjectParsed.TryGetValue("desktop_credits",
                    out JToken desktopCreditsValue))
            {
                desktopCredits = desktopCreditsValue.Value<string>();
            }

            if (jObjectParsed.TryGetValue("campaign_day_names", out JToken customCampaignDaysNamesValue))
            {
                JArray _customCampaignDays = (JArray)customCampaignDaysNamesValue;

                foreach (JToken campaignDay in _customCampaignDays)
                {
                    dayTitleStrings.Add(campaignDay.Value<string>());
                }
            }
            
            if (jObjectParsed.TryGetValue("entry_browser_state", out var entryBrowserAlwaysActiveValue))
            {
                entryBrowserActive = (bool) entryBrowserAlwaysActiveValue;
                entryBrowserChanged = true;
            }

            if (jObjectParsed.TryGetValue("scorecard_state", out var scorecardAlwaysActiveValue))
            {
                scorecardActive = (bool) scorecardAlwaysActiveValue;
                scorecardChanged = true;
            }

            if (jObjectParsed.TryGetValue("artbook_state", out var artbookAlwaysActiveValue))
            {
                artbookActive = (bool) artbookAlwaysActiveValue;
                artbookChanged = true;
            }

            if (jObjectParsed.TryGetValue("arcade_state", out var arcadeAlwaysActiveValue))
            {
                arcadeActive = (bool) arcadeAlwaysActiveValue;
                arcadeChanged = true;
            }

            return new ModifierExtraInfo
            {
                customCampaignName = customCampaignName,

                unlockDays = unlockDays,

                usernameText = username,

                renameMainGameDesktopIcon = renameMainGameDesktopIcon,
                mainGameDesktopIcon = mainGameDesktopIconSprite,

                desktopBackgrounds = backgroundSprites,
                gameFinishedBackground = gameFinishedBackgroundSprite,
                disableColorBackground = disableGreenColorBackground,
                desktopBackgroundColor = desktopBackgroundColor,

                customBackgroundLogo = backgroundLogo,
                disableDesktopLogo = disableBackgroundLogo,
                backgroundLogoTransparency = backgroundLogoTransparency,
                
                desktopCredits = desktopCredits,

                dayTitleStrings = dayTitleStrings,
                
                entryBrowserActive = entryBrowserActive,
                entryBrowserChanged = entryBrowserChanged,
                
                scorecardActive = scorecardActive,
                scorecardChanged = scorecardChanged,
                
                artbookActive = artbookActive,
                artbookChanged = artbookChanged,
                
                arcadeActive = arcadeActive,
                arcadeChanged = arcadeChanged
            };
        }
    }
}