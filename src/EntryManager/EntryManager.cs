using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NewSafetyHelp.src.EntryManager
{
    public static class EntryManager
    {
        /// <summary>
        /// Function for adding a monster/entity to the entries. Please note, if you need a specific MonsterProfile list, use the function getters.
        /// Please note, that if the MonsterID is already present, it will replace it instead. This helps avoid duplicated versions being added.
        /// </summary>
        /// <param name="newProfile"> The new monster to add. </param>
        /// <param name="monsterProfiles"> Array of monster profiles. </param>
        /// <param name="profileName"> Name of the profile to be added, used for debugging. </param>
        public static void AddMonsterToTheProfile(MonsterProfile newProfile, ref MonsterProfile[] monsterProfiles, string profileName)
        {
            if (monsterProfiles == null) // Empty MonsterProfile array, so we create a new one.
            {
                monsterProfiles = new MonsterProfile[] { newProfile };
            }
            else
            {
                MelonLogger.Msg($"DEBUG: Adding (New Name: {newProfile.monsterName}, New ID: {newProfile.monsterID}) to profile: {profileName}.");

                // Before adding we check if the ID already exists.
                int idToCheck = newProfile.monsterID;

                for (int i = 0; i < monsterProfiles.Length; i++)
                {
                    if (monsterProfiles[i].monsterID == idToCheck) // Duplicate
                    {
                        if (profileName != "NONE") // Not display it if we are just readding things that are more than welcome to replace entries.
                        {
                            MelonLogger.Warning($"WARNING: An existing entry was overriden (Old Name: {monsterProfiles[i].name}, Old ID: {monsterProfiles[i].monsterID}) (New Name: {newProfile.monsterName}, New ID: {newProfile.monsterID}).\n If this was intentional, you can safely ignore it.");
                        }

                        monsterProfiles[i] = newProfile;
                        return; // Replaced the profile and we return.
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
                        FixPermissionOverride.entriesReaddTierOne.Add(newProfile);
                        break;

                    case "secondTierUnlocks":
                        FixPermissionOverride.entriesReaddTierTwo.Add(newProfile);
                        break;

                    case "thirdTierUnlocks":
                        FixPermissionOverride.entriesReaddTierThree.Add(newProfile);
                        break;

                    case "fourthTierUnlocks":
                        FixPermissionOverride.entriesReaddTierFour.Add(newProfile);
                        break;

                    case "fifthTierUnlocks":
                        FixPermissionOverride.entriesReaddTierFive.Add(newProfile);
                        break;

                    case "sixthTierUnlocks":
                        FixPermissionOverride.entriesReaddTierSix.Add(newProfile);
                        break;

                }
                
            }
        }

        /// <summary>
        /// Creates a new Monster with the given parameters and returns it.
        /// </summary>
        /// <param name="_monsterName"> Name of the monster to show. </param>
        /// <param name="_monsterDescription"> Description of the monster, see examples to understand formatting. ("<b>Works</b>") </param>
        /// <param name="_monsterID"> ID of the monster, if provided an already existing, it will replace it. To make sure no duplicate exist it is best to use the length of the monsters size. </param>
        /// <param name="_monsterPortrait"> Sprite image of the monster to show. </param>
        /// <param name="_monsterAudioClip"> RichAudioClip to play the monsters sound. Use the provided function for creating a rich audio clip. </param>
        /// <param name="_arcadeCalls"> Array of strings that contain different types of calls for the monster in arcade mode. </param>
        /// <param name="_spiderPhobia"> If to hide the image from people afraid of spiders. (PLEASE MARK IT IF THE ENTRY HAS IT) </param>
        /// <param name="_darknessPhobia"> If to hide the image it from people afraid of the dark. </param>
        /// <param name="_dogPhobia"> If to hide the image it from people afraid of dogs. </param>
        /// <param name="_holesPhobia"> If to hide the image from people afraid of many holes. </param>
        /// <param name="_insectPhobia"> If to hide the image from people afraid of insects. </param>
        /// <param name="_watchingPhobia"> If to hide the image from people afraid of being watched. </param>
        /// <param name="_tightSpacePhobia"> If to hide the image from people afraid of tight spaces. </param>
        public static MonsterProfile CreateMonster(string _monsterName = "NO_NAME", string _monsterDescription = "NO_DESCRIPTION", int _monsterID = -1, Sprite _monsterPortrait = null, RichAudioClip _monsterAudioClip = null, String[] _arcadeCalls = null, bool _spiderPhobia = false, bool _darknessPhobia = false, bool _dogPhobia = false, bool _holesPhobia = false, bool _insectPhobia = false, bool _watchingPhobia = false, bool _tightSpacePhobia = false)
        {

            MonsterProfile newMonster = ScriptableObject.CreateInstance<MonsterProfile>();
            newMonster.name = _monsterName; // Set the name for the inspector

            newMonster.monsterName = _monsterName;
            newMonster.monsterDescription = _monsterDescription;
            newMonster.monsterID = _monsterID;
            newMonster.monsterPortrait = _monsterPortrait;
            newMonster.monsterAudioClip = _monsterAudioClip;

            // Phobias (Hides the image when selected)
            newMonster.spider = _spiderPhobia;
            newMonster.dark = _darknessPhobia;
            newMonster.dog = _dogPhobia;
            newMonster.holes = _holesPhobia;
            newMonster.insect = _insectPhobia;
            newMonster.watching = _watchingPhobia;
            newMonster.tightSpace = _tightSpacePhobia;

            // Arcade Calls (Which do not have a voice over)
            newMonster.arcadeCalls = _arcadeCalls; // Must be done correctly or else it will fail.

            return newMonster;
        }

        /// <summary>
        /// Replaces a single Monster Image with a given sprite. (It is more for testing than rather actually changing something.
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace the Sprite with. </param>
        /// <param name="monsterName"> Name of the entry to find. </param>
        /// <param name="entryImage"> Sprite to insert into the entry. </param>
        /// <param name="monsterID"> Alternative way of finding the entry. </param>
        public static void replaceMonsterImage(ref MonsterProfile[] monsterProfiles, string monsterName, Sprite entryImage, int monsterID = -1)
        {
            for (int i = 0; i < monsterProfiles.Length; i++)
            {
                if (monsterProfiles[i].monsterName == monsterName || (monsterProfiles[i].monsterID == monsterID && monsterID >= 0))
                {
                    monsterProfiles[i].monsterPortrait = entryImage;
                }
            }
        }

        /// <summary>
        /// Finds an Entry by name or ID and returns a reference to it. It returns the first find, to avoid any issues you can search via ID.
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace find the entry in. </param>
        /// <param name="monsterName"> Name of the entry to find. </param>
        /// <param name="monsterID"> Alternative way of finding the entry. </param>
        public static MonsterProfile FindEntry(ref MonsterProfile[] monsterProfiles, string monsterName, int monsterID = -1)
        {
            for (int i = 0; i < monsterProfiles.Length; i++)
            {
                if (monsterProfiles[i].monsterName == monsterName || (monsterProfiles[i].monsterID == monsterID && monsterID >= 0))
                {
                    return monsterProfiles[i]; // Correction, this seems to be a real reference     OLD: --Please note, this is a copy.--
                }
            }

            // Nothing found, so we return null. 
            return null;
        }

        /// <summary>
        /// Finds the Entry and replaces it.
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace find the entry in. </param>
        /// <param name="monsterName"> Name of the entry to find. </param>
        /// <param name="replaceProfile"> Entry to replace the original with </param>
        /// <param name="monsterID"> Alternative way of finding the entry. </param>
        public static void ReplaceEntry(ref MonsterProfile[] monsterProfiles, string monsterName, MonsterProfile replaceProfile, int monsterID = -1)
        {
            for (int i = 0; i < monsterProfiles.Length; i++)
            {
                if (monsterProfiles[i].monsterName == monsterName || (monsterProfiles[i].monsterID == monsterID && monsterID >= 0))
                {
                    monsterProfiles[i] = replaceProfile;
                }
            }
        }

        /// <summary>
        /// Returns a new ID that should be +1 from the largest.
        /// </summary>
        /// <param name="entryUnlocker"> Instance of the entryunlockercontroller </param>
        /// <param name="type"> Type of entry type. (0 = monsterProfiles default, 1 = allXmasEntries DLC) </param>
        public static int getNewEntryID(EntryUnlockController entryUnlocker, int type = 0)
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
        /// Function sorting all of the monsterProfiles by alphabetical order.
        /// </summary>
        /// <param name="monsterProfiles"> Array of monster profiles. </param>
        public static void SortMonsterProfiles(ref MonsterProfile[] monsterProfiles)
        {
            Array.Sort(monsterProfiles, (x, y) => String.Compare(x.monsterName, y.monsterName));
        }
    }

    // Patches the entry unlocker to readd the missing entries to the different permission tiers that get lost upon reloading the computer scene.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "Awake", new Type[] { })]
    public static class FixPermissionOverride
    {
        // List of entrie permissions
        public static List<MonsterProfile> entriesReaddTierOne = new List<MonsterProfile>();
        public static List<MonsterProfile> entriesReaddTierTwo = new List<MonsterProfile>();
        public static List<MonsterProfile> entriesReaddTierThree = new List<MonsterProfile>();
        public static List<MonsterProfile> entriesReaddTierFour = new List<MonsterProfile>();
        public static List<MonsterProfile> entriesReaddTierFive = new List<MonsterProfile>();
        public static List<MonsterProfile> entriesReaddTierSix = new List<MonsterProfile>();

        /// <summary>
        /// Re adds entries that were removed upon reload of the EntryUnlocker Object.
        /// </summary>
        /// <param name="__originalMethod"> Method Caller </param>
        /// <param name="__instance"> Caller of function instance </param>
        private static void Postfix(MethodBase __originalMethod, EntryUnlockController __instance)
        {
            // I am aware there are more beautifull ways of achieving this. However, I am going to do it like the game.

            MelonLogger.Msg("INFO: Now readding permissions for entries again.");

            for (int i = 0; i < entriesReaddTierOne.Count; i++)
            {
                if (!__instance.firstTierUnlocks.monsterProfiles.Contains<MonsterProfile>(entriesReaddTierOne[i]))
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierOne[i], ref __instance.firstTierUnlocks.monsterProfiles, "NONE");
                }
            }

            for (int i = 0; i < entriesReaddTierTwo.Count; i++)
            {
                if (!__instance.secondTierUnlocks.monsterProfiles.Contains<MonsterProfile>(entriesReaddTierOne[i]))
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierTwo[i], ref __instance.secondTierUnlocks.monsterProfiles, "NONE");
                }
            }

            for (int i = 0; i < entriesReaddTierThree.Count; i++)
            {
                if (!__instance.thirdTierUnlocks.monsterProfiles.Contains<MonsterProfile>(entriesReaddTierOne[i]))
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierThree[i], ref __instance.thirdTierUnlocks.monsterProfiles, "NONE");
                }
            }

            for (int i = 0; i < entriesReaddTierFour.Count; i++)
            {
                if (!__instance.fourthTierUnlocks.monsterProfiles.Contains<MonsterProfile>(entriesReaddTierOne[i]))
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierFour[i], ref __instance.fourthTierUnlocks.monsterProfiles, "NONE");
                }
            }

            for (int i = 0; i < entriesReaddTierFive.Count; i++)
            {
                if (!__instance.fifthTierUnlocks.monsterProfiles.Contains<MonsterProfile>(entriesReaddTierOne[i]))
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierFive[i], ref __instance.fifthTierUnlocks.monsterProfiles, "NONE");
                }
            }

            for (int i = 0; i < entriesReaddTierSix.Count; i++)
            {
                if (!__instance.sixthTierUnlocks.monsterProfiles.Contains<MonsterProfile>(entriesReaddTierOne[i]))
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierSix[i], ref __instance.sixthTierUnlocks.monsterProfiles, "NONE");
                }
            }
        }
    }
}
