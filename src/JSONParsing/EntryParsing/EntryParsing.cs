using System.Collections.Generic;
using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.EntryParsing
{
    public static class EntryParsing
    {
        private static void ParseEntry(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref int accessLevel, ref bool accessLevelAdded, ref bool replaceEntry,
            ref bool onlyDLC, ref bool includeDLC, ref bool includeMainCampaign, ref string _monsterName, 
            ref string _monsterDescription, ref List<string> _arcadeCalls, ref Sprite _monsterPortrait,
            ref string _monsterPortraitLocation, ref string _monsterAudioClipLocation, ref bool deleteReplaceEntry,
            ref bool _inCustomCampaign, ref string _customCampaignName)
        {
            /* 
             * Monster Information
            */

            // Replace Entry rather than add it, important for warnings.
            if (jsonObjectParsed.TryGetValue("replace_entry", out var replaceEntryValue))
            {
                replaceEntry = (bool) replaceEntryValue;
            }

            // Monster Name
            if (jsonObjectParsed.TryGetValue("monster_name", out var monsterNameValue))
            {
                _monsterName = (string) monsterNameValue;
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Warning($"WARNING: No Monster name given for file in {usermodFolderPath}. Defaulting to NO_NAME.");
                }
            }

            // Monster Description
            if (jsonObjectParsed.TryGetValue("monster_description", out var monsterDescriptionValue))
            {
                _monsterDescription = (string) monsterDescriptionValue;
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Warning($"WARNING: No Monster description given for file in {usermodFolderPath}. Defaulting to NO_DESCRIPTION.");
                }
            }


            // DLC xMas
            if (jsonObjectParsed.TryGetValue("only_dlc", out var onlyDLCValue))
            {
                onlyDLC = (bool) onlyDLCValue;
            }
            if (jsonObjectParsed.TryGetValue("include_dlc", out var includeDLCValue))
            {
                includeDLC = (bool) includeDLCValue;
            }

            // Currently is used to distinguish if a caller should appear in the main campaign and if in custom campaign.
            if (jsonObjectParsed.TryGetValue("include_campaign", out var includeCampaignValue)) 
            {
                // OLD COMMENT, kind of incorrect but useful to know what I thought:
                // (Unsure, what exactly it does, since it does not prevent it from appearing in the campaign.)
                includeMainCampaign = (bool) includeCampaignValue;
            }


            // Access Level and Arcade Calls
            if (jsonObjectParsed.TryGetValue("access_level", out var accessLevelValue))
            {
                accessLevel = (int) accessLevelValue;
                accessLevelAdded = true;
            }

            if (jsonObjectParsed.TryGetValue("arcade_calls", out var arcadeCallsValue))
            {
                JArray test = (JArray) arcadeCallsValue;

                foreach (JToken arcadeCustomCall in test)
                {
                    _arcadeCalls.Add((string) arcadeCustomCall);
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Msg($"Info: No Arcade Calls given for file in {usermodFolderPath}. Defaulting to empty values.");
                }
            }


            // Image
            if (jsonObjectParsed.TryGetValue("monster_portrait_image_name", out var monsterPortraitImageNameValue))
            {
                _monsterPortraitLocation = (string) monsterPortraitImageNameValue;

                if (string.IsNullOrEmpty(_monsterPortraitLocation))
                {
                    _monsterPortrait = null;

                    if (!replaceEntry)
                    {
                        MelonLogger.Warning($"WARNING: No monster portrait given for file in {usermodFolderPath}. No image will be shown.");
                    }
                }
                else
                {
                    _monsterPortrait = ImageImport.LoadImage(jsonFolderPath + "\\" + _monsterPortraitLocation,
                        usermodFolderPath + "\\" + _monsterPortraitLocation);
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Warning($"WARNING: No monster portrait given for file in {usermodFolderPath}. No image will be shown.");
                }
            }

            // Monster Audio Path (Later gets added with coroutine)
            if (jsonObjectParsed.TryGetValue("monster_audio_clip_name", out var monsterAudioClipNameValue))
            {
                _monsterAudioClipLocation = (string) monsterAudioClipNameValue;

                if (string.IsNullOrEmpty(_monsterAudioClipLocation) && !replaceEntry)
                {
                    MelonLogger.Msg($"INFO: No monster audio given for file in {usermodFolderPath}. No audio will be shown.");
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Msg($"INFO: No monster audio given for file in {usermodFolderPath}. No audio will be shown.");
                }
            }
            
            // Custom Campaign

            if (jsonObjectParsed.ContainsKey("attached_custom_campaign_name"))
            {
                
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found an entry that is custom campaign only.");
                #endif
                
                _customCampaignName = (string) jsonObjectParsed["attached_custom_campaign_name"];
                _inCustomCampaign = true;
            }
            
            // Parse if the "replace" entry should be deleted.
            if (jsonObjectParsed.TryGetValue("delete_entry", out var deleteEntryValue))
            {
                deleteReplaceEntry = (bool) deleteEntryValue;
            }
            
        }

        private static void ParsePhobias(ref JObject jsonObjectParsed, ref bool _spiderPhobia,
            ref bool _spiderPhobiaIncluded, ref bool _darknessPhobia, ref bool _darknessPhobiaIncluded,
            ref bool _dogPhobia, ref bool _dogPhobiaIncluded,
            ref bool _holesPhobia, ref bool _holesPhobiaIncluded, ref bool _insectPhobia, ref bool _insectPhobiaIncluded,
            ref bool _watchingPhobia, ref bool _watchingPhobiaIncluded, ref bool _tightSpacePhobia,
            ref bool _tightSpacePhobiaIncluded)
        {
            // Phobias, they don't require to be warned, since they optional.

            if (jsonObjectParsed.TryGetValue("spider_phobia", out var spiderPhobiaValue))
            {
                _spiderPhobia = (bool) spiderPhobiaValue;
                _spiderPhobiaIncluded = true;
            }

            if (jsonObjectParsed.TryGetValue("darkness_phobia", out var darknessPhobiaValue))
            {
                _darknessPhobia = (bool) darknessPhobiaValue;
                _darknessPhobiaIncluded = true;
            }

            if (jsonObjectParsed.TryGetValue("dog_phobia", out var dogPhobiaValue))
            {
                _dogPhobia = (bool) dogPhobiaValue;
                _dogPhobiaIncluded = true;
            }

            if (jsonObjectParsed.TryGetValue("holes_phobia", out var holesPhobiaValue))
            {
                _holesPhobia = (bool) holesPhobiaValue;
                _holesPhobiaIncluded = true;
            }

            if (jsonObjectParsed.TryGetValue("insect_phobia", out var insectPhobiaValue))
            {
                _insectPhobia = (bool) insectPhobiaValue;
                _insectPhobiaIncluded = true;
            }

            if (jsonObjectParsed.TryGetValue("watching_phobia", out var watchingPhobiaValue))
            {
                _watchingPhobia = (bool) watchingPhobiaValue;
                _watchingPhobiaIncluded = true;
            }

            if (jsonObjectParsed.TryGetValue("tight_space_phobia", out var tightSpacePhobiaValue))
            {
                _tightSpacePhobia = (bool) tightSpacePhobiaValue;
                _tightSpacePhobiaIncluded = true;
            }
        }
        
        // ----------------------------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------
        
        // ReSharper disable once RedundantAssignment
        private static void CreateNewExtra(ref EntryExtraInfo newExtra, ref string _monsterName, ref int newID,
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
                    MelonLogger.Warning(
                        $"WARNING: Provided entry '{_monsterName}' cannot be deleted as it is not replacing an entry.");
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
            ParseEntry(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath, ref accessLevel,
                ref accessLevelAdded, ref replaceEntry, ref onlyDLC, ref includeDLC, ref includeMainCampaign,
                ref _monsterName, ref _monsterDescription, ref _arcadeCalls, ref _monsterPortrait,
                ref _monsterPortraitLocation, ref _monsterAudioClipLocation, ref deleteReplaceEntry,
                ref _inCustomCampaign, ref _customCampaignName);

            // Parse Phobias
            ParsePhobias(ref jObjectParsed, ref _spiderPhobia, ref _spiderPhobiaIncluded,
                ref _darknessPhobia, ref _darknessPhobiaIncluded, ref _dogPhobia, ref _dogPhobiaIncluded,
                ref _holesPhobia, ref _holesPhobiaIncluded, ref _insectPhobia, ref _insectPhobiaIncluded,
                ref _watchingPhobia, ref _watchingPhobiaIncluded, ref _tightSpacePhobia, ref _tightSpacePhobiaIncluded);

            // Parse Default Caller
            CallerParsing.ParseCaller(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath, ref _callerName,
                ref _callerTranscript, ref _callerImageLocation, ref _callerReplaceChance, ref _callerRestartCallAgain,
                ref _callerPortrait);

            CallerParsing.ParseConsequenceCaller(ref jObjectParsed, ref usermodFolderPath, ref jsonFolderPath,
                ref _consequenceCallerName, ref _consequenceCallerTranscript, ref _consequenceCallerImageLocation,
                ref _consequenceCallerPortrait);

            // Create new extra info.
            CreateNewExtra(newExtra: ref newExtra, ref _monsterName, ref newID, ref replaceEntry, ref _callerName,
                ref _callerTranscript, ref _callerPortrait, ref _callerReplaceChance, ref _callerRestartCallAgain,
                ref accessLevel, ref onlyDLC,
                ref includeDLC, ref includeMainCampaign, ref _consequenceCallerName, ref _consequenceCallerTranscript,
                ref _consequenceCallerPortrait, ref deleteReplaceEntry, ref _inCustomCampaign, ref _customCampaignName);

            // Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.TryGetValue("caller_audio_clip_name", out var callerAudioClipNameValue))
            {
                string _callerAudioClipLocation = (string)callerAudioClipNameValue;
                string callerAudioClipLocationLambdaCopy = _callerAudioClipLocation; // Create copy for lambda function.

                if (string.IsNullOrEmpty(_callerAudioClipLocation) && !replaceEntry)
                {
                    MelonLogger.Msg(
                        $"INFO: No caller audio given for file in {jsonFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(jsonFolderPath + "\\" + _callerAudioClipLocation) &&
                         !File.Exists(usermodFolderPath + "\\" + _callerAudioClipLocation))
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain {_callerAudioClipLocation}." +
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
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {callerAudioClipLocationLambdaCopy}.");
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
                _consequenceCallerAudioClipLocation = (string)consequenceCallerAudioClipNameValue;

                if (string.IsNullOrEmpty(_consequenceCallerAudioClipLocation) && !replaceEntry)
                {
                    MelonLogger.Msg(
                        $"INFO: No caller audio given for file in {usermodFolderPath}. No audio will be heard.");
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
                GlobalParsingVariables.EntriesMetadata.Add(newExtra);
            }

            // Generate new ID if not provided.
            ParsingHelper.GenerateNewID(ref newExtra, ref newID, ref replaceEntry, ref jsonFolderPath,
                ref onlyDLC, ref includeDLC, ref entryUnlockerInstance, ref _inCustomCampaign);

            if (replaceEntry) // We replace an Entry
            {
                // Returns a copy of the foundMonster

                MonsterProfile foundMonster = null;
                MonsterProfile foundMonsterXMAS = null; // For replacing DLC version as well

                ReplaceEntryFunction(ref entryUnlockerInstance, ref onlyDLC, ref includeDLC, ref _monsterName,
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

                CreateNewEntryFunction(ref entryUnlockerInstance, ref onlyDLC, ref includeDLC,
                    ref _monsterName, ref newID, ref _monsterPortrait, ref _monsterDescription, ref _arcadeCalls,
                    ref accessLevel, ref _spiderPhobia, ref _darknessPhobia, ref _dogPhobia,
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

                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
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
        
        private static void ReplaceEntryFunction(ref EntryUnlockController entryUnlockerInstance,
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
                foundMonster =
                    EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allXmasEntries.monsterProfiles,
                        _monsterName, newID);
            }
            else if (includeDLC)
            {
                // Will search both and attempt to find the entry.
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles,
                    _monsterName, newID);
                foundMonsterXMAS =
                    EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allXmasEntries.monsterProfiles,
                        _monsterName, newID); // New Monster to also replace
            }
            else if (includeMainCampaign) // Main Campaign
            {
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles,
                    _monsterName, newID);
            }
            else if (inCustomCampaign) // In custom campaign.
            {
                foundMonster =
                    ScriptableObject
                        .CreateInstance<
                            MonsterProfile>(); // Create empty foundMonster to avoid replacing actual values.
                foundMonster.monsterID = newID;
                foundMonster.monsterName = _monsterName;
            }

            if ((foundMonster == null && !onlyDLC && !includeDLC) || (foundMonster == null && foundMonsterXMAS == null))
            {
                MelonLogger.Warning(
                    "WARNING: Entry that was suppose to replace an entry failed. Information about the entry: " +
                    $"Was found: {foundMonster != null} and was found in DLC: {foundMonsterXMAS != null}. " +
                    $"Replacer Name: {_monsterName} with Replacer ID: {newID}.");
                return;
            }

            MelonLogger.Msg(
                $"INFO: Found in the original list {_monsterName} / {newID}." +
                " Now replacing/updating (for the main campaign / custom campaign)" +
                $" the entry with given information for {_monsterName} / {newID}.");

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
            if (_monsterName != "NO_NAME" && newID >= 0 && foundMonster != null && replaceEntry &&
                _monsterName != foundMonster.monsterName)
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
                        ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, _monsterName, foundMonster);

                    // Include a copy of the monster in the extra info
                    GlobalParsingVariables.EntriesMetadata.Find(item =>
                            item.Name == foundMonsterCopy.monsterName || item.ID == foundMonsterCopy.monsterID)
                        .referenceCopyEntry = foundMonster;
                }

                if (foundMonsterXMAS != null)
                {
                    MonsterProfile foundMonsterXMASCopy = foundMonsterXMAS;

                    EntryManager.EntryManager.ReplaceEntry(
                        ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, _monsterName,
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

                    EntryExtraInfo extraEntryInfo = GlobalParsingVariables.EntriesMetadata.Find(item => item.Name == monsterNameCopy);
                    if (extraEntryInfo != null) // Only if it exists.
                    {
                        extraEntryInfo.referenceCopyEntry = foundMonster;

                        // Add the extraEntryInfo in custom campaign things.

                        string customCampaignNameCopy = customCampaignName;

                        CustomCampaignExtraInfo currentCustomCampaign =
                            CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                                customCampaignSearch.campaignName == customCampaignNameCopy);

                        if (currentCustomCampaign == null)
                        {
                            #if DEBUG
                            MelonLogger.Msg(
                                "DEBUG: Custom Campaign replace entry found before custom campaign has been parsed." +
                                " Adding to late add.");
                            #endif

                            GlobalParsingVariables.PendingCustomCampaignReplaceEntries.Add(extraEntryInfo);

                            return;
                        }

                        currentCustomCampaign.entryReplaceOnlyInCampaign.Add(extraEntryInfo);
                    }
                }
            }
        }
        
        private static void CreateNewEntryFunction(ref EntryUnlockController entryUnlockerInstance, ref bool onlyDLC,
            ref bool includeDLC, ref string _monsterName, ref int newID, ref Sprite _monsterPortrait,
            ref string _monsterDescription, ref List<string> _arcadeCalls, ref int accessLevel,
            ref bool _spiderPhobia, ref bool _darknessPhobia, ref bool _dogPhobia,
            ref bool _holesPhobia, ref bool _insectPhobia, ref bool _watchingPhobia, ref bool _tightSpacePhobia,
            // ReSharper disable once RedundantAssignment
            ref MonsterProfile _newMonster, ref bool inCustomCampaign, ref string customCampaignName)
        {
            // Create Monster and add him
            // NOTE: AudioClip is added later, since we need to do load it separately from the main thread.
            _newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: _monsterName,
                _monsterDescription: _monsterDescription, _monsterID: newID,
                _arcadeCalls: _arcadeCalls.ToArray(), _monsterPortrait: _monsterPortrait, _monsterAudioClip: null,
                _spiderPhobia: _spiderPhobia, _darknessPhobia: _darknessPhobia, _dogPhobia: _dogPhobia,
                _holesPhobia: _holesPhobia, _insectPhobia: _insectPhobia, _watchingPhobia: _watchingPhobia,
                _tightSpacePhobia: _tightSpacePhobia);

            // Create copy for lambda functions.
            MonsterProfile _newMonsterCopy = _newMonster;

            // Include a copy of the monster in the extra info
            EntryExtraInfo extraEntryInfo = GlobalParsingVariables.EntriesMetadata.Find(item =>
                item.Name == _newMonsterCopy.monsterName || item.ID == _newMonsterCopy.monsterID);
            if (extraEntryInfo != null) // Only if it exists.
            {
                extraEntryInfo.referenceCopyEntry = _newMonster;
            }

            // Decide where to add the Entry to. (Main Game or DLC or even both)

            if (onlyDLC) // Only DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                    ref entryUnlockerInstance.allXmasEntries.monsterProfiles, "allXmasEntries");
            }
            else if (includeDLC) // Also allow in DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                    ref entryUnlockerInstance.allEntries.monsterProfiles, "allEntries");
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                    ref entryUnlockerInstance.allXmasEntries.monsterProfiles, "allXmasEntries");
            }
            else if (inCustomCampaign) // Custom Campaign
            {
                // Please note that the entry never gets added to the main monster profile array.
                // But we do add them to the permission tiers. As they never get rendered, it is fine to store it this way.
                string customCampaignNameCopy = customCampaignName;

                // Add to correct campaign.
                CustomCampaignExtraInfo foundCustomCampaign =
                    CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.campaignName == customCampaignNameCopy);

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
                        MelonLogger.Warning(
                            "WARNING: " +
                            "Entry that was suppose to be added in custom campaign does not exist as extra info." +
                            " (Error Type: 1) ");
                    }
                }
                else
                {
                    #if DEBUG
                    MelonLogger.Msg(
                        $"DEBUG: Found monster entry before the custom campaign was found / does not exist.");
                    #endif

                    if (extraEntryInfo != null)
                    {
                        GlobalParsingVariables.PendingCustomCampaignEntries.Add(extraEntryInfo);
                    }
                    else
                    {
                        MelonLogger.Warning(
                            "WARNING: " +
                            "Entry that was suppose to be added in custom campaign does not exist as extra info. " +
                            "(Error Type: 2) ");
                    }
                }
            }
            else // Only base game. (OLD: // Only base game.)
            {
                // This will add the entry to the base game, regardless of what was chosen. This is to avoid any issues.
                // I don't see any reason to why one should not always add it to the base game if nothing was given.
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
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
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                    // ReSharper disable once StringLiteralTypo
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                    break;

                case 1: // Second Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles, "secondTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmasSecondTier.monsterProfiles, "xmasSecondTier");
                    break;

                case 2: // Third Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles, "thirdTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmasThirdTier.monsterProfiles, "xmasThirdTier");
                    break;

                case 3: // Fourth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles, "fourthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier");
                    break;

                case 4: // Fifth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                        "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    break;

                case 5: // Sixth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmasFourthTier.monsterProfiles,
                        "xmasFourthTier"); // We keep it fourth level in case they also want Christmas
                    break;

                default: // In case we somehow have an unknown value, we also default to first level.
                    MelonLogger.Warning(
                        "WARNING: Provided access level is invalid (0-5). Defaulting to 0th access level.");
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");

                    // ReSharper disable once StringLiteralTypo
                    EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster,
                        ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                    break;
            }

            #if DEBUG
            MelonLogger.Msg($"DEBUG: Finished parsing entry: {_newMonster.monsterName}.");
            #endif
        }
    }
}
