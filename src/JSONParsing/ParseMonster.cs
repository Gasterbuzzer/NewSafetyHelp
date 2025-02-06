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
using static MelonLoader.MelonLogger;

namespace NewSafetyHelp.src.JSONParsing
{
    public static class ParseMonster
    {
        public static void LoadMonster(string folderFilePath, EntryUnlockController __instance)
        {
            string[] filesDataPath = Directory.GetFiles(folderFilePath);

            for (int i = 0; i < filesDataPath.Length; i++)
            {
                if (filesDataPath[i].ToLower().EndsWith(".json"))
                {
                    MelonLogger.Msg($"Found new Monster Entry at {filesDataPath[i]}, adding now.");

                    string jsonString = File.ReadAllText(filesDataPath[i]);

                    Variant variant = JSON.Load(jsonString);

                    CreateMonsterFromJSON(variant, filePath: folderFilePath, entryUnlockerInstance: __instance);
                }
            }
        }

        public static void LoadAllMonsters(EntryUnlockController __instance)
        {
            string userDataPath = FileImporter.getUserDataFolderPath();

            string[] foldersDataPath = Directory.GetDirectories(userDataPath);

            for (int i = 0; i < foldersDataPath.Length; i++)
            {
                LoadMonster(foldersDataPath[i], __instance);
            }
        }

        public static void CreateMonsterFromJSON(Variant jsonText, int newID = -1, string filePath = "", EntryUnlockController entryUnlockerInstance = null)
        {
            // Values used for storing the information.
            int accessLevel = 0;

            bool onlyDLC = false;
            bool includeDLC = false;

            bool includeCampaign = false;

            string _monsterName = "NO_NAME";
            string _monsterDescription = "NO_DESCRIPTION";

            List<string> _arcadeCalls = new List<string>();

            Sprite _monsterPortrait = null;
            string _monsterPortraitLocation = "";

            string _monsterAudioClipLocation = "";

            // Phobias
            bool _spiderPhobia = false;
            bool _darknessPhobia = false;
            bool _dogPhobia = false;
            bool _holesPhobia = false;
            bool _insectPhobia = false;
            bool _watchingPhobia = false;
            bool _tightSpacePhobia = false;

            // We extract the info and save it
            if (jsonText is ProxyObject jsonObject)
            {
                // Monster Name
                if (jsonObject.Keys.Contains("monster_name"))
                {
                    _monsterName = jsonObject["monster_name"];
                }
                else
                {
                    MelonLogger.Warning($"WARNING: No Monster name given for file in {filePath}. Defaulting to NO_NAME.");
                }

                // Monster Description
                if (jsonObject.Keys.Contains("monster_description"))
                {
                    _monsterDescription = jsonObject["monster_description"];
                }
                else
                {
                    MelonLogger.Warning($"WARNING: No Monster description given for file in {filePath}. Defaulting to NO_DESCRIPTION.");
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
                }

                if (jsonObject.Keys.Contains("darkness_phobia"))
                {
                    _darknessPhobia = jsonObject["darkness_phobia"];
                }

                if (jsonObject.Keys.Contains("dog_phobia"))
                {
                    _dogPhobia = jsonObject["dog_phobia"];
                }

                if (jsonObject.Keys.Contains("holes_phobia"))
                {
                    _holesPhobia = jsonObject["holes_phobia"];
                }

                if (jsonObject.Keys.Contains("insect_phobia"))
                {
                    _insectPhobia = jsonObject["insect_phobia"];
                }

                if (jsonObject.Keys.Contains("watching_phobia"))
                {
                    _watchingPhobia = jsonObject["watching_phobia"];
                }

                if (jsonObject.Keys.Contains("tight_space_phobia"))
                {
                    _tightSpacePhobia = jsonObject["tight_space_phobia"];
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
                    MelonLogger.Warning($"WARNING: No Arcade Calls given for file in {filePath}. Defaulting to empty values.");
                }


                // Image
                if (jsonObject.Keys.Contains("monster_portrait_image_name"))
                {
                    _monsterPortraitLocation = jsonObject["monster_portrait_image_name"];

                    if (_monsterPortraitLocation == "" || _monsterPortraitLocation == null)
                    {
                        _monsterPortrait = null;
                        MelonLogger.Warning($"WARNING: No monster portrait given for file in {filePath}. No image will be shown.");
                    }
                    else
                    {
                        _monsterPortrait = ImageImport.LoadImage(filePath + "\\" + _monsterPortraitLocation);
                    }
                }
                else
                {
                    MelonLogger.Warning($"WARNING: No monster portrait given for file in {filePath}. No image will be shown.");
                }

                // Audio Path (Later gets added with coroutine)
                if (jsonObject.Keys.Contains("monster_audio_clip_name"))
                {
                    _monsterAudioClipLocation = jsonObject["monster_audio_clip_name"];

                    if (_monsterAudioClipLocation == "" || _monsterAudioClipLocation == null)
                    {
                        MelonLogger.Msg($"INFO: No monster audio given for file in {filePath}. No audio will be shown.");
                    }
                }
                else
                {
                    MelonLogger.Msg($"INFO: No monster audio given for file in {filePath}. No audio will be shown.");
                }
            }

            // Update ID if not given.
            if (newID == -1) //
            {
                MelonLogger.Msg($"Info: Defaulting to a new Monster ID for file in {filePath}. (This is not a problem, this is recommended.)");

                // Get the max Monster ID.
                int maxEntryIDMainCampaing = EntryManager.EntryManager.getlargerID(entryUnlockerInstance);
                int maxEntryIDMainDLC = EntryManager.EntryManager.getlargerID(entryUnlockerInstance, 1);

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

            // Create Monster and add him
            // NOTE: AudioClip is added later, since we need to do load it seperately from the main thread.
            MonsterProfile _newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: _monsterName, _monsterDescription: _monsterDescription, _monsterID: newID,
                _arcadeCalls: _arcadeCalls.ToArray(), _monsterPortrait: _monsterPortrait, _monsterAudioClip: null,
                _spiderPhobia: _spiderPhobia, _darknessPhobia: _darknessPhobia, _dogPhobia: _dogPhobia, _holesPhobia: _holesPhobia, _insectPhobia: _insectPhobia, _watchingPhobia: _watchingPhobia,
                _tightSpacePhobia: _tightSpacePhobia);


            // Decide where to add the Entry to. (Main Game or DLC or even both)

            if (onlyDLC) // Only DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allXmasEntries.monsterProfiles);
            }
            else if (includeDLC) // Also allow in DLC
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allEntries.monsterProfiles);
                EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allXmasEntries.monsterProfiles);
            }
            else // Only base game.
            {
                EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allEntries.monsterProfiles);
            }

            // Add audio to it
            if (_monsterAudioClipLocation != null || _monsterAudioClipLocation != "")
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
                EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allMainCampaignEntries.monsterProfiles);
            }

            // This also counts the same for christmas
            switch (accessLevel)
            {
                case 0: // First Level, is also default if not provided.
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles);
                    break;

                case 1: // Second Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.secondTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmasSecondTier.monsterProfiles);
                    break;

                case 2: // Third Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.thirdTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmasThirdTier.monsterProfiles);
                    break;

                case 3: // Fourth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.fourthTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles);
                    break;

                case 4: // Fifth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles); // We keep it fourth level incase they also want christmas
                    break;

                case 5: // Sixth Level
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.fifthTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmasFourthTier.monsterProfiles); // We keep it fourth level incase they also want christmas
                    break;

                default: // Incase we somehow have an unknown value, we also default to first level.
                    MelonLogger.Warning("WARNING: Provided access level is unknown (0-5). Defaulting to 0th access level.");
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.firstTierUnlocks.monsterProfiles);
                    EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.xmastFirstTier.monsterProfiles);
                    break;
            }
        }

        // Helper coroutine for getting an AudioClip
        public static IEnumerator UpdateAudioClip(System.Action<AudioClip> callback, string audioPath, AudioType _audioType = AudioType.WAV)
        {
            AudioClip monsterSoundClip = null;

            yield return MelonCoroutines.Start(
            AudioImport.LoadAudio
            (
                (myReturnValue) =>
                {
                    monsterSoundClip = myReturnValue;
                },
                audioPath, _audioType)
            );

            callback(monsterSoundClip);
        }
    }
}
