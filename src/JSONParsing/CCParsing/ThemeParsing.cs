using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.Themes;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static NewSafetyHelp.CustomThemes.ColorHelper;

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

            CustomTheme customTheme = ParseTheme(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign.CustomCampaignModel.CustomCampaign foundCustomCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);
            
            if (customTheme.inMainCampaign)
            {
                GlobalParsingVariables.MainGameThemes.Add(customTheme);
            }
            else
            {
                if (foundCustomCampaign != null)
                {
                    if (customTheme.unlockDays == null)
                    {
                        foundCustomCampaign.CustomThemesGeneral.Add(customTheme);
                    }
                    else
                    {
                        foundCustomCampaign.CustomThemesDays.Add(customTheme);
                    }
                }
                else
                {
                    #if DEBUG
                    MelonLogger.Msg("DEBUG: Found theme file before the custom campaign was found / does not exist.");
                    #endif

                    GlobalParsingVariables.PendingCustomCampaignThemes.Add(customTheme);
                }
            }
        }

        private static CustomTheme ParseTheme(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            bool inMainCampaign = false; // If available in main campaign.
            
            string themeName = "NO THEME NAME SET";
            
            string attachedToTheme = null; // If a conditional theme, what theme is it attached to?
            
            // If theme is a conditional theme (Not null). If general (equal to null)
            List<int> unlockDays = null;
            
            // Theme Colors
            ColorPalette themeColorPalette = ScriptableObject.CreateInstance<ColorPalette>();
            themeColorPalette.colorSwatch = new Color[4];

            // Default Colors (Red => Error / Not Provided)
            for (int i = 0; i < themeColorPalette.colorSwatch.Length; i++)
            {
                themeColorPalette.colorSwatch[i] = new Color(1, 0.2178f, 0.2745f, 1);
            }

            if (jObjectParsed.TryGetValue("theme_custom_campaign_attached", out JToken customCampaignNameValue))
            {
                customCampaignName = customCampaignNameValue.Value<string>();
            }
            else if (jObjectParsed.TryGetValue("theme_in_main_campaign", out JToken themeInMainCampaignValue))
            {
                inMainCampaign = themeInMainCampaignValue.Value<bool>();
            }
            
            ParsingHelper.TryAssign(jObjectParsed, "theme_name", ref themeName);
            ParsingHelper.TryAssign(jObjectParsed, "attached_to_theme", ref attachedToTheme);

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

            if (jObjectParsed.TryGetValue("title_bar_color", out JToken titleBarColorValue))
            {
                SetColor(ref titleBarColorValue, ref themeColorPalette, 0);
            }
            
            if (jObjectParsed.TryGetValue("menu_color", out JToken menuColorValue))
            {
                SetColor(ref menuColorValue, ref themeColorPalette, 1);
            }
            
            if (jObjectParsed.TryGetValue("third_color", out JToken thirdColorValue))
            {
                SetColor(ref thirdColorValue, ref themeColorPalette, 2);
            }
            
            if (jObjectParsed.TryGetValue("entry_font_color", out JToken entryFontColorValue))
            {
                SetColor(ref entryFontColorValue, ref themeColorPalette, 2);
            }
            
            if (jObjectParsed.TryGetValue("main_window_color", out JToken mainWindowColorValue))
            {
                SetColor(ref mainWindowColorValue, ref themeColorPalette, 3);
            }
            
            return new CustomTheme()
            {
                inMainCampaign = inMainCampaign,
                
                themeName = themeName,
                customCampaignName = customCampaignName,
                
                attachedToTheme = attachedToTheme,
                
                unlockDays = unlockDays,
                
                customThemePalette = themeColorPalette
            };
        }

        private static void SetColor(ref JToken jsonValue, ref ColorPalette themeColorPalette, int colorIndex)
        {
            if (jsonValue.Type == JTokenType.Array)
            {
                // We first create a list and all floats.
                // If we have 3 colors, we simply add these, if we have four,
                // we interpret the 4th value as the alpha value.
                List<float> desktopBackgroundColorList = new List<float>();

                foreach (JToken desktopBackgroundColorToken in (JArray) jsonValue)
                {
                    desktopBackgroundColorList.Add(desktopBackgroundColorToken.Value<float>());
                }

                switch (desktopBackgroundColorList.Count)
                {
                    case 3:
                        themeColorPalette.colorSwatch[colorIndex] = new Color(GetConvertedColorFloat(desktopBackgroundColorList[0]),
                            GetConvertedColorFloat(desktopBackgroundColorList[1]), GetConvertedColorFloat(desktopBackgroundColorList[2]));
                        break;

                    case 4:
                        themeColorPalette.colorSwatch[colorIndex] = new Color(GetConvertedColorFloat(desktopBackgroundColorList[0]),
                            GetConvertedColorFloat(desktopBackgroundColorList[1]), GetConvertedColorFloat(desktopBackgroundColorList[2]), 
                            GetConvertedColorFloat(desktopBackgroundColorList[3]));
                        break;

                    default:
                        MelonLogger.Error("ERROR: " +
                                          "Provided color for setting color is invalid! " +
                                          "Make sure it's 3 or 4 values.");
                        break;
                }
            }
        }
    }
}