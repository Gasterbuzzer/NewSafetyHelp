using System;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.CustomDesktop;
using NewSafetyHelp.JSONParsing;
using TMPro;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomCampaign.Themes
{
    public static class ThemePatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnEnable", new Type[] { })]
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
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null in options prefix. Unable of adding options in custom campaign.");
                        return;
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
                        CustomDesktopHelper.disableThemeDropdownInGame();
                    }
                }
                
                __instance.colorDropdown.RefreshShownValue();
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ColorPaletteController), "UpdateColorTheme", new Type[] { })]
        public static class UpdateColorThemePatch
        {
            /// <summary>
            /// Patches the color palette controller to also use custom themes.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ColorPaletteController __instance)
            {
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.saveManagerScript.savedColorTheme <= 3)
                    {
                        normalPaletteUpdate(__instance);
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
                                    __instance.colorPalette = GlobalParsingVariables.MainGameThemes[i].CustomThemePalette;
                                }
                            }
                        }
                    }
                }
                else
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable to updating color palette." +
                                          " Calling original function.");
                        return true;
                    }
                    
                    #if DEBUG
                    if (NewSafetyHelpMainClass.ShowThemeDebugLog.Value)
                    {
                        MelonLogger.Msg($"DEBUG: Called with saved color theme: {GlobalVariables.saveManagerScript.savedColorTheme} " +
                                        $"and custom campaign activeTheme: {customCampaign.ActiveTheme}.");
                    }
                    #endif
                    
                    // Now if we have not loaded in the default theme ever, we do it now.
                    if (!string.IsNullOrEmpty(customCampaign.DefaultTheme) 
                        && !customCampaign.DefaultThemeAppliedOnce)
                    {
                        int themeID = CustomCampaignGlobal.GetThemeIDFromName(customCampaign.DefaultTheme);

                        if (themeID > 0)
                        {
                            customCampaign.DefaultThemeAppliedOnce = true;
                            customCampaign.ActiveTheme = themeID;
                        }
                    }

                    if (customCampaign.ActiveTheme <= 3)
                    {
                        normalPaletteUpdate(__instance);
                    }
                    else
                    {
                        int conditionalTheme = CustomCampaignGlobal.CheckIfConditionalTheme();
                        
                        if (conditionalTheme != -1) // We have a conditional theme that we need to apply.
                        {
                            CustomTheme theme = CustomCampaignGlobal.GetThemeFromID(conditionalTheme);
                            
                            if (theme != null
                                && theme.CustomThemePalette != null
                                && theme.CustomThemePalette.colorSwatch != null
                                && theme.CustomThemePalette.colorSwatch.Length > 0)
                            {
                                __instance.colorPalette = theme.CustomThemePalette;
                            }
                            else
                            {
                                normalPaletteUpdate(__instance);
                            }
                        }
                        else // We don't have a conditional theme to apply.
                        {
                            bool isCustomTheme = false;
                            CustomTheme theme = CustomCampaignGlobal.GetActiveTheme(ref isCustomTheme);
                        
                            #if DEBUG
                            MelonLogger.Msg($"DEBUG: Is the theme custom? '{isCustomTheme}'. " +
                                            $"Was theme invalid? '{theme != null}'. ");
                        
                            MelonLogger.Msg($"DEBUG: How many general themes? '{customCampaign.CustomThemesGeneral.Count}'. " +
                                            $"How many conditional themes? '{customCampaign.CustomThemesDays.Count}'.");
                            #endif

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
                                normalPaletteUpdate(__instance);
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

            private static void normalPaletteUpdate(ColorPaletteController __instance)
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
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "ColorThemeChanged", new Type[] { })]
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
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable to updating color palette. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.ActiveTheme = __instance.colorDropdown.value;
                    GlobalVariables.saveManagerScript.savedColorTheme = __instance.colorDropdown.value;
                }
                
                if (!GlobalVariables.colorPaletteController)
                {
                    return false;
                }

                #if DEBUG
                if (NewSafetyHelpMainClass.ShowThemeDebugLog.Value)
                {
                    MelonLogger.Msg($"DEBUG: Color Palette change called with ID: {__instance.colorDropdown.value}");
                }
                #endif
                
                GlobalVariables.colorPaletteController.UpdateColorTheme();
                
                return false; // Skip original function.
            }
        }
    }
}