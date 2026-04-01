using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

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

                LoggingHelper.DebugLog(
                    $"Entries in Main Campaign: {maxEntryIDMainCampaign} and entries in DLC: {maxEntryIDMainDlc}.");

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

            LoggingHelper.InfoLog($"Defaulting to a new Monster ID {newExtra.ID} for file in {jsonFolderPath}.");
            LoggingHelper.InfoLog("(This is the intended and recommended way of providing the ID.)");
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
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// This version takes in a bool that updates to "true" if updated.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssignWithChangedBool<T>(JObject jObjectParsed, string key, ref VariableChanged<T> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                target.HasChanged = false;
                return;
            }

            target.HasChanged = true;
            target.Data = token.Value<T>();
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

                    case "":
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
                        LoggingHelper.WarningLog("Provided accuracy check type" +
                                                 $" '{accuracyCheckTypeString}' is not in any known format." +
                                                 " Please double check.");
                        return AccuracyHelper.CheckOptions.NoneSet;
                }
            }

            LoggingHelper.WarningLog("Unable of parsing accuracy check type. Possible syntax problem?");
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
        public static bool TryAssignListAccuracyType(JObject jObjectParsed, ref List<CallerAccuracyType> target)
        {
            if (!jObjectParsed.TryGetValue(AccuracyCheckTypeString, out _))
            {
                return false;
            }

            if (target == null)
            {
                target = new List<CallerAccuracyType>();
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
                LoggingHelper.ErrorLog("Provided accuracy lists are empty or could not be parsed. " +
                                       "Unable of parsing accuracy checks.");
                return false;
            }

            if (accuracyRequiredList.Count != accuracyCheckType.Count)
            {
                LoggingHelper.ErrorLog("Provided accuracy lists must all have equal length. " +
                                       "Unable of parsing accuracy checks.");
                return false;
            }

            if (isTotalAccuracyList.Count > accuracyCheckType.Count)
            {
                LoggingHelper.ErrorLog("Provided list of total accuracy is larger than available accuracy checks. " +
                                       "Unable of parsing accuracy checks.");
                return false;
            }

            for (int i = 0; i < accuracyCheckType.Count; i++)
            {
                CallerAccuracyType newCallerAccuracyType = new CallerAccuracyType();

                if (!string.IsNullOrEmpty(accuracyCheckType[i]))
                {
                    newCallerAccuracyType.AccuracyCheck = TryAssignSingleAccuracyType(accuracyCheckType[i]);
                }
                else
                {
                    LoggingHelper.WarningLog("Provided accuracy type is invalid. Defaulting to 'none'.");
                }

                if (providedSingleValueTA != null)
                {
                    if ((bool)providedSingleValueTA && isTotalAccuracyList.Count > 0)
                    {
                        newCallerAccuracyType.UseTotalAccuracy = isTotalAccuracyList[0];
                    }
                    else if (i < isTotalAccuracyList.Count)
                    {
                        newCallerAccuracyType.UseTotalAccuracy = isTotalAccuracyList[i];
                    }
                }

                newCallerAccuracyType.RequiredAccuracy = accuracyRequiredList[i];

                target.Add(newCallerAccuracyType);
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the check option for a given general accuracy type list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="isUsingOldSystem">(Ref Bool) if we found at least one element for the new system.</param>
        /// <param name="requiredAccuracyKey">Key for the required accuracy list or element.</param>
        /// <param name="accuracyDaysKey">Key for the different days that the accuracy checks for.</param>
        /// <param name="accuracyCheckTypesKey">Key for the different check types.</param>
        public static void TryAssignListGeneralAccuracyType(JObject jObjectParsed, ref List<GeneralAccuracyType> target,
            ref bool isUsingOldSystem,
            string requiredAccuracyKey = "email_required_accuracy", string accuracyDaysKey = "email_accuracy_days", 
            string accuracyCheckTypesKey = "email_accuracy_check_type")
        {
            isUsingOldSystem = true;

            if (!jObjectParsed.TryGetValue(requiredAccuracyKey, out _))
            {
                return;
            }

            if (target == null)
            {
                target = new List<GeneralAccuracyType>();
            }

            List<int> differentAccuracyDays = new List<int>();
            bool? hasOnlyOneAccuracyDay = TryAssignListOrSingleElement(jObjectParsed, accuracyDaysKey,
                ref differentAccuracyDays);

            List<float> accuracyRequiredList = new List<float>();
            TryAssignListOrSingleElement(jObjectParsed, requiredAccuracyKey, ref accuracyRequiredList);

            List<string> accuracyCheckType = new List<string>();
            TryAssignListOrSingleElement(jObjectParsed, accuracyCheckTypesKey, ref accuracyCheckType);

            if (accuracyRequiredList.Count != accuracyCheckType.Count)
            {
                if (accuracyRequiredList.Count < accuracyCheckType.Count)
                {
                    LoggingHelper.ErrorLog("Provided accuracy lists must all should have equal length. " +
                                           "Unable of parsing accuracy checks.");
                    return;
                }

                if (accuracyCheckType.Count >= 1)
                {
                    LoggingHelper.WarningLog("Provided accuracy lists must all should have equal length. " +
                                             "Adding any missing accuracy checks with 'geq' (greater or equal).");
                }

                while (accuracyCheckType.Count < accuracyRequiredList.Count)
                {
                    accuracyCheckType.Add("geq");
                }
            }

            if (differentAccuracyDays.Count > accuracyCheckType.Count)
            {
                LoggingHelper.ErrorLog("Provided accuracy days list has too many elements. " +
                                       "Unable of parsing accuracy checks.");
                return;
            }

            if (accuracyCheckType.Count > 0
                || accuracyCheckType.Count > 0
                || differentAccuracyDays.Count > 0)
            {
                isUsingOldSystem = false;
            }

            for (int i = 0; i < accuracyCheckType.Count; i++)
            {
                GeneralAccuracyType newAccuracyType = new GeneralAccuracyType();

                if (!string.IsNullOrEmpty(accuracyCheckType[i]))
                {
                    newAccuracyType.AccuracyCheck = TryAssignSingleAccuracyType(accuracyCheckType[i]);
                }
                else
                {
                    LoggingHelper.WarningLog("Provided general accuracy type is invalid. " +
                                             "Defaulting to 'greater or equal'.");
                    newAccuracyType.AccuracyCheck = AccuracyHelper.CheckOptions.GreaterThanOrEqualTo;
                }

                if (hasOnlyOneAccuracyDay != null)
                {
                    if ((bool)hasOnlyOneAccuracyDay && differentAccuracyDays.Count > 0)
                    {
                        newAccuracyType.CheckDay = differentAccuracyDays[0];
                    }
                    else if (i < differentAccuracyDays.Count)
                    {
                        newAccuracyType.CheckDay = differentAccuracyDays[i];
                    }
                }

                newAccuracyType.RequiredAccuracy = accuracyRequiredList[i];

                target.Add(newAccuracyType);
            }
        }

        /// <summary>
        /// Attempts to parse the check option.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="callerRequirementIDKey">Key to read the caller ID from.</param>
        /// <param name="callerCorrectnessKey">Key that describes if the caller is correct or wrong.</param>
        public static void TryAssignCallerRequirement(JObject jObjectParsed, ref List<CallerRequirement> target,
            string callerRequirementIDKey = "email_caller_requirement_ids",
            string callerCorrectnessKey = "email_caller_requirement_should_be_correct")
        {
            if (!jObjectParsed.TryGetValue(callerRequirementIDKey, out _))
            {
                return;
            }

            if (target == null)
            {
                target = new List<CallerRequirement>();
            }

            List<int> callerRequirementIDs = new List<int>();
            TryAssignListOrSingleElement(jObjectParsed, callerRequirementIDKey, ref callerRequirementIDs);

            List<bool> callerCorrectness = new List<bool>();
            bool? singleCorrectness =
                TryAssignListOrSingleElement(jObjectParsed, callerCorrectnessKey, ref callerCorrectness);

            if (callerCorrectness.Count > callerRequirementIDs.Count)
            {
                LoggingHelper.WarningLog("Too many caller correctness given, " +
                                         "the given caller requirement will not use all elements." +
                                         "If this is intentional, then no action is required.");
            }

            for (int i = 0; i < callerRequirementIDs.Count; i++)
            {
                CallerRequirement newCallerRequirement = new CallerRequirement
                {
                    CallerID = callerRequirementIDs[i]
                };

                if (singleCorrectness != null)
                {
                    if ((bool)singleCorrectness
                        && callerCorrectness.Count > 0)
                    {
                        newCallerRequirement.ShouldCallerBeCorrect = callerCorrectness[0];
                    }
                    else if (i < callerCorrectness.Count)
                    {
                        newCallerRequirement.ShouldCallerBeCorrect = callerCorrectness[i];
                    }
                }

                target.Add(newCallerRequirement);
            }
        }

        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single element (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignListOrSingleElement<T>(JObject jObjectParsed, string key, ref List<T> target)
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
                    LoggingHelper.ErrorLog($"For provided key '{key}' " +
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
        /// <param name="jsonFolderPath"> Folder path where the JSON is located. </param>
        /// <param name="usermodFolderPath"> Folder path where the usermod is located. </param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single element (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignUrlListOrSingleUrl(JObject jObjectParsed, string key, ref List<string> target,
            string jsonFolderPath, string usermodFolderPath)
        {
            bool? result = TryAssignListOrSingleElement(jObjectParsed, key, ref target);

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
                LoggingHelper.ErrorLog($"Provided key '{key}' does not contain a list.");
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

        /// <summary>
        /// Attempts to assign the video file path to the target string. But only if the video file exists.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        public static bool TryAssignURL(JObject jObjectParsed, string key, ref Uri target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return false;
            }

            string givenStringURL = token.Value<string>();

            if (!URLVerification.SetEmailClickURL(givenStringURL, ref target))
            {
                LoggingHelper.WarningLog($"Provided URL with the key '{key}' is invalid. Unable of setting url.");
                return false;
            }

            LoggingHelper.DebugLog($"Found email URL: '{target.AbsoluteUri.Substring(0, 10)}[...]'." +
                                   " It has been marked as valid.");

            return true;
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
                        LoggingHelper.DebugLog(
                            $"Adding missing {elementName} to the custom campaign: {customCampaignName}.");

                        listToBeAddedTo.Add(missingElement);

                        pendingList.Remove(missingElement);
                    }
                }
            }
        }
    }
}