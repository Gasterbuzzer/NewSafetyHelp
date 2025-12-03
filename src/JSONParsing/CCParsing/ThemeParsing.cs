using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.CustomCampaign.Themes.Data;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class ThemeParsing
    {
        /// <summary>
        /// Load a music from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateTheme(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as music. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            ThemeExtraInfo customTheme = ParseTheme(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaignExtraInfo foundCustomCampaign =
                CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.campaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                foundCustomCampaign.customThemes.Add(customTheme);
            }
            else
            {
                #if DEBUG
                MelonLogger.Msg("DEBUG: Found theme file before the custom campaign was found / does not exist.");
                #endif

                ParseJSONFiles.missingCustomCampaignThemes.Add(customTheme);
            }
        }

        private static ThemeExtraInfo ParseTheme(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            List<int>
                unlockDays = null; // When the theme is unlocked. If not set, this theme will become the default theme.

            /*
             * Desktop Settings
             */
            string username = string.Empty;

            // Main Program
            string renameMainGameDesktopIcon = "";
            Sprite mainGameDesktopIconSprite = null;

            // Backgrounds
            List<Sprite> backgroundSprites = new List<Sprite>();
            bool disableGreenColorBackground = false;
            Color? desktopBackgroundColor = null;

            Sprite backgroundLogo = null;
            bool disableBackgroundLogo = false;
            float backgroundLogoTransparency = 0.2627f;

            // Day Strings
            List<string> dayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.

            if (jObjectParsed.TryGetValue("custom_campaign_attached", out var customCampaignNameValue))
            {
                customCampaignName = (string)customCampaignNameValue;
            }

            if (jObjectParsed.TryGetValue("unlock_day", out var unlockDayValue))
            {
                if (unlockDayValue.Type == JTokenType.Integer)
                {
                    unlockDays = new List<int> { (int)unlockDayValue };
                }
                else if (unlockDayValue.Type == JTokenType.Array)
                {
                    unlockDays = unlockDayValue.ToObject<List<int>>();
                }
            }

            if (jObjectParsed.TryGetValue("desktop_username_text", out var desktopUsernameTextValue))
            {
                username = (string)desktopUsernameTextValue;
            }

            if (jObjectParsed.TryGetValue("rename_main_game_desktop_icon", out var renameMainGameDesktopIconValue))
            {
                renameMainGameDesktopIcon = (string)renameMainGameDesktopIconValue;
            }

            if (jObjectParsed.TryGetValue("main_game_desktop_icon_path", out var mainGameDesktopIconPathValue))
            {
                string customMainGameDesktopIcon = (string)mainGameDesktopIconPathValue;

                if (string.IsNullOrEmpty(customMainGameDesktopIcon))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given in theme for '{customMainGameDesktopIcon}'. Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
                }
                else
                {
                    mainGameDesktopIconSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + customMainGameDesktopIcon,
                        usermodFolderPath + "\\" + customMainGameDesktopIcon);
                }
            }

            if (jObjectParsed.TryGetValue("desktop_backgrounds", out var customCampaignDesktopBackgrounds))
            {
                JArray backgroundNames = (JArray)customCampaignDesktopBackgrounds;

                foreach (string backgroundName in backgroundNames)
                {
                    if (string.IsNullOrEmpty(backgroundName))
                    {
                        MelonLogger.Error($"ERROR: Did not find '{backgroundName}'. Adding no background.");
                        backgroundSprites.Add(null);
                    }
                    else
                    {
                        backgroundSprites.Add(ImageImport.LoadImage(jsonFolderPath + "\\" + backgroundName,
                            usermodFolderPath + "\\" + backgroundName));
                    }
                }
            }

            if (jObjectParsed.TryGetValue("disable_green_color_on_desktop", out var disableGreenColorOnDesktopValue))
            {
                disableGreenColorBackground = (bool)disableGreenColorOnDesktopValue;
            }

            if (jObjectParsed.TryGetValue("desktop_background_color", out var _desktopBackgroundColor))
            {
                if (_desktopBackgroundColor.Type == JTokenType.Array)
                {
                    List<float> desktopBackgroundColorList = _desktopBackgroundColor.ToObject<List<float>>();

                    switch (desktopBackgroundColorList.Count)
                    {
                        case 3:
                            desktopBackgroundColor = new Color(desktopBackgroundColorList[0],
                                desktopBackgroundColorList[1], desktopBackgroundColorList[2]);
                            break;

                        case 4:
                            desktopBackgroundColor = new Color(desktopBackgroundColorList[0],
                                desktopBackgroundColorList[1], desktopBackgroundColorList[2], 
                                desktopBackgroundColorList[3]);
                            break;

                        default:
                            MelonLogger.Error("ERROR: " +
                                              "Provided color for desktop background is invalid! " +
                                              "Make sure its 3 or 4 values.");
                            break;
                    }
                }
            }

            if (jObjectParsed.TryGetValue("desktop_logo_image_name", out var customDesktopLogoNameValue))
            {
                string customDesktopLogoPath = (string)customDesktopLogoNameValue;

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

            if (jObjectParsed.TryGetValue("disable_desktop_logo", out var disableDesktopLogoValue))
            {
                disableBackgroundLogo = (bool)disableDesktopLogoValue;
            }

            if (jObjectParsed.TryGetValue("desktop_logo_transparency",
                    out var customDesktopLogoTransparencyValue))
            {
                backgroundLogoTransparency = (float)customDesktopLogoTransparencyValue;
            }

            if (jObjectParsed.TryGetValue("campaign_day_names", out var customCampaignDaysNamesValue))
            {
                JArray _customCampaignDays = (JArray)customCampaignDaysNamesValue;

                foreach (JToken campaignDay in _customCampaignDays)
                {
                    dayTitleStrings.Add((string)campaignDay);
                }
            }

            return new ThemeExtraInfo
            {
                customCampaignName = customCampaignName,

                unlockDays = unlockDays,

                usernameText = username,

                renameMainGameDesktopIcon = renameMainGameDesktopIcon,
                mainGameDesktopIcon = mainGameDesktopIconSprite,

                desktopBackgrounds = backgroundSprites,
                disableColorBackground = disableGreenColorBackground,
                desktopBackgroundColor = desktopBackgroundColor,

                customBackgroundLogo = backgroundLogo,
                disableDesktopLogo = disableBackgroundLogo,
                backgroundLogoTransparency = backgroundLogoTransparency,

                dayTitleStrings = dayTitleStrings
            };
        }
    }
}