using MelonLoader;
using System;
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
        public static void AddMonsterToTheProfile(ref MonsterProfile newProfile, ref MonsterProfile[] monsterProfiles)
        {
            if (monsterProfiles == null) // Empty MonsterProfile array, so we create a new one.
            {
                monsterProfiles = new MonsterProfile[] { newProfile };
            }
            else
            {
                // Before adding we check if the ID already exists.
                int idToCheck = newProfile.monsterID;

                for (int i = 0; i < monsterProfiles.Length; i++)
                {
                    if (monsterProfiles[i].monsterID == idToCheck) // Duplicate
                    {
                        MelonLogger.Warning("Warning: An existing entry was overriden. If this was intentional, you can safely ignore it.");
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
        /// Returns a new ID that should be +1 from the largest.
        /// </summary>
        /// <param name="entryUnlocker"> Instance of the entryunlockercontroller </param>
        /// <param name="type"> Type of entry type. (0 = monsterProfiles default, 1 = allXmasEntries DLC) </param>
        public static int getlargerID(EntryUnlockController entryUnlocker, int type = 0)
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
            Array.Sort(monsterProfiles, (x, y) => String.Compare(x.name, y.name));
        }
    }
}
