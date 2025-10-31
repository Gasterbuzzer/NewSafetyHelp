using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using UnityEngine;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.EntryManager
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
        /// <param name="isPermissionAdd"> If the current add is being added to a tier / permission array and not a normal add. </param>
        public static void AddMonsterToTheProfile(MonsterProfile newProfile, ref MonsterProfile[] monsterProfiles, string profileName, bool isPermissionAdd = false)
        {
            if (monsterProfiles == null) // Empty MonsterProfile array, so we create a new one.
            {
                monsterProfiles = new MonsterProfile[] { newProfile };
            }
            else
            {
                #if DEBUG
                    if (profileName != "NO_PRINT")
                    {
                        MelonLogger.Msg($"DEBUG: Adding (New Name: {newProfile.monsterName}, New ID: {newProfile.monsterID}) to profile: {profileName}.");
                    }
                #endif

                // Before adding we check if the ID already exists. And if yes, we replace it.
                int idToCheck = newProfile.monsterID;

                #if DEBUG
                    if (profileName != "NO_PRINT")
                    {
                        MelonLogger.Msg($"DEBUG: Checking IDS with monster profile array of size {monsterProfiles.Length}.");
                    }
                #endif
    
                // Check if it is a duplicate. Not done for permission adds.
                if (!isPermissionAdd && monsterProfiles.Length > 0 && idToCheck != -1)
                {
                    for (int i = 0; i < monsterProfiles.Length; i++)
                    {
                        if (monsterProfiles[i].monsterID == idToCheck) // Duplicate
                        {
                            if (profileName != "NONE" && profileName != "NO_PRINT") // Not display it if we are just readding things that are more than welcome to replace entries.
                            {
                                MelonLogger.Warning($"WARNING: An existing entry was overriden (Old Name: {monsterProfiles[i].name}, Old ID: {monsterProfiles[i].monsterID}) (New Name: {newProfile.monsterName}, New ID: {newProfile.monsterID}).\n If this was intentional, you can safely ignore it.");
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
        /// Deletes an entry that was provided from the list.
        /// </summary>
        /// <param name="monsterProfiles"> Array of monster profiles. </param>
        /// <param name="profileToDelete"> Entry to delete. </param>
        /// <param name="monsterName"> Entry to delete. (Search via name instead) </param>
        public static void DeleteMonsterProfile(ref MonsterProfile[] monsterProfiles, MonsterProfile profileToDelete = null, string monsterName = "NOT_PROVIDED")
        {
            if (monsterProfiles == null) // Empty MonsterProfile array, we skip.
            {
                MelonLogger.Warning("WARNING: Profile to be deleted was not found! Empty entry.");
            }
            else
            {
                // Check if it exists and find the index of that entry.
                int monsterProfileIndex;
                
                if (profileToDelete != null)
                {
                    monsterProfileIndex = Array.FindIndex(monsterProfiles, p => p == profileToDelete);
                }
                else if (monsterName != "NOT_PROVIDED")
                {
                    monsterProfileIndex = Array.FindIndex(monsterProfiles, p => p.monsterName == monsterName);
                }
                else
                {
                    MelonLogger.Warning("WARNING: No name and no profile provided. Unable of deleting.");
                    return;
                }
                
                if (monsterProfileIndex < 0) // Not found.
                {
                    MelonLogger.Warning("WARNING: Profile to be deleted was not found! Unknown entry.");
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

            // Arcade Calls (Which do not have a voice-over)
            newMonster.arcadeCalls = _arcadeCalls; // Must be done correctly or else it will fail.

            return newMonster;
        }

        /// <summary>
        /// Replaces a single Monster Image with a given sprite. (It is more for testing than rather actually changing something.)
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace the Sprite with. </param>
        /// <param name="monsterName"> Name of the entry to find. </param>
        /// <param name="entryImage"> Sprite to insert into the entry. </param>
        /// <param name="monsterID"> Alternative way of finding the entry. </param>
        public static void ReplaceMonsterImage(ref MonsterProfile[] monsterProfiles, string monsterName, Sprite entryImage, int monsterID = -1)
        {
            foreach (MonsterProfile entryProfile in monsterProfiles)
            {
                if (entryProfile.monsterName == monsterName || (entryProfile.monsterID == monsterID && monsterID >= 0))
                {
                    entryProfile.monsterPortrait = entryImage;
                }
            }
        }

        /// <summary>
        /// Finds an Entry by name or ID and returns a reference to it. It returns the first find, to avoid any issues you can search via ID.
        /// </summary>
        /// <param name="monsterProfiles"> Reference of the monsterProfile to replace find the entry in. </param>
        /// <param name="monsterName"> Name of the entry to find. </param>
        /// <param name="monsterID"> Alternative way of finding the entry. </param>
        public static MonsterProfile FindEntry(ref MonsterProfile[] monsterProfiles, string monsterName = "SKIP_MONSTER_NAME_TO_SEARCH", int monsterID = -1)
        {
            foreach (MonsterProfile entryProfile in monsterProfiles)
            {
                if ((entryProfile.monsterName == monsterName && monsterName != "SKIP_MONSTER_NAME_TO_SEARCH") || (entryProfile.monsterID == monsterID && monsterID >= 0))
                {
                    return entryProfile; // Correction, this seems to be a real reference     OLD: --Please note, this is a copy.--
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
        /// <param name="entryUnlocker"> Instance of the EntryUnlockerController </param>
        /// <param name="type"> Type of entry type. (0 = monsterProfiles default, 1 = allXmasEntries DLC) </param>
        public static int GetNewEntryID(EntryUnlockController entryUnlocker, int type = 0)
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
        /// Function sorting all the monsterProfiles by alphabetical order.
        /// </summary>
        /// <param name="monsterProfiles"> Array of monster profiles. </param>
        public static void SortMonsterProfiles(ref MonsterProfile[] monsterProfiles)
        {
            Array.Sort(monsterProfiles, (x, y) => String.Compare(x.monsterName, y.monsterName, StringComparison.InvariantCulture));
        }
    }

    // Patches the entry unlocker to readd the missing entries to the different permission tiers that get lost upon reloading the computer scene.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "Awake", new Type[] { })]
    public static class FixPermissionOverride
    {
        // List of entire permissions
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
            // I am aware there are more beautiful ways of achieving this. However, I am going to do it like the game.

            #if DEBUG
                MelonLogger.Msg("DEBUG: If tier/permission levels for extra entries were lost, they will now be readded.");
            #endif

            for (int i = 0; i < entriesReaddTierOne.Count; i++)
            {
                if (!__instance.firstTierUnlocks.monsterProfiles.Contains(entriesReaddTierOne[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierOne[i], ref __instance.firstTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < entriesReaddTierTwo.Count; i++)
            {
                if (!__instance.secondTierUnlocks.monsterProfiles.Contains(entriesReaddTierTwo[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierTwo[i], ref __instance.secondTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < entriesReaddTierThree.Count; i++)
            {
                if (!__instance.thirdTierUnlocks.monsterProfiles.Contains(entriesReaddTierThree[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierThree[i], ref __instance.thirdTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < entriesReaddTierFour.Count; i++)
            {
                if (!__instance.fourthTierUnlocks.monsterProfiles.Contains(entriesReaddTierFour[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierFour[i], ref __instance.fourthTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < entriesReaddTierFive.Count; i++)
            {
                if (!__instance.fifthTierUnlocks.monsterProfiles.Contains(entriesReaddTierFive[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierFive[i], ref __instance.fifthTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < entriesReaddTierSix.Count; i++)
            {
                if (!__instance.sixthTierUnlocks.monsterProfiles.Contains(entriesReaddTierSix[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(entriesReaddTierSix[i], ref __instance.sixthTierUnlocks.monsterProfiles, "NONE", true);
                }
            }
        }
    }
    
    // Patches the entry unlocker to accept all default entries in custom campaign if the option was provided.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "CheckMonsterIsUnlocked", typeof(MonsterProfile))]
    public static class CheckMonsterIsUnlockedPatch
    {
        /// <summary>
        /// If the current custom campaign is active, and we have the option enabled to show all default at 0 permission tier. We show them.
        /// </summary>
        /// <param name="__originalMethod"> Method Caller </param>
        /// <param name="__instance"> Caller of function instance </param>
        /// <param name="profileToCheck"> Profile to check. </param>
        /// <param name="__result"> Result if to show it or not. </param>
        private static void Postfix(MethodBase __originalMethod, EntryUnlockController __instance, ref MonsterProfile profileToCheck, ref bool __result)
        {
            // I am aware there are more beautiful ways of achieving this. However, I am going to do it like the game.
            
            // Custom Campaign to reset default entry permission.
            if (CustomCampaignGlobal.inCustomCampaign)
            {
                if (profileToCheck == null)
                {
                    MelonLogger.Error("ERROR: Profile to check is empty!");
                    return;
                }
                
                CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();
                
                if (customCampaign == null)
                {
                    MelonLogger.Error("ERROR: No active custom campaign!");
                    return;
                }

                if (!customCampaign.removeExistingEntries && customCampaign.resetDefaultEntriesPermission)
                {
                    if (MainClassForMonsterEntries.copyMonsterProfiles != null)
                    {
                        // We have the copies. So we can check if to enable it.
                        if (MainClassForMonsterEntries.copyMonsterProfiles.Contains(profileToCheck)) // A default entry to show. We return true.
                        {
                            __result = true;
                        }
                    }
                    else
                    {
                        MelonLogger.Error("ERROR: Copy of entry profiles does not exist! Possibly called before initialization.");
                    }
                }
            }
        }
    }
    
    [HarmonyLib.HarmonyPatch(typeof(EntryListingBehavior), "ShowEntryInfo", new Type[] { })]
    public static class ShowEntryInfoPatch
    {
        /// <summary>
        /// Postfixes the show entry info to not show "NEW" on main campaign entries if in a custom campaign.
        /// </summary>
        /// <param name="__originalMethod"> Method Caller </param>
        /// <param name="__instance"> Caller of function instance </param>
        private static void Postfix(MethodBase __originalMethod, EntryListingBehavior __instance)
        {
            if (CustomCampaignGlobal.inCustomCampaign)
            {
                CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                if (customCampaign == null)
                {
                    MelonLogger.Error("ERROR: No active custom campaign!");
                    return;
                }

                if (!customCampaign.removeExistingEntries && customCampaign.resetDefaultEntriesPermission && !customCampaign.doShowNewTagForMainGameEntries) // If allowed to hide the name, we do it. 
                {
                    if (MainClassForMonsterEntries.copyMonsterProfiles.Contains(__instance.myProfile)) // Contained in main campaign.
                    {
                        __instance.myText.text = __instance.myProfile.monsterName;
                    }   
                }
            }
        }
    }
}
