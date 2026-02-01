using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.CallerPatches.UI
{
    public static class UIFallbacks
    {
        
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PickRandomClip")]
        public static class PickRandomClipPatch
        {
            /// <summary>
            /// The original function picks a random caller clip. Since missing callers may cause issues or errors. We now inform the user and prevent the issue.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance,
                // ReSharper disable once RedundantAssignment
                ref RichAudioClip __result)
            {
                if (__instance.randomCallerClips.Length <= 0)
                {
                    MelonLogger.Warning("WARNING: No caller audio available for caller!");
                    __result = null;
                    return false;
                }

                int num = Random.Range(0, __instance.randomCallerClips.Length);
                __result = __instance.randomCallerClips[num];

                return false; // Skip the original function
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PickRandomPic")]
        public static class PickRandomPicPatch
        {
            /// <summary>
            /// The original function picks a random caller picture. Since missing callers may cause issues or errors. We now inform the user and prevent the issue.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance, ref Sprite __result)
            {
                if (__instance.randomCallerPics.Length <= 0)
                {
                    MelonLogger.Warning("WARNING: No image available for caller!");
                    __result = null;
                    return false;
                }

                int num = Random.Range(0, __instance.randomCallerPics.Length);
                __result = __instance.randomCallerPics[num];

                return false; // Skip the original function
            }
        }
    }
}