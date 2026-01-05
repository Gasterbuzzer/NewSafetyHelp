using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.JSONParsing.CCParsing;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing
{
    public static class ParseJSONFiles
    {
        public enum JSONParseTypes
        {
            Campaign,
            Call,
            Entry,
            Email,
            Video,
            Music,
            Modifier,
            Theme,
            Ringtone,
            Invalid
        }

        /// <summary>
        /// Goes through all directories in the mods userdata folder and tries adding parsing it,
        /// if it contains an entry or something to be added.
        /// </summary>
        /// <param name="__instance"> Instance of the EntryUnlockController. Needed for accessing and adding some entries. </param>
        public static void LoadAllJSON(EntryUnlockController __instance)
        {
            // Halt the game while we are parsing and loading all necessary files.
            Time.timeScale = 0.0f;

            string userDataPath = FileImporter.GetUserDataFolderPath();

            string[] foldersDataPath = Directory.GetDirectories(userDataPath);

            foreach (string foldersStringName in foldersDataPath)
            {
                LoadJsonFilesFromFolder(foldersStringName, __instance);
            }

            // If no audio is loading, we can reset the game back. If not, we let the audios do so.
            if (AudioImport.currentLoadingAudios.Count <= 0)
            {
                Time.timeScale = 1.0f;
            }
        }

        /// <summary>
        /// Function for adding a single entry.
        /// </summary>
        /// <param name="modFolderPath"> Path to the folder containing the entry. </param>
        /// <param name="__instance"> Instance of the EntryUnlockController. Needed for accessing and adding some entries. </param>
        public static void LoadJsonFilesFromFolder(string modFolderPath, EntryUnlockController __instance)
        {
            string[] filesDataPath = Directory.GetFiles(modFolderPath, "*.json", SearchOption.AllDirectories);

            foreach (string jsonPathFile in filesDataPath)
            {
                try
                {
                    MelonLogger.Msg($"INFO: Found new JSON file at '{jsonPathFile}', attempting to parse it now.");

                    string jsonString = File.ReadAllText(jsonPathFile);

                    string jsonFolderPath = Path.GetDirectoryName(jsonPathFile);

                    JObject jObjectParse = JObject.Parse(jsonString);

                    JSONParseTypes jsonType = GetJSONParsingType(jObjectParse, modFolderPath);

                    switch (jsonType)
                    {
                        case JSONParseTypes.Campaign: // The provided JSON is a standalone campaign declaration.
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a custom campaign.");
                            CustomCampaignParsing.CreateCustomCampaign(jObjectParse, modFolderPath, jsonFolderPath);
                            break;

                        case JSONParseTypes.Call: // The provided JSON is a standalone call.
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a custom caller.");
                            CustomCallerParsing.CreateCustomCaller(jObjectParse, modFolderPath, jsonFolderPath);
                            break;

                        case JSONParseTypes.Entry: // The provided JSON is a standalone entry.
                            MelonLogger.Msg(
                                "INFO: " +
                                $"Provided JSON file at '{jsonPathFile}' has been interpreted as a monster entry.");
                            EntryParsing.EntryParsing.CreateMonsterFromJSON(jObjectParse, usermodFolderPath: modFolderPath,
                                jsonFolderPath: jsonFolderPath, entryUnlockerInstance: __instance);
                            break;

                        case JSONParseTypes.Email: // The provided JSON is an email (for custom campaigns).
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a email.");
                            EmailParsing.CreateEmail(jObjectParse, modFolderPath, jsonFolderPath);
                            break;

                        case JSONParseTypes.Video: // The provided JSON is a video (for custom campaigns).
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a video.");
                            VideoParsing.CreateVideo(jObjectParse, modFolderPath, jsonFolderPath);
                            break;

                        case JSONParseTypes.Music: // The provided JSON is a music file (for custom campaigns).
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a music file.");
                            MusicParsing.CreateMusic(jObjectParse, modFolderPath, jsonFolderPath);
                            break;

                        case JSONParseTypes.Modifier: // The provided JSON is a modifier file (for custom campaigns).
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a modifier file.");
                            ModifierParsing.CreateModifier(jObjectParse, modFolderPath, jsonFolderPath);
                            break;
                        
                        case JSONParseTypes.Theme: // The provided JSON is a theme file (for custom campaigns).
                            MelonLogger.Msg(
                                $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a theme file.");
                            ThemeParsing.CreateTheme(jObjectParse, modFolderPath, jsonFolderPath);
                            break;

                        case JSONParseTypes.Invalid: // The provided JSON is invalid / unknown of.
                            MelonLogger.Error(
                                "ERROR: Provided JSON file parsing failed or is not any known provided format." +
                                "\n(If this intended, you can ignore this, if not, check if you have written your JSON correctly)." +
                                "\nSkipping trying to read this file.");
                            break;

                        default: // Unknown Error
                            MelonLogger.Error("ERROR: This error should not happen. Possible file corruption.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"ERROR: Failed in reading file '{jsonPathFile}'. " +
                                      $"Error message: '{e.Message};{e.StackTrace}'.");
                }
            }
        }

        /// <summary>
        /// Checks if the JSON object contains any of the keys.
        /// </summary>
        /// <param name="keys">List of keys to check </param>
        /// <param name="json">JObject with the keys</param>
        /// <returns></returns>
        public static bool containsKeys(List<string> keys, JObject json)
        {
            return keys.Any(json.ContainsKey); // Checks if any of the keys is in the JSON via the flag ContainsKey
        }

        /// <summary>
        /// Checks what type of JSON file it is and returns the type back. Used for checking what to do with the file.
        /// </summary>
        /// <param name="json"> JSON Object</param>
        /// <param name="filePath"> File path to the JSON file.</param>
        /// <returns></returns>
        public static JSONParseTypes GetJSONParsingType(JObject json, string filePath = "")
        {
            // Invalid JSON.
            if (json is null || json.Type != JTokenType.Object || string.IsNullOrEmpty(filePath))
            {
                return JSONParseTypes.Invalid;
            }

            // Added Campaign Settings
            if (containsKeys(
                    new List<string>
                        { "custom_campaign_name", "custom_campaign_days", "custom_campaign_icon_image_name" }, json))
            {
                return JSONParseTypes.Campaign;
            }

            // Custom Call added either to main campaign or custom campaign.
            if (containsKeys(
                    new List<string>
                    {
                        "custom_caller_transcript", "custom_caller_name", "order_in_campaign",
                        "custom_caller_audio_clip_name"
                    }, json))
            {
                return JSONParseTypes.Call;
            }

            // Entry was provided.
            if (containsKeys(new List<string> { "monster_name", "replace_entry", "caller_name" }, json))
            {
                return JSONParseTypes.Entry;
            }

            // Email was provided. 
            if (containsKeys(
                    new List<string> { "email_subject", "email_in_main_campaign", "email_custom_campaign_name" }, json))
            {
                return JSONParseTypes.Email;
            }

            // Video was provided (Desktop Video!)
            if (containsKeys(new List<string> { "video_desktop_name", "video_file_name", "video_unlock_day" }, json))
            {
                return JSONParseTypes.Video;
            }

            // Music was provided
            if (containsKeys(new List<string> { "music_audio_clip_name" }, json))
            {
                return JSONParseTypes.Music;
            }

            // Modifier was provided
            if (!containsKeys(
                    new List<string>
                        { "custom_campaign_name", "custom_campaign_days", "custom_campaign_icon_image_name", 
                            "email_subject", "email_in_main_campaign", "email_custom_campaign_name" }, json)
                &&
                containsKeys(new List<string>
                {
                    "modifier_custom_campaign_attached"
                }, json))
            {
                return JSONParseTypes.Modifier;
            }
            
            // Theme was provided
            if (!containsKeys(
                    new List<string>
                    { "custom_campaign_name", "custom_campaign_days", "custom_campaign_icon_image_name", 
                        "email_subject", "email_in_main_campaign", "email_custom_campaign_name" }, json)
                &&
                containsKeys(new List<string>
                {
                    "theme_custom_campaign_attached", "theme_name", "title_bar_color", "theme_in_main_campaign"
                }, json))
            {
                return JSONParseTypes.Theme;
            }
            
            // Unknown JSON type or failed parsing the file.
            return JSONParseTypes.Invalid;
        }
    }
}