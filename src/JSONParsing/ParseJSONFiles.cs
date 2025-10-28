using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CallerPatches;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.JSONParsing.CustomCampaignParsing;
using NewSafetyHelp.JSONParsing.EntryParsing;
using UnityEngine;
using Newtonsoft.Json.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

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
            Invalid
        }
        
        // "Global" Variables for handling extra information such as caller audio. Gets stored as its ID and with its Name.
        public static List<EntryExtraInfo> entriesExtraInfo = new List<EntryExtraInfo>();
        
        // Map for custom callers to replaced in the main game. (ID of the call to replace, Caller for that ID)
        public static Dictionary<int, CustomCallerExtraInfo> customCallerMainGame = new Dictionary<int, CustomCallerExtraInfo>();
        
        // List of custom caller yet to be added to custom campaign. Happens when the custom caller file was found before.
        public static List<CustomCallerExtraInfo> missingCustomCallerCallersCustomCampaign = new List<CustomCallerExtraInfo>();
        
        // List of entries yet to be added to custom campaign. Happens when the entries file was found before.
        public static List<EntryExtraInfo> missingEntriesCustomCampaign = new List<EntryExtraInfo>();
        
        // List of entries that replace yet to be added to custom campaign. Happens when the replacement entries file was found before.
        public static List<EntryExtraInfo> missingReplaceEntriesCustomCampaign = new List<EntryExtraInfo>();
        
        // List of emails to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<EmailExtraInfo> missingCustomCampaignEmails = new List<EmailExtraInfo>();
        
        // List of videos to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<CustomVideoExtraInfo> missingCustomCampaignVideo = new List<CustomVideoExtraInfo>();
        
        // List of emails to be added in the main campaign.
        public static List<EmailExtraInfo> mainCampaignEmails = new List<EmailExtraInfo>();

        public static int amountExtra = 100000;
        
        // Campaign Information
        const int mainCampaignCallAmount = 116;
        
        /// <summary>
        /// Goes through all directories in the mods userdata folder and tries adding parsing it, if it contains an entry or something to be added.
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
                        CreateCustomCampaign(jObjectParse, modFolderPath, jsonFolderPath);
                        break;

                    case JSONParseTypes.Call: // The provided JSON is a standalone call.
                        MelonLogger.Msg(
                            $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a custom caller.");
                        CreateCustomCaller(jObjectParse, modFolderPath, jsonFolderPath);
                        break;

                    case JSONParseTypes.Entry: // The provided JSON is a standalone entry.
                        MelonLogger.Msg(
                            $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a monster entry.");
                        CreateMonsterFromJSON(jObjectParse, usermodFolderPath: modFolderPath,
                            jsonFolderPath: jsonFolderPath, entryUnlockerInstance: __instance);
                        break;

                    case JSONParseTypes.Email: // The provided JSON is an email (for custom campaigns).
                        MelonLogger.Msg(
                            $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a email.");
                        CreateEmail(jObjectParse, modFolderPath, jsonFolderPath);
                        break;

                    case JSONParseTypes.Video: // The provided JSON is a video (for custom campaigns).
                        MelonLogger.Msg(
                            $"INFO: Provided JSON file at '{jsonPathFile}' has been interpreted as a video.");
                        CreateVideo(jObjectParse, modFolderPath, jsonFolderPath);
                        break;

                    case JSONParseTypes.Invalid: // The provided JSON is invalid / unknown of.
                        MelonLogger.Error(
                            "ERROR: Provided JSON file parsing failed or is not any known provided format. Skipped.");
                        break;

                    default: // Unknown Error
                        MelonLogger.Error("ERROR: This error should not happen. Possible file corruption.");
                        break;
                }
                
            }
        }

        /// <summary>
        /// Checks if the json object contains any of the keys.
        /// </summary>
        /// <param name="keys">List of keys to check </param>
        /// <param name="json">JObject with the keys</param>
        /// <returns></returns>
        public static bool containsKeys(List<string> keys, JObject json)
        {
            return keys.Any(json.ContainsKey); // Checks if any of the keys is in the json via the flag ContainsKey
        }
        
        /// <summary>
        /// Checks what type of JSON file it is and returns the type back. Used for checking what to do with the file.
        /// </summary>
        /// <param name="json"> JSON Object</param>
        /// <param name="filePath"> File path to the JSON file.</param>
        /// <returns></returns>
        public static JSONParseTypes GetJSONParsingType(JObject json, string filePath = "")
        {
            if (json is null || json.Type != JTokenType.Object || string.IsNullOrEmpty(filePath)) // Invalid JSON.
            {
                return JSONParseTypes.Invalid;
            }
            
            if (containsKeys(new List<string> {"custom_campaign_name", "custom_campaign_days", "custom_campaign_icon_image_name"}, json)) // Added Campaign Settings
            {
                return JSONParseTypes.Campaign;
            }
            
            if (containsKeys(new List<string> {"custom_caller_transcript", "custom_caller_name", "order_in_campaign", "custom_caller_audio_clip_name"}, json)) // Custom Call added either to main campaign or custom campaign.
            {
                return JSONParseTypes.Call;
            }
            
            if (containsKeys(new List<string> {"monster_name", "replace_entry", "caller_name"}, json)) // Entry was provided.
            {
                return JSONParseTypes.Entry;
            }
            
            if (containsKeys(new List<string> {"email_subject", "email_in_main_campaign", "email_custom_campaign_name"}, json)) // Email was provided. 
            {
                return JSONParseTypes.Email;
            }
            
            if (containsKeys(new List<string> {"video_desktop_name", "video_file_name", "video_unlock_day"}, json)) // Video was provided (Desktop Video!)
            {
                return JSONParseTypes.Video;
            }
            
            // Unknown JSON type or failed parsing the file.
            return JSONParseTypes.Invalid;
        }

        /// <summary>
        /// Creates a custom campaign from a provided json file.
        /// </summary>
        /// <param name="jObjectParsed"></param>
        /// <param name="usermodFolderPath"></param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateCustomCampaign(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object || string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a custom campaign. Possible syntax mistake?");
                return;
            }
            
            string customCampaignName = "NO_CAMPAIGN_NAME_PROVIDED";
            
            CustomCampaignExtraInfo _customCampaign = 
                CustomCampaignParsing.CustomCampaignParsing.parseCampaignFile(ref jObjectParsed, ref usermodFolderPath,
                    ref jsonFolderPath, ref customCampaignName);
            
            // Check if any callers have to be added to this campaign.
            if (missingCustomCallerCallersCustomCampaign.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomCallerExtraInfo> tempList =
                    new List<CustomCallerExtraInfo>(missingCustomCallerCallersCustomCampaign);

                foreach (CustomCallerExtraInfo customCallerCC in tempList)
                {
                    if (customCallerCC.belongsToCustomCampaign == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing custom caller {customCallerCC.callerName} to the custom campaign: {customCampaignName}.");
                        #endif

                        if (customCallerCC.isGameOverCaller)
                        {
                            _customCampaign.customGameOverCallersInCampaign.Add(customCallerCC);
                        }
                        else if (customCallerCC.isWarningCaller)
                        {
                            _customCampaign.customWarningCallersInCampaign.Add(customCallerCC);
                        }
                        else
                        {
                            _customCampaign.customCallersInCampaign.Add(customCallerCC);
                        }

                        missingCustomCallerCallersCustomCampaign.Remove(customCallerCC);
                    }
                }
            }

            // Check if any entries have to be added to this campaign.
            if (missingEntriesCustomCampaign.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<EntryExtraInfo> tempList = new List<EntryExtraInfo>(missingEntriesCustomCampaign);

                foreach (EntryExtraInfo missingEntry in tempList)
                {
                    if (missingEntry.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing entry to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.entriesOnlyInCampaign.Add(missingEntry);

                        missingEntriesCustomCampaign.Remove(missingEntry);
                    }
                }
            }

            // Check if any entries have to be added to this campaign.
            if (missingReplaceEntriesCustomCampaign.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<EntryExtraInfo> tempList = new List<EntryExtraInfo>(missingReplaceEntriesCustomCampaign);

                foreach (EntryExtraInfo missingEntry in tempList)
                {
                    if (missingEntry.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding 'replace' missing entry to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.entryReplaceOnlyInCampaign.Add(missingEntry);
                        missingReplaceEntriesCustomCampaign.Remove(missingEntry);
                    }
                }
            }

            // Check if any emails have to be added to a custom campaign.
            if (missingCustomCampaignEmails.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<EmailExtraInfo> tempList = new List<EmailExtraInfo>(missingCustomCampaignEmails);

                foreach (EmailExtraInfo missingEmail in tempList)
                {
                    if (missingEmail.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing email to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.emails.Add(missingEmail);
                        missingCustomCampaignEmails.Remove(missingEmail);
                    }
                }
            }

            // Check if any videos have to be added to a custom campaign.
            if (missingCustomCampaignVideo.Count > 0)
            {
                // Create a copy of the list to iterate over
                List<CustomVideoExtraInfo> tempList = new List<CustomVideoExtraInfo>(missingCustomCampaignVideo);

                foreach (CustomVideoExtraInfo missingVideo in tempList)
                {
                    if (missingVideo.customCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing video to the custom campaign: {customCampaignName}.");
                        #endif

                        _customCampaign.allDesktopVideos.Add(missingVideo);
                        missingCustomCampaignVideo.Remove(missingVideo);
                    }
                }
            }


            // We finished adding all missing values and now add the campaign as available.
            CustomCampaignGlobal.customCampaignsAvailable.Add(_customCampaign);
        }
        
        /// <summary>
        /// Creates a video program from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateVideo(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object || string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a video. Possible syntax mistake?");
                return;
            }
            
            // Campaign Values
            string customCampaignName = "";

            CustomVideoExtraInfo _customVideo = VideoParsing.ParseVideo(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaignExtraInfo foundCustomCampaign =
                CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.campaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                foundCustomCampaign.allDesktopVideos.Add(_customVideo);
            }
            else
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found Video before the custom campaign was found / does not exist.");
                #endif

                missingCustomCampaignVideo.Add(_customVideo);
            }
        }

        /// <summary>
        /// Creates an email from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed">JSON Parsed</param>
        /// <param name="usermodFolderPath">Filepath to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateEmail(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object || string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a email. Possible syntax mistake?");
                return;
            }
            
            // Campaign Values
            string customCampaignName = "";
            bool inMainCampaign = false;

            EmailExtraInfo _customEmail = EmailParsing.ParseEmail(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath,
                ref customCampaignName, ref inMainCampaign);

            if (inMainCampaign)
            {
                mainCampaignEmails.Add(_customEmail);
            }
            else
            {
                // Add to correct campaign.
                CustomCampaignExtraInfo foundCustomCampaign =
                    CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.campaignName == customCampaignName);

                if (foundCustomCampaign != null)
                {
                    foundCustomCampaign.emails.Add(_customEmail);
                }
                else
                {
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Found Email before the custom campaign was found / does not exist.");
                    #endif

                    missingCustomCampaignEmails.Add(_customEmail);
                }
            }
        }
        
        /// <summary>
        /// Creates a custom caller from a provided json file.
        /// </summary>
        /// <param name="jObjectParsed"></param>
        /// <param name="usermodFolderPath"></param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateCustomCaller(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object || string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a custom caller. Possible syntax mistake?");
                return;
            }
            
            // Actual logic
            string customCampaignName = "NO_CUSTOM_CAMPAIGN";
            bool inMainCampaign = false;
            
            // Campaign Values
            int orderInCampaign = -1;
            
            // Entry / Monster
            string customCallerMonsterName = "NO_CUSTOM_CALLER_MONSTER_NAME";
            
            // Audio
            string customCallerAudioPath = "";

            // First create a CustomCallerExtraInfo to assign audio later for it later automatically.
            CustomCallerExtraInfo _customCaller = CustomCallerParsing.ParseCustomCaller(ref jObjectParsed, 
                ref usermodFolderPath, ref jsonFolderPath, ref customCampaignName, ref inMainCampaign, 
                ref customCallerMonsterName, ref customCallerAudioPath,
                ref orderInCampaign, mainCampaignCallAmount, ref customCallerMainGame);

            if (customCallerMonsterName != "NO_CUSTOM_CALLER_MONSTER_NAME")
            {
                _customCaller.monsterNameAttached = customCallerMonsterName;
            }

            // Custom Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.ContainsKey("custom_caller_audio_clip_name"))
            {
                if (string.IsNullOrEmpty(customCallerAudioPath))
                {
                    MelonLogger.Warning(
                        $"WARNING: No caller audio given for file in {jsonFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(customCallerAudioPath)) 
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain '{customCallerAudioPath}'. Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    MelonCoroutines.Start(ParseJSONFiles.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    _customCaller.callerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                    _customCaller.isCallerClipLoaded = true;

                                    if (AudioImport.currentLoadingAudios.Count <= 0)
                                    {
                                        // We finished loading all audios. We call the start function again.
                                        AudioImport.reCallCallerListStart();
                                    }
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {customCallerAudioPath} for custom caller.");
                                }
                            },
                            customCallerAudioPath)
                    );
                }
            }

            // Now after parsing all values, we add the custom caller to our map

            if (inMainCampaign)
            {
                MelonLogger.Msg("INFO: Found entry to add to the main game.");
                customCallerMainGame.Add(orderInCampaign, _customCaller);
            }
            else
            {
                // Add to correct campaign.
                CustomCampaignExtraInfo foundCustomCampaign =
                    CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.campaignName == customCampaignName);

                if (foundCustomCampaign != null)
                {
                    if (_customCaller.isGameOverCaller)
                    {
                        foundCustomCampaign.customGameOverCallersInCampaign.Add(_customCaller);
                    }
                    else if (_customCaller.isWarningCaller)
                    {
                        foundCustomCampaign.customWarningCallersInCampaign.Add(_customCaller);
                    }
                    else
                    {
                        foundCustomCampaign.customCallersInCampaign.Add(_customCaller);
                    }
                }
                else
                {
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Found entry before the custom campaign was found / does not exist.");
                    #endif

                    missingCustomCallerCallersCustomCampaign.Add(_customCaller);
                }
            }

            #if DEBUG
                MelonLogger.Msg($"DEBUG: Finished adding this custom caller.");
            #endif
        }

        /// <summary>
        /// Function for adding a single entry.
        /// </summary>
        /// <param name="jObjectParsed"> JSON Data for reading. </param>
        /// <param name="newID"> If we wish to provide the ID via parameter. </param>
        /// <param name="usermodFolderPath"> Folder path to the entries directory </param>
        /// <param name="entryUnlockerInstance"> Instance of the EntryUnlockController. Needed for accessing and adding some entries. </param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateMonsterFromJSON(JObject jObjectParsed, int newID = -1, string usermodFolderPath = "",
            string jsonFolderPath = "",
            EntryUnlockController entryUnlockerInstance = null)
        {
            
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object || string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as an entry. Possible syntax mistake?");
                return;
            }
            
            // Values used for storing the information.
            int accessLevel = -1;
            bool accessLevelAdded = false;

            bool replaceEntry = false;

            bool onlyDLC = false;
            bool includeDLC = false;

            bool includeMainCampaign = false;

            // Entry / Monster Values
            string _monsterName = "NO_NAME";
            string _monsterDescription = "NO_DESCRIPTION";

            List<string> _arcadeCalls = new List<string>();

            Sprite _monsterPortrait = null;
            string _monsterPortraitLocation = "";

            string _monsterAudioClipLocation = "";

            // Caller Audio
            string _callerName = "NO_CALLER_NAME";
            string _callerTranscript = "NO_TRANSCRIPT";
            string _callerImageLocation = "";
            float _callerReplaceChance = 0.1f;
            bool _callerRestartCallAgain = true;
            Sprite _callerPortrait = null;

            // Consequence Caller Audio
            string _consequenceCallerAudioClipLocation;
            string _consequenceCallerName = "NO_CALLER_NAME";
            string _consequenceCallerTranscript = "NO_TRANSCRIPT";
            string _consequenceCallerImageLocation = "";
            Sprite _consequenceCallerPortrait = null;
            
            // Custom Campaigns
            bool _inCustomCampaign = false;
            string _customCampaignName = "NO_CUSTOM_CAMPAIGN_NAME";

            bool deleteReplaceEntry = false;

            // Phobias
            bool _spiderPhobia = false;
            bool _spiderPhobiaIncluded = false;
            bool _darknessPhobia = false;
            bool _darknessPhobiaIncluded = false;
            bool _dogPhobia = false;
            bool _dogPhobiaIncluded = false;
            bool _holesPhobia = false;
            bool _holesPhobiaIncluded = false;
            bool _insectPhobia = false;
            bool _insectPhobiaIncluded = false;
            bool _watchingPhobia = false;
            bool _watchingPhobiaIncluded = false;
            bool _tightSpacePhobia = false;
            bool _tightSpacePhobiaIncluded = false;

            // Persistent information for caller.
            EntryExtraInfo newExtra = null;

            // We extract the info and save it (if the file is valid)
            // Parse Entry
            MonsterParsing.parseEntry(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath, ref accessLevel,
                ref accessLevelAdded, ref replaceEntry, ref onlyDLC, ref includeDLC, ref includeMainCampaign,
                ref _monsterName, ref _monsterDescription, ref _arcadeCalls, ref _monsterPortrait,
                ref _monsterPortraitLocation, ref _monsterAudioClipLocation, ref deleteReplaceEntry, 
                ref _inCustomCampaign, ref _customCampaignName);

            // Parse Phobias
            MonsterParsing.parsePhobias(ref jObjectParsed, ref _spiderPhobia, ref _spiderPhobiaIncluded,
                ref _darknessPhobia, ref _darknessPhobiaIncluded, ref _dogPhobia, ref _dogPhobiaIncluded,
                ref _holesPhobia, ref _holesPhobiaIncluded, ref _insectPhobia, ref _insectPhobiaIncluded,
                ref _watchingPhobia, ref _watchingPhobiaIncluded, ref _tightSpacePhobia, ref _tightSpacePhobiaIncluded);

            // Parse Default Caller
            CallerParsing.parseCaller(ref jObjectParsed, ref usermodFolderPath, ref _callerName, ref _callerTranscript,
                ref _callerImageLocation, ref _callerReplaceChance, ref _callerRestartCallAgain, ref _callerPortrait);

            CallerParsing.parseConsequenceCaller(ref jObjectParsed, ref usermodFolderPath, ref _consequenceCallerName,
                ref _consequenceCallerTranscript, ref _consequenceCallerImageLocation, ref _consequenceCallerPortrait);

            // Create new extra info.
            createNewExtra(ref newExtra, ref _monsterName, ref newID, ref replaceEntry, ref _callerName,
                ref _callerTranscript, ref _callerPortrait, ref _callerReplaceChance, ref _callerRestartCallAgain,
                ref accessLevel, ref onlyDLC,
                ref includeDLC, ref includeMainCampaign, ref _consequenceCallerName, ref _consequenceCallerTranscript, 
                ref _consequenceCallerPortrait, ref deleteReplaceEntry, ref _inCustomCampaign, ref _customCampaignName);

            // Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.TryGetValue("caller_audio_clip_name", out var callerAudioClipNameValue))
            {
                string _callerAudioClipLocation = (string) callerAudioClipNameValue;
                string callerAudioClipLocationLambdaCopy = _callerAudioClipLocation; // Create copy for lambda function.

                if (string.IsNullOrEmpty(_callerAudioClipLocation) && !replaceEntry)
                {
                    MelonLogger.Msg($"INFO: No caller audio given for file in {jsonFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(jsonFolderPath + "\\" + _callerAudioClipLocation) &&
                         !File.Exists(usermodFolderPath + "\\" + _callerAudioClipLocation)) 
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain {_callerAudioClipLocation}. Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + _callerAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + _callerAudioClipLocation;
                    }
                    
                    MelonCoroutines.Start(UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    // ReSharper disable once AccessToModifiedClosure
                                    newExtra.callerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {callerAudioClipLocationLambdaCopy}.");
                                }
                            },
                            audioLocation)
                    );
                }
            }


            // Consequence Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.TryGetValue("consequence_caller_audio_clip_name", out var consequenceCallerAudioClipNameValue))
            {
                _consequenceCallerAudioClipLocation = (string) consequenceCallerAudioClipNameValue;

                if (string.IsNullOrEmpty(_consequenceCallerAudioClipLocation) && !replaceEntry)
                {
                    MelonLogger.Msg($"INFO: No caller audio given for file in {usermodFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(jsonFolderPath + "\\" + _consequenceCallerAudioClipLocation) &&
                         !File.Exists(usermodFolderPath + "\\" + _consequenceCallerAudioClipLocation)) 
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain {_consequenceCallerAudioClipLocation}." +
                        " Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + _consequenceCallerAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + _consequenceCallerAudioClipLocation;
                    }
                    
                    MelonCoroutines.Start(UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    // ReSharper disable once AccessToModifiedClosure
                                    newExtra.consequenceCallerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {_consequenceCallerAudioClipLocation}.");
                                }
                            },
                            audioLocation)
                    );
                }
            }

            // Add the extra information entry.
            if ((jObjectParsed.ContainsKey("caller_audio_clip_name") || includeMainCampaign || _inCustomCampaign ||
                replaceEntry) && newExtra != null)
            {
                entriesExtraInfo.Add(newExtra);
            }

            // Generate new ID if not provided.
            generateNewID(ref newExtra, ref newID, ref replaceEntry, ref jsonFolderPath,
                ref onlyDLC, ref includeDLC, ref entryUnlockerInstance, ref _inCustomCampaign);

            if (replaceEntry) // We replace an Entry
            {
                // Returns a copy of the foundMonster

                MonsterProfile foundMonster = null;
                MonsterProfile foundMonsterXMAS = null; // For replacing DLC version as well

                replaceEntryFunction(ref entryUnlockerInstance, ref onlyDLC, ref includeDLC, ref _monsterName, 
                    ref newID, ref _monsterPortraitLocation, ref _monsterPortrait,
                    ref _monsterDescription, ref replaceEntry, ref _arcadeCalls, ref accessLevel, ref accessLevelAdded,
                    ref includeMainCampaign, ref _spiderPhobiaIncluded, ref _spiderPhobia, ref _darknessPhobiaIncluded,
                    ref _darknessPhobia, ref _dogPhobiaIncluded, ref _dogPhobia, ref _holesPhobiaIncluded,
                    ref _holesPhobia, ref _insectPhobiaIncluded, ref _insectPhobia, ref _watchingPhobiaIncluded,
                    ref _watchingPhobia, ref _tightSpacePhobiaIncluded, ref _tightSpacePhobia, ref foundMonster,
                    ref foundMonsterXMAS, ref _inCustomCampaign, ref _customCampaignName);

                // We replace the audio if needed.
                if (!string.IsNullOrEmpty(_monsterAudioClipLocation))
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + _monsterAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + _monsterAudioClipLocation;
                    }
                    
                    MelonCoroutines.Start(UpdateAudioClip
                    (
                        (myReturnValue) =>
                        {
                            if (myReturnValue != null)
                            {
                                if (foundMonster != null) foundMonster.monsterAudioClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterAudioClip = AudioImport.CreateRichAudioClip(myReturnValue);
                            }
                            else
                            {
                                MelonLogger.Error($"ERROR: Failed to load audio clip {_monsterAudioClipLocation}.");
                            }

                        },
                        audioLocation)
                    );
                }

            }
            else // We add it instead of replacing the entry
            {
                MonsterProfile _newMonster = null;

                createNewEntryFunction(ref entryUnlockerInstance, ref onlyDLC, ref includeDLC,
                    ref _monsterName, ref newID, ref _monsterPortrait, ref _monsterDescription, ref _arcadeCalls,
                    ref accessLevel, ref includeMainCampaign, ref _spiderPhobia, ref _darknessPhobia, ref _dogPhobia,
                    ref _holesPhobia, ref _insectPhobia, ref _watchingPhobia, ref _tightSpacePhobia, ref _newMonster,
                    ref _inCustomCampaign, ref _customCampaignName);

                // Add audio to it
                if (!string.IsNullOrEmpty(_monsterAudioClipLocation))
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + _monsterAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + _monsterAudioClipLocation;
                    }
                    
                    MelonCoroutines.Start(UpdateAudioClip
                    (
                        (myReturnValue) =>
                        {
                            if (myReturnValue != null)
                            {
                                _newMonster.monsterAudioClip = AudioImport.CreateRichAudioClip(myReturnValue);
                            }
                            else
                            {
                                MelonLogger.Error($"ERROR: Failed to load audio clip {_monsterAudioClipLocation}.");
                            }

                        },
                        audioLocation)
                    );
                }
            }

            // We added or replaced the entry, now we finish up.

            // Sort the Entries in alphabetical order
            EntryManager.EntryManager.SortMonsterProfiles(ref entryUnlockerInstance.allEntries.monsterProfiles);
            EntryManager.EntryManager.SortMonsterProfiles(ref entryUnlockerInstance.allXmasEntries.monsterProfiles);
        }

        /// <summary>
        /// Helper coroutine for updating the audio correctly for a monster clip.
        /// </summary>
        /// <param name="callback"> Callback function for returning values and doing stuff with it that require the coroutine to finish first. </param>
        /// <param name="audioPath"> Path to the audio file. </param>
        /// <param name="_audioType"> Audio type to parse. </param>
        public static IEnumerator UpdateAudioClip(Action<AudioClip> callback, string audioPath,
            AudioType _audioType = AudioType.WAV)
        {
            AudioClip monsterSoundClip = null;

            // Attempt to get the type
            if (_audioType != AudioType.UNKNOWN)
            {
                _audioType = AudioImport.GetAudioType(audioPath);

                yield return MelonCoroutines.Start(
                AudioImport.LoadAudio
                (
                    (myReturnValue) =>
                    {
                        monsterSoundClip = myReturnValue;
                    },
                    audioPath, _audioType)
                );
            }

            callback(monsterSoundClip);
        }

        public static void createNewExtra(ref EntryExtraInfo newExtra, ref string _monsterName, ref int newID,
            ref bool replaceEntry, ref string _callerName, ref string _callerTranscript, ref Sprite _callerPortrait,
            ref float _callerReplaceChance, ref bool _callerRestartCallAgain, ref int accessLevel, ref bool onlyDLC,
            ref bool includeDLC, ref bool includeMainCampaign, ref string _consequenceCallerName,
            ref string _consequenceCallerTranscript, ref Sprite _consequenceCallerPortrait, 
            ref bool deleteReplaceEntry, ref bool _inCustomCampaign, ref string _customCampaignName)
        {
            newExtra = new EntryExtraInfo(_monsterName, newID)
            {
                replace = replaceEntry,
                callerName = _callerName,
                callTranscript = _callerTranscript
            }; // ID will not work if not provided, but this shouldn't be an issue.

            if (_callerPortrait != null)
            {
                newExtra.callerImage = _callerPortrait;
            }

            newExtra.callerReplaceChance = _callerReplaceChance;

            newExtra.allowCallAgainOverRestart = _callerRestartCallAgain;

            newExtra.permissionLevel = accessLevel; // Minimum Access level required for call

            // DLC Handling
            newExtra.onlyDLC = onlyDLC;
            newExtra.includeInDLC = includeDLC;

            newExtra.inMainCampaign = includeMainCampaign;

            // Consequence Caller Handling
            newExtra.consequenceName = _consequenceCallerName;
            newExtra.consequenceTranscript = _consequenceCallerTranscript;
            newExtra.consequenceCallerImage = _consequenceCallerPortrait;
            
            // Custom Campaign
            newExtra.onlyCustomCampaign = _inCustomCampaign;
            newExtra.customCampaignName = _customCampaignName;

            if (deleteReplaceEntry)
            {
                if (replaceEntry)
                {
                    newExtra.deleteEntry = deleteReplaceEntry;
                }
                else
                {
                    MelonLogger.Warning($"WARNING: Provided entry '{_monsterName}' cannot be deleted as it is not replacing an entry.");
                }
            }
        }

        public static void generateNewID(ref EntryExtraInfo newExtra, ref int newID, ref bool replaceEntry, 
            ref string jsonFolderPath, ref bool onlyDLC, ref bool includeDLC, 
            ref EntryUnlockController entryUnlockerInstance, ref bool inCustomCampaign)
        {
            // Update ID if not given.
            if (newID == -1 && !replaceEntry && !inCustomCampaign) 
            {
                // Get the max Monster ID.
                int maxEntryIDMainCampaign = EntryManager.EntryManager.GetNewEntryID(entryUnlockerInstance);
                int maxEntryIDMainDLC = EntryManager.EntryManager.GetNewEntryID(entryUnlockerInstance, 1);

                if (onlyDLC) // Only DLC
                {
                    newID = maxEntryIDMainDLC;
                }
                else if (includeDLC) // Also allow in DLC (We pick the largest from both)
                {
                    newID = (maxEntryIDMainCampaign < maxEntryIDMainDLC) ? maxEntryIDMainDLC : maxEntryIDMainCampaign;
                }
                else // Only base game.
                {
                    newID = maxEntryIDMainCampaign;
                }
            }

            // In custom campaign we first get our main game IDs and then add the offset by the size of the custom campaign sizes.
            if (newID == -1 && !replaceEntry && inCustomCampaign)
            {
                int tempID = EntryManager.EntryManager.GetNewEntryID(entryUnlockerInstance);
                        
                // We add our amountExtra and increment it for the next extra.
                tempID += amountExtra;
                amountExtra++;
                
                newID = tempID;
            }
            
            newExtra.ID = newID;
            
            MelonLogger.Msg($"INFO: Defaulting to a new Monster ID {newExtra.ID} for file in {jsonFolderPath}.");
            MelonLogger.Msg("(This intended and recommended way of providing the ID.)");
        }

        public static void replaceEntryFunction(ref EntryUnlockController entryUnlockerInstance, 
            ref bool onlyDLC, ref bool includeDLC, ref string _monsterName, ref int newID, 
            ref string _monsterPortraitLocation, ref Sprite _monsterPortrait, ref string _monsterDescription,
            ref bool replaceEntry, ref List<string> _arcadeCalls, ref int accessLevel, ref bool accessLevelAdded,
            ref bool includeMainCampaign, ref bool _spiderPhobiaIncluded, ref bool _spiderPhobia,
            ref bool _darknessPhobiaIncluded, ref bool _darknessPhobia, ref bool _dogPhobiaIncluded,
            ref bool _dogPhobia, ref bool _holesPhobiaIncluded, ref bool _holesPhobia, ref bool _insectPhobiaIncluded,
            ref bool _insectPhobia, ref bool _watchingPhobiaIncluded, ref bool _watchingPhobia,
            ref bool _tightSpacePhobiaIncluded, ref bool _tightSpacePhobia, ref MonsterProfile foundMonster,
            ref MonsterProfile foundMonsterXMAS, ref bool inCustomCampaign, ref string customCampaignName)
        {

            if (onlyDLC)
            {
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allXmasEntries.monsterProfiles, _monsterName, newID);
            }
            else if (includeDLC)
            {
                // Will search both and attempt to find the entry.
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles, _monsterName, newID);
                foundMonsterXMAS = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allXmasEntries.monsterProfiles, _monsterName, newID); // New Monster to also replace
            }
            else if (includeMainCampaign) // Main Campaign
            {
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles, _monsterName, newID);
            }
            else if (inCustomCampaign) // In custom campaign.
            {
                foundMonster = ScriptableObject.CreateInstance<MonsterProfile>(); // Create empty foundMonster to avoid replacing actual values.
                foundMonster.monsterID = newID;
                foundMonster.monsterName = _monsterName;
            }

            if ((foundMonster == null && !onlyDLC && !includeDLC) || (foundMonster == null && foundMonsterXMAS == null))
            {
                MelonLogger.Warning($"WARNING: Entry that was suppose to replace an entry failed. Information about the entry: Was found: {foundMonster != null} and was found in DLC: {foundMonsterXMAS != null}. Replacer Name: {_monsterName} with Replacer ID: {newID}.");
                return;
            }

            MelonLogger.Msg($"INFO: Found in the original list {_monsterName} / {newID}. Now replacing/updating (for the main campaign / custom campaign) the entry with given information for {_monsterName} / {newID}.");

            // Portrait
            if (!string.IsNullOrEmpty(_monsterPortraitLocation))
            {
                if (foundMonster != null) foundMonster.monsterPortrait = _monsterPortrait;
                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterPortrait = _monsterPortrait;
            }

            // Description
            if (_monsterDescription != "NO_DESCRIPTION")
            {
                if (foundMonster != null) foundMonster.monsterDescription = _monsterDescription;
                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterDescription = _monsterDescription;
            }

            // Name (Only works if ID was provided)
            if (_monsterName != "NO_NAME" && newID >= 0 && foundMonster != null && replaceEntry && _monsterName != foundMonster.monsterName)
            {
                if (foundMonster != null) foundMonster.monsterName = _monsterName;
                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterName = _monsterName;
                if (foundMonster != null) foundMonster.name = _monsterName;
                if (foundMonsterXMAS != null) foundMonsterXMAS.name = _monsterName;
            }

            // Arcade Calls
            if (_arcadeCalls.Count > 0)
            {
                if (foundMonster != null) foundMonster.arcadeCalls = _arcadeCalls.ToArray();
                if (foundMonsterXMAS != null) foundMonsterXMAS.arcadeCalls = _arcadeCalls.ToArray();
            }

            // Phobias, they don't require to be warned, since they optional.

            if (_spiderPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.spider = _spiderPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.spider = _spiderPhobia;
            }

            if (_darknessPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.dark = _darknessPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.dark = _darknessPhobia;
            }

            if (_dogPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.dog = _dogPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.dog = _dogPhobia;
            }

            if (_holesPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.holes = _holesPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.holes = _holesPhobia;
            }

            if (_insectPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.insect = _insectPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.insect = _insectPhobia;
            }

            if (_watchingPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.watching = _watchingPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.watching = _watchingPhobia;
            }

            if (_tightSpacePhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.tightSpace = _tightSpacePhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.tightSpace = _tightSpacePhobia;
            }

            // Access level, since we do not wish to accidentally include them, we do not add them to default

            // If no access level was provided, we default to 0.
            if (accessLevel == -1)
            {
                accessLevel = 0;
            }

            // This also counts the same for Christmas
            switch (accessLevel)
            {
                case 0: // First Level, is also default if not provided.
                    if (accessLevelAdded)
                    {
                        if (foundMonster != null)
                        {
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                            // ReSharper disable once StringLiteralTypo
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                        }

                        if (foundMonsterXMAS != null)
                        {
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                            // ReSharper disable once StringLiteralTypo
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                        }
                    }
                    break;

                case 1: // Second Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    }

                    break;

                case 2: // Third Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    }

                    break;

                case 3: // Fourth Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    }

                    break;

                case 4: // Fifth Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    break;

                case 5: // Sixth Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    break;
            }

            // Now if we also use "includeCampaign" we need to replace it there as well
            if (includeMainCampaign)
            {
                if (foundMonster != null)
                {
                    MonsterProfile foundMonsterCopy = foundMonster;

                    EntryManager.EntryManager.ReplaceEntry(ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, _monsterName, foundMonster);

                    // Include a copy of the monster in the extra info
                    entriesExtraInfo.Find(item => item.Name == foundMonsterCopy.monsterName || item.ID == foundMonsterCopy.monsterID).referenceCopyEntry = foundMonster;
                }

                if (foundMonsterXMAS != null)
                {

                    MonsterProfile foundMonsterXMASCopy = foundMonsterXMAS;

                    EntryManager.EntryManager.ReplaceEntry(ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, _monsterName, foundMonsterXMAS);

                    if (foundMonster == null)
                    {
                        // Include a copy of the monster in the extra info
                        entriesExtraInfo.Find(item => item.Name == foundMonsterXMASCopy.monsterName || item.ID == foundMonsterXMASCopy.monsterID).referenceCopyEntry = foundMonster;
                    }
                }
            }
            else if (inCustomCampaign) // Add entry to replace in custom campaign.
            {
                // Include a copy of the monster in the extra info
                if (foundMonster != null)
                {
                    string monsterNameCopy = foundMonster.monsterName;
                
                    EntryExtraInfo extraEntryInfo = entriesExtraInfo.Find(item => item.Name == monsterNameCopy);
                    if (extraEntryInfo != null) // Only if it exists.
                    {
                        extraEntryInfo.referenceCopyEntry = foundMonster;
                        
                        // Add the extraEntryInfo in custom campaign things.

                        string customCampaignNameCopy = customCampaignName;
                        
                        CustomCampaignExtraInfo currentCustomCampaign = CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch => customCampaignSearch.campaignName == customCampaignNameCopy);
                        
                        if (currentCustomCampaign == null)
                        {
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Custom Campaign replace entry found before custom campaign has been parsed. Adding to late add.");
                            #endif
                            
                            missingReplaceEntriesCustomCampaign.Add(extraEntryInfo);
                            
                            return;
                        }
                        
                        currentCustomCampaign.entryReplaceOnlyInCampaign.Add(extraEntryInfo);
                    }
                }
            }
        }
    
        public static void createNewEntryFunction(ref EntryUnlockController entryUnlockerInstance, ref bool onlyDLC,
            ref bool includeDLC, ref string _monsterName, ref int newID, ref Sprite _monsterPortrait,
            ref string _monsterDescription, ref List<string> _arcadeCalls, ref int accessLevel, 
            ref bool includeMainCampaign, ref bool _spiderPhobia, ref bool _darknessPhobia, ref bool _dogPhobia,
            ref bool _holesPhobia, ref bool _insectPhobia, ref bool _watchingPhobia, ref bool _tightSpacePhobia,
            ref MonsterProfile _newMonster, ref bool inCustomCampaign, ref string customCampaignName)
        {
            
            // Create Monster and add him
            // NOTE: AudioClip is added later, since we need to do load it separately from the main thread.
            _newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: _monsterName, _monsterDescription: _monsterDescription, _monsterID: newID,
                _arcadeCalls: _arcadeCalls.ToArray(), _monsterPortrait: _monsterPortrait, _monsterAudioClip: null,
                _spiderPhobia: _spiderPhobia, _darknessPhobia: _darknessPhobia, _dogPhobia: _dogPhobia, _holesPhobia: _holesPhobia, _insectPhobia: _insectPhobia, _watchingPhobia: _watchingPhobia,
                _tightSpacePhobia: _tightSpacePhobia);
            
            // Create copy for lambda functions.
            MonsterProfile _newMonsterCopy = _newMonster;
            
            // Include a copy of the monster in the extra info
            EntryExtraInfo extraEntryInfo = entriesExtraInfo.Find(item => item.Name == _newMonsterCopy.monsterName || item.ID == _newMonsterCopy.monsterID);
            if (extraEntryInfo != null) // Only if it exists.
            {
                extraEntryInfo.referenceCopyEntry = _newMonster;
            }

            // Decide where to add the Entry to. (Main Game or DLC or even both)

            if (onlyDLC) // Only DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.allXmasEntries.monsterProfiles, "allXmasEntries");
            }
            else if (includeDLC) // Also allow in DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.allEntries.monsterProfiles, "allEntries");
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.allXmasEntries.monsterProfiles, "allXmasEntries");
            }
            else if (includeMainCampaign) // Only base game.
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.allEntries.monsterProfiles, "allEntries");
            }
            else if (inCustomCampaign) // Custom Campaign
            {
                // Please note that the entry never gets added to the main monster profile array.
                // But we do add them to the permission tiers. As they never get rendered, it is fine to store it this way.
                string customCampaignNameCopy = customCampaignName;
                
                // Add to correct campaign.
                CustomCampaignExtraInfo foundCustomCampaign = CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch => customCampaignSearch.campaignName == customCampaignNameCopy);

                if (foundCustomCampaign != null)
                {
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Adding found custom campaign entry to the custom campaign.");
                    #endif

                    if (extraEntryInfo != null)
                    {
                        foundCustomCampaign.entriesOnlyInCampaign.Add(extraEntryInfo);
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Entry that was suppose to be added in custom campaign does not exist as extra info. (Error Type: 1) ");
                    }
                    
                }
                else
                {
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Found monster entry before the custom campaign was found / does not exist.");
                    #endif
                            
                    if (extraEntryInfo != null)
                    {
                        missingEntriesCustomCampaign.Add(extraEntryInfo);
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Entry that was suppose to be added in custom campaign does not exist as extra info. (Error Type: 2) ");
                    }
                }
            }

            /*
             * Removed the section that adds the entry also to entryUnlockerInstance.allMainCampaignEntries . However, this added the entry twice for unknown reasons. So we do not do it.
             */

            // If no access level provided. We use the lowest.
            if (accessLevel == -1)
            {
                accessLevel = 0;
            }

            // This also counts the same for Christmas
            switch (accessLevel)
            {

                case 0: // First Level, is also default if not provided.
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                    // ReSharper disable once StringLiteralTypo
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                    break;

                case 1: // Second Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    break;

                case 2: // Third Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    break;

                case 3: // Fourth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    break;

                case 4: // Fifth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    break;

                case 5: // Sixth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    break;

                default: // In case we somehow have an unknown value, we also default to first level.
                    MelonLogger.Warning("WARNING: Provided access level is invalid (0-5). Defaulting to 0th access level.");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                    
                    // ReSharper disable once StringLiteralTypo
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                    break;
            }
            
            #if DEBUG
                MelonLogger.Msg($"DEBUG: Finished parsing entry: {_newMonster.monsterName}.");
            #endif
        }
    }
}
