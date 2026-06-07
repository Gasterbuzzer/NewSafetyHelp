using System.Reflection;
using HarmonyLib;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class ScreenResolutionBugFix
    {
        [HarmonyPatch(typeof(ScreenResolutions), "SetMenuValue", typeof(int), typeof(int), typeof(int))]
        public static class FixScreenResolutionOptions
        {
            /// <summary>
            /// Fixes an out of range exception bug.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="width"> Width of screen. </param>
            /// <param name="height"> Height of screen. </param>
            /// <param name="refresh"> Refresh rate of screen. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ScreenResolutions __instance, ref int width, ref int height, ref int refresh)
            {
                FieldInfo resolutionField = typeof(ScreenResolutions).GetField("resolutions", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                
                if (resolutionField == null)
                {
                    LoggingHelper.ErrorLog("ScreenResolutions field not found." +
                                           " Unable of patching out bug. Defaulting to normal function.");
                    return true; // Call normal function instead.
                }
                
                Resolution[] resolutions = (Resolution[]) resolutionField.GetValue(__instance);

                if (resolutions.Length <= 0)
                {
                    LoggingHelper.ErrorLog("ScreenResolutions field was empty." +
                                           " Unable of patching out bug. Defaulting to normal function.");
                    return true; // Call normal function instead.
                }
                
                for (int i = 0; i < resolutions.Length; i++)
                {
                    Resolution resolution = resolutions[i];
                    if (resolution.width == width && resolution.height == height && resolution.refreshRate == refresh)
                    {
                        if (i >= __instance.dropdownMenu.options.Count) // "i" is larger than the amount of options. We default to i only.
                        {
                            __instance.dropdownMenu.value = i;
                        }
                        else // Normal interaction.
                        {
                            __instance.dropdownMenu.value = i;
                            __instance.dropdownMenu.captionText.text = __instance.dropdownMenu.options[i].text;
                        }
                        return false;
                    }
                }
                
                return false; // Skip the original function
            }
        }
    }
}