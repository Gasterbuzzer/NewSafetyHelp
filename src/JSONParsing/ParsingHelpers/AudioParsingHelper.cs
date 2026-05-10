using System;
using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class AudioParsingHelper
    {
        /// <summary>
        /// Attempts to update the audio at a given location to a given audio variable via coroutines.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is located at.</param>
        /// <param name="audioLocation">Location of the audio to read</param>
        /// <param name="setAudioClip">Function to set the rich audio clip by the function caller.</param>
        /// <param name="jsonFolderPath">Folder path to the JSON.</param>
        /// <param name="key">Key for the audio.</param>
        public static void UpdateAudioAtLocation(JObject jObjectParsed, string audioLocation,
            // ReSharper disable once RedundantAssignment
            Action<RichAudioClip> setAudioClip, string jsonFolderPath,
            string key = "audio_clip_location")
        {
            if (setAudioClip == null)
            {
                LoggingHelper.ErrorLog("Provided lambda function was not set. Unable of updating the audio.");
                return;
            }
            
            if (jObjectParsed.ContainsKey(key))
            {
                if (string.IsNullOrEmpty(audioLocation))
                {
                    LoggingHelper.WarningLog($"No valid audio file given for file in {jsonFolderPath}.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(audioLocation))
                {
                    LoggingHelper.ErrorLog($"Location {jsonFolderPath} does not contain '{audioLocation}'. " +
                                           "Unable of adding the audio.");
                }
                else // Valid location, so we load in the value.
                {
                    MelonCoroutines.Start(AudioImport.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    setAudioClip(AudioImport.CreateRichAudioClip(myReturnValue));
                                }
                                else
                                {
                                    LoggingHelper.ErrorLog($"Failed to load audio clip '{audioLocation}'.");
                                }
                            },
                            audioLocation)
                    );
                }
            }
        }
        
        /// <summary>
        /// Attempts to update the audio at a given location to a given audio variable via coroutines.
        /// </summary>
        /// <param name="audioLocation">Location of the audio to read</param>
        /// <param name="setAudioClip">Function to set the rich audio clip by the function caller.</param>
        /// <param name="jsonFolderPath">Folder path to the JSON.</param>
        public static void UpdateAudioAtLocationNoKey(string audioLocation,
            // ReSharper disable once RedundantAssignment
            Action<RichAudioClip> setAudioClip, string jsonFolderPath)
        {
            if (setAudioClip == null)
            {
                LoggingHelper.ErrorLog("Provided lambda function was not set. Unable of updating the audio.");
                return;
            }
            
            if (string.IsNullOrEmpty(audioLocation))
            {
                LoggingHelper.WarningLog($"No valid audio file given for file in {jsonFolderPath}.");
            }
            // Check if location is valid now, since we are storing it now.
            else if (!File.Exists(audioLocation))
            {
                LoggingHelper.ErrorLog($"Location {jsonFolderPath} does not contain '{audioLocation}'. " +
                                       "Unable of adding the audio.");
            }
            else // Valid location, so we load in the value.
            {
                MelonCoroutines.Start(AudioImport.UpdateAudioClip
                    (
                        (myReturnValue) =>
                        {
                            if (myReturnValue != null)
                            {
                                // Add the audio
                                setAudioClip(AudioImport.CreateRichAudioClip(myReturnValue));
                            }
                            else
                            {
                                LoggingHelper.ErrorLog($"Failed to load audio clip '{audioLocation}'.");
                            }
                        },
                        audioLocation)
                );
            }
        }
        
        /// <summary>
        /// Attempts to assign the audio file path to the target string. But only if the audio file exists.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        /// <param name="nameOfTarget">(Optional) Provide the name of the target (For example for a custom caller).
        /// Used to display errors.</param>
        public static void TryAssignAudioPath(JObject jObjectParsed, string key, ref string target,
            string jsonFolderPath, string usermodFolderPath, string nameOfTarget = null)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            string audioPath = token.Value<string>();

            if (!File.Exists(jsonFolderPath + "\\" + audioPath))
            {
                if (!File.Exists(usermodFolderPath + "\\" + audioPath))
                {
                    LoggingHelper.WarningLog($"Could not find provided audio file for key '{key}' at " +
                                             $"'{jsonFolderPath}' (For Audio '{audioPath}').");
                    if (nameOfTarget != null)
                    {
                        LoggingHelper.WarningLog($"For '{nameOfTarget}'.");
                    }
                }
                else
                {
                    target = usermodFolderPath + "\\" + audioPath;
                }
            }
            else
            {
                target = jsonFolderPath + "\\" + audioPath;
            }
        }
    }
}