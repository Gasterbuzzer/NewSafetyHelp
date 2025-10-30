using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
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
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
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
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once RedundantAssignment
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
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MethodBase __originalMethod, MusicController __instance)
            {

                // If in the main game and the current day is the 7th day.
                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    if (GlobalVariables.currentDay == 7 && !GlobalVariables.arcadeMode)
                    {
                        return false;
                    }
                }
                
                int chosenMusicIndex = 0;
                
                FieldInfo _previousHoldMusicIndex = typeof(MusicController).GetField("previousHoldMusicIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_previousHoldMusicIndex == null)
                {
                    MelonLogger.Error("ERROR: PreviousHoldMusicIndex is null. Unable of replacing MusicController StartNewRandomMusic. Calling original function.");
                    return true;
                }

                if (!CustomCampaignGlobal.inCustomCampaign) // Main game
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }
                        
                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                        
                    }
                    else
                    {
                        for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, GlobalVariables.currentDay);
                        }
                    
                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                }
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign Music. Ignores the day
                {

                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign is null! Cannot play random music. Using default logic.");
                        return true;
                    }

                    if (customCampaign.alwaysRandomMusic)
                    {
                        for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }
                        
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Chose to play the music track: {chosenMusicIndex} with the previous being {(int) _previousHoldMusicIndex.GetValue(__instance)}");
                        #endif
                    
                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);
                    }
                    else
                    {
                        if (GlobalVariables.currentDay > 4)
                        {
                            for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                            {
                                chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                            }
                        
                            _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                        
                        }
                        else
                        {
                            for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                            {
                                chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                            }
                    
                            _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                        }
                    }
                }
                
                __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                
                return false; // Skip function with false.
            }
        }
    }
}