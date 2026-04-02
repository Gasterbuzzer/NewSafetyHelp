using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class ImageParsingHelper
    {
        /// <summary>
        /// Tries to assign the target with the image from the given JSON at the given key.
        /// If not found or if any problems happen, it will not write.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        /// <param name="customCampaignName">(Optional) Name of the custom campaign. Used to display errors.</param>
        public static bool TryAssignSprite(JObject jObjectParsed, string key, ref Sprite target, string jsonFolderPath,
            string usermodFolderPath, string customCampaignName = null)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return false;
            }

            string imagePath = token.Value<string>();

            if (string.IsNullOrEmpty(imagePath))
            {
                LoggingHelper.ErrorLog($"Invalid file name given for '{imagePath}' for key {key}. " +
                                       $"Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
            }
            else
            {
                target = ImageImport.LoadImage(jsonFolderPath + "\\" + imagePath,
                    usermodFolderPath + "\\" + imagePath);
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the key for an image list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="jsonFolderPath">Path where the JSON is located at.</param>
        /// <param name="usermodFolderPath">Path where the usermod is located at.</param>
        /// <param name="ignoreIfNull">If to ignore if a given image is empty or null.</param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single image (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignImageListOrSingleImage(JObject jObjectParsed, string key, ref List<Sprite> target,
            string jsonFolderPath, string usermodFolderPath, bool ignoreIfNull = false)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return null;
            }

            if (target == null)
            {
                target = new List<Sprite>();
            }

            if (token.Type == JTokenType.Array)
            {
                TryAssignSpriteList(jObjectParsed, key, ref target, jsonFolderPath, usermodFolderPath, ignoreIfNull);
                return false;
            }
            else
            {
                try
                {
                    Sprite value = token.Value<Sprite>();
                    target.Add(value);

                    return true;
                }
                catch
                {
                    LoggingHelper.ErrorLog($"For provided key '{key}' " +
                                           "we were unable of assigning any value, as the wrong value was given.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Tries to assign the target list with the images from the given JSON at the given key.
        /// If not found or if any problems happen, it will not add to the list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        /// <param name="ignoreIfNull">Ignores if the given image is null.</param>
        public static bool TryAssignSpriteList(JObject jObjectParsed, string key, ref List<Sprite> target,
            string jsonFolderPath, string usermodFolderPath, bool ignoreIfNull = false)
        {
            if (!jObjectParsed.TryGetValue(key, out JToken token))
            {
                return false;
            }

            JArray pathImages = (JArray)token;

            foreach (JToken imageName in pathImages)
            {
                string imagePath = imageName.Value<string>();

                if (string.IsNullOrEmpty(imagePath))
                {
                    if (ignoreIfNull)
                    {
                        target.Add(null);
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Invalid file name given for '{imagePath}' for key {key}.");
                    }
                }
                else
                {
                    target.Add(
                        ImageImport.LoadImage(jsonFolderPath + "\\" + imagePath,
                            usermodFolderPath + "\\" + imagePath)
                    );
                }
            }

            return true;
        }
        
        /// <summary>
        /// Tries to assign the target with the image from the given JSON at the given key.
        /// If not found or if any problems happen, it will not write.
        /// It will use the VariableChanged generic class.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        /// <param name="customCampaignName">(Optional) Name of the custom campaign. Used to display errors.</param>
        public static void TryAssignSpriteChanged(JObject jObjectParsed, string key, ref VariableChanged<Sprite> target,
            string jsonFolderPath, string usermodFolderPath, string customCampaignName = null)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            string imagePath = token.Value<string>();

            target = new VariableChanged<Sprite>();

            if (string.IsNullOrEmpty(imagePath))
            {
                LoggingHelper.ErrorLog($"Invalid file name given for '{imagePath}' for key {key}. " +
                                       $"Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
            }
            else
            {
                Sprite parsedSprite = ImageImport.LoadImage(jsonFolderPath + "\\" + imagePath, usermodFolderPath + "\\" + imagePath);
                if (parsedSprite != null)
                {
                    target.HasChanged = true;
                    target.Data = parsedSprite;
                }
            }
        }
    }
}