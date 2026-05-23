using System.Reflection;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Fade
{
    public static class FadeScreenPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(FadeBehavior), "Update")]
        public static class FadeBehaviorUpdatePatch
        {
            private static readonly FieldInfo IsFadingToBlack = 
                typeof(FadeBehavior).GetField("isFadingToBlack",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            
            private static readonly FieldInfo TimeSinceFadeToBlack = 
                typeof(FadeBehavior).GetField("timeSinceFadeToBlack",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            
            /// <summary>
            /// Patches the update of the fade screen to not stop after 15 seconds.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(FadeBehavior __instance)
            {
                // Main Campaign
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (!(bool) IsFadingToBlack.GetValue(__instance))
                    {
                        // OLD: __instance.timeSinceFadeToBlack = 0.0f;
                        TimeSinceFadeToBlack.SetValue(__instance, 0.0f);
                    }
                    else
                    {
                        // OLD: __instance.timeSinceFadeToBlack += Time.deltaTime;
                        float addedDeltaTime = (float) TimeSinceFadeToBlack.GetValue(__instance) + Time.deltaTime;
                        TimeSinceFadeToBlack.SetValue(__instance, addedDeltaTime);
                        
                        // OLD: __instance.timeSinceFadeToBlack
                        if ((float) TimeSinceFadeToBlack.GetValue(__instance) < 15.0)
                        {
                            return false;
                        }
                        
                        __instance.FadeOut();
                    }
                }
                
                return false; // Skip function with false.
            }
        }
    }
}