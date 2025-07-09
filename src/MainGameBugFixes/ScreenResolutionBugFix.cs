using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class ScreenResolutionBugFix
    {
        [HarmonyLib.HarmonyPatch(typeof(ScreenResolutions), "SetMenuValue", new Type[] { typeof(int), typeof(int), typeof(int) })]
        public static class FixScreenResolutionOptions
        {

            /// <summary>
            /// Fixes an out of range exception bug.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, ScreenResolutions __instance, ref int width, ref int height, ref int refresh)
            {

                FieldInfo resolutionField = typeof(ScreenResolutions).GetField("resolutions", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                
                if (resolutionField == null)
                {
                    MelonLogger.Error("ERROR: ScreenResolutions field not found. Unable of patching out bug. Defaulting to normal function.");
                    return true; // Call normal function instead.
                }
                
                Resolution[] _resolution = (Resolution[]) resolutionField.GetValue(__instance);

                if (_resolution.Length <= 0)
                {
                    MelonLogger.Error("ERROR: ScreenResolutions field was empty. Unable of patching out bug. Defaulting to normal function.");
                    return true; // Call normal function instead.
                }
                
                for (int i = 0; i < _resolution.Length; i++)
                {
                    Resolution resolution = _resolution[i];
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