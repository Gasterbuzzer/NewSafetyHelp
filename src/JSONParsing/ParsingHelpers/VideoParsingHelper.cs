using System.Collections.Generic;
using System.IO;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class VideoParsingHelper
    {
        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="jsonFolderPath"> Folder path where the JSON is located. </param>
        /// <param name="usermodFolderPath"> Folder path where the usermod is located. </param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single element (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignUrlListOrSingleUrl(JObject jObjectParsed, string key, ref List<string> target,
            string jsonFolderPath, string usermodFolderPath)
        {
            bool? result = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, key, ref target);

            for (int i = 0; i < target.Count; i++)
            {
                if (string.IsNullOrEmpty(target[i]))
                {
                    LoggingHelper.WarningLog("Provided video path is empty. Unable to show show video.");
                }
                else
                {
                    string firstFilePath = jsonFolderPath + "\\" + target[i];
                    string videoFileAlternativePath = usermodFolderPath + "\\" + target[i];

                    if (File.Exists(firstFilePath))
                    {
                        target[i] = firstFilePath;
                    }
                    else if (File.Exists(videoFileAlternativePath))
                    {
                        target[i] = videoFileAlternativePath;
                    }
                    else if (!File.Exists(firstFilePath) && !File.Exists(videoFileAlternativePath))
                    {
                        LoggingHelper.WarningLog(
                            $"Could not find video '{target[i]}' in either: '{firstFilePath}' or " +
                            $"'{videoFileAlternativePath}'.");
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// Attempts to assign the video file path to the target string. But only if the video file exists.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        public static bool TryAssignVideoPath(JObject jObjectParsed, string key, ref string target,
            string jsonFolderPath, string usermodFolderPath)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return false;
            }

            string videoFilePath = token.Value<string>();

            string updatedFilePath = jsonFolderPath + "\\" + videoFilePath;
            string videoFileAlternativePath = usermodFolderPath + "\\" + videoFilePath;

            if (string.IsNullOrEmpty(videoFilePath))
            {
                LoggingHelper.WarningLog("Provided video path but name is empty. Unable to show show video.");
                target = "";
            }
            else
            {
                if (File.Exists(updatedFilePath))
                {
                    target = updatedFilePath;
                }
                else if (File.Exists(videoFileAlternativePath))
                {
                    target = videoFileAlternativePath;
                }
                else
                {
                    LoggingHelper.WarningLog($"Provided video '{videoFilePath}' could not be found in either " +
                                             $"'{updatedFilePath}' " +
                                             $"or '{videoFileAlternativePath}'.");
                    target = "";
                }
            }

            return true;
        }
    }
}