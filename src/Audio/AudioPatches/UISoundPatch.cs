using System.Collections;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.Audio.AudioPatches
{
    public static class UISoundPatch
    {
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "Start")]
        public static class StartPatch
        {
            // If we have run the function for the first time.
            // Is used to get the default ringtone.
            private static bool firstRun;
            
            public static RichAudioClip DefaultRingtone;
            public static RichAudioClip DefaultWarpedRingtone;
            
            /// <summary>
            /// Patches the start function to replace audio clips.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(UISoundController __instance)
            {
                if (!firstRun)
                {
                    DefaultRingtone = __instance.phoneCall;
                    DefaultWarpedRingtone = __instance.phoneCallWarped;
                    firstRun = true;
                }
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayGlitchSound")]
        public static class PlayGlitchSoundPatch
        {
            /// <summary>
            /// Patches the play glitch sound to be more conform with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(UISoundController __instance)
            {
                if (__instance.myGlitchLoopingSource.isPlaying)
                {
                    __instance.myGlitchLoopingSource.volume = 0.0f;
                    __instance.myGlitchLoopingSource.Stop();
                }
                
                if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign) // Don't play scary sound in custom campaign.
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
        
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayGlitchSoundWhileGlitchAnimationIsPlaying")]
        public static class PlayGlitchSoundWhileGlitchAnimationIsPlayingPatch
        {
            /// <summary>
            /// Patches the play glitch sound with glitch animation to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(UISoundController __instance, ref IEnumerator __result)
            {
                __result = PatchedPlayGlitchSound(__instance);
                
                return false; // Skip function with false.
            }

            private static IEnumerator PatchedPlayGlitchSound(UISoundController __instance)
            {
                UISoundController uiSoundController = __instance;
                
                if (!uiSoundController.myGlitchLoopingSource.isPlaying)
                {
                    if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign) // Don't play glitch looping source if in custom campaign.
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
                    LoggingHelper.ErrorLog("[UNITY] Camera is null! Cannot play glitch effect.");
                    yield break;
                }
                
                while (Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips.Length == 0)
                {
                    LoggingHelper.DebugLog("[UNITY] Waiting for animationClips[] to populate.");
                    yield return null;
                }

                while (Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips.Length != 0)
                {
                    yield return null;
                }
                    
                uiSoundController.StartCoroutine(uiSoundController.FadeOutLoopingSound(uiSoundController.myGlitchLoopingSource, 5f));
            }
        }
    }
}