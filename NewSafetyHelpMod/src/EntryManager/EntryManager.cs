using System;
using NewSafetyHelp.EntryManager.EntryUnlocker;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.EntryManager
{
    public static class EntryManager
    {
        /// <summary>
        /// Function for adding a monster/entity to the entries. Please note, if you need a specific MonsterProfile list, use the function getters.
        /// Please note, that if the MonsterID is already present, it will replace it instead. This helps avoid duplicated versions being added.
        /// </summary>
        /// <param name="newProfile"> The new entry to add. </param>
        /// <param name="monsterProfiles"> Array of entry profiles. </param>
        /// <param name="profileName"> Name of the profile to be added, used for debugging. </param>
        /// <param name="isPermissionAdd"> If the current add is being added to a tier / permission array and not a normal add. </param>
        public static void AddEntryToTheProfile(MonsterProfile newProfile, ref MonsterProfile[] monsterProfiles,
            string profileName, bool isPermissionAdd = false)
        {
            if (monsterProfiles == null) // Empty MonsterProfile array, so we create a new one.
            {
                monsterProfiles = new[] { newProfile }; // MonsterProfile array
            }
            else
            {
                if (profileName != "NO_PRINT")
                {
                    LoggingHelper.DebugLog(() => 
                        $"Adding (New Name: {newProfile.monsterName}, " +
                        $"New ID: {newProfile.monsterID}) to profile: {profileName}.",
                        LoggingHelper.LoggingCategory.ENTRY);
                }

                // Before adding we check if the ID already exists. And if yes, we replace it.
                int idToCheck = newProfile.monsterID;

                if (profileName != "NO_PRINT")
                {
                    LoggingHelper.DebugLog(
                        $"Checking IDs with monster profile array of size {monsterProfiles.Length}.",
                        LoggingHelper.LoggingCategory.ENTRY);
                }

                // Check if it is a duplicate. Not done for permission adds.
                if (!isPermissionAdd && monsterProfiles.Length > 0 && idToCheck != -1)
                {
                    for (int i = 0; i < monsterProfiles.Length; i++)
                    {
                        if (monsterProfiles[i].monsterID == idToCheck) // Duplicate
                        {
                            // Not display it if we are just readding things that are more than welcome to replace entries.
                            if (profileName != "NONE" 
                                && profileName != "NO_PRINT") 
                            {
                                LoggingHelper.WarningLog(
                                    $"An existing entry was overriden (Old Name: {monsterProfiles[i].name}, " +
                                    $"Old ID: {monsterProfiles[i].monsterID})" +
                                    $"(New Name: {newProfile.monsterName}, " +
                                    $"New ID: {newProfile.monsterID}).\n " +
                                    "If this was intentional, you can safely ignore it.");
                            }

                            monsterProfiles[i] = newProfile;
                            return; // Replaced the profile and we return.
                        }
                    }
                }

                // Create a new array with an extra slot
                MonsterProfile[] newArray = new MonsterProfile[monsterProfiles.Length + 1];

                // Copy existing profiles
                for (int i = 0; i < monsterProfiles.Length; i++)
                {
                    newArray[i] = monsterProfiles[i];
                }

                // Add the new profile
                newArray[newArray.Length - 1] = newProfile;

                // Replace the old array
                monsterProfiles = newArray;

                // Add the new entry to the entry fixer
                switch (profileName)
                {
                    case "firstTierUnlocks":
                        EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierOne.Add(newProfile);
                        break;

                    case "secondTierUnlocks":
                        EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierTwo.Add(newProfile);
                        break;

                    case "thirdTierUnlocks":
                        EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierThree.Add(newProfile);
                        break;

                    case "fourthTierUnlocks":
                        EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierFour.Add(newProfile);
                        break;

                    case "fifthTierUnlocks":
                        EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierFive.Add(newProfile);
                        break;

                    case "sixthTierUnlocks":
                        EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierSix.Add(newProfile);
                        break;
                }
            }
        }

        /// <summary>
        /// Deletes an entry that was provided from the list.
        /// </summary>
        /// <param name="monsterProfiles"> Array of entry profiles. </param>
        /// <param name="profileToDelete"> Entry to delete. </param>
        /// <param name="entryName"> Entry to delete. (Search via name instead) </param>
        public static void DeleteEntryProfile(ref MonsterProfile[] monsterProfiles,
            MonsterProfile profileToDelete = null, string entryName = "NOT_PROVIDED")
        {
            if (monsterProfiles == null) // Empty MonsterProfile array, we skip.
            {
                LoggingHelper.WarningLog("Profile to be deleted was not found! Empty entry.");
            }
            else
            {
                // Check if it exists and find the index of that entry.
                int monsterProfileIndex;

                if (profileToDelete != null)
                {
                    monsterProfileIndex = Array.FindIndex(monsterProfiles, p => p == profileToDelete);
                }
                else if (entryName != "NOT_PROVIDED")
                {
                    monsterProfileIndex = Array.FindIndex(monsterProfiles, p => p.monsterName == entryName);
                }
                else
                {
                    LoggingHelper.WarningLog("No name and no profile provided. Unable of deleting.");
                    return;
                }

                if (monsterProfileIndex < 0) // Not found.
                {
                    LoggingHelper.WarningLog("Profile to be deleted was not found! Unknown entry.");
                    return;
                }

                // Create a new array with one less entry.
                MonsterProfile[] newArray = new MonsterProfile[monsterProfiles.Length - 1];

                // Copy existing profiles 

                // Copy: Before the index.
                for (int i = 0; i < monsterProfileIndex; i++)
                {
                    newArray[i] = monsterProfiles[i];
                }

                // Copy: After the index.
                for (int i = monsterProfileIndex + 1; i < monsterProfiles.Length; i++)
                {
                    newArray[i - 1] = monsterProfiles[i];
                }

                // Replace the old array
                monsterProfiles = newArray;
            }
        }

        /// <summary>
        /// Creates a new entry (monster) with the given parameters and returns it.
        /// </summary>
        /// <param name="entryName"> Name of the entry to show. </param>
        /// <param name="entryDescription"> Description of the entry, see examples to understand formatting. ("<b>Works</b>") </param>
        /// <param name="entryID"> ID of the entry, if provided an already existing, it will replace it.
        /// To make sure no duplicate exist it is best to use the length of the entries size. </param>
        /// <param name="entryPortrait"> Sprite image of the entry to show. </param>
        /// <param name="entryAudioClip"> RichAudioClip to play the entries sound. Use the provided function for creating a rich audio clip. </param>
        /// <param name="arcadeCalls"> Array of strings that contain different types of calls for the entry in arcade mode. </param>
        /// <param name="spiderPhobia"> If to hide the image from people afraid of spiders. (PLEASE MARK IT IF THE ENTRY HAS IT) </param>
        /// <param name="darknessPhobia"> If to hide the image it from people afraid of the dark. </param>
        /// <param name="dogPhobia"> If to hide the image it from people afraid of dogs. </param>
        /// <param name="holesPhobia"> If to hide the image from people afraid of many holes. </param>
        /// <param name="insectPhobia"> If to hide the image from people afraid of insects. </param>
        /// <param name="watchingPhobia"> If to hide the image from people afraid of being watched. </param>
        /// <param name="tightSpacePhobia"> If to hide the image from people afraid of tight spaces. </param>
        public static MonsterProfile CreateEntry(string entryName = "NO_NAME",
            string entryDescription = "NO_DESCRIPTION", int entryID = -1, Sprite entryPortrait = null,
            RichAudioClip entryAudioClip = null, String[] arcadeCalls = null, bool spiderPhobia = false,
            bool darknessPhobia = false, bool dogPhobia = false, bool holesPhobia = false,
            bool insectPhobia = false, bool watchingPhobia = false, bool tightSpacePhobia = false)
        {
            MonsterProfile newMonster = ScriptableObject.CreateInstance<MonsterProfile>();
            newMonster.name = entryName; // Set the name for the inspector

            newMonster.monsterName = entryName;
            newMonster.monsterDescription = entryDescription;
            newMonster.monsterID = entryID;
            newMonster.monsterPortrait = entryPortrait;
            newMonster.monsterAudioClip = entryAudioClip;

            // Phobias (Hides the image when selected)
            newMonster.spider = spiderPhobia;
            newMonster.dark = darknessPhobia;
            newMonster.dog = dogPhobia;
            newMonster.holes = holesPhobia;
            newMonster.insect = insectPhobia;
            newMonster.watching = watchingPhobia;
            newMonster.tightSpace = tightSpacePhobia;

            // Arcade Calls (Which do not have a voice-over)
            // Must be done correctly or else it will fail.
            newMonster.arcadeCalls = arcadeCalls; 

            return newMonster;
        }

        /// <summary>
        /// Replaces a single Monster Image with a given sprite. (It is more for testing than rather actually changing something.)
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace the Sprite with. </param>
        /// <param name="entryName"> Name of the entry to find. </param>
        /// <param name="entryImage"> Sprite to insert into the entry. </param>
        /// <param name="entryID"> Alternative way of finding the entry. </param>
        public static void ReplaceEntryImage(ref MonsterProfile[] monsterProfiles, string entryName,
            Sprite entryImage, int entryID = -1)
        {
            foreach (MonsterProfile entryProfile in monsterProfiles)
            {
                if (entryProfile.monsterName == entryName || (entryProfile.monsterID == entryID && entryID >= 0))
                {
                    entryProfile.monsterPortrait = entryImage;
                }
            }
        }

        /// <summary>
        /// Finds an Entry by name or ID and returns a reference to it.
        /// It returns the first find, to avoid any issues you can search via ID.
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace find the entry in. </param>
        /// <param name="entryName"> Name of the entry to find. </param>
        /// <param name="entryID"> Alternative way of finding the entry. </param>
        public static MonsterProfile FindEntry(ref MonsterProfile[] monsterProfiles,
            string entryName = "SKIP_MONSTER_NAME_TO_SEARCH", int entryID = -1)
        {
            foreach (MonsterProfile entryProfile in monsterProfiles)
            {
                if ((entryProfile.monsterName == entryName && entryName != "SKIP_MONSTER_NAME_TO_SEARCH") ||
                    (entryProfile.monsterID == entryID && entryID >= 0))
                {
                    // This seems to be a reference.
                    return entryProfile;
                }
            }

            // Nothing found, so we return null. 
            return null;
        }

        /// <summary>
        /// Finds the Entry and replaces it.
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace find the entry in. </param>
        /// <param name="entryName"> Name of the entry to find. </param>
        /// <param name="replaceProfile"> Entry to replace the original with </param>
        /// <param name="entryID"> Alternative way of finding the entry. </param>
        public static void ReplaceEntry(ref MonsterProfile[] monsterProfiles, string entryName,
            MonsterProfile replaceProfile, int entryID = -1)
        {
            for (int i = 0; i < monsterProfiles.Length; i++)
            {
                if (monsterProfiles[i].monsterName == entryName ||
                    (monsterProfiles[i].monsterID == entryID && entryID >= 0))
                {
                    monsterProfiles[i] = replaceProfile;
                }
            }
        }

        /// <summary>
        /// Function sorting all the monsterProfiles by alphabetical order.
        /// </summary>
        /// <param name="monsterProfiles"> Array of monster profiles. </param>
        public static void SortEntryProfiles(ref MonsterProfile[] monsterProfiles)
        {
            Array.Sort(monsterProfiles,
                (x, y) => 
                    String.Compare(x.monsterName, y.monsterName, StringComparison.InvariantCulture));
        }
    }
}