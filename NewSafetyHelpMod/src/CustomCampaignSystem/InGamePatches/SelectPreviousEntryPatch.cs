using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.LoggingSystem;
using TMPro;

namespace NewSafetyHelp.CustomCampaignSystem.InGamePatches
{
    public static class SelectPreviousEntryPatch
    {
        [HarmonyLib.HarmonyPatch(typeof(SubmitWindowBehavior), "PopulateDropdownList")]
        public static class SubmitWindowPopulateDropdownList
        {
            /// <summary>
            /// The last selected entry.
            /// </summary>
            public static MonsterProfile LastSelectedEntry;

            /// <summary>
            /// Changes the populate function to not select animation at the start.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(SubmitWindowBehavior __instance)
            {
                __instance.myDropdown.ClearOptions();
                foreach (MonsterProfile monsterProfile in GlobalVariables.entryUnlockScript.allEntries.monsterProfiles)
                {
                    if (GlobalVariables.entryUnlockScript.CheckMonsterIsUnlocked(monsterProfile))
                        __instance.myDropdown.options.Add(new TMP_Dropdown.OptionData()
                        {
                            text = monsterProfile.monsterName
                        });
                    else
                        __instance.myDropdown.options.Add(new TMP_Dropdown.OptionData()
                        {
                            text = "ENTRY INACCESSIBLE"
                        });
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    (bool foundModifier, VariableChanged<bool> value) selectPreviouslySelectedEntryInSubmitWindow =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.SelectPreviouslySelectedEntryInSubmitWindow,
                            vCs => vCs.HasChanged);

                    if (!(selectPreviouslySelectedEntryInSubmitWindow.foundModifier
                          && selectPreviouslySelectedEntryInSubmitWindow.value.Data))
                    {
                        __instance.OnDropdownItemSelected();
                    }
                    else if (LastSelectedEntry != null)
                    {
                        int index = __instance.myDropdown.options.FindIndex(option =>
                            option.text == LastSelectedEntry.monsterName);

                        if (index < 0)
                        {
                            LoggingHelper.TestLog("Not Found");
                        }

                        __instance.myDropdown.value = index;
                        __instance.myDropdown.RefreshShownValue();
                    }
                }
                else
                {
                    __instance.OnDropdownItemSelected();
                }

                return false; // Skip original
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SubmitWindowBehavior), "OnDropdownItemSelected")]
        public static class SubmitWindowLastSelectedItem
        {
            /// <summary>
            /// Prefixes the function to save the last selected profile.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static void Prefix(SubmitWindowBehavior __instance)
            {
                SubmitWindowPopulateDropdownList.LastSelectedEntry = __instance.GetMonsterProfileFromName();
            }
        }
    }
}