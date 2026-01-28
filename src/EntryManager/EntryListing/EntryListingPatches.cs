using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.EntryManager.EntryData;

namespace NewSafetyHelp.EntryManager.EntryListing
{
    public static class EntryListingPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(EntryListingBehavior), "ShowEntryInfo", new Type[] { })]
        public static class ShowEntryInfoPatch
        {
            /// <summary>
            /// Postfixes the show entry info to not show "NEW" on main campaign entries if in a custom campaign.
            /// </summary>
            /// <param name="__instance"> Caller of function instance </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(EntryListingBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: No active custom campaign!");
                        return;
                    }

                    if (!customCampaign.RemoveExistingEntries && customCampaign.ResetDefaultEntriesPermission &&
                        !customCampaign.DoShowNewTagForMainGameEntries) // If allowed to hide the name, we do it. 
                    {
                        if (MainClassForMonsterEntries.CopyMonsterProfiles
                            .Contains(__instance.myProfile)) // Contained in main campaign.
                        {
                            FieldInfo hasClickedField = typeof(EntryListingBehavior).GetField("hasClicked",
                                BindingFlags.NonPublic | BindingFlags.Instance);

                            if (hasClickedField == null)
                            {
                                MelonLogger.Error("ERROR: HasClicked Method is null! Unable of setting as viewed!");
                            }
                            else
                            {
                                hasClickedField.SetValue(__instance, true);
                            }

                            // Set name to normal.
                            __instance.myText.text = __instance.myProfile.monsterName;
                        }
                    }
                }
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(EntryListingBehavior), "DelayedStart")]
        public static class DelayedStartPatch
        {
            /// <summary>
            /// Changes the DelayedStart function to consider custom campaign entries.
            /// </summary>
            /// <param name="__instance"> Caller of function instance </param>
            /// <param name="__result"> Caller of function instance </param>
            private static bool Prefix(EntryListingBehavior __instance,
                [UsedImplicitly] ref IEnumerator __result)
            {
                __result = DelayedStartCoroutine(__instance);

                return false; // Skip original function.
            }

            private static IEnumerator DelayedStartCoroutine(EntryListingBehavior __instance)
            {
                yield return null;

                FieldInfo hasClicked = typeof(EntryListingBehavior).GetField("hasClicked", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo determineLocked = typeof(EntryListingBehavior).GetMethod("DetermineLocked", BindingFlags.NonPublic | BindingFlags.Instance);

                if (hasClicked == null || determineLocked == null)
                {
                    MelonLogger.Error("CRITICAL ERROR: hasClicked and/or DetermineLocked could not be found and are null.");
                    yield break;
                }

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Game
                {
                    if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile) 
                        && GlobalVariables.currentDay >= GlobalVariables.entryUnlockScript.currentTier + 1)
                    {
                        hasClicked.SetValue(__instance, true); // __instance.hasClicked = true;
                    }
                
                    determineLocked.Invoke(__instance, null); // __instance.DetermineLocked();
                }
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("CRITICAL ERROR: Custom Campaign is active but no campaign was found.");
                        yield break;
                    }

                    EntryMetadata entryFound = CustomCampaignGlobal.GetEntryFromActiveCampaign(__instance.myProfile.monsterName);

                    if (entryFound != null)
                    {
                        // If the current entry is unlocked.
                        if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile))
                        {
                            // Our permission tier is 1 larger than the current permission.
                            if (GlobalVariables.entryUnlockScript.currentTier - 1 > entryFound.permissionLevel)
                            {
                                hasClicked.SetValue(__instance, true);
                            }

                            // If our current day is one later than the out tier + 1.
                            // Example: Our current day is "1",
                            // our current tier came from the previous day, which is "0".
                            // As such, our current day is equal 1, and as such, we hide the NEW tag.
                            if (GlobalVariables.currentDay >= GlobalVariables.entryUnlockScript.currentTier + 1)
                            {
                                hasClicked.SetValue(__instance, true);
                            }
                        }
                        
                        #if DEBUG
                        /*MelonLogger.Msg(ConsoleColor.Blue, "DEBUG: " +
                                                           $"Entry Name: '{__instance.myProfile.monsterName}'. " +
                                                           $"Is Unlocked? '{GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile)}'. " +
                                                           "Current Tier - 1 > Entry Permission Level : " +
                                                           $"'{GlobalVariables.entryUnlockScript.currentTier}' - 1 > '{entryFound.permissionLevel}' = '{GlobalVariables.entryUnlockScript.currentTier - 1 > entryFound.permissionLevel}'.");
                        */
                        #endif
                    }
                    else // Main Campaign Entries, for now we just default.
                    {
                        if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile) 
                            && GlobalVariables.currentDay >= GlobalVariables.entryUnlockScript.currentTier + 1)
                        {
                            hasClicked.SetValue(__instance, true); // __instance.hasClicked = true;
                        }
                    }
                    
                    determineLocked.Invoke(__instance, null);
                }
            }
        }
        
    }
}