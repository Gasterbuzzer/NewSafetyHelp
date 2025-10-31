using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.EntryManager;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.EntryParsing
{
    public static class MonsterParsing
    {
        public static void parseEntry(ref JObject jsonObjectParsed, ref string usermodFolderPath,
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

        public static void parsePhobias(ref JObject jsonObjectParsed, ref bool _spiderPhobia,
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
    }
}
