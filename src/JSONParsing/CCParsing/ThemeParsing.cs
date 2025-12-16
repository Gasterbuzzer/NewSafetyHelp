using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.CustomCampaign.Themes;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class ThemeParsing
    {
        /// <summary>
        /// Load a theme from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateTheme(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a theme. Possible syntax mistake?");
                return;
            }
            
            // Campaign Values
            string customCampaignName = "";

            ThemesExtraInfo customTheme = ParseTheme(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaignExtraInfo foundCustomCampaign =
                CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.campaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                if (customTheme.unlockDays == null)
                {
                    foundCustomCampaign.customThemesGeneral.Add(customTheme);
                }
                else
                {
                    foundCustomCampaign.customThemesDays.Add(customTheme);
                }
            }
            else
            {
                #if DEBUG
                MelonLogger.Msg("DEBUG: Found theme file before the custom campaign was found / does not exist.");
                #endif

                ParseJSONFiles.missingCustomCampaignTheme.Add(customTheme);
            }
        }

        private static ThemesExtraInfo ParseTheme(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            string themeName = "NO THEME NAME SET";
            
            // If theme is a conditional theme (Not null). If general (equal to null)
            List<int> unlockDays = null;
            
            // Theme Colors
            Color? desktopBackgroundColor = null;

            if (jObjectParsed.TryGetValue("theme_name", out JToken themeNameValue))
            {
                themeName = themeNameValue.Value<string>();
            }
            
            if (jObjectParsed.TryGetValue("theme_custom_campaign_attached", out JToken customCampaignNameValue))
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
            
            return new ThemesExtraInfo()
            {
                themeName = themeName,
                customCampaignName = customCampaignName,
                
                unlockDays = unlockDays,
            };
        }
    }
}