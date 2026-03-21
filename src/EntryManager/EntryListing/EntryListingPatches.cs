using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.EntryManager.EntryListing
{
    public static class EntryListingPatches
    {
        private static readonly FieldInfo HasClickedField = typeof(EntryListingBehavior).GetField("hasClicked",
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        [HarmonyLib.HarmonyPatch(typeof(EntryListingBehavior), "ShowEntryInfo")]
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
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return;
                    }

                    if (!customCampaign.RemoveExistingEntries && customCampaign.ResetDefaultEntriesPermission &&
                        !customCampaign.DoShowNewTagForMainGameEntries) // If allowed to hide the name, we do it. 
                    {
                        if (MainClassForMonsterEntries.CopyMonsterProfiles
                            .Contains(__instance.myProfile)) // Contained in main campaign.
                        {
                            if (HasClickedField == null)
                            {
                                LoggingHelper.ReflectionError(nameof(HasClickedField));
                            }
                            else
                            {
                                HasClickedField.SetValue(__instance, true);
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
            private static readonly FieldInfo HasClicked =
                typeof(EntryListingBehavior).GetField("hasClicked",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            private static readonly MethodInfo DetermineLocked =
                typeof(EntryListingBehavior).GetMethod("DetermineLocked",
                    BindingFlags.NonPublic | BindingFlags.Instance);

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

                if (HasClicked == null || DetermineLocked == null)
                {
                    LoggingHelper.ReflectionError(nameof(HasClicked), nameof(DetermineLocked));
                    yield break;
                }

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Game
                {
                    if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile) 
                        && GlobalVariables.currentDay >= GlobalVariables.entryUnlockScript.currentTier + 1)
                    {
                        // OLD: __instance.hasClicked = true;
                        HasClicked.SetValue(__instance, true); 
                    }
                
                    // OLD: __instance.DetermineLocked();
                    DetermineLocked.Invoke(__instance, null); 
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    EntryMetadata entryFound = CustomCampaignGlobal.GetEntryFromActiveCampaign(__instance.myProfile.monsterName);

                    if (entryFound != null)
                    {
                        // If the current entry is unlocked.
                        if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile))
                        {
                            // Our permission tier is 1 larger than the current permission.
                            if (GlobalVariables.entryUnlockScript.currentTier - 1 > entryFound.PermissionLevel)
                            {
                                HasClicked.SetValue(__instance, true);
                            }

                            // If our current day is one later than the out tier + 1.
                            // Example: Our current day is "1",
                            // our current tier came from the previous day, which is "0".
                            // As such, our current day is equal 1, and as such, we hide the NEW tag.
                            if (GlobalVariables.currentDay >= GlobalVariables.entryUnlockScript.currentTier + 1)
                            {
                                HasClicked.SetValue(__instance, true);
                            }
                        }
                    }
                    else // Main Campaign Entries, for now we just default.
                    {
                        if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(__instance.myProfile)
                            && GlobalVariables.currentDay >= GlobalVariables.entryUnlockScript.currentTier + 1)
                        {
                            // OLD: __instance.hasClicked = true;
                            HasClicked.SetValue(__instance, true); 
                        }
                    }
                    
                    DetermineLocked.Invoke(__instance, null);
                }
            }
        }
        
    }
}