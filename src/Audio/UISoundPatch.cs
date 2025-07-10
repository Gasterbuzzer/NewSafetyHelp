using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio
{
    public static class UISoundPatch
    {
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayGlitchSound", new Type[] { })]
        public static class PlayGlitchSoundPatch
        {

            /// <summary>
            /// Patches the play glitch sound to be more conform with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, UISoundController __instance)
            {
                if (__instance.myGlitchLoopingSource.isPlaying)
                {
                    __instance.myGlitchLoopingSource.volume = 0.0f;
                    __instance.myGlitchLoopingSource.Stop();
                }
                if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.inCustomCampaign) // Don't play scary sound in custom campaign.
                {
                    __instance.myGlitchLoopingSource.clip = __instance.scaryGlitch.clip;
                    __instance.myGlitchLoopingSource.volume = __instance.scaryGlitch.volume;
                }
                else
                {
                    __instance.myGlitchLoopingSource.clip = __instance.normalGlitch.clip;
                    __instance.myGlitchLoopingSource.volume = __instance.normalGlitch.volume;
                }
                __instance.myGlitchLoopingSource.Play();
                
                return false; // Skip function with false.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayGlitchSoundWhileGlitchAnimationIsPlaying", new Type[] { })]
        public static class PlayGlitchSoundWhileGlitchAnimationIsPlayingPatch
        {

            /// <summary>
            /// Patches the play glitch sound with glitch animation to work better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, UISoundController __instance, ref IEnumerator __result)
            {
                
                __result = patchedPlayGlitchSound(__instance);
                
                return false; // Skip function with false.
            }

            public static IEnumerator patchedPlayGlitchSound(UISoundController __instance)
            {
                UISoundController uiSoundController = __instance;
                
                if (!uiSoundController.myGlitchLoopingSource.isPlaying)
                {
                    if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.inCustomCampaign) // Don't play glitch looping source if in custom campaign.
                    {
                        uiSoundController.myGlitchLoopingSource.clip = uiSoundController.scaryGlitch.clip;
                        uiSoundController.myGlitchLoopingSource.volume = uiSoundController.scaryGlitch.volume;
                    }
                    else
                    {
                        uiSoundController.myGlitchLoopingSource.clip = uiSoundController.normalGlitch.clip;
                        uiSoundController.myGlitchLoopingSource.volume = uiSoundController.normalGlitch.volume;
                    }
                    uiSoundController.myGlitchLoopingSource.Play();
                }

                if (Camera.main == null)
                {
                    MelonLogger.Error("UNITY ERROR: Camera is null! Cannot play glitch effect.");
                    yield break;
                }
                
                while (Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips.Length == 0)
                {
                    MelonLogger.Msg( "UNITY: Waiting for animationClips[] to populate.");
                    yield return null;
                }

                while (Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips.Length != 0)
                {
                    yield return null;
                }
                    
                uiSoundController.StartCoroutine(uiSoundController.FadeOutLoopingSound(uiSoundController.myGlitchLoopingSource, 5f));
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartRandomMusic", new Type[] { })]
        public static class StartRandomMusicPatch
        {

            /// <summary>
            /// Patches the play random music to not play day 7 music in custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, MusicController __instance)
            {

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    if (GlobalVariables.currentDay == 7 && !GlobalVariables.arcadeMode)
                    {
                        return false;
                    }
                }
                
                // Custom Campaign ignores the above line.
                
                int index1 = 0;
                
                FieldInfo _previousHoldMusicIndex = typeof(MusicController).GetField("previousHoldMusicIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_previousHoldMusicIndex == null)
                {
                    MelonLogger.Error("ERROR: PreviousHoldMusicIndex is null. Unable of replacing MusicController StartNewRandomMusic. Calling original function.");
                    return true;
                }

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        for (int index2 = 0; index1 == (int) _previousHoldMusicIndex.GetValue(__instance) && index2 < 3; ++index2) // __instance.previousHoldMusicIndex
                        {
                            index1 = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }
                        
                        _previousHoldMusicIndex.SetValue(__instance, index1); // __instance.previousHoldMusicIndex = index1;
                        
                    }
                    else
                    {
                        for (int index3 = 0; index1 == (int) _previousHoldMusicIndex.GetValue(__instance) && index3 < 3; ++index3) // __instance.previousHoldMusicIndex
                        {
                            index1 = Random.Range(0, GlobalVariables.currentDay);
                        }
                    
                        _previousHoldMusicIndex.SetValue(__instance, index1); // __instance.previousHoldMusicIndex = index1;
                    }
                }
                else // Custom Campaign Music. Ignores the day
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        for (int index2 = 0; index1 == (int) _previousHoldMusicIndex.GetValue(__instance) && index2 < 3; ++index2) // __instance.previousHoldMusicIndex
                        {
                            index1 = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }
                        
                        _previousHoldMusicIndex.SetValue(__instance, index1); // __instance.previousHoldMusicIndex = index1;
                        
                    }
                    else
                    {
                        for (int index3 = 0; index1 == (int) _previousHoldMusicIndex.GetValue(__instance) && index3 < 3; ++index3) // __instance.previousHoldMusicIndex
                        {
                            index1 = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                        }
                    
                        _previousHoldMusicIndex.SetValue(__instance, index1); // __instance.previousHoldMusicIndex = index1;
                    }
                }
                
                __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[index1]);
                
                return false; // Skip function with false.
            }
        }
    }
}