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

            string imageFileName = token.Value<string>();

            if (string.IsNullOrEmpty(imageFileName))
            {
                LoggingHelper.ErrorLog($"Invalid file name given for '{imageFileName}' for key '{key}'. " +
                                       $"Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
            }
            else
            {
                string correctPath = ImageImport.GetCorrectImagePath(imageFileName, jsonFolderPath, usermodFolderPath);

                Sprite imageCache = ImageCache.TryGet(correctPath);

                if (imageCache == null)
                {
                    imageCache = ImageImport.LoadImage(jsonFolderPath + "\\" + imageFileName,
                        usermodFolderPath + "\\" + imageFileName);

                    ImageCache.AddCache(correctPath, imageCache);
                }

                target = imageCache;
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
        /// <param name="ignoreIfNull">If to ignore a given image if it is empty or null.</param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single image (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignSpriteListOrSingleSprite(JObject jObjectParsed, string key,
            ref List<Sprite> target,
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
                    Sprite value = null;

                    string imageFileName = token.Value<string>();

                    if (string.IsNullOrEmpty(imageFileName))
                    {
                        LoggingHelper.ErrorLog($"Invalid file name given for '{imageFileName}' for key '{key}'.");
                    }
                    else
                    {
                        string correctPath =
                            ImageImport.GetCorrectImagePath(imageFileName, jsonFolderPath, usermodFolderPath);

                        Sprite imageCache = ImageCache.TryGet(correctPath);

                        if (imageCache == null)
                        {
                            imageCache = ImageImport.LoadImage(jsonFolderPath + "\\" + imageFileName,
                                usermodFolderPath + "\\" + imageFileName);

                            ImageCache.AddCache(correctPath, imageCache);
                        }

                        value = imageCache;
                    }

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
                string imageFileName = imageName.Value<string>();

                if (string.IsNullOrEmpty(imageFileName))
                {
                    if (ignoreIfNull)
                    {
                        target.Add(null);
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Invalid file name given for '{imageFileName}' for key '{key}'.");
                    }
                }
                else
                {
                    string correctPath =
                        ImageImport.GetCorrectImagePath(imageFileName, jsonFolderPath, usermodFolderPath);

                    Sprite imageCache = ImageCache.TryGet(correctPath);

                    if (imageCache == null)
                    {
                        imageCache = ImageImport.LoadImage(jsonFolderPath + "\\" + imageFileName,
                            usermodFolderPath + "\\" + imageFileName);

                        ImageCache.AddCache(correctPath, imageCache);
                    }

                    target.Add(imageCache);
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the key for an image list. (Variable Changed Version)
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="jsonFolderPath">Path where the JSON is located at.</param>
        /// <param name="usermodFolderPath">Path where the usermod is located at.</param>
        /// <param name="ignoreIfNull">If to ignore the given image if it is empty or null.</param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single image (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignSpriteListOrSingleSpriteVariableChanged(JObject jObjectParsed, string key,
            ref VariableChanged<List<Sprite>> target, string jsonFolderPath, string usermodFolderPath,
            bool ignoreIfNull = false)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return null;
            }

            if (target.Data == null)
            {
                target.Data = new List<Sprite>();
            }

            if (token.Type == JTokenType.Array)
            {
                TryAssignSpriteList(jObjectParsed, key, ref target.Data, jsonFolderPath, usermodFolderPath,
                    ignoreIfNull);

                if (target.Data.Count > 0)
                {
                    target.HasChanged = true;
                }

                return false;
            }
            else
            {
                try
                {
                    Sprite value = null;

                    string imageFileName = token.Value<string>();

                    if (string.IsNullOrEmpty(imageFileName))
                    {
                        LoggingHelper.ErrorLog($"Invalid file name given for '{imageFileName}' for key '{key}'.");
                    }
                    else
                    {
                        string correctPath =
                            ImageImport.GetCorrectImagePath(imageFileName, jsonFolderPath, usermodFolderPath);

                        Sprite imageCache = ImageCache.TryGet(correctPath);

                        if (imageCache == null)
                        {
                            imageCache = ImageImport.LoadImage(jsonFolderPath + "\\" + imageFileName,
                                usermodFolderPath + "\\" + imageFileName);

                            ImageCache.AddCache(correctPath, imageCache);
                        }

                        value = imageCache;
                    }

                    target.Data.Add(value);
                    target.HasChanged = true;

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
        /// (Variable Changed Version)
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        /// <param name="ignoreIfNull">Ignores if the given image is null.</param>
        public static bool TryAssignSpriteListVariableChanged(JObject jObjectParsed, string key,
            ref VariableChanged<List<Sprite>> target, string jsonFolderPath, string usermodFolderPath,
            bool ignoreIfNull = false)
        {
            if (!jObjectParsed.TryGetValue(key, out JToken token))
            {
                return false;
            }

            if (target.Data == null)
            {
                target.Data = new List<Sprite>();
            }

            JArray pathImages = (JArray)token;

            foreach (JToken imageName in pathImages)
            {
                string imageFileName = imageName.Value<string>();

                if (string.IsNullOrEmpty(imageFileName))
                {
                    if (ignoreIfNull)
                    {
                        target.Data.Add(null);
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Invalid file name given for '{imageFileName}' for key '{key}'.");
                    }
                }
                else
                {
                    string correctPath =
                        ImageImport.GetCorrectImagePath(imageFileName, jsonFolderPath, usermodFolderPath);

                    Sprite imageCache = ImageCache.TryGet(correctPath);

                    if (imageCache == null)
                    {
                        imageCache = ImageImport.LoadImage(jsonFolderPath + "\\" + imageFileName,
                            usermodFolderPath + "\\" + imageFileName);

                        ImageCache.AddCache(correctPath, imageCache);
                    }

                    target.Data.Add(imageCache);
                }
            }

            if (target.Data.Count > 0)
            {
                target.HasChanged = true;
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

            string imageName = token.Value<string>();

            target = new VariableChanged<Sprite>();

            if (string.IsNullOrEmpty(imageName))
            {
                LoggingHelper.ErrorLog($"Invalid file name given for '{imageName}' for key '{key}'. " +
                                       $"Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
            }
            else
            {
                string correctPath = ImageImport.GetCorrectImagePath(imageName, jsonFolderPath, usermodFolderPath);

                Sprite imageCache = ImageCache.TryGet(correctPath);

                if (imageCache == null)
                {
                    imageCache = ImageImport.LoadImage(jsonFolderPath + "\\" + imageName,
                        usermodFolderPath + "\\" + imageName);

                    ImageCache.AddCache(correctPath, imageCache);
                }

                if (imageCache != null)
                {
                    LoggingHelper.DebugLog($"Loaded in sprite (image) '{imageName}' successfully.");
                    target.HasChanged = true;
                    target.Data = imageCache;
                }
            }
        }
    }
}