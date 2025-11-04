using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;

namespace NewSafetyHelp.EntryManager.EntryUnlocker
{
    public static class EntryUnlockerPatcher
    {
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
    }
}