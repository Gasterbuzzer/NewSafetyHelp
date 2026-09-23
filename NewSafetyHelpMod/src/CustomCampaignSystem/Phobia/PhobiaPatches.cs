using System;
using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Phobia
{
    public static class PhobiaPatches
    {
        /// <summary>
        /// Helps decide which image to display with the given phobia settings.
        /// </summary>
        /// <param name="profile">Profile of the monster profile to display.</param>
        /// <returns>True: Hide the image. False: Show original image.</returns>
        private static bool DecidePhobiaImage(ref MonsterProfile profile)
        {
            bool baseGamePhobias = GlobalVariables.saveManagerScript.savedSpiderToggle > 0 && profile.spider ||
                                   GlobalVariables.saveManagerScript.savedInsectToggle > 0 && profile.insect ||
                                   GlobalVariables.saveManagerScript.savedDarkToggle > 0 && profile.dark ||
                                   GlobalVariables.saveManagerScript.savedHoleToggle > 0 && profile.holes ||
                                   GlobalVariables.saveManagerScript.savedWatchToggle > 0 && profile.watching ||
                                   GlobalVariables.saveManagerScript.savedTightToggle > 0 && profile.tightSpace ||
                                   GlobalVariables.saveManagerScript.savedDogToggle > 0 && profile.dog;

            bool hideOriginalSprite = false;

            if (CustomCampaignGlobal.InCustomCampaign)
            {
                CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (customCampaign == null)
                {
                    LoggingHelper.CampaignNullError();
                    return true;
                }

                EntryMetadata entry = CustomCampaignGlobal.GetEntryFromActiveCampaign(profile.monsterName);

                if (entry.CustomPhobias.HasChanged)
                {
                    HashSet<string> entryPhobias = new HashSet<string>(
                        entry.CustomPhobias.Data,
                        StringComparer.OrdinalIgnoreCase
                    );

                    foreach (CustomPhobia customPhobia in customCampaign.CustomPhobias)
                    {
                        if (customPhobia.PreferenceReference.Value
                            && entryPhobias.Contains(customPhobia.PhobiaName))
                        {
                            hideOriginalSprite = true;
                            break;
                        }
                    }
                }
            }

            return baseGamePhobias || hideOriginalSprite; // Skip original
        }

        [HarmonyLib.HarmonyPatch(typeof(EntryCanvasStandaloneBehavior), "SelectMonsterPortrait",
            typeof(MonsterProfile))]
        public static class SelectMonsterPortraitEntryCanvasStandalonePatch
        {
            /// <summary>
            /// UpdateSelectedEntry patch to fix the double loading bug.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="profile">Profile selected to be shown.</param>
            /// <param name="__result">Sprite to be shown.</param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once InconsistentNaming
            // ReSharper disable twice RedundantAssignment
            private static bool Prefix(EntryCanvasStandaloneBehavior __instance, ref MonsterProfile profile,
                ref Sprite __result)
            {
                bool outcome = DecidePhobiaImage(ref profile);

                if (outcome)
                {
                    __result = __instance.phobiaReplacementSprite;

                    if (CustomCampaignGlobal.InCustomCampaign)
                    {
                        (bool foundModifier, VariableChanged<Sprite> value) entryPlaceholderImage =
                            CustomCampaignGlobal.GetActiveModifierValue(c => c.EntryPlaceholderImage,
                                vCs => vCs.HasChanged);

                        if (entryPlaceholderImage.foundModifier
                            && entryPlaceholderImage.value.HasChanged)
                        {
                            __result = entryPlaceholderImage.value.Data;
                        }
                    }
                }
                else
                {
                    __result = profile.monsterPortrait;
                }

                return false;
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "SelectMonsterPortrait",
            typeof(MonsterProfile))]
        public static class SelectMonsterPortraitMainCanvasPatch
        {
            /// <summary>
            /// UpdateSelectedEntry patch to fix the double loading bug.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="profile">Profile selected to be shown.</param>
            /// <param name="__result">Sprite to be shown.</param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once InconsistentNaming
            // ReSharper disable twice RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref MonsterProfile profile,
                ref Sprite __result)
            {
                bool outcome = DecidePhobiaImage(ref profile);

                if (outcome)
                {
                    __result = __instance.phobiaReplacementSprite;

                    if (CustomCampaignGlobal.InCustomCampaign)
                    {
                        (bool foundModifier, VariableChanged<Sprite> value) entryPlaceholderImage =
                            CustomCampaignGlobal.GetActiveModifierValue(c => c.EntryPlaceholderImage,
                                vCs => vCs.HasChanged);

                        if (entryPlaceholderImage.foundModifier
                            && entryPlaceholderImage.value.HasChanged)
                        {
                            __result = entryPlaceholderImage.value.Data;
                        }
                    }
                }
                else
                {
                    __result = profile.monsterPortrait;
                }

                return false;
            }
        }
    }
}