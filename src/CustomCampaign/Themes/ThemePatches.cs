using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using TMPro;

namespace NewSafetyHelp.CustomCampaign.Themes
{
    public static class ThemePatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "Start", new Type[] { })]
        public static class LoadPatch
        {
            /// <summary>
            /// Patches the options menu to add our own options.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static void Postfix(MethodBase __originalMethod, OptionsMenuBehavior __instance)
            {
                if (!CustomCampaignGlobal.inCustomCampaign) // Main Game
                {
                    // TODO: Add main campaign theme.
                }
                else
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null in options postfix. Unable of adding options in custom campaign.");
                        return;
                    }

                    foreach (ThemesExtraInfo theme in customCampaign.customThemesGeneral)
                    {
                        __instance.colorDropdown.options.Add(new TMP_Dropdown.OptionData(theme.themeName));
                    }
                }
                
                __instance.colorDropdown.RefreshShownValue();
            }
        }
    }
}