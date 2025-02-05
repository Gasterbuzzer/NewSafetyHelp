using MelonLoader;
using MelonLoader.TinyJSON;
using NewSafetyHelp.src.ImportFiles;
using NewSafetyHelp.src.EntryManager;
using System.IO;
using NewSafetyHelp.src.AudioHandler;
using UnityEngine;
using System.Collections;
using System.Linq;
using static MelonLoader.MelonLogger;
using System;
using System.Collections.Generic;

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

            // bool _spiderPhobia = false, bool _darknessPhobia = false, bool _dogPhobia = false, bool _holesPhobia = false, bool _insectPhobia = false, bool _watchingPhobia = false, bool _tightSpacePhobia = false)

            int accessLevel = 0;

            string _monsterName = "NO_NAME";
            string _monsterDescription = "NO_DESCRIPTION";

            List<string> _arcadeCalls = new List<string>();

            Sprite _monsterPortrait = null;
            string _monsterPortraitLocation = "";

            string _monsterAudioClipLocation = "";

            //Phobias
            bool _spiderPhobia = false;
            bool _darknessPhobia = false;
            bool _dogPhobia = false;
            bool _holesPhobia = false;
            bool _insectPhobia = false;
            bool _watchingPhobia = false;
            bool _tightSpacePhobia = false;

            // First we attempt to extract the information;
            if (jsonText is ProxyObject jsonObject)
            {
                if (jsonObject.Keys.Contains("monster_name"))
                {
                    _monsterName = jsonObject["monster_name"];
                }
                else
                {
                    MelonLogger.Warning($"WARNING: No Monster name given for file in {filePath}. Defaulting to NO_NAME.");
                }

                if (jsonObject.Keys.Contains("monster_description"))
                {
                    _monsterDescription = jsonObject["monster_description"];
                }
                else
                {
                    MelonLogger.Warning($"WARNING: No Monster description given for file in {filePath}. Defaulting to NO_DESCRIPTION.");
                }

                if (jsonObject.Keys.Contains("monster_id"))
                {
                    _monsterDescription = jsonObject["monster_id"];
                }

                // Phobias

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


                //Rest
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

                // TODO Add logic to load the sprite.
                if (jsonObject.Keys.Contains("monster_portrait_image_name"))
                {
                    _monsterPortraitLocation = jsonObject["monster_portrait_image_name"];

                    if (_monsterPortraitLocation == "")
                    {
                        MelonLogger.Warning($"WARNING: No monster portrait given for file in {filePath}. No image will be shown.");
                    }

                    _monsterPortrait = null;
                }
                else
                {
                    MelonLogger.Warning($"WARNING: No monster portrait given for file in {filePath}. No image will be shown.");
                }

                if (jsonObject.Keys.Contains("monster_audio_clip_name"))
                {
                    _monsterAudioClipLocation = jsonObject["monster_audio_clip_name"];

                    if (_monsterAudioClipLocation == "")
                    {
                        MelonLogger.Msg($"INFO: No monster audio given for file in {filePath}. No audio will be shown.");
                    }
                }
                else
                {
                    MelonLogger.Msg($"INFO: No monster audio given for file in {filePath}. No audio will be shown.");
                }
            }

            if (newID == -1) // No ID given, we default to a new one.
            {
                MelonLogger.Msg($"Info: No Monster ID given for file in {filePath}. Defaulting to a valid ID. (This is not a problem, this is recommended.)");
                newID = -1; //TODO Replace with a new valid ID.
            }

            // Create Monster and add him
            // AudioClip is added later
            MonsterProfile _newMonster = EntryManager.EntryManager.CreateMonster(_monsterName: _monsterName, _monsterDescription: _monsterDescription, _monsterID: newID,
                _arcadeCalls: _arcadeCalls.ToArray(), _monsterPortrait: _monsterPortrait, _monsterAudioClip: null,
                _spiderPhobia: _spiderPhobia, _darknessPhobia: _darknessPhobia, _dogPhobia: _dogPhobia, _holesPhobia: _holesPhobia, _insectPhobia: _insectPhobia, _watchingPhobia: _watchingPhobia,
                _tightSpacePhobia: _tightSpacePhobia);

            EntryManager.EntryManager.AddMonsterToTheProfile(ref _newMonster, ref entryUnlockerInstance.allEntries.monsterProfiles);

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


            //__instance.allMainCampaignEntries.monsterProfiles.Append<MonsterProfile>(fakeMonster);

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
