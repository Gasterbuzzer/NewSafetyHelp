using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.EntryManager.EntryData;
using Newtonsoft.Json.Linq;
using UnityEngine;

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
        public static void GenerateNewID(ref EntryExtraInfo newExtra, ref int newID, ref bool replaceEntry,
            ref string jsonFolderPath, ref bool onlyDlc, ref bool includeDlc,
            ref EntryUnlockController entryUnlockerInstance, ref bool inCustomCampaign)
        {
            // Update ID if not given.
            if (newID == -1 && !replaceEntry && !inCustomCampaign)
            {
                // Get the max Monster ID.
                int maxEntryIDMainCampaign = GetNewEntryID(entryUnlockerInstance);
                int maxEntryIDMainDlc = GetNewEntryID(entryUnlockerInstance, 1);

                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Entries in Main Campaign: {maxEntryIDMainCampaign} and entries in DLC: {maxEntryIDMainDlc}.");            
                #endif
                
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

            MelonLogger.Msg($"INFO: Defaulting to a new Monster ID {newExtra.ID} for file in {jsonFolderPath}.");
            MelonLogger.Msg("(This is the intended and recommended way of providing the ID.)");
        }
        
        /// <summary>
        /// Helper coroutine for updating the audio correctly for a monster clip.
        /// </summary>
        /// <param name="callback"> Callback function for returning values and doing stuff with it that require the coroutine to finish first. </param>
        /// <param name="audioPath"> Path to the audio file. </param>
        /// <param name="audioType"> Audio type to parse. </param>
        public static IEnumerator UpdateAudioClip(Action<AudioClip> callback, string audioPath,
            AudioType audioType = AudioType.WAV)
        {
            AudioClip monsterSoundClip = null;

            // Attempt to get the type
            if (audioType != AudioType.UNKNOWN)
            {
                audioType = AudioImport.GetAudioType(audioPath);

                yield return MelonCoroutines.Start(
                    AudioImport.LoadAudio
                    (
                        (myReturnValue) => { monsterSoundClip = myReturnValue; },
                        audioPath, audioType)
                );
            }

            callback(monsterSoundClip);
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
    }
}