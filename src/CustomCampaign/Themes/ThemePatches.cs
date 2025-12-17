using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using TMPro;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomCampaign.Themes
{
    public static class ThemePatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnEnable", new Type[] { })]
        public static class OptionsAddCustomSettings
        {
            private static bool addedThemeOptions;
            
            /// <summary>
            /// Patches the options menu to add our own options.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static void Prefix(MethodBase __originalMethod, OptionsMenuBehavior __instance)
            {
                if (!CustomCampaignGlobal.inCustomCampaign) // Main Game
                {
                    // TODO: Add main campaign theme.
                }
                else if (!addedThemeOptions)
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null in options prefix. Unable of adding options in custom campaign.");
                        return;
                    }

                    foreach (ThemesExtraInfo theme in customCampaign.customThemesGeneral)
                    {
                        __instance.colorDropdown.options.Add(new TMP_Dropdown.OptionData(theme.themeName));
                    }

                    addedThemeOptions = true;
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
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, ColorPaletteController __instance)
            {
                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    normalPaletteUpdate(__instance);
                }
                else
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable to updating color palette." +
                                          " Calling original function.");
                        return true;
                    }
                    
                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Called with saved color theme: {GlobalVariables.saveManagerScript.savedColorTheme} " +
                                    $"and custom campaign activeTheme: {customCampaign.activeTheme}.");
                    #endif

                    if (customCampaign.activeTheme <= 3)
                    {
                        normalPaletteUpdate(__instance);
                    }
                    else
                    {
                        bool isCustomTheme = false;
                        ThemesExtraInfo theme = CustomCampaignGlobal.getActiveTheme(ref isCustomTheme);
                        
                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: Is the theme custom? '{isCustomTheme}'. " +
                                        $"Was theme invalid? '{theme != null}'. ");
                        
                        MelonLogger.Msg($"DEBUG: How many general themes? '{customCampaign.customThemesGeneral.Count}'. " +
                                        $"How many conditional themes? '{customCampaign.customThemesDays.Count}'.");
                        #endif

                        if (isCustomTheme 
                            && theme != null
                            && theme.customThemePalette != null
                            && theme.customThemePalette.colorSwatch != null
                            && theme.customThemePalette.colorSwatch.Length > 0)
                        {
                            __instance.colorPalette = theme.customThemePalette;
                        }
                        else
                        {
                            normalPaletteUpdate(__instance);
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
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, OptionsMenuBehavior __instance)
            {
                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    GlobalVariables.saveManagerScript.savedColorTheme = __instance.colorDropdown.value;
                }
                else
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable to updating color palette. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.activeTheme = __instance.colorDropdown.value;
                    GlobalVariables.saveManagerScript.savedColorTheme = __instance.colorDropdown.value;
                }
                
                if (!GlobalVariables.colorPaletteController)
                {
                    return false;
                }

                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Color Palette change called with ID: {__instance.colorDropdown.value}");
                #endif
                
                GlobalVariables.colorPaletteController.UpdateColorTheme();
                
                return false; // Skip original function.
            }
        }
    }
}