using System.Collections;
using HarmonyLib;
using MelonLoader;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class UISoundBugFix
    {
        [HarmonyPatch(typeof(UISoundController), "FadeInLoopingSound", typeof(RichAudioClip), typeof(AudioSource), typeof(float))]
        public static class FadeInLoopingSoundBugFix
        {
            /// <summary>
            /// Fixes an audio bug when switching back to main menu.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of function </param>
            /// <param name="myRichClip"> Clip to play in fadein. </param>
            /// <param name="mySource"> Source to play the clip in </param>
            /// <param name="interpolaterScalar"> Scalar interpolate.</param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(UISoundController __instance, ref IEnumerator __result,
                ref RichAudioClip myRichClip, ref AudioSource mySource, ref float interpolaterScalar)
            {
                __result = FadeInLoopingSoundChanged(__instance, myRichClip, mySource, interpolaterScalar);
                
                return false; // Skip the original function
            }

            private static IEnumerator FadeInLoopingSoundChanged(UISoundController __instance,
                RichAudioClip myRichClip, AudioSource mySource, float interpolaterScalar)
            {
                float interpolater;

                if (mySource == null) // If empty, we can just skip it.
                {
                    yield break;
                }
                
                if (!(bool) mySource)
                {

                    // In case the clip is empty. We just skip it.
                    if (myRichClip.clip == null)
                    {
                        yield break;
                    }
                    
                    __instance.myLoopingSource.clip = myRichClip.clip;
                    __instance.myLoopingSource.volume = 0.0f;
                    __instance.myLoopingSource.Play();
                    
                    interpolater = 0.0f;
                    
                    while (__instance.myLoopingSource.volume < myRichClip.volume)
                    {
                        __instance.myLoopingSource.volume = Mathf.Lerp(0.0f, myRichClip.volume, interpolater);
                        interpolater += interpolaterScalar * Time.deltaTime;
                        
                        yield return null;
                    }
                    
                    __instance.myLoopingSource.volume = myRichClip.volume;
                }
                else
                {
                    LoggingHelper.DebugLog($"[UNITY] {mySource} is fading in.");
                    
                    mySource.clip = myRichClip.clip;
                    mySource.volume = 0.0f;
                    mySource.Play();
                    
                    interpolater = 0.0f;
                    
                    while (mySource.volume < myRichClip.volume)
                    {
                        mySource.volume = Mathf.Lerp(0.0f, myRichClip.volume, interpolater);
                        interpolater += interpolaterScalar * Time.deltaTime;
                        
                        yield return null;
                    }
                    mySource.volume = myRichClip.volume;
                }
            }
        }
    }
}