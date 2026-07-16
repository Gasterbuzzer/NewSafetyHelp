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
                    {
                        __instance.myDropdown.options.Add(new TMP_Dropdown.OptionData
                        {
                            text = monsterProfile.monsterName
                        });
                    }
                    else
                    {
                        __instance.myDropdown.options.Add(new TMP_Dropdown.OptionData
                        {
                            text = "ENTRY INACCESSIBLE"
                        });
                    }
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    (bool foundModifier, VariableChanged<bool> value) selectPreviouslySelectedEntryInSubmitWindow =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.SelectPreviouslySelectedEntryInSubmitWindow,
                            vCs => vCs.HasChanged);

                    (bool foundModifier, VariableChanged<bool> value)
                        selectCurrentlyMainViewSelectedEntryInSubmitWindow =
                            CustomCampaignGlobal.GetActiveModifierValue(
                                c => c.SelectCurrentlyMainViewSelectedEntryInSubmitWindow,
                                vCs => vCs.HasChanged);

                    if (selectPreviouslySelectedEntryInSubmitWindow.foundModifier
                        && selectPreviouslySelectedEntryInSubmitWindow.value.Data
                        && LastSelectedEntry != null)
                    {
                        int index = __instance.myDropdown.options.FindIndex(option =>
                            option.text == LastSelectedEntry.monsterName);

                        if (index < 0)
                        {
                            LoggingHelper.DebugLog("Entry to be selected not found.");

                            __instance.myDropdown.value = 0;
                            __instance.myDropdown.RefreshShownValue();
                            
                            __instance.OnDropdownItemSelected();
                        }
                        else
                        {
                            __instance.myDropdown.value = index;
                            __instance.myDropdown.RefreshShownValue();
                        }
                    }
                    else if (selectCurrentlyMainViewSelectedEntryInSubmitWindow.foundModifier
                             && selectCurrentlyMainViewSelectedEntryInSubmitWindow.value.Data)
                    {
                        if (GlobalVariables.mainCanvasScript.selectedMonsterTitle.text.ToLower().Trim() ==
                            "no entry selected."
                            && string.IsNullOrEmpty(GlobalVariables.mainCanvasScript.selectedMonsterDescription.text
                                .ToLower().Trim()))
                        {
                            LoggingHelper.DebugLog("No entry selected. Defaulting to first entry.");

                            __instance.myDropdown.value = 0;
                            __instance.myDropdown.RefreshShownValue();
                            
                            __instance.OnDropdownItemSelected();
                        }
                        else
                        {
                            int index = __instance.myDropdown.options.FindIndex(option =>
                                option.text == GlobalVariables.mainCanvasScript.selectedMonsterTitle.text);

                            if (index < 0)
                            {
                                LoggingHelper.DebugLog("Entry to be selected not found.");

                                __instance.myDropdown.value = 0;
                                __instance.myDropdown.RefreshShownValue();
                                
                                __instance.OnDropdownItemSelected();
                            }
                            else
                            {
                                __instance.myDropdown.value = index;
                                __instance.myDropdown.RefreshShownValue();
                            }
                        }
                    }
                    else
                    {
                        __instance.OnDropdownItemSelected();
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