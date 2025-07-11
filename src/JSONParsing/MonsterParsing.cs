using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using MelonLoader.TinyJSON;
using NewSafetyHelp.EntryManager;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing
{
    public static class MonsterParsing
    {
        public static void parseEntry(ref ProxyObject jsonObject, ref string filePath, ref int accessLevel, ref bool accessLevelAdded, ref bool replaceEntry, ref bool onlyDLC, ref bool includeDLC, ref bool includeMainCampaign, ref string _monsterName,
            ref string _monsterDescription, ref List<string> _arcadeCalls, ref Sprite _monsterPortrait, ref string _monsterPortraitLocation, ref string _monsterAudioClipLocation, ref bool _inCustomCampaign,  ref string _customCampaignName)
        {
            /* 
             * Monster Information
            */

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


            // DLC xMas
            if (jsonObject.Keys.Contains("only_dlc"))
            {
                onlyDLC = jsonObject["only_dlc"];
            }
            if (jsonObject.Keys.Contains("include_dlc"))
            {
                includeDLC = jsonObject["include_dlc"];
            }

            if (jsonObject.Keys.Contains("include_campaign")) // Currently is used to distinguish if a caller should appear in the main campaign and if in custom campaign.
            {
                // OLD COMMENT, kind of incorrect but useful to know what I thought: (Unsure, what exactly it does, since it does not prevent it from appearing in the campaign.)
                includeMainCampaign = jsonObject["include_campaign"];
            }


            // Access Level and Arcade Calls
            if (jsonObject.Keys.Contains("access_level"))
            {
                accessLevel = jsonObject["access_level"];
                accessLevelAdded = true;
            }

            if (jsonObject.Keys.Contains("arcade_calls"))
            {
                var test = (ProxyArray)jsonObject["arcade_calls"];

                foreach (Variant arcadeCustomCall in test)
                {
                    _arcadeCalls.Add(arcadeCustomCall);
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
            if (jsonObject.Keys.Contains("monster_audio_clip_name"))
            {
                _monsterAudioClipLocation = jsonObject["monster_audio_clip_name"];

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

            if (jsonObject.Keys.Contains("attached_custom_campaign_name"))
            {
                
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found an entry that is custom campaign only.");
                #endif
                
                _customCampaignName = jsonObject["attached_custom_campaign_name"];
                _inCustomCampaign = true;
            }
        }

        public static void parsePhobias(ref ProxyObject jsonObject, ref string filePath, ref bool _spiderPhobia, ref bool _spiderPhobiaIncluded, ref bool _darknessPhobia, ref bool _darknessPhobiaIncluded, ref bool _dogPhobia, ref bool _dogPhobiaIncluded,
            ref bool _holesPhobia, ref bool _holesPhobiaIncluded, ref bool _insectPhobia, ref bool _insectPhobiaIncluded, ref bool _watchingPhobia, ref bool _watchingPhobiaIncluded, ref bool _tightSpacePhobia, ref bool _tightSpacePhobiaIncluded)
        {
            /* 
             * Monster Information
            */

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
        }
    }
}
