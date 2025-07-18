﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using MelonLoader.TinyJSON;
using NewSafetyHelp.Audio;
using NewSafetyHelp.EntryManager;
using NewSafetyHelp.ImportFiles;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing
{
    public static class ParseMonster
    {
        // "Global" Variables for handling caller audio. Gets stored as its ID and with its Name.
        public static List<EntryExtraInfo> entriesExtraInfo = new List<EntryExtraInfo>();

        /// <summary>
        /// Function for adding a single entry.
        /// </summary>
        /// <param name="folderFilePath"> Path to the folder containing the entry. </param>
        /// <param name="__instance"> Instance of the EntryUnlockController. Needed for accessing and adding some entries. </param>
        public static void LoadMonster(string folderFilePath, EntryUnlockController __instance)
        {
            string[] filesDataPath = Directory.GetFiles(folderFilePath);

            foreach (string entryPath in filesDataPath)
            {
                if (entryPath.ToLower().EndsWith(".json"))
                {
                    MelonLogger.Msg($"INFO: Found new Entry at '{entryPath}', attempting to add it now.");

                    string jsonString = File.ReadAllText(entryPath);

                    Variant variant = JSON.Load(jsonString);

                    CreateMonsterFromJSON(variant, filePath: folderFilePath, entryUnlockerInstance: __instance);
                }
            }
        }

        /// <summary>
        /// Goes through all directories in the mods userdata folder and tries adding for each of them the monster if it contains an entry to be added.
        /// </summary>
        /// <param name="__instance"> Instance of the EntryUnlockController. Needed for accessing and adding some entries. </param>
        public static void LoadAllMonsters(EntryUnlockController __instance)
        {
            string userDataPath = FileImporter.GetUserDataFolderPath();

            string[] foldersDataPath = Directory.GetDirectories(userDataPath);

            foreach (string foldersStringName in foldersDataPath)
            {
                LoadMonster(foldersStringName, __instance);
            }
        }

        /// <summary>
        /// Function for adding a single entry.
        /// </summary>
        /// <param name="jsonText"> JSON Data for reading. </param>
        /// <param name="newID"> If we wish to provide the ID via parameter. </param>
        /// <param name="filePath"> Folder path to the entries directory </param>
        /// <param name="entryUnlockerInstance"> Instance of the EntryUnlockController. Needed for accessing and adding some entries. </param>
        public static void CreateMonsterFromJSON(Variant jsonText, int newID = -1, string filePath = "", EntryUnlockController entryUnlockerInstance = null)
        {
            // Values used for storing the information.
            int accessLevel = -1;
            bool accessLevelAdded = false;

            bool replaceEntry = false;

            bool onlyDLC = false;
            bool includeDLC = false;

            bool includeCampaign = false;

            // Entry / Monster Values
            string _monsterName = "NO_NAME";
            string _monsterDescription = "NO_DESCRIPTION";

            List<string> _arcadeCalls = new List<string>();

            Sprite _monsterPortrait = null;
            string _monsterPortraitLocation = "";

            string _monsterAudioClipLocation = "";

            // Caller Audio
            string _callerAudioClipLocation = "";
            string _callerName = "NO_CALLER_NAME";
            string _callerTranscript = "NO_TRANSCRIPT";
            string _callerImageLocation = "";
            float _callerReplaceChance = 0.1f;
            bool _callerRestartCallAgain = true;
            Sprite _callerPortrait = null;

            // Consequence Caller Audio
            string _consequenceCallerAudioClipLocation = "";
            string _consequenceCallerName = "NO_CALLER_NAME";
            string _consequenceCallerTranscript = "NO_TRANSCRIPT";
            string _consequenceCallerImageLocation = "";
            Sprite _consequenceCallerPortrait = null;

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
            if (jsonText is ProxyObject jsonObject)
            {
                // Parse Entry
                MonsterParsing.parseEntry(ref jsonObject, ref filePath, ref accessLevel, ref accessLevelAdded, ref replaceEntry, ref onlyDLC, ref includeDLC, ref includeCampaign, ref _monsterName, ref _monsterDescription, ref _arcadeCalls,
                    ref _monsterPortrait, ref _monsterPortraitLocation, ref _monsterAudioClipLocation);

                // Parse Phobias
                MonsterParsing.parsePhobias(ref jsonObject, ref filePath, ref _spiderPhobia, ref _spiderPhobiaIncluded, ref _darknessPhobia, ref _darknessPhobiaIncluded, ref _dogPhobia, ref _dogPhobiaIncluded, ref _holesPhobia, ref _holesPhobiaIncluded,
                    ref _insectPhobia, ref _insectPhobiaIncluded, ref _watchingPhobia, ref _watchingPhobiaIncluded, ref _tightSpacePhobia, ref _tightSpacePhobiaIncluded);

                // Parse Default Caller
                CallerParsing.parseCaller(ref jsonObject, ref filePath, ref _callerAudioClipLocation, ref _callerName, ref _callerTranscript, ref _callerImageLocation, ref _callerReplaceChance, ref _callerRestartCallAgain, ref _callerPortrait,
                    ref replaceEntry, ref newExtra);

                CallerParsing.parseConsequenceCaller(ref jsonObject, ref filePath, ref _consequenceCallerName, ref _consequenceCallerTranscript, ref _consequenceCallerImageLocation, ref _consequenceCallerPortrait);

                // Create new extra info.
                ParseMonster.createNewExtra(ref newExtra, ref _monsterName, ref newID, ref replaceEntry, ref _callerName, ref _callerTranscript, ref _callerPortrait, ref _callerReplaceChance, ref _callerRestartCallAgain, ref accessLevel, ref onlyDLC,
                    ref includeDLC, ref includeCampaign, ref _consequenceCallerName, ref _consequenceCallerTranscript, ref _consequenceCallerImageLocation, ref _consequenceCallerPortrait);

                // Caller Audio Path (Later gets added with coroutine)
                if (jsonObject.Keys.Contains("caller_audio_clip_name"))
                {
                    _callerAudioClipLocation = jsonObject["caller_audio_clip_name"];
                    string callerAudioClipLocationLambdaCopy = _callerAudioClipLocation; // Create copy for lambda function.

                    if (string.IsNullOrEmpty(_callerAudioClipLocation) && !replaceEntry)
                    {
                        MelonLogger.Msg($"INFO: No caller audio given for file in {filePath}. No audio will be heard.");
                    }
                    else if (!File.Exists(filePath + "\\" + _callerAudioClipLocation)) // Check if location is valid now, since we are storing it now.
                    {
                        MelonLogger.Error($"ERROR: Location {filePath} does not contain {_callerAudioClipLocation}. Unable to add audio.");
                    }
                    else // Valid location, so we load in the value.
                    {

                        MelonCoroutines.Start(ParseMonster.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    newExtra.callerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error($"ERROR: Failed to load audio clip {callerAudioClipLocationLambdaCopy}.");
                                }

                            },
                            filePath + "\\" + _callerAudioClipLocation)
                        );

                    }
                }


                // Consequence Caller Audio Path (Later gets added with coroutine)
                if (jsonObject.Keys.Contains("consequence_caller_audio_clip_name"))
                {
                    _consequenceCallerAudioClipLocation = jsonObject["consequence_caller_audio_clip_name"];

                    if (string.IsNullOrEmpty(_consequenceCallerAudioClipLocation) && !replaceEntry)
                    {
                        MelonLogger.Msg($"INFO: No caller audio given for file in {filePath}. No audio will be heard.");
                    }
                    else if (!File.Exists(filePath + "\\" + _consequenceCallerAudioClipLocation)) // Check if location is valid now, since we are storing it now.
                    {
                        MelonLogger.Error($"ERROR: Location {filePath} does not contain {_consequenceCallerAudioClipLocation}. Unable to add audio.");
                    }
                    else // Valid location, so we load in the value.
                    {
                        MelonCoroutines.Start(UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    newExtra.consequenceCallerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error($"ERROR: Failed to load audio clip {_consequenceCallerAudioClipLocation}.");
                                }

                            },
                            filePath + "\\" + _consequenceCallerAudioClipLocation)
                        );
                    }
                }

                // Add the extra information entry.
                if ((jsonObject.Keys.Contains("caller_audio_clip_name") || includeCampaign) && newExtra != null)
                {
                    entriesExtraInfo.Add(newExtra);
                }
            }

            // Generate new ID if not provided.
            ParseMonster.generateNewID(ref newID, ref replaceEntry, ref filePath, ref onlyDLC, ref includeDLC, ref entryUnlockerInstance);

            if (replaceEntry) // We replace an Entry
            {
                // Returns a copy of the foundMonster

                MonsterProfile foundMonster = null;
                MonsterProfile foundMonsterXMAS = null; // For replacing DLC version as well

                replaceEntryFunction(ref filePath, ref entryUnlockerInstance, ref onlyDLC, ref includeDLC, ref _monsterName, ref newID, ref _monsterAudioClipLocation, ref _monsterPortraitLocation, ref _monsterPortrait, ref _monsterDescription, ref replaceEntry,
                    ref _arcadeCalls, ref accessLevel, ref accessLevelAdded, ref includeCampaign, ref _spiderPhobiaIncluded, ref _spiderPhobia, ref _darknessPhobiaIncluded, ref _darknessPhobia, ref _dogPhobiaIncluded, ref _dogPhobia, ref _holesPhobiaIncluded,
                    ref _holesPhobia, ref _insectPhobiaIncluded, ref _insectPhobia, ref _watchingPhobiaIncluded, ref _watchingPhobia, ref _tightSpacePhobiaIncluded, ref _tightSpacePhobia, ref foundMonster, ref foundMonsterXMAS);

                // We replace the audio if needed.
                if (!string.IsNullOrEmpty(_monsterAudioClipLocation))
                {
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
                        filePath + "\\" + _monsterAudioClipLocation)
                    );
                }

            }
            else // We add it instead of replacing the entry
            {
                MonsterProfile _newMonster = null;

                createNewEntryFunction(ref filePath, ref entryUnlockerInstance, ref onlyDLC, ref includeDLC, ref _monsterName, ref newID, ref _monsterAudioClipLocation, ref _monsterPortraitLocation, ref _monsterPortrait, ref _monsterDescription,
                    ref replaceEntry, ref _arcadeCalls, ref accessLevel, ref accessLevelAdded, ref includeCampaign, ref _spiderPhobiaIncluded, ref _spiderPhobia, ref _darknessPhobiaIncluded, ref _darknessPhobia, ref _dogPhobiaIncluded, ref _dogPhobia,
                    ref _holesPhobiaIncluded, ref _holesPhobia, ref _insectPhobiaIncluded, ref _insectPhobia, ref _watchingPhobiaIncluded, ref _watchingPhobia, ref _tightSpacePhobiaIncluded, ref _tightSpacePhobia,
                    ref _newMonster);

                // Add audio to it
                if (!string.IsNullOrEmpty(_monsterAudioClipLocation))
                {
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
                        filePath + "\\" + _monsterAudioClipLocation)
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
        public static IEnumerator UpdateAudioClip(System.Action<AudioClip> callback, string audioPath, AudioType _audioType = AudioType.WAV)
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

        public static void createNewExtra(ref EntryExtraInfo newExtra, ref string _monsterName, ref int newID, ref bool replaceEntry, ref string _callerName, ref string _callerTranscript, ref Sprite _callerPortrait, ref float _callerReplaceChance,
            ref bool _callerRestartCallAgain, ref int accessLevel, ref bool onlyDLC, ref bool includeDLC, ref bool includeCampaign, ref string _consequenceCallerName, ref string _consequenceCallerTranscript, ref string _consequenceCallerImageLocation,
            ref Sprite _consequenceCallerPortrait)
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

            newExtra.inCampaign = includeCampaign;

            // Consequence Caller Handling
            newExtra.consequenceName = _consequenceCallerName;
            newExtra.consequenceTranscript = _consequenceCallerTranscript;
            newExtra.consequenceCallerImage = _consequenceCallerPortrait;
        }

        public static void generateNewID(ref int newID, ref bool replaceEntry, ref string filePath, ref bool onlyDLC, ref bool includeDLC, ref EntryUnlockController entryUnlockerInstance)
        {
            // Update ID if not given.
            if (newID == -1 && !replaceEntry)
            {
                MelonLogger.Msg($"INFO: Defaulting to a new Monster ID for file in {filePath}.");
                MelonLogger.Msg($"(This intended and recommended).");

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
        }

        public static void replaceEntryFunction(ref string filePath, ref EntryUnlockController entryUnlockerInstance, ref bool onlyDLC, ref bool includeDLC, ref string _monsterName, ref int newID, ref string _monsterAudioClipLocation,
            ref string _monsterPortraitLocation, ref Sprite _monsterPortrait, ref string _monsterDescription, ref bool replaceEntry, ref List<string> _arcadeCalls, ref int accessLevel, ref bool accessLevelAdded, ref bool includeCampaign,
            ref bool _spiderPhobiaIncluded, ref bool _spiderPhobia, ref bool _darknessPhobiaIncluded, ref bool _darknessPhobia, ref bool _dogPhobiaIncluded, ref bool _dogPhobia, ref bool _holesPhobiaIncluded, ref bool _holesPhobia,
            ref bool _insectPhobiaIncluded, ref bool _insectPhobia, ref bool _watchingPhobiaIncluded, ref bool _watchingPhobia, ref bool _tightSpacePhobiaIncluded, ref bool _tightSpacePhobia,
            ref MonsterProfile foundMonster, ref MonsterProfile foundMonsterXMAS)
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
            else
            {
                foundMonster = EntryManager.EntryManager.FindEntry(ref entryUnlockerInstance.allEntries.monsterProfiles, _monsterName, newID);
            }

            if ((foundMonster == null && !onlyDLC && !includeDLC) || (foundMonster == null && foundMonsterXMAS == null))
            {
                MelonLogger.Warning($"WARNING: Entry that was suppose to replace an entry failed. Information about the entry: Was found: {foundMonster != null} and was found in DLC: {foundMonsterXMAS != null}. Replacer Name: {_monsterName} with Replacer ID: {newID}.");
                return;
            }

            MelonLogger.Msg($"INFO: Found in the original list {_monsterName} / {newID}. Now replacing/updating the entry with given information for {_monsterName} / {newID}.");

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
            if (includeCampaign)
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
        }
    
        public static void createNewEntryFunction(ref string filePath, ref EntryUnlockController entryUnlockerInstance, ref bool onlyDLC, ref bool includeDLC, ref string _monsterName, ref int newID, ref string _monsterAudioClipLocation,
            ref string _monsterPortraitLocation, ref Sprite _monsterPortrait, ref string _monsterDescription, ref bool replaceEntry, ref List<string> _arcadeCalls, ref int accessLevel, ref bool accessLevelAdded, ref bool includeCampaign,
            ref bool _spiderPhobiaIncluded, ref bool _spiderPhobia, ref bool _darknessPhobiaIncluded, ref bool _darknessPhobia, ref bool _dogPhobiaIncluded, ref bool _dogPhobia, ref bool _holesPhobiaIncluded, ref bool _holesPhobia,
            ref bool _insectPhobiaIncluded, ref bool _insectPhobia, ref bool _watchingPhobiaIncluded, ref bool _watchingPhobia, ref bool _tightSpacePhobiaIncluded, ref bool _tightSpacePhobia,
            ref MonsterProfile _newMonster)
        {
            // Create Monster and add him
            // NOTE: AudioClip is added later, since we need to do load it separately from the main thread.
            _newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: _monsterName, _monsterDescription: _monsterDescription, _monsterID: newID,
                _arcadeCalls: _arcadeCalls.ToArray(), _monsterPortrait: _monsterPortrait, _monsterAudioClip: null,
                _spiderPhobia: _spiderPhobia, _darknessPhobia: _darknessPhobia, _dogPhobia: _dogPhobia, _holesPhobia: _holesPhobia, _insectPhobia: _insectPhobia, _watchingPhobia: _watchingPhobia,
                _tightSpacePhobia: _tightSpacePhobia);

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
            else // Only base game.
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.allEntries.monsterProfiles, "allEntries");
            }

            MonsterProfile _newMonsterCopy = _newMonster;
            // Include a copy of the monster in the extra info
            if (entriesExtraInfo.Find(item => item.Name == _newMonsterCopy.monsterName || item.ID == _newMonsterCopy.monsterID) != null) // Only if it exists.
            {
                entriesExtraInfo.Find(item => item.Name == _newMonsterCopy.monsterName || item.ID == _newMonsterCopy.monsterID).referenceCopyEntry = _newMonster;
            }

            /*
             * Removed the section that adds the entry also to entryUnlockerInstance.allMainCampaignEntries . However, this added the entry twice for unknown reasons. So we do not do it.
             */

            // If no access level provided.
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
        }
    }
}
