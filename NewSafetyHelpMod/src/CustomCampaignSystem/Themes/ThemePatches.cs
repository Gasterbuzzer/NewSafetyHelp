using System.Linq;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomDesktop.Utils;
using NewSafetyHelp.JSONParsing;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomCampaignSystem.Themes
{
    public static class ThemePatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnEnable")]
        public static class OptionsAddCustomSettings
        {
            /// <summary>
            /// Patches the options menu to add our own options.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static void Prefix(OptionsMenuBehavior __instance)
            {
                if (!CustomCampaignGlobal.InCustomCampaign) // Main Game
                {
                    foreach (CustomTheme theme in GlobalParsingVariables.MainGameThemes)
                    {
                        if (theme != null)
                        {
                            bool alreadyPresent = __instance.colorDropdown.options.Any(o => o.text == theme.ThemeName);

                            if (!alreadyPresent)
                            {
                                __instance.colorDropdown.options.Add(new TMP_Dropdown.OptionData(theme.ThemeName));
                            }
                        }
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return;
                    }

                    if (customCampaign.RemoveDefaultThemes.HasChanged
                        && customCampaign.RemoveDefaultThemes.Data)
                    {
                        __instance.colorDropdown.options.Clear();
                    }

                    foreach (CustomTheme theme in customCampaign.CustomThemesGeneral)
                    {
                        if (theme != null)
                        {
                            bool alreadyPresent = __instance.colorDropdown.options.Any(o => o.text == theme.ThemeName);

                            if (!alreadyPresent)
                            {
                                __instance.colorDropdown.options.Add(new TMP_Dropdown.OptionData(theme.ThemeName));
                            }
                        }
                    }

                    if (customCampaign.DisablePickingThemeOption)
                    {
                        ThemeProgramHelper.DisableThemeDropdownInGame();
                    }

                    if (__instance.colorDropdown.options.Count == 1)
                    {
                        __instance.colorDropdown.value = 0;
                        __instance.colorDropdown.onValueChanged.Invoke(0);
                    }
                }

                __instance.colorDropdown.RefreshShownValue();
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(ColorPaletteController), "UpdateColorTheme")]
        public static class UpdateColorThemePatch
        {
            /// <summary>
            /// Patches the color palette controller to also use custom themes.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ColorPaletteController __instance)
            {
                // Main Campaign
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.saveManagerScript.savedColorTheme <= 3)
                    {
                        NormalPaletteUpdate(__instance);
                    }
                    else // themeID >= 4
                    {
                        for (int i = 0; i < GlobalParsingVariables.MainGameThemes.Count; i++)
                        {
                            if (GlobalVariables.saveManagerScript.savedColorTheme == i + 4)
                            {
                                if (GlobalParsingVariables.MainGameThemes[i] != null
                                    && GlobalParsingVariables.MainGameThemes[i].CustomThemePalette != null
                                    && GlobalParsingVariables.MainGameThemes[i].CustomThemePalette.colorSwatch != null)
                                {
                                    __instance.colorPalette =
                                        GlobalParsingVariables.MainGameThemes[i].CustomThemePalette;
                                }
                            }
                        }
                    }
                }
                else
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return true;
                    }

                    LoggingHelper.DebugLog(
                        $"Called with saved color theme: '{GlobalVariables.saveManagerScript.savedColorTheme}' " +
                        $"and custom campaign activeTheme: '{customCampaign.ActiveTheme}'.",
                        LoggingHelper.LoggingCategory.THEME);
                    
                    bool removeDefaultThemes = false;
                    
                    if (customCampaign.RemoveDefaultThemes.HasChanged)
                    {
                        removeDefaultThemes = customCampaign.RemoveDefaultThemes.Data;
                    }

                    // Now if we have not loaded in the default theme ever, we do it now.
                    if (!string.IsNullOrEmpty(customCampaign.DefaultTheme)
                        && !customCampaign.DefaultThemeAppliedOnce)
                    {
                        int themeID = CustomCampaignGlobal.GetThemeIDFromName(customCampaign.DefaultTheme, removeDefaultThemes);

                        if (themeID > 0)
                        {
                            LoggingHelper.DebugLog($"Setting first time default theme to: '{themeID}'.");
                            
                            customCampaign.DefaultThemeAppliedOnce = true;
                            customCampaign.ActiveTheme = themeID;
                        }
                    }
                    
                    if (!removeDefaultThemes && customCampaign.ActiveTheme <= 3)
                    {
                        NormalPaletteUpdate(__instance);
                    }
                    else
                    {
                        int conditionalTheme = CustomCampaignGlobal.CheckIfConditionalTheme(removeDefaultThemes);

                        if (conditionalTheme != -1) // We have a conditional theme that we need to apply.
                        {
                            CustomTheme theme = CustomCampaignGlobal.GetThemeFromID(conditionalTheme, removeDefaultThemes);

                            if (theme != null
                                && theme.CustomThemePalette != null
                                && theme.CustomThemePalette.colorSwatch != null
                                && theme.CustomThemePalette.colorSwatch.Length > 0)
                            {
                                __instance.colorPalette = theme.CustomThemePalette;
                            }
                            else
                            {
                                NormalPaletteUpdate(__instance);
                            }
                        }
                        else // We don't have a conditional theme to apply.
                        {
                            bool isCustomTheme = false;
                            CustomTheme theme = CustomCampaignGlobal.GetActiveTheme(ref isCustomTheme, removeDefaultThemes);

                            LoggingHelper.DebugLog($"Is the theme custom? '{isCustomTheme}'. " +
                                                   $"Was theme valid? '{theme != null}'. ");

                            LoggingHelper.DebugLog(
                                $"How many general themes? '{customCampaign.CustomThemesGeneral.Count}'. " +
                                $"How many conditional themes? '{customCampaign.CustomThemesDays.Count}'.");

                            if (isCustomTheme
                                && theme != null
                                && theme.CustomThemePalette != null
                                && theme.CustomThemePalette.colorSwatch != null
                                && theme.CustomThemePalette.colorSwatch.Length > 0)
                            {
                                __instance.colorPalette = theme.CustomThemePalette;
                            }
                            else
                            {
                                NormalPaletteUpdate(__instance);
                            }
                        }
                    }
                }

                foreach (AdhereToPalette adhereToPalette in Object.FindObjectsOfType<AdhereToPalette>(true))
                {
                    adhereToPalette.ChangeColors();
                }

                return false;
            }

            private static void NormalPaletteUpdate(ColorPaletteController __instance)
            {
                switch (GlobalVariables.saveManagerScript.savedColorTheme)
                {
                    case 0:
                        __instance.colorPalette = __instance.defaultPalette;

                        if (GlobalVariables.isXmasDLC)
                        {
                            __instance.colorPalette = __instance.xmasPalette;
                        }

                        break;

                    case 1:
                        __instance.colorPalette = __instance.windowsPalette;
                        break;

                    case 2:
                        __instance.colorPalette = __instance.tirePalette;
                        break;

                    case 3:
                        __instance.colorPalette = __instance.nightPalette;
                        break;
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "ColorThemeChanged")]
        public static class ColorThemeChangedPatch
        {
            /// <summary>
            /// Patches the theme option to use our saving system instead. And we also allow picking custom themes.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(OptionsMenuBehavior __instance)
            {
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    GlobalVariables.saveManagerScript.savedColorTheme = __instance.colorDropdown.value;
                }
                else
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return true;
                    }

                    customCampaign.ActiveTheme = __instance.colorDropdown.value;
                    GlobalVariables.saveManagerScript.savedColorTheme = __instance.colorDropdown.value;
                }

                if (!GlobalVariables.colorPaletteController)
                {
                    return false;
                }

                LoggingHelper.DebugLog($"Color Palette change called with ID: {__instance.colorDropdown.value}",
                    LoggingHelper.LoggingCategory.THEME);

                GlobalVariables.colorPaletteController.UpdateColorTheme();

                return false; // Skip original function.
            }
        }
    }
}