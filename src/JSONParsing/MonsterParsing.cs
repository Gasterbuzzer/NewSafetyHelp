using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.EntryManager;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing
{
    public static class MonsterParsing
    {
        public static void parseEntry(ref JObject jsonObjectParsed, ref string filePath, ref int accessLevel, ref bool accessLevelAdded, ref bool replaceEntry, ref bool onlyDLC, ref bool includeDLC, ref bool includeMainCampaign, ref string _monsterName,
            ref string _monsterDescription, ref List<string> _arcadeCalls, ref Sprite _monsterPortrait, ref string _monsterPortraitLocation, ref string _monsterAudioClipLocation, ref bool deleteReplaceEntry, ref bool _inCustomCampaign,
            ref string _customCampaignName)
        {
            /* 
             * Monster Information
            */

            // Replace Entry rather than add it, important for warnings.
            if (jsonObjectParsed.ContainsKey("replace_entry"))
            {
                replaceEntry = (bool) jsonObjectParsed["replace_entry"];
            }

            // Monster Name
            if (jsonObjectParsed.ContainsKey("monster_name"))
            {
                _monsterName = (string) jsonObjectParsed["monster_name"];
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Warning($"WARNING: No Monster name given for file in {filePath}. Defaulting to NO_NAME.");
                }
            }

            // Monster Description
            if (jsonObjectParsed.ContainsKey("monster_description"))
            {
                _monsterDescription = (string) jsonObjectParsed["monster_description"];
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Warning($"WARNING: No Monster description given for file in {filePath}. Defaulting to NO_DESCRIPTION.");
                }
            }


            // DLC xMas
            if (jsonObjectParsed.ContainsKey("only_dlc"))
            {
                onlyDLC = (bool) jsonObjectParsed["only_dlc"];
            }
            if (jsonObjectParsed.ContainsKey("include_dlc"))
            {
                includeDLC = (bool) jsonObjectParsed["include_dlc"];
            }

            if (jsonObjectParsed.ContainsKey("include_campaign")) // Currently is used to distinguish if a caller should appear in the main campaign and if in custom campaign.
            {
                // OLD COMMENT, kind of incorrect but useful to know what I thought: (Unsure, what exactly it does, since it does not prevent it from appearing in the campaign.)
                includeMainCampaign = (bool) jsonObjectParsed["include_campaign"];
            }


            // Access Level and Arcade Calls
            if (jsonObjectParsed.ContainsKey("access_level"))
            {
                accessLevel = (int) jsonObjectParsed["access_level"];
                accessLevelAdded = true;
            }

            if (jsonObjectParsed.ContainsKey("arcade_calls"))
            {
                JArray test = (JArray) jsonObjectParsed["arcade_calls"];

                foreach (JToken arcadeCustomCall in test)
                {
                    _arcadeCalls.Add((string) arcadeCustomCall);
                }
            }
            else
            {
                if (!replaceEntry)
                {
                    MelonLogger.Msg($"Info: No Arcade Calls given for file in {filePath}. Defaulting to empty values.");
                }
            }


            // Image
            if (jsonObjectParsed.ContainsKey("monster_portrait_image_name"))
            {
                _monsterPortraitLocation = (string) jsonObjectParsed["monster_portrait_image_name"];

                if (string.IsNullOrEmpty(_monsterPortraitLocation))
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
            if (jsonObjectParsed.ContainsKey("monster_audio_clip_name"))
            {
                _monsterAudioClipLocation = (string) jsonObjectParsed["monster_audio_clip_name"];

                if (string.IsNullOrEmpty(_monsterAudioClipLocation) && !replaceEntry)
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
            if (jsonObjectParsed.ContainsKey("delete_entry"))
            {
                deleteReplaceEntry = (bool) jsonObjectParsed["delete_entry"];
            }
            
        }

        public static void parsePhobias(ref JObject jsonObjectParsed, ref string filePath, ref bool _spiderPhobia, ref bool _spiderPhobiaIncluded, ref bool _darknessPhobia, ref bool _darknessPhobiaIncluded, ref bool _dogPhobia, ref bool _dogPhobiaIncluded,
            ref bool _holesPhobia, ref bool _holesPhobiaIncluded, ref bool _insectPhobia, ref bool _insectPhobiaIncluded, ref bool _watchingPhobia, ref bool _watchingPhobiaIncluded, ref bool _tightSpacePhobia, ref bool _tightSpacePhobiaIncluded)
        {
            /* 
             * Monster Information
            */

            // Phobias, they don't require to be warned, since they optional.

            if (jsonObjectParsed.ContainsKey("spider_phobia"))
            {
                _spiderPhobia = (bool) jsonObjectParsed["spider_phobia"];
                _spiderPhobiaIncluded = true;
            }

            if (jsonObjectParsed.ContainsKey("darkness_phobia"))
            {
                _darknessPhobia = (bool) jsonObjectParsed["darkness_phobia"];
                _darknessPhobiaIncluded = true;
            }

            if (jsonObjectParsed.ContainsKey("dog_phobia"))
            {
                _dogPhobia = (bool) jsonObjectParsed["dog_phobia"];
                _dogPhobiaIncluded = true;
            }

            if (jsonObjectParsed.ContainsKey("holes_phobia"))
            {
                _holesPhobia = (bool) jsonObjectParsed["holes_phobia"];
                _holesPhobiaIncluded = true;
            }

            if (jsonObjectParsed.ContainsKey("insect_phobia"))
            {
                _insectPhobia = (bool) jsonObjectParsed["insect_phobia"];
                _insectPhobiaIncluded = true;
            }

            if (jsonObjectParsed.ContainsKey("watching_phobia"))
            {
                _watchingPhobia = (bool) jsonObjectParsed["watching_phobia"];
                _watchingPhobiaIncluded = true;
            }

            if (jsonObjectParsed.ContainsKey("tight_space_phobia"))
            {
                _tightSpacePhobia = (bool) jsonObjectParsed["tight_space_phobia"];
                _tightSpacePhobiaIncluded = true;
            }
        }
    }
}
