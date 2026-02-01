using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CustomCampaign.Abstract;
using NewSafetyHelp.CustomCampaign.Helper;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing
{
    public static class ParsingHelper
    {
        /// <summary>
        /// Returns a new ID that should be +1 from the largest.
        /// </summary>
        /// <param name="entryUnlocker"> Instance of the EntryUnlockerController </param>
        /// <param name="type"> Type of entry type. (0 = monsterProfiles default, 1 = allXmasEntries DLC) </param>
        private static int GetNewEntryID(EntryUnlockController entryUnlocker, int type = 0)
        {
            switch (type)
            {
                case 0:
                    return entryUnlocker.allEntries.monsterProfiles.Length;

                case 1:
                    return entryUnlocker.allXmasEntries.monsterProfiles.Length;

                default: 
                    return entryUnlocker.allEntries.monsterProfiles.Length;
            }
        }
        
        /// <summary>
        /// Generates a new ID based on the given information.
        /// </summary>
        /// <param name="newExtra">Entry which will have its ID updated.</param>
        /// <param name="newID">If an ID was provided, use this.</param>
        /// <param name="replaceEntry">If this is supposed to replace an entry.</param>
        /// <param name="jsonFolderPath">Folder path to the JSON file.</param>
        /// <param name="onlyDlc">If only to consider the DLC.</param>
        /// <param name="includeDlc">If to also consider the DLC included.</param>
        /// <param name="entryUnlockerInstance">Instance of the entry unlocker. (Used to get new ID)</param>
        /// <param name="inCustomCampaign">If this is in a custom campaign.</param>
        public static void GenerateNewID(ref EntryMetadata newExtra, ref int newID, ref bool replaceEntry,
            ref string jsonFolderPath, ref bool onlyDlc, ref bool includeDlc,
            ref EntryUnlockController entryUnlockerInstance, ref bool inCustomCampaign)
        {
            // Update ID if not given.
            if (newID == -1 && !replaceEntry && !inCustomCampaign)
            {
                // Get the max Monster ID.
                int maxEntryIDMainCampaign = GetNewEntryID(entryUnlockerInstance);
                int maxEntryIDMainDlc = GetNewEntryID(entryUnlockerInstance, 1);

                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Entries in Main Campaign: {maxEntryIDMainCampaign} and entries in DLC: {maxEntryIDMainDlc}.");            
                #endif
                
                if (onlyDlc) // Only DLC
                {
                    newID = maxEntryIDMainDlc;
                }
                else if (includeDlc) // Also allow in DLC (We pick the largest from both)
                {
                    newID = (maxEntryIDMainCampaign < maxEntryIDMainDlc) ? maxEntryIDMainDlc : maxEntryIDMainCampaign;
                }
                else // Only base game.
                {
                    newID = maxEntryIDMainCampaign;
                }
            }

            // In custom campaign we first get our main game IDs and then add the offset by the size of the custom campaign sizes.
            if (newID == -1 && !replaceEntry && inCustomCampaign)
            {
                int tempID = GetNewEntryID(entryUnlockerInstance);

                // We add our CustomCampaignEntryIDOffset and increment it for the next extra.
                tempID += GlobalParsingVariables.CustomCampaignEntryIDOffset;
                GlobalParsingVariables.CustomCampaignEntryIDOffset++;

                newID = tempID;
            }

            newExtra.ID = newID;

            MelonLogger.Msg($"INFO: Defaulting to a new Monster ID {newExtra.ID} for file in {jsonFolderPath}.");
            MelonLogger.Msg("(This is the intended and recommended way of providing the ID.)");
        }
        
        /// <summary>
        /// Helper coroutine for updating the audio correctly for a monster clip.
        /// </summary>
        /// <param name="callback"> Callback function for returning values and doing stuff with it that require the coroutine to finish first. </param>
        /// <param name="audioPath"> Path to the audio file. </param>
        /// <param name="audioType"> Audio type to parse. </param>
        public static IEnumerator UpdateAudioClip(Action<AudioClip> callback, string audioPath,
            AudioType audioType = AudioType.WAV)
        {
            AudioClip monsterSoundClip = null;

            // Attempt to get the type
            if (audioType != AudioType.UNKNOWN)
            {
                audioType = AudioImport.GetAudioType(audioPath);

                yield return MelonCoroutines.Start(
                    AudioImport.LoadAudio
                    (
                        myReturnValue => { monsterSoundClip = myReturnValue; },
                        audioPath, audioType)
                );
            }

            callback(monsterSoundClip);
        }
        
        /// <summary>
        /// Checks if the JSON object contains any of the keys.
        /// </summary>
        /// <param name="keys">List of keys to check </param>
        /// <param name="json">JObject with the keys</param>
        /// <returns></returns>
        public static bool ContainsKeys(List<string> keys, JObject json)
        {
            return keys.Any(json.ContainsKey); // Checks if any of the keys is in the JSON via the flag ContainsKey
        }
        
        /// <summary>
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssign<T>(JObject jObjectParsed, string key, ref T target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            target = token.Value<T>();
        }
        
        /// <summary>
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// This version takes in a bool that updates to "true" if updated.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="wasAssigned"> If the value was assigned. This is used in some parsing to allow both
        /// true and false and default values.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssignWithBool<T>(JObject jObjectParsed, string key, ref T target, ref bool wasAssigned)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                wasAssigned = false;
                return;
            }

            wasAssigned = true;
            target = token.Value<T>();
        }
        
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
        public static void TryAssignSprite(JObject jObjectParsed, string key, ref Sprite target, string jsonFolderPath,
            string usermodFolderPath, string customCampaignName = null)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            string imagePath = token.Value<string>();

            if (string.IsNullOrEmpty(imagePath))
            {
                MelonLogger.Error(
                    $"ERROR: Invalid file name given for '{imagePath}' for key {key}." +
                    $" Not updating {(!string.IsNullOrEmpty(customCampaignName) ? $"for {customCampaignName}." : ".")}");
            }
            else
            {
                target = ImageImport.LoadImage(jsonFolderPath + "\\" + imagePath,
                    usermodFolderPath + "\\" + imagePath);
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
        /// <param name="customCallerName">(Optional) Name of the custom caller. Used to display errors.</param>
        public static void TryAssignAudioPath(JObject jObjectParsed, string key, ref string target,
            string jsonFolderPath, string usermodFolderPath, string customCallerName = null)
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
                    MelonLogger.Warning(
                        $"WARNING: Could not find provided audio file for key '{key}' at " +
                        $"'{jsonFolderPath}'" +
                        $" {(customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME" ? $"for {customCallerName}" : "")}.");
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
        
        /// <summary>
        /// Attempts to parse the check option provided.
        /// </summary>
        /// <param name="accuracyCheckTypeString">String describing the accuracy tape.</param>
        private static AccuracyHelper.CheckOptions TryAssignSingleAccuracyType(string accuracyCheckTypeString)
        {
            if (!string.IsNullOrEmpty(accuracyCheckTypeString))
            {
                switch (accuracyCheckTypeString.ToLowerInvariant())
                {
                    case "equal":
                    case "eq": // Equal
                        return AccuracyHelper.CheckOptions.EqualTo;
                        
                    case "no":
                    case "n":
                    case "none": // None
                        return AccuracyHelper.CheckOptions.NoneSet;
                        
                    case "greaterorequal":
                    case "geq": // Greater than or equal to
                        return AccuracyHelper.CheckOptions.GreaterThanOrEqualTo;
                        
                    case "lesserorequal":
                    case "lessorequal":
                    case "leq": // Less than or equal to
                        return AccuracyHelper.CheckOptions.LessThanOrEqualTo;
                    
                    case "nequal":
                    case "notequal":
                    case "!equal":
                    case "!eq":
                    case "noteq":
                    case "neq": // Not equal to
                        return AccuracyHelper.CheckOptions.NotEqualTo;
                        
                    default:
                        MelonLogger.Warning("WARNING: Provided accuracy check type" +
                                            $" '{accuracyCheckTypeString}' is not in any known format." +
                                            " Please double check.");
                        return AccuracyHelper.CheckOptions.NoneSet;
                }
            }

            MelonLogger.Warning("WARNING: Unable of parsing accuracy check type. Possible syntax problem?");
            return AccuracyHelper.CheckOptions.NoneSet;
        }

        /*
         * Const strings for assign list. This ensures more consistency.
         */
        
        private const string AccuracyCheckTypeString = "accuracy_check_type";
        private const string AccuracyRequiredString = "accuracy_required";
        private const string TotalAccuracyString = "use_total_accuracy";
        
        /// <summary>
        /// Attempts to parse the check option provided.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="target">Targets to write the value to.</param>
        public static void TryAssignListAccuracyType(JObject jObjectParsed, ref List<AccuracyType> target)
        {
            if (!jObjectParsed.TryGetValue(AccuracyCheckTypeString, out _))
            {
                return;
            }
            
            if (target == null)
            {
                target = new List<AccuracyType>();
            }
            
            List<bool> isTotalAccuracyList = new List<bool>();
            bool? providedSingleValueTA = TryAssignListOrSingleElement(jObjectParsed, TotalAccuracyString, 
                ref isTotalAccuracyList);
            
            List<float> accuracyRequiredList = new List<float>();
            TryAssignListOrSingleElement(jObjectParsed, AccuracyRequiredString, ref accuracyRequiredList);
            
            List<string> accuracyCheckType = new List<string>();
            TryAssignListOrSingleElement(jObjectParsed, AccuracyCheckTypeString, ref accuracyCheckType);

            // It means we have no elements, or we simply failed parsing any. 
            // The error printed by the helper function will inform the user what was the cause. 
            // So here we simply need to return.
            if (accuracyCheckType.Count < 1)
            {
                MelonLogger.Error("ERROR: Provided accuracy lists are empty or could not be parsed. " +
                                  "Unable of parsing accuracy checks.");
                return;
            }

            if (accuracyRequiredList.Count != accuracyCheckType.Count)
            {
                MelonLogger.Error("ERROR: Provided accuracy lists must all have equal length. " +
                                  "Unable of parsing accuracy checks.");
                return;
            }

            if (isTotalAccuracyList.Count > accuracyCheckType.Count)
            {
                MelonLogger.Error("ERROR: Provided list of total accuracy is larger than available accuracy checks. " +
                                  "Unable of parsing accuracy checks.");
                return;
            }
            
            for (int i = 0; i < accuracyCheckType.Count; i++)
            {
                AccuracyType newAccuracyType = new AccuracyType();

                if (!string.IsNullOrEmpty(accuracyCheckType[i]))
                {
                    newAccuracyType.AccuracyCheck = TryAssignSingleAccuracyType(accuracyCheckType[i]);
                }
                else
                {
                    MelonLogger.Warning("WARNING: Provided accuracy type is invalid. Defaulting to 'none'.");
                }

                if (providedSingleValueTA != null)
                {
                    if ((bool) providedSingleValueTA)
                    {
                        newAccuracyType.UseTotalAccuracy = isTotalAccuracyList[0];
                    }
                    else if (i < isTotalAccuracyList.Count)
                    {
                        newAccuracyType.UseTotalAccuracy = isTotalAccuracyList[i];
                    }
                }
                
                newAccuracyType.RequiredAccuracy = accuracyRequiredList[i];

                target.Add(newAccuracyType);
            }
        }
        
        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <returns>(Bool) If the parsed value was an array or a single element. Null if we failed parsing.</returns>
        private static bool? TryAssignListOrSingleElement<T>(JObject jObjectParsed, string key, ref List<T> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return null;
            }

            if (target == null)
            {
                target = new List<T>();
            }
            
            if (token.Type == JTokenType.Array)
            {
                foreach (JToken element in token)
                {
                    T value = element.Value<T>();
                    target.Add(value);
                }

                return false;
            }
            else
            {
                try
                {
                    T value = token.Value<T>();
                    target.Add(value);
                
                    return true;
                }
                catch
                {
                    MelonLogger.Error($"ERROR: For provided key '{key}' " +
                                      "we were unable of assigning any value, as the wrong value was given.");
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        public static void TryAssignList<T>(JObject jObjectParsed, string key, ref List<T> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            if (target == null)
            {
                target = new List<T>();
            }
            
            if (token.Type == JTokenType.Array)
            {
                foreach (JToken element in token)
                {
                    T value = element.Value<T>();
                    target.Add(value);
                }
            }
            else
            {
                MelonLogger.Error($"ERROR: Provided key '{key}' does not contain a list.");
            }
            
        }
        
        /// <summary>
        /// Attempts to assign the video file path to the target string. But only if the video file exists.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="jsonFolderPath">Path to where the JSON is located.</param>
        /// <param name="usermodFolderPath">Path to the parent usermod folder.</param>
        public static void TryAssignVideoPath(JObject jObjectParsed, string key, ref string target,
            string jsonFolderPath, string usermodFolderPath)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }
            
            string videoFilePath = token.Value<string>();
            
            videoFilePath = jsonFolderPath + "\\" + videoFilePath;
            string videoFileAlternativePath = usermodFolderPath + "\\" + videoFilePath;

            if (string.IsNullOrEmpty(videoFilePath))
            {
                MelonLogger.Warning("WARNING: Provided video path but name is empty. Unable to show show video.");
                target = "";
            }
            else
            {
                if (File.Exists(videoFilePath))
                {
                    target = videoFilePath;
                }
                else if (File.Exists(videoFileAlternativePath))
                {
                    target = videoFileAlternativePath;
                }
                else
                {
                    MelonLogger.Warning($"WARNING: Provided video {videoFilePath} does not exist.");
                    target = "";
                }
            }
        }
        
        /// <summary>
        /// Adds any pending elements (elements that were parsed before the campaign was parsed)
        /// to the provided campaign list.
        /// </summary>
        /// <param name="pendingList">List of pending to be added.</param>
        /// <param name="listToBeAddedTo">List where to add the pending elements.</param>
        /// <param name="customCampaignName">Custom Campaign to be which the elements get added to.</param>
        /// <param name="elementName">For debug printing. It shows what type of element was added.</param>
        /// <typeparam name="T">Type of the target in the lists.</typeparam>
        public static void AddPendingElementsToCampaign<T>(ref List<T> pendingList, ref List<T> listToBeAddedTo,
            string customCampaignName, string elementName = "NO_NAME_GIVEN") where T : CustomCampaignElementBase
        {
            if (pendingList.Count > 0)
            {
                // Create a copy of the list to iterate over.
                List<T> tempList = new List<T>(pendingList);

                foreach (T missingElement in tempList)
                {
                    if (missingElement.CustomCampaignName == customCampaignName)
                    {
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding missing {elementName} to the custom campaign: {customCampaignName}.");
                        #endif

                        listToBeAddedTo.Add(missingElement);

                        pendingList.Remove(missingElement);
                    }
                }
            }
        }
    }
}