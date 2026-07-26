using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static NewSafetyHelp.CustomThemes.ColorHelper;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class ColorParsingHelper
    {
        /// <summary>
        /// Sets color for a theme.
        /// </summary>
        /// <param name="jsonValue">Value from key.</param>
        /// <param name="themeColorPalette">ColorPalette for Themes.</param>
        /// <param name="colorIndex">Index in color palette to write the color into.</param>
        public static void SetColor(ref JToken jsonValue, ref ColorPalette themeColorPalette, int colorIndex)
        {
            if (jsonValue.Type == JTokenType.Array)
            {
                // We first create a list and all floats.
                // If we have 3 colors, we simply add these, if we have four,
                // we interpret the 4th value as the alpha value.
                List<float> colorList = new List<float>();

                foreach (JToken colorToken in (JArray)jsonValue)
                {
                    colorList.Add(colorToken.Value<float>());
                }

                switch (colorList.Count)
                {
                    case 3:
                        themeColorPalette.colorSwatch[colorIndex] = new Color(GetConvertedColorFloat(colorList[0]),
                            GetConvertedColorFloat(colorList[1]), GetConvertedColorFloat(colorList[2]));
                        break;

                    case 4:
                        themeColorPalette.colorSwatch[colorIndex] = new Color(GetConvertedColorFloat(colorList[0]),
                            GetConvertedColorFloat(colorList[1]), GetConvertedColorFloat(colorList[2]),
                            GetConvertedColorFloat(colorList[3]));
                        break;

                    default:
                        LoggingHelper.ErrorLog("Provided color for setting color is invalid! " +
                                               "Make sure it's 3 or 4 values.");
                        break;
                }
            }
        }

        /// <summary>
        /// Set color for given target array.
        /// </summary>
        /// <param name="jsonValue"></param>
        /// <param name="parsedColor"></param>
        public static void SetColor(ref JToken jsonValue, ref Color parsedColor)
        {
            if (jsonValue.Type == JTokenType.Array)
            {
                // We first create a list and all floats.
                // If we have 3 colors, we simply add these, if we have four,
                // we interpret the 4th value as the alpha value.
                List<float> colorList = new List<float>();

                foreach (JToken colorToken in (JArray)jsonValue)
                {
                    colorList.Add(colorToken.Value<float>());
                }

                switch (colorList.Count)
                {
                    case 3:
                        parsedColor = new Color(GetConvertedColorFloat(colorList[0]),
                            GetConvertedColorFloat(colorList[1]), GetConvertedColorFloat(colorList[2]));
                        break;

                    case 4:
                        parsedColor = new Color(GetConvertedColorFloat(colorList[0]),
                            GetConvertedColorFloat(colorList[1]), GetConvertedColorFloat(colorList[2]),
                            GetConvertedColorFloat(colorList[3]));
                        break;

                    default:
                        LoggingHelper.ErrorLog("Provided color for setting color is invalid! " +
                                               "Make sure it's 3 or 4 values.");
                        break;
                }
            }
        }

        /// <summary>
        /// Set color for given target array. (Variable Changed)
        /// </summary>
        /// <param name="jsonValue"></param>
        /// <param name="parsedColor"></param>
        public static void SetColor(ref JToken jsonValue, ref VariableChanged<Color> parsedColor)
        {
            if (jsonValue.Type == JTokenType.Array)
            {
                // We first create a list and all floats.
                // If we have 3 colors, we simply add these, if we have four,
                // we interpret the 4th value as the alpha value.
                List<float> colorList = new List<float>();

                foreach (JToken colorToken in (JArray)jsonValue)
                {
                    colorList.Add(colorToken.Value<float>());
                }

                switch (colorList.Count)
                {
                    case 3:
                        parsedColor.Data = new Color(GetConvertedColorFloat(colorList[0]),
                            GetConvertedColorFloat(colorList[1]), GetConvertedColorFloat(colorList[2]));
                        parsedColor.HasChanged = true;
                        break;

                    case 4:
                        parsedColor.Data = new Color(GetConvertedColorFloat(colorList[0]),
                            GetConvertedColorFloat(colorList[1]), GetConvertedColorFloat(colorList[2]),
                            GetConvertedColorFloat(colorList[3]));
                        parsedColor.HasChanged = true;
                        break;

                    default:
                        LoggingHelper.ErrorLog("Provided color for setting color is invalid! " +
                                               "Make sure it's 3 or 4 values.");
                        break;
                }
            }
        }

        /// <summary>
        /// Parses a color from a given key.
        /// </summary>
        /// <param name="jObjectParsed"></param>
        /// <param name="key"></param>
        /// <param name="target"></param>
        public static void ParseColor(JObject jObjectParsed, string key, ref Color target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            SetColor(ref token, ref target);
        }

        /// <summary>
        /// Parses a color from a given key. (For Variable Changed)
        /// </summary>
        /// <param name="jObjectParsed"></param>
        /// <param name="key"></param>
        /// <param name="target"></param>
        public static void ParseColor(JObject jObjectParsed, string key, ref VariableChanged<Color> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            SetColor(ref token, ref target);
        }
    }
}