using MelonLoader;
using MelonLoader.TinyJSON;
using NewSafetyHelp.src.ImportFiles;
using NewSafetyHelp.src.EntryManager;
using System.IO;
using NewSafetyHelp.src.AudioHandler;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace NewSafetyHelp.src.JSONParsing
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

            for (int i = 0; i < filesDataPath.Length; i++)
            {
                if (filesDataPath[i].ToLower().EndsWith(".json"))
                {
                    MelonLogger.Msg($"INFO: Found new Entry at '{filesDataPath[i]}', attempting to add it now.");

                    string jsonString = File.ReadAllText(filesDataPath[i]);

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
            string userDataPath = FileImporter.getUserDataFolderPath();

            string[] foldersDataPath = Directory.GetDirectories(userDataPath);

            for (int i = 0; i < foldersDataPath.Length; i++)
            {
                LoadMonster(foldersDataPath[i], __instance);
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
            int accessLevel = 0;
            bool accessLevelAdded = false;

            bool replaceEntry = false;

            bool onlyDLC = false;
            bool includeDLC = false;

            bool includeCampaign = false;

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
            bool _callerRestartCallAgain = false;
            Sprite _callerPortrait = null;

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


            // We extract the info and save it
            if (jsonText is ProxyObject jsonObject)
            {

                // Replace Entry rather than add it, important for warnings.
                if (jsonObject.Keys.Contains("replace_entry"))
                {
                    replaceEntry = jsonObject["replace_entry"];
                }

                // Monster Name
                if (jsonObject.Keys.Contains("monster_name"))
                {
                    _monsterName = jsonObject["monster_name"];
                }
                else
                {
                    if (!replaceEntry)
                    {
                        MelonLogger.Warning($"WARNING: No Monster name given for file in {filePath}. Defaulting to NO_NAME.");
                    }
                }

                // Monster Description
                if (jsonObject.Keys.Contains("monster_description"))
                {
                    _monsterDescription = jsonObject["monster_description"];
                }
                else
                {
                    if (!replaceEntry)
                    {
                        MelonLogger.Warning($"WARNING: No Monster description given for file in {filePath}. Defaulting to NO_DESCRIPTION.");
                    }
                }


                // If the ID is provided we use that one instead. (Usefull for replacing entries)
                if (jsonObject.Keys.Contains("monster_id"))
                {
                    _monsterDescription = jsonObject["monster_id"];
                }

                // Phobias, they don't require to be warned, since they optional.

                if (jsonObject.Keys.Contains("spider_phobia"))
                {
                    _spiderPhobia = jsonObject["spider_phobia"];
                    _spiderPhobiaIncluded = true;
                }

                if (jsonObject.Keys.Contains("darkness_phobia"))
                {
                    _darknessPhobia = jsonObject["darkness_phobia"];
                    _darknessPhobiaIncluded = true;
                }

                if (jsonObject.Keys.Contains("dog_phobia"))
                {
                    _dogPhobia = jsonObject["dog_phobia"];
                    _dogPhobiaIncluded = true;
                }

                if (jsonObject.Keys.Contains("holes_phobia"))
                {
                    _holesPhobia = jsonObject["holes_phobia"];
                    _holesPhobiaIncluded = true;
                }

                if (jsonObject.Keys.Contains("insect_phobia"))
                {
                    _insectPhobia = jsonObject["insect_phobia"];
                    _insectPhobiaIncluded = true;
                }

                if (jsonObject.Keys.Contains("watching_phobia"))
                {
                    _watchingPhobia = jsonObject["watching_phobia"];
                    _watchingPhobiaIncluded = true;
                }

                if (jsonObject.Keys.Contains("tight_space_phobia"))
                {
                    _tightSpacePhobia = jsonObject["tight_space_phobia"];
                    _tightSpacePhobiaIncluded = true;
                }


                // DLC xMas
                if (jsonObject.Keys.Contains("only_dlc"))
                {
                    onlyDLC = jsonObject["only_dlc"];
                }
                if (jsonObject.Keys.Contains("include_dlc"))
                {
                    includeDLC = jsonObject["include_dlc"];
                }

                if (jsonObject.Keys.Contains("include_campaign")) // Unsure, what exactly it does, since it does not prevent it from appearing in the campaing.
                {
                    includeCampaign = jsonObject["include_campaign"];
                }


                // Access Level and Aracade Calls
                if (jsonObject.Keys.Contains("access_level"))
                {
                    accessLevel = jsonObject["access_level"];
                    accessLevelAdded = true;
                }

                if (jsonObject.Keys.Contains("arcade_calls"))
                {
                    var test = (ProxyArray) jsonObject["arcade_calls"];

                    for (int i = 0; i < test.Count; i++)
                    {
                        _arcadeCalls.Add(test[i]);
                    }
                }
                else
                {
                    if (!replaceEntry)
                    {
                        MelonLogger.Warning($"WARNING: No Arcade Calls given for file in {filePath}. Defaulting to empty values.");
                    }
                }


                // Image
                if (jsonObject.Keys.Contains("monster_portrait_image_name"))
                {
                    _monsterPortraitLocation = jsonObject["monster_portrait_image_name"];

                    if (_monsterPortraitLocation == "" || _monsterPortraitLocation == null)
                    {
                        _monsterPortrait = null;

                        if (!replaceEntry)
                        {
                            MelonLogger.Warning($"WARNING: No monster portrait given for file in {filePath}. No image will be shown.");
                        }
                    }
                    else
                    {
                        _monsterPortrait = ImageImport.LoadImage(filePath + "\\" + _monsterPortraitLocation);
                    }
                }
                else
                {
                    if (!replaceEntry)
                    {
                        MelonLogger.Warning($"WARNING: No monster portrait given for file in {filePath}. No image will be shown.");
                    }
                }

                // Monster Audio Path (Later gets added with coroutine)
                if (jsonObject.Keys.Contains("monster_audio_clip_name"))
                {
                    _monsterAudioClipLocation = jsonObject["monster_audio_clip_name"];

                    if ((_monsterAudioClipLocation == "" || _monsterAudioClipLocation == null) && !replaceEntry)
                    {
                        MelonLogger.Msg($"INFO: No monster audio given for file in {filePath}. No audio will be shown.");
                    }
                }
                else
                {
                    if (!replaceEntry)
                    {
                        MelonLogger.Msg($"INFO: No monster audio given for file in {filePath}. No audio will be shown.");
                    }
                }

                // Caller Information

                // Caller Name
                if (jsonObject.Keys.Contains("caller_name"))
                {
                    _callerName = jsonObject["caller_name"];
                }

                // Caller Transcript
                if (jsonObject.Keys.Contains("caller_transcript"))
                {
                    _callerTranscript = jsonObject["caller_transcript"];
                }

                // Caller Image
                if (jsonObject.Keys.Contains("caller_image_name"))
                {
                    _callerImageLocation = jsonObject["caller_image_name"];

                    if (_callerImageLocation == "" || _callerImageLocation == null)
                    {
                        _callerPortrait = null;

                        MelonLogger.Warning($"WARNING: Invalid Caller Portrait for {filePath}. No image will be shown.");
                    }
                    else
                    {
                        _callerPortrait = ImageImport.LoadImage(filePath + "\\" + _callerImageLocation);
                    }
                }

                // Caller Replace Chance
                if (jsonObject.Keys.Contains("caller_chance"))
                {
                    _callerReplaceChance = jsonObject["caller_chance"];
                }

                
                // If to store the information if it was already called once.
                if (jsonObject.Keys.Contains("allow_calling_again_over_restarts"))
                {
                    _callerRestartCallAgain = jsonObject["allow_calling_again_over_restarts"];
                }


                // If to include in the main campaing.
                if (includeCampaign)
                {
                    newExtra = new EntryExtraInfo(_monsterName, newID); // ID will not work if not provided, but this shouldn't be an issue.
                    newExtra.replace = replaceEntry;
                    newExtra.callerName = _callerName;
                    newExtra.callTranscript = _callerTranscript;

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
                }


                // Caller Audio Path (Later gets added with coroutine)
                if (jsonObject.Keys.Contains("caller_audio_clip_name"))
                {
                    _callerAudioClipLocation = jsonObject["caller_audio_clip_name"];

                    if ((_callerAudioClipLocation == "" || _callerAudioClipLocation == null) && !replaceEntry)
                    {
                        MelonLogger.Msg($"INFO: No caller audio given for file in {filePath}. No audio will be heard.");
                    }
                    else if (!File.Exists(filePath + "\\" + _callerAudioClipLocation)) // Check if location is valid now, since we are storing it now.
                    {
                        MelonLogger.Error($"ERROR: Location {filePath} does not contain {_callerAudioClipLocation}. Unable to add audio.");
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

                                    if (newExtra == null) // Incase we didn't create it
                                    {
                                        newExtra = new EntryExtraInfo(_monsterName, newID); // ID will not work if not provided, but this shouldn't be an issue.

                                        // Add extra information used for distinguishing entries from campaign.
                                        newExtra.replace = replaceEntry;
                                        newExtra.callerName = _callerName;
                                        newExtra.callTranscript = _callerTranscript;

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
                                    }

                                    newExtra.callerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error($"ERROR: Failed to load audio clip {_callerAudioClipLocation}.");
                                }

                            },
                            filePath + "\\" + _callerAudioClipLocation)
                        );
                    }
                }

                // Add the extra information entry.
                if ((jsonObject.Keys.Contains("caller_audio_clip_name") || includeCampaign) && newExtra != null)
                {
                    entriesExtraInfo.Add(newExtra);
                }
            }

            // Update ID if not given.
            if (newID == -1 && !replaceEntry)
            {
                MelonLogger.Msg($"INFO: Defaulting to a new Monster ID for file in {filePath}.\n (This intended and recommended.)");

                // Get the max Monster ID.
                int maxEntryIDMainCampaing = EntryManager.EntryManager.getNewEntryID(entryUnlockerInstance);
                int maxEntryIDMainDLC = EntryManager.EntryManager.getNewEntryID(entryUnlockerInstance, 1);

                if (onlyDLC) // Only DLC
                {
                    newID = maxEntryIDMainDLC;
                }
                else if (includeDLC) // Also allow in DLC (We pick the largest from both)
                {
                    newID = (maxEntryIDMainCampaing < maxEntryIDMainDLC) ? maxEntryIDMainDLC : maxEntryIDMainCampaing;
                }
                else // Only base game.
                {
                    newID = maxEntryIDMainCampaing;
                }
            }

            if (replaceEntry) // We replace an Entry
            {

                // Returns a copy of the foundMonster

                MonsterProfile foundMonster = null;
                MonsterProfile foundMonsterXMAS = null; // For replacing DLC version aswell

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

                if ((foundMonster == null  && !onlyDLC && !includeDLC) || (foundMonster == null && foundMonsterXMAS == null))
                {
                    MelonLogger.Warning($"WARNING: Entry that was suppose to replace an entry failed. Information about the entry: Was found: {foundMonster != null} and was found in DLC: {foundMonsterXMAS != null}. Replacer Name: {_monsterName} with Replacer ID: {newID}.");
                    return;
                }

                MelonLogger.Msg($"INFO: Found in the original list {_monsterName} / {newID}. Now replacing/updating the entry with given information for {_monsterName} / {newID}.");

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
                // This also counts the same for christmas
                switch (accessLevel)
                {
                    case 0: // First Level, is also default if not provided.
                        if (accessLevelAdded)
                        {

                            if (foundMonster != null)
                            {
                                EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                                EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                            }

                            if (foundMonsterXMAS != null)
                            {
                                EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
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
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level incase they also want christmas
                        }

                        if (foundMonsterXMAS != null)
                        {
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles, "fifthTierUnlocks");
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level incase they also want christmas
                        }

                        break;

                    case 5: // Sixth Level

                        if (foundMonster != null)
                        {
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level incase they also want christmas
                        }

                        if (foundMonsterXMAS != null)
                        {
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                            EntryManager.EntryManager.AddMonsterToTheProfile(foundMonsterXMAS, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level incase they also want christmas
                        }

                        break;
                }

                // Now if we also use "includeCampaign" we need to replace it there aswell
                if (includeCampaign)
                {
                    if (foundMonster != null)
                    {
                        EntryManager.EntryManager.ReplaceEntry(ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, _monsterName, foundMonster);

                        // Include a copy of the monster in the extrainfo
                        entriesExtraInfo.Find(item => item.Name == foundMonster.monsterName || item.ID == foundMonster.monsterID).referenceCopyEntry = foundMonster;
                    }
                    
                    if (foundMonsterXMAS != null)
                    {
                        EntryManager.EntryManager.ReplaceEntry(ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, _monsterName, foundMonsterXMAS);

                        if (foundMonster == null)
                        {
                            // Include a copy of the monster in the extrainfo
                            entriesExtraInfo.Find(item => item.Name == foundMonster.monsterName || item.ID == foundMonster.monsterID).referenceCopyEntry = foundMonster;
                        }
                    }

                    
                }
            }
            else // We add it instead of replacing the entry
            {
                // Create Monster and add him
                // NOTE: AudioClip is added later, since we need to do load it seperately from the main thread.
                MonsterProfile _newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: _monsterName, _monsterDescription: _monsterDescription, _monsterID: newID,
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

                // Include a copy of the monster in the extrainfo
                entriesExtraInfo.Find(item => item.Name == _newMonster.monsterName || item.ID == _newMonster.monsterID).referenceCopyEntry = _newMonster;

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

                // If to include in the Main Campaign. I assume.
                if (includeCampaign)
                {
                    // It seems to be a duplicate entry list of allEntries. As adding here results in fiding the same entry again.
                    //EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles, "allMainCampaignEntries");
                }

                // This also counts the same for christmas
                switch (accessLevel)
                {
                    case 0: // First Level, is also default if not provided.
                        EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
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
                        EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level incase they also want christmas
                        break;

                    case 5: // Sixth Level
                        EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.sixthTierUnlocks.monsterProfiles, "sixthTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles, "xmasFourthTier"); // We keep it fourth level incase they also want christmas
                        break;

                    default: // Incase we somehow have an unknown value, we also default to first level.
                        MelonLogger.Warning("WARNING: Provided access level is invalid (0-5). Defaulting to 0th access level.");
                        EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles, "firstTierUnlocks");
                        EntryManager.EntryManager.AddMonsterToTheProfile(_newMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles, "xmastFirstTier");
                        break;
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
    }
}
