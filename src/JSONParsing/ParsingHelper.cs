using MelonLoader;
using NewSafetyHelp.EntryManager.EntryData;

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
        
        public static void generateNewID(ref EntryExtraInfo newExtra, ref int newID, ref bool replaceEntry,
            ref string jsonFolderPath, ref bool onlyDLC, ref bool includeDLC,
            ref EntryUnlockController entryUnlockerInstance, ref bool inCustomCampaign)
        {
            // Update ID if not given.
            if (newID == -1 && !replaceEntry && !inCustomCampaign)
            {
                // Get the max Monster ID.
                int maxEntryIDMainCampaign = GetNewEntryID(entryUnlockerInstance);
                int maxEntryIDMainDLC = GetNewEntryID(entryUnlockerInstance, 1);

                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Entries in Main Campaign: {maxEntryIDMainCampaign} and entries in DLC: {maxEntryIDMainDLC}.");            
                #endif
                
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

            // In custom campaign we first get our main game IDs and then add the offset by the size of the custom campaign sizes.
            if (newID == -1 && !replaceEntry && inCustomCampaign)
            {
                int tempID = GetNewEntryID(entryUnlockerInstance);

                // We add our amountExtra and increment it for the next extra.
                tempID += ParseJSONFiles.amountExtra;
                ParseJSONFiles.amountExtra++;

                newID = tempID;
            }

            newExtra.ID = newID;

            MelonLogger.Msg($"INFO: Defaulting to a new Monster ID {newExtra.ID} for file in {jsonFolderPath}.");
            MelonLogger.Msg("(This intended and recommended way of providing the ID.)");
        }
    }
}