using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;

namespace NewSafetyHelp.EntryManager.EntryUnlocker
{
    public static class EntryUnlockerPatcher
    {
        // Patches the entry unlocker to readd the missing entries to the different permission tiers that get lost upon reloading the computer scene.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "Awake", new Type[] { })]
    public static class FixPermissionOverride
    {
        // List of entire permissions
        public static List<MonsterProfile> EntriesReaddTierOne = new List<MonsterProfile>();
        public static List<MonsterProfile> EntriesReaddTierTwo = new List<MonsterProfile>();
        public static List<MonsterProfile> EntriesReaddTierThree = new List<MonsterProfile>();
        public static List<MonsterProfile> EntriesReaddTierFour = new List<MonsterProfile>();
        public static List<MonsterProfile> EntriesReaddTierFive = new List<MonsterProfile>();
        public static List<MonsterProfile> EntriesReaddTierSix = new List<MonsterProfile>();

        /// <summary>
        /// Re adds entries that were removed upon reload of the EntryUnlocker Object.
        /// </summary>
        /// <param name="__instance"> Caller of function instance </param>
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(EntryUnlockController __instance)
        {
            // I am aware there are more beautiful ways of achieving this. However, I am going to do it like the game.

            #if DEBUG
                MelonLogger.Msg("DEBUG: If tier/permission levels for extra entries were lost, they will now be readded.");
            #endif

            for (int i = 0; i < EntriesReaddTierOne.Count; i++)
            {
                if (!__instance.firstTierUnlocks.monsterProfiles.Contains(EntriesReaddTierOne[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(EntriesReaddTierOne[i], ref __instance.firstTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < EntriesReaddTierTwo.Count; i++)
            {
                if (!__instance.secondTierUnlocks.monsterProfiles.Contains(EntriesReaddTierTwo[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(EntriesReaddTierTwo[i], ref __instance.secondTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < EntriesReaddTierThree.Count; i++)
            {
                if (!__instance.thirdTierUnlocks.monsterProfiles.Contains(EntriesReaddTierThree[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(EntriesReaddTierThree[i], ref __instance.thirdTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < EntriesReaddTierFour.Count; i++)
            {
                if (!__instance.fourthTierUnlocks.monsterProfiles.Contains(EntriesReaddTierFour[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(EntriesReaddTierFour[i], ref __instance.fourthTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < EntriesReaddTierFive.Count; i++)
            {
                if (!__instance.fifthTierUnlocks.monsterProfiles.Contains(EntriesReaddTierFive[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(EntriesReaddTierFive[i], ref __instance.fifthTierUnlocks.monsterProfiles, "NONE", true);
                }
            }

            for (int i = 0; i < EntriesReaddTierSix.Count; i++)
            {
                if (!__instance.sixthTierUnlocks.monsterProfiles.Contains(EntriesReaddTierSix[i])) // Avoid duplicate adding.
                {
                    EntryManager.AddMonsterToTheProfile(EntriesReaddTierSix[i], ref __instance.sixthTierUnlocks.monsterProfiles, "NONE", true);
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
        /// <param name="profileToCheck"> Profile to check. </param>
        /// <param name="__result"> Result if to show it or not. </param>
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(ref MonsterProfile profileToCheck, ref bool __result)
        {
            // I am aware there are more beautiful ways of achieving this. However, I am going to do it like the game.
            
            // Custom Campaign to reset default entry permission.
            if (CustomCampaignGlobal.InCustomCampaign)
            {
                if (profileToCheck == null)
                {
                    MelonLogger.Error("ERROR: Profile to check is empty!");
                    return;
                }
                
                CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();
                
                if (customCampaign == null)
                {
                    MelonLogger.Error("ERROR: No active custom campaign!");
                    return;
                }

                if (!customCampaign.removeExistingEntries && customCampaign.resetDefaultEntriesPermission)
                {
                    if (MainClassForMonsterEntries.CopyMonsterProfiles != null)
                    {
                        // We have the copies. So we can check if to enable it.
                        if (MainClassForMonsterEntries.CopyMonsterProfiles.Contains(profileToCheck)) // A default entry to show. We return true.
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
    }
}