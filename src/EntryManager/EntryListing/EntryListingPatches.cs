using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;

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

                    if (!customCampaign.removeExistingEntries && customCampaign.resetDefaultEntriesPermission &&
                        !customCampaign.doShowNewTagForMainGameEntries) // If allowed to hide the name, we do it. 
                    {
                        if (MainClassForMonsterEntries.copyMonsterProfiles
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
        
        
    }
}