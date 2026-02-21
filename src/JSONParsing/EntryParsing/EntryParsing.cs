using System.Collections.Generic;
using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.EntryParsing
{
    public static class EntryParsing
    {
        private static void ParseEntry(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref int accessLevel, ref bool accessLevelAdded, ref bool replaceEntry,
            ref bool onlyDLC, ref bool includeDLC, ref bool includeMainCampaign, ref string monsterName, 
            ref string monsterDescription, ref List<string> arcadeCalls, ref Sprite monsterPortrait,
            ref string monsterPortraitLocation, ref string monsterAudioClipLocation, ref bool deleteReplaceEntry,
            ref bool inCustomCampaign, ref string customCampaignName)
        {
            /* 
             * Monster Information
            */

            ParsingHelper.TryAssign(jsonObjectParsed, "replace_entry", ref replaceEntry);

            // Monster Name
            if (jsonObjectParsed.TryGetValue("monster_name", out var monsterNameValue))
            {
                monsterName = (string) monsterNameValue;
            }
            else
            {
                if (!replaceEntry)
                {
                    LoggingHelper.WarningLog($"No Monster name given for file in {usermodFolderPath}. Defaulting to NO_NAME.");
                }
            }

            // Monster Description
            if (jsonObjectParsed.TryGetValue("monster_description", out var monsterDescriptionValue))
            {
                monsterDescription = monsterDescriptionValue.Value<string>();
            }
            else
            {
                if (!replaceEntry)
                {
                    LoggingHelper.WarningLog($"No Monster description given for file in {usermodFolderPath}. Defaulting to NO_DESCRIPTION.");
                }
            }


            // DLC
            ParsingHelper.TryAssign(jsonObjectParsed, "only_dlc", ref onlyDLC);
            ParsingHelper.TryAssign(jsonObjectParsed, "include_dlc", ref includeDLC);
            
            // Main Game
            ParsingHelper.TryAssign(jsonObjectParsed, "include_campaign", ref includeMainCampaign);
            
            // Access Level and Arcade Calls
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "access_level", ref accessLevel,
                ref accessLevelAdded);

            if (jsonObjectParsed.TryGetValue("arcade_calls", out var arcadeCallsValue))
            {
                JArray test = (JArray) arcadeCallsValue;

                foreach (JToken arcadeCustomCall in test)
                {
                    arcadeCalls.Add(arcadeCustomCall.Value<string>());
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    LoggingHelper.InfoLog($"No Arcade Calls given for file in {usermodFolderPath}. Defaulting to empty values.");
                }
            }
            
            // Image
            if (jsonObjectParsed.TryGetValue("monster_portrait_image_name", out var monsterPortraitImageNameValue))
            {
                monsterPortraitLocation = monsterPortraitImageNameValue.Value<string>();

                if (string.IsNullOrEmpty(monsterPortraitLocation))
                {
                    monsterPortrait = null;

                    if (!replaceEntry)
                    {
                        LoggingHelper.WarningLog($"No monster portrait given for file in {usermodFolderPath}. No image will be shown.");
                    }
                }
                else
                {
                    monsterPortrait = ImageImport.LoadImage(jsonFolderPath + "\\" + monsterPortraitLocation,
                        usermodFolderPath + "\\" + monsterPortraitLocation);
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    LoggingHelper.WarningLog($"No monster portrait given for file in {usermodFolderPath}. No image will be shown.");
                }
            }

            // Monster Audio Path (Later gets added with coroutine)
            if (jsonObjectParsed.TryGetValue("monster_audio_clip_name", out var monsterAudioClipNameValue))
            {
                monsterAudioClipLocation = monsterAudioClipNameValue.Value<string>();

                if (string.IsNullOrEmpty(monsterAudioClipLocation) && !replaceEntry)
                {
                    LoggingHelper.InfoLog($"No monster audio given for file in {usermodFolderPath}. No audio will be shown.");
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    LoggingHelper.InfoLog($"No monster audio given for file in {usermodFolderPath}. No audio will be shown.");
                }
            }
            
            // Custom Campaign

            if (jsonObjectParsed.TryGetValue("attached_custom_campaign_name", out var attachedCustomCampaignName))
            {
                LoggingHelper.DebugLog("Found an entry that is custom campaign only.");
                
                customCampaignName = attachedCustomCampaignName.Value<string>();
                inCustomCampaign = true;
            }
            
            // Parse if the "replace" entry should be deleted.
            ParsingHelper.TryAssign(jsonObjectParsed, "delete_entry", ref deleteReplaceEntry);
        }

        private static void ParsePhobias(ref JObject jsonObjectParsed, ref bool spiderPhobia, 
            ref bool spiderPhobiaIncluded, ref bool darknessPhobia, ref bool darknessPhobiaIncluded, 
            ref bool dogPhobia, ref bool dogPhobiaIncluded, ref bool holesPhobia, ref bool holesPhobiaIncluded,
            ref bool insectPhobia, ref bool insectPhobiaIncluded, ref bool watchingPhobia, 
            ref bool watchingPhobiaIncluded, ref bool tightSpacePhobia, ref bool tightSpacePhobiaIncluded)
        {
            // Phobias, they don't require to be warned, since they optional.

            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "spider_phobia", ref spiderPhobia,
                ref spiderPhobiaIncluded);
            
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "darkness_phobia", ref darknessPhobia,
                ref darknessPhobiaIncluded);
            
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "dog_phobia", ref dogPhobia,
                ref dogPhobiaIncluded);
            
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "holes_phobia", ref holesPhobia,
                ref holesPhobiaIncluded);
            
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "insect_phobia", ref insectPhobia,
                ref insectPhobiaIncluded);
            
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "watching_phobia", ref watchingPhobia,
                ref watchingPhobiaIncluded);
            
            ParsingHelper.TryAssignWithBool(jsonObjectParsed, "tight_space_phobia", ref tightSpacePhobia,
                ref tightSpacePhobiaIncluded);
        }
        
        // ----------------------------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------
        
        // ReSharper disable once RedundantAssignment
        private static void CreateNewExtra(ref EntryMetadata newExtra, ref string monsterName, ref int newID,
            ref bool replaceEntry, ref string callerName, ref string callerTranscript, ref Sprite callerPortrait,
            ref float callerReplaceChance, ref bool callerRestartCallAgain, ref int accessLevel, ref bool onlyDLC,
            ref bool includeDLC, ref bool includeMainCampaign, ref string consequenceCallerName,
            ref string consequenceCallerTranscript, ref Sprite consequenceCallerPortrait,
            ref bool deleteReplaceEntry, ref bool inCustomCampaign, ref string customCampaignName)
        {
            newExtra = new EntryMetadata(monsterName, newID)
            {
                replace = replaceEntry,
                callerName = callerName,
                callTranscript = callerTranscript
            }; // ID will not work if not provided, but this shouldn't be an issue.

            if (callerPortrait != null)
            {
                newExtra.callerImage = callerPortrait;
            }

            newExtra.callerReplaceChance = callerReplaceChance;

            newExtra.allowCallAgainOverRestart = callerRestartCallAgain;

            newExtra.permissionLevel = accessLevel; // Minimum Access level required for call

            // DLC Handling
            newExtra.onlyDLC = onlyDLC;
            newExtra.includeInDLC = includeDLC;

            newExtra.inMainCampaign = includeMainCampaign;

            // Consequence Caller Handling
            newExtra.consequenceName = consequenceCallerName;
            newExtra.consequenceTranscript = consequenceCallerTranscript;
            newExtra.consequenceCallerImage = consequenceCallerPortrait;

            // Custom Campaign
            newExtra.OnlyCustomCampaign = inCustomCampaign;
            newExtra.CustomCampaignName = customCampaignName;

            if (deleteReplaceEntry)
            {
                if (replaceEntry)
                {
                    newExtra.DeleteEntry = deleteReplaceEntry;
                }
                else
                {
                    LoggingHelper.WarningLog($"Provided entry '{monsterName}' cannot be deleted as it is not replacing an entry.");
                }
            }
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
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as an entry. Possible syntax mistake?");
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
            string monsterName = "NO_NAME";
            string monsterDescription = "NO_DESCRIPTION";

            List<string> arcadeCalls = new List<string>();

            Sprite monsterPortrait = null;
            string monsterPortraitLocation = "";

            string monsterAudioClipLocation = "";

            // Caller Audio
            string callerName = "NO_CALLER_NAME";
            string callerTranscript = "NO_TRANSCRIPT";
            string callerImageLocation = "";
            float callerReplaceChance = 0.1f;
            bool callerRestartCallAgain = true;
            Sprite callerPortrait = null;

            // Consequence Caller Audio
            string consequenceCallerAudioClipLocation;
            string consequenceCallerName = "NO_CALLER_NAME";
            string consequenceCallerTranscript = "NO_TRANSCRIPT";
            string consequenceCallerImageLocation = "";
            Sprite consequenceCallerPortrait = null;

            // Custom Campaigns
            bool inCustomCampaign = false;
            string customCampaignName = "NO_CUSTOM_CAMPAIGN_NAME";

            bool deleteReplaceEntry = false;

            // Phobias
            bool spiderPhobia = false;
            bool spiderPhobiaIncluded = false;
            bool darknessPhobia = false;
            bool darknessPhobiaIncluded = false;
            bool dogPhobia = false;
            bool dogPhobiaIncluded = false;
            bool holesPhobia = false;
            bool holesPhobiaIncluded = false;
            bool insectPhobia = false;
            bool insectPhobiaIncluded = false;
            bool watchingPhobia = false;
            bool watchingPhobiaIncluded = false;
            bool tightSpacePhobia = false;
            bool tightSpacePhobiaIncluded = false;

            // Persistent information for caller.
            EntryMetadata newExtra = null;

            // We extract the info and save it (if the file is valid)
            // Parse Entry
            ParseEntry(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath, ref accessLevel,
                ref accessLevelAdded, ref replaceEntry, ref onlyDLC, ref includeDLC, ref includeMainCampaign,
                ref monsterName, ref monsterDescription, ref arcadeCalls, ref monsterPortrait,
                ref monsterPortraitLocation, ref monsterAudioClipLocation, ref deleteReplaceEntry,
                ref inCustomCampaign, ref customCampaignName);

            // Parse Phobias
            ParsePhobias(ref jObjectParsed, ref spiderPhobia, ref spiderPhobiaIncluded,
                ref darknessPhobia, ref darknessPhobiaIncluded, ref dogPhobia, ref dogPhobiaIncluded,
                ref holesPhobia, ref holesPhobiaIncluded, ref insectPhobia, ref insectPhobiaIncluded,
                ref watchingPhobia, ref watchingPhobiaIncluded, ref tightSpacePhobia, ref tightSpacePhobiaIncluded);

            // Parse Default Caller
            MainCampaignCallerParsing.ParseCaller(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath, ref callerName,
                ref callerTranscript, ref callerImageLocation, ref callerReplaceChance, ref callerRestartCallAgain,
                ref callerPortrait);

            MainCampaignCallerParsing.ParseConsequenceCaller(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath,
                ref consequenceCallerName, ref consequenceCallerTranscript, ref consequenceCallerImageLocation,
                ref consequenceCallerPortrait);

            // Create new extra info.
            CreateNewExtra(newExtra: ref newExtra, ref monsterName, ref newID, ref replaceEntry, ref callerName,
                ref callerTranscript, ref callerPortrait, ref callerReplaceChance, ref callerRestartCallAgain,
                ref accessLevel, ref onlyDLC,
                ref includeDLC, ref includeMainCampaign, ref consequenceCallerName, ref consequenceCallerTranscript,
                ref consequenceCallerPortrait, ref deleteReplaceEntry, ref inCustomCampaign, ref customCampaignName);

            // Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.TryGetValue("caller_audio_clip_name", out var callerAudioClipNameValue))
            {
                string _callerAudioClipLocation = (string)callerAudioClipNameValue;
                string callerAudioClipLocationLambdaCopy = _callerAudioClipLocation; // Create copy for lambda function.

                if (string.IsNullOrEmpty(_callerAudioClipLocation) && !replaceEntry)
                {
                    LoggingHelper.InfoLog($"No caller audio given for file in {jsonFolderPath}." +
                                          " No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(jsonFolderPath + "\\" + _callerAudioClipLocation) &&
                         !File.Exists(usermodFolderPath + "\\" + _callerAudioClipLocation))
                {
                    LoggingHelper.ErrorLog($"Location {jsonFolderPath} does not contain {_callerAudioClipLocation}." +
                                           " Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + _callerAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + _callerAudioClipLocation;
                    }

                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
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
                                    LoggingHelper.ErrorLog($"Failed to load audio clip {callerAudioClipLocationLambdaCopy}.");
                                }
                            },
                            audioLocation)
                    );
                }
            }


            // Consequence Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.TryGetValue("consequence_caller_audio_clip_name",
                    out var consequenceCallerAudioClipNameValue))
            {
                consequenceCallerAudioClipLocation = (string)consequenceCallerAudioClipNameValue;

                if (string.IsNullOrEmpty(consequenceCallerAudioClipLocation) && !replaceEntry)
                {
                    LoggingHelper.InfoLog($"No caller audio given for file in {usermodFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(jsonFolderPath + "\\" + consequenceCallerAudioClipLocation) &&
                         !File.Exists(usermodFolderPath + "\\" + consequenceCallerAudioClipLocation))
                {
                    LoggingHelper.ErrorLog( $"Location {jsonFolderPath} does not contain {consequenceCallerAudioClipLocation}." +
                                            " Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + consequenceCallerAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + consequenceCallerAudioClipLocation;
                    }

                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
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
                                    LoggingHelper.ErrorLog($"Failed to load audio clip {consequenceCallerAudioClipLocation}.");
                                }
                            },
                            audioLocation)
                    );
                }
            }

            // Add the extra information entry.
            if ((jObjectParsed.ContainsKey("caller_audio_clip_name") || includeMainCampaign || inCustomCampaign ||
                 replaceEntry) && newExtra != null)
            {
                GlobalParsingVariables.EntriesMetadata.Add(newExtra);
            }

            // Generate new ID if not provided.
            ParsingHelper.GenerateNewID(ref newExtra, ref newID, ref replaceEntry, ref jsonFolderPath,
                ref onlyDLC, ref includeDLC, ref entryUnlockerInstance, ref inCustomCampaign);

            if (replaceEntry) // We replace an Entry
            {
                // Returns a copy of the foundMonster

                MonsterProfile foundMonster = null;
                MonsterProfile foundMonsterXMAS = null; // For replacing DLC version as well

                ReplaceEntryFunction(ref entryUnlockerInstance, ref onlyDLC, ref includeDLC, ref monsterName,
                    ref newID, ref monsterPortraitLocation, ref monsterPortrait,
                    ref monsterDescription, ref replaceEntry, ref arcadeCalls, ref accessLevel, ref accessLevelAdded,
                    ref includeMainCampaign, ref spiderPhobiaIncluded, ref spiderPhobia, ref darknessPhobiaIncluded,
                    ref darknessPhobia, ref dogPhobiaIncluded, ref dogPhobia, ref holesPhobiaIncluded,
                    ref holesPhobia, ref insectPhobiaIncluded, ref insectPhobia, ref watchingPhobiaIncluded,
                    ref watchingPhobia, ref tightSpacePhobiaIncluded, ref tightSpacePhobia, ref foundMonster,
                    ref foundMonsterXMAS, ref inCustomCampaign, ref customCampaignName);

                // We replace the audio if needed.
                if (!string.IsNullOrEmpty(monsterAudioClipLocation))
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + monsterAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + monsterAudioClipLocation;
                    }

                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    if (foundMonster != null)
                                        foundMonster.monsterAudioClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                    if (foundMonsterXMAS != null)
                                        foundMonsterXMAS.monsterAudioClip =
                                            AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    LoggingHelper.ErrorLog($"Failed to load audio clip {monsterAudioClipLocation}.");
                                }
                            },
                            audioLocation)
                    );
                }
            }
            else // We add it instead of replacing the entry
            {
                MonsterProfile newMonster = null;

                CreateNewEntryFunction(ref entryUnlockerInstance, ref onlyDLC, ref includeDLC,
                    ref monsterName, ref newID, ref monsterPortrait, ref monsterDescription, ref arcadeCalls,
                    ref accessLevel, ref spiderPhobia, ref darknessPhobia, ref dogPhobia,
                    ref holesPhobia, ref insectPhobia, ref watchingPhobia, ref tightSpacePhobia, ref newMonster,
                    ref inCustomCampaign, ref customCampaignName);

                // Add audio to it
                if (!string.IsNullOrEmpty(monsterAudioClipLocation))
                {
                    // Use correct location.
                    string audioLocation = jsonFolderPath + "\\" + monsterAudioClipLocation;

                    if (!File.Exists(audioLocation))
                    {
                        audioLocation = usermodFolderPath + "\\" + monsterAudioClipLocation;
                    }

                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    newMonster.monsterAudioClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    LoggingHelper.ErrorLog($"Failed to load audio clip {monsterAudioClipLocation}.");
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
        
        private static void ReplaceEntryFunction(ref EntryUnlockController entryUnlockerInstance,
            ref bool onlyDLC, ref bool includeDLC, ref string monsterName, ref int newID,
            ref string monsterPortraitLocation, ref Sprite monsterPortrait, ref string monsterDescription,
            ref bool replaceEntry, ref List<string> arcadeCalls, ref int accessLevel, ref bool accessLevelAdded,
            ref bool includeMainCampaign, ref bool spiderPhobiaIncluded, ref bool spiderPhobia,
            ref bool darknessPhobiaIncluded, ref bool darknessPhobia, ref bool dogPhobiaIncluded,
            ref bool dogPhobia, ref bool holesPhobiaIncluded, ref bool holesPhobia, ref bool insectPhobiaIncluded,
            ref bool insectPhobia, ref bool watchingPhobiaIncluded, ref bool watchingPhobia,
            ref bool tightSpacePhobiaIncluded, ref bool tightSpacePhobia, ref MonsterProfile foundMonster,
            ref MonsterProfile foundMonsterXMAS, ref bool inCustomCampaign, ref string customCampaignName)
        {
            if (onlyDLC)
            {
                foundMonster =
                    EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allXmasEntries.monsterProfiles,
                        monsterName, newID);
            }
            else if (includeDLC)
            {
                // Will search both and attempt to find the entry.
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles,
                    monsterName, newID);
                foundMonsterXMAS =
                    EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allXmasEntries.monsterProfiles,
                        monsterName, newID); // New Monster to also replace
            }
            else if (includeMainCampaign) // Main Campaign
            {
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles,
                    monsterName, newID);
            }
            else if (inCustomCampaign) // In custom campaign.
            {
                foundMonster =
                    ScriptableObject
                        .CreateInstance<
                            MonsterProfile>(); // Create empty foundMonster to avoid replacing actual values.
                foundMonster.monsterID = newID;
                foundMonster.monsterName = monsterName;
            }

            if ((foundMonster == null && !onlyDLC && !includeDLC) || (foundMonster == null && foundMonsterXMAS == null))
            {
                LoggingHelper.WarningLog("Entry that was suppose to replace an entry failed. Information about the entry: " +
                                         $"Was found: {foundMonster != null} and was found in DLC: {foundMonsterXMAS != null}. " +
                                         $"Replacer Name: {monsterName} with Replacer ID: {newID}.");
                return;
            }

            LoggingHelper.InfoLog($"Found in the original list {monsterName} / {newID}." +
                                  " Now replacing/updating (for the main campaign / custom campaign)" +
                                  $" the entry with given information for {monsterName} / {newID}.");

            // Portrait
            if (!string.IsNullOrEmpty(monsterPortraitLocation))
            {
                if (foundMonster != null) foundMonster.monsterPortrait = monsterPortrait;
                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterPortrait = monsterPortrait;
            }

            // Description
            if (monsterDescription != "NO_DESCRIPTION")
            {
                if (foundMonster != null) foundMonster.monsterDescription = monsterDescription;
                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterDescription = monsterDescription;
            }

            // Name (Only works if ID was provided)
            if (monsterName != "NO_NAME" && newID >= 0 && foundMonster != null && replaceEntry &&
                monsterName != foundMonster.monsterName)
            {
                if (foundMonster != null) foundMonster.monsterName = monsterName;
                if (foundMonsterXMAS != null) foundMonsterXMAS.monsterName = monsterName;
                if (foundMonster != null) foundMonster.name = monsterName;
                if (foundMonsterXMAS != null) foundMonsterXMAS.name = monsterName;
            }

            // Arcade Calls
            if (arcadeCalls.Count > 0)
            {
                if (foundMonster != null) foundMonster.arcadeCalls = arcadeCalls.ToArray();
                if (foundMonsterXMAS != null) foundMonsterXMAS.arcadeCalls = arcadeCalls.ToArray();
            }

            // Phobias, they don't require to be warned, since they optional.

            if (spiderPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.spider = spiderPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.spider = spiderPhobia;
            }

            if (darknessPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.dark = darknessPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.dark = darknessPhobia;
            }

            if (dogPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.dog = dogPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.dog = dogPhobia;
            }

            if (holesPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.holes = holesPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.holes = holesPhobia;
            }

            if (insectPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.insect = insectPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.insect = insectPhobia;
            }

            if (watchingPhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.watching = watchingPhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.watching = watchingPhobia;
            }

            if (tightSpacePhobiaIncluded)
            {
                if (foundMonster != null) foundMonster.tightSpace = tightSpacePhobia;
                if (foundMonsterXMAS != null) foundMonsterXMAS.tightSpace = tightSpacePhobia;
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
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                                ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                            // ReSharper disable once StringLiteralTypo
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                                ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                        }

                        if (foundMonsterXMAS != null)
                        {
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                                ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                            // ReSharper disable once StringLiteralTypo
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                                ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                        }
                    }

                    break;

                case 1: // Second Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    }

                    break;

                case 2: // Third Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    }

                    break;

                case 3: // Fourth Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    }

                    break;

                case 4: // Fifth Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                            "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                            "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    break;

                case 5: // Sixth Level

                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster,
                            ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                            "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS,
                            ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                            "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    }

                    break;
            }

            // Now if we also use "includeCampaign" we need to replace it there as well
            if (includeMainCampaign)
            {
                if (foundMonster != null)
                {
                    MonsterProfile foundMonsterCopy = foundMonster;

                    EntryManager.EntryManager.ReplaceEntry(
                        ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, monsterName, foundMonster);

                    // Include a copy of the monster in the extra info
                    GlobalParsingVariables.EntriesMetadata.Find(item =>
                            item.Name == foundMonsterCopy.monsterName || item.ID == foundMonsterCopy.monsterID)
                        .referenceCopyEntry = foundMonster;
                }

                if (foundMonsterXMAS != null)
                {
                    MonsterProfile foundMonsterXMASCopy = foundMonsterXMAS;

                    EntryManager.EntryManager.ReplaceEntry(
                        ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, monsterName,
                        foundMonsterXMAS);

                    if (foundMonster == null)
                    {
                        // Include a copy of the monster in the extra info
                        GlobalParsingVariables.EntriesMetadata.Find(item =>
                                item.Name == foundMonsterXMASCopy.monsterName ||
                                item.ID == foundMonsterXMASCopy.monsterID)
                            .referenceCopyEntry = foundMonster;
                    }
                }
            }
            else if (inCustomCampaign) // Add entry to replace in custom campaign.
            {
                // Include a copy of the monster in the extra info
                if (foundMonster != null)
                {
                    string monsterNameCopy = foundMonster.monsterName;

                    EntryMetadata extraEntryInfo = GlobalParsingVariables.EntriesMetadata.Find(item => item.Name == monsterNameCopy);
                    if (extraEntryInfo != null) // Only if it exists.
                    {
                        extraEntryInfo.referenceCopyEntry = foundMonster;

                        // Add the extraEntryInfo in custom campaign things.

                        string customCampaignNameCopy = customCampaignName;

                        CustomCampaign currentCustomCampaign =
                            CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                                customCampaignSearch.CampaignName == customCampaignNameCopy);

                        if (currentCustomCampaign == null)
                        {
                            LoggingHelper.DebugLog("Custom Campaign replace entry found before custom campaign has been parsed." +
                                                   " Adding to late add.");

                            GlobalParsingVariables.PendingCustomCampaignReplaceEntries.Add(extraEntryInfo);

                            return;
                        }

                        currentCustomCampaign.EntryReplaceOnlyInCampaign.Add(extraEntryInfo);
                    }
                }
            }
        }
        
        private static void CreateNewEntryFunction(ref EntryUnlockController entryUnlockerInstance, ref bool onlyDLC,
            ref bool includeDLC, ref string monsterName, ref int newID, ref Sprite monsterPortrait,
            ref string monsterDescription, ref List<string> arcadeCalls, ref int accessLevel,
            ref bool spiderPhobia, ref bool darknessPhobia, ref bool dogPhobia,
            ref bool holesPhobia, ref bool insectPhobia, ref bool watchingPhobia, ref bool tightSpacePhobia,
            // ReSharper disable once RedundantAssignment
            ref MonsterProfile newMonster, ref bool inCustomCampaign, ref string customCampaignName)
        {
            // Create Monster and add him
            // NOTE: AudioClip is added later, since we need to do load it separately from the main thread.
            newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: monsterName,
                _monsterDescription: monsterDescription, _monsterID: newID,
                _arcadeCalls: arcadeCalls.ToArray(), _monsterPortrait: monsterPortrait, _monsterAudioClip: null,
                _spiderPhobia: spiderPhobia, _darknessPhobia: darknessPhobia, _dogPhobia: dogPhobia,
                _holesPhobia: holesPhobia, _insectPhobia: insectPhobia, _watchingPhobia: watchingPhobia,
                _tightSpacePhobia: tightSpacePhobia);

            // Create copy for lambda functions.
            MonsterProfile _newMonsterCopy = newMonster;

            // Include a copy of the monster in the extra info
            EntryMetadata extraEntryInfo = GlobalParsingVariables.EntriesMetadata.Find(item =>
                item.Name == _newMonsterCopy.monsterName || item.ID == _newMonsterCopy.monsterID);
            if (extraEntryInfo != null) // Only if it exists.
            {
                extraEntryInfo.referenceCopyEntry = newMonster;
            }

            // Decide where to add the Entry to. (Main Game or DLC or even both)

            if (onlyDLC) // Only DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                    ref entryUnlockerInstance.allXmasEntries.monsterProfiles, "allXmasEntries");
            }
            else if (includeDLC) // Also allow in DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                    ref entryUnlockerInstance.allEntries.monsterProfiles, "allEntries");
                EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                    ref entryUnlockerInstance.allXmasEntries.monsterProfiles, "allXmasEntries");
            }
            else if (inCustomCampaign) // Custom Campaign
            {
                // Please note that the entry never gets added to the main monster profile array.
                // But we do add them to the permission tiers. As they never get rendered, it is fine to store it this way.
                string customCampaignNameCopy = customCampaignName;

                // Add to correct campaign.
                CustomCampaign foundCustomCampaign =
                    CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.CampaignName == customCampaignNameCopy);

                if (foundCustomCampaign != null)
                {
                    LoggingHelper.DebugLog("Adding found custom campaign entry to the custom campaign.");

                    if (extraEntryInfo != null)
                    {
                        foundCustomCampaign.EntriesOnlyInCampaign.Add(extraEntryInfo);
                    }
                    else
                    {
                        LoggingHelper.WarningLog("Entry that was suppose to be added in custom campaign does not exist as extra info." +
                                                 " (Error Type: 1) ");
                    }
                }
                else
                {
                    LoggingHelper.DebugLog("Found monster entry before the custom campaign was found / does not exist.");

                    if (extraEntryInfo != null)
                    {
                        GlobalParsingVariables.PendingCustomCampaignEntries.Add(extraEntryInfo);
                    }
                    else
                    {
                        LoggingHelper.WarningLog("Entry that was suppose to be added in custom campaign does not exist as extra info. " +
                                                  "(Error Type: 2) ");
                    }
                }
            }
            else // Only base game. (OLD: // Only base game.)
            {
                // This will add the entry to the base game, regardless of what was chosen. This is to avoid any issues.
                // I don't see any reason to why one should not always add it to the base game if nothing was given.
                EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                    ref entryUnlockerInstance.allEntries.monsterProfiles, "allEntries");
            }

            /*
             * Removed the section that adds the entry also to entryUnlockerInstance.allMainCampaignEntries.
             * However, this added the entry twice for unknown reasons. So we do not do it.
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
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                    // ReSharper disable once StringLiteralTypo
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                    break;

                case 1: // Second Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    break;

                case 2: // Third Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    break;

                case 3: // Fourth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    break;

                case 4: // Fifth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                        "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    break;

                case 5: // Sixth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                        "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    break;

                default: // In case we somehow have an unknown value, we also default to first level.
                    MelonLogger.Warning(
                        "WARNING: Provided access level is invalid (0-5). Defaulting to 0th access level.");
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");

                    // ReSharper disable once StringLiteralTypo
                    EntryManager.EntryManager.AddMonsterToTheProfile(newMonster,
                        ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                    break;
            }

            LoggingHelper.DebugLog($"Finished parsing entry: {newMonster.monsterName}.");
        }
    }
}
