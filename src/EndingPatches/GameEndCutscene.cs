using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CutsceneLogic;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.CustomDesktop.Utils;
using NewSafetyHelp.LoggingSystem;
using Steamworks;
using UnityEngine;

namespace NewSafetyHelp.EndingPatches
{
    public static class GameEndCutscene
    {
        // Cached animator lookups.
        private static readonly int Shake = Animator.StringToHash("shake");
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "EndingCutsceneRoutine")]
        public static class EndingCutsceneRoutinePatch
        {
            private static readonly MethodInfo SaveCallerAnswers = typeof(MainCanvasBehavior).GetMethod(
                "SaveCallerAnswers",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static readonly MethodInfo AchievedHundredPercentAccuracyRating =
                typeof(MainCanvasBehavior).GetMethod("AchievedHundredPercentAccuracyRating",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            /// <summary>
            /// Patches the EndingCutsceneRoutine coroutine to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called after wards. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                __result = EndingCutsceneRoutineChanged(__instance);

                return false; // Skip function with false.
            }

            private static IEnumerator EndingCutsceneRoutineChanged(MainCanvasBehavior __instance)
            {
                MainCanvasBehavior mainCanvasBehavior = __instance;

                if (Camera.main == null)
                {
                    LoggingHelper.CriticalErrorLog("Camera was null. Catastrophic failure!");
                    yield break;
                }

                if (mainCanvasBehavior.videoPlayer.isPlaying 
                    || Camera.main.gameObject.GetComponent<Animator>().GetBool(Shake))
                {
                    LoggingHelper.InfoLog("Ending cutscene is already playing. Not calling again.");
                    yield break;
                }

                if (!GlobalVariables.isXmasDLC)
                {
                    // Custom Campaign
                    if (CustomCampaignGlobal.InCustomCampaign)
                    {
                        (bool foundModifier, VariableChanged<bool> value) finalCutsceneShouldShake = 
                            CustomCampaignGlobal.GetActiveModifierValue(
                                c => c.FinalCutsceneShake, vCb => vCb.HasChanged);
                    
                        bool shouldCutsceneShake = true;

                        if (finalCutsceneShouldShake.foundModifier)
                        {
                            shouldCutsceneShake = finalCutsceneShouldShake.value.Data;
                        }

                        if (shouldCutsceneShake)
                        {
                            Camera.main.gameObject.GetComponent<Animator>().SetBool(Shake, true);
                        }
                        
                        (bool foundModifier, VariableChanged<bool> value) finalCutsceneGlitchSoundEffect = 
                            CustomCampaignGlobal.GetActiveModifierValue(
                                c => c.FinalCutsceneGlitchSounds, vCb => vCb.HasChanged);
                        
                        (bool foundModifier, VariableChanged<RichAudioClip> value) finalCutsceneAudio = 
                            CustomCampaignGlobal.GetActiveModifierValue(
                                c => c.FinalCutsceneAudio, vCb => vCb.HasChanged);
                        
                        bool shouldPlayGlitchSound = true;
                        bool playCustomSound = false;

                        if (finalCutsceneGlitchSoundEffect.foundModifier)
                        {
                            shouldPlayGlitchSound = finalCutsceneGlitchSoundEffect.value.Data;
                        }
                        
                        if (finalCutsceneAudio.foundModifier 
                            && finalCutsceneAudio.value.Data != null)
                        {
                            shouldPlayGlitchSound = false;
                            playCustomSound = true;
                        }

                        if (playCustomSound)
                        {
                            mainCanvasBehavior.StartCoroutine(
                                GlobalVariables.UISoundControllerScript.FadeInLoopingSound(
                                    finalCutsceneAudio.value.Data,
                                    GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));
                        }
                        else if (shouldPlayGlitchSound)
                        {
                            mainCanvasBehavior.StartCoroutine(
                                GlobalVariables.UISoundControllerScript.FadeInLoopingSound(
                                    GlobalVariables.UISoundControllerScript.screenShakeLoop,
                                    GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));
                            
                            yield return new WaitForSeconds(6f);

                            mainCanvasBehavior.StartCoroutine(
                                GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(
                                    GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));
                        }
                    }
                    // Main Campaign
                    else
                    {
                        Camera.main.gameObject.GetComponent<Animator>().SetBool(Shake, true);
                        
                        mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(
                            GlobalVariables.UISoundControllerScript.screenShakeLoop,
                            GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));
                        
                        yield return new WaitForSeconds(6f);
                        
                        mainCanvasBehavior.StartCoroutine(
                            GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(
                                GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));
                    }
                    
                    GlobalVariables.musicControllerScript.StopTrialMusic();
                }

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    GlobalVariables.saveManagerScript.savedGameFinished = 1;
                    GlobalVariables.saveManagerScript.savedGameFinishedDisplay = 1;

                    if (SaveCallerAnswers == null)
                    {
                        LoggingHelper.ReflectionError(nameof(SaveCallerAnswers));
                        yield break;
                    }

                    // OLD: mainCanvasBehavior.SaveCallerAnswers();
                    SaveCallerAnswers.Invoke(mainCanvasBehavior, null);
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    customCampaign.SavedGameFinished = 1;
                    customCampaign.SavedGameFinishedDisplay = 1;

                    List<bool> flagArray = new List<bool>();

                    // Create missing values.
                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray.Add(false);
                    }

                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray[index] = GlobalVariables.callerControllerScript.callers[index].answeredCorrectly;
                    }

                    customCampaign.SavedCallersCorrectAnswer = flagArray;
                    customCampaign.SavedCallerArrayLength = GlobalVariables.callerControllerScript.callers.Length;
                }

                // Works for both custom campaigns and main campaign.
                GlobalVariables.saveManagerScript.SaveGameProgress();
                GlobalVariables.saveManagerScript.SaveGameFinished();

                // Custom Campaign
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    (bool foundModifier, VariableChanged<bool> value) finalCutsceneFadeToBlack = 
                        CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.FinalCutsceneFadeToBlack, vCb => vCb.HasChanged);
                    
                    bool shouldPlayFadeToBlack = true;

                    if (finalCutsceneFadeToBlack.foundModifier)
                    {
                        shouldPlayFadeToBlack = finalCutsceneFadeToBlack.value.Data;
                    }
                    
                    (bool foundModifier, VariableChanged<float> value) finalCutsceneFadeDuration = 
                        CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.FinalCutsceneFadeDuration, vCb => vCb.HasChanged);
                    
                    (bool foundModifier, VariableChanged<float> value) finalCutsceneFadePaddingDuration = 
                        CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.FinalCutsceneFadePaddingDuration, vCb => vCb.HasChanged);
                    
                    (bool foundModifier, VariableChanged<bool> value) finalCutsceneStopAudioAfterFade = 
                        CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.FinalCutsceneStopAudioAfterFade, vCb => vCb.HasChanged);

                    float fadeDuration = 3f;
                    float fadeOutPadding = 1f;
                    
                    bool stopAudioAfterFade = true;

                    if (finalCutsceneFadeDuration.foundModifier)
                    {
                        fadeDuration = finalCutsceneFadeDuration.value.Data;
                    }
                    
                    if (finalCutsceneFadePaddingDuration.foundModifier)
                    {
                        fadeOutPadding = finalCutsceneFadePaddingDuration.value.Data;
                    }
                    
                    if (finalCutsceneStopAudioAfterFade.foundModifier)
                    {
                        stopAudioAfterFade = finalCutsceneStopAudioAfterFade.value.Data;
                    }
                    
                    if (shouldPlayFadeToBlack)
                    {
                        GlobalVariables.fade.FadeIn(fadeDuration);

                        yield return new WaitForSeconds(fadeDuration+fadeOutPadding);

                        if (stopAudioAfterFade)
                        {
                            // Stop music if it is running
                            mainCanvasBehavior.StartCoroutine(
                                GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(
                                    GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));
                        }

                        GlobalVariables.fade.FadeOut();
                    }
                }
                // Main Campaign
                else
                {
                    GlobalVariables.fade.FadeIn(3f);

                    yield return new WaitForSeconds(4f);

                    GlobalVariables.fade.FadeOut();
                }
                
                mainCanvasBehavior.cutsceneCanvas.SetActive(true);
                
                yield return new WaitForSeconds(0.5f);

                // For custom campaign:
                bool isPlayingCustomVideo = false;
                
                // Inject custom end clip here.
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.endClip;
                    if (GlobalVariables.isXmasDLC)
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.xmasEndClip;
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    string endCutsceneVideoURL = null;
                    
                    if (!string.IsNullOrEmpty(customCampaign.EndCutsceneVideoName)) 
                    {
                        endCutsceneVideoURL = customCampaign.EndCutsceneVideoName;
                    }

                    // Checks for all custom cutscenes and picks the first valid one.
                    if (customCampaign.CustomCutscenes != null
                        && customCampaign.CustomCutscenes.Count > 0)
                    {
                        foreach (CustomCutscene customCutscene in customCampaign.CustomCutscenes)
                        {
                            if (AccuracyCutsceneHelper.CheckCutsceneAccuracy(customCutscene)
                                && !string.IsNullOrEmpty(customCutscene.CutsceneVideoPath))
                            {
                                endCutsceneVideoURL = customCutscene.CutsceneVideoPath;
                                break;
                            }
                        }
                    }

                    // If provided we play it.
                    if (!string.IsNullOrEmpty(endCutsceneVideoURL))
                    {
                        isPlayingCustomVideo = true;
                        mainCanvasBehavior.videoPlayer.url = endCutsceneVideoURL;
                    }
                    else // If not, we show the default one.
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.endClip;
                    }
                }

                mainCanvasBehavior.videoPlayer.Play();

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    yield return new WaitForSeconds((float)mainCanvasBehavior.videoPlayer.clip.length);
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }
                    
                    bool prepareVideo = false;

                    if (isPlayingCustomVideo)
                    {
                        prepareVideo = true;
                    }

                    // If provided
                    if (prepareVideo) 
                    {
                        // Get video length and then wait for it.
                        mainCanvasBehavior.videoPlayer.Prepare();

                        // While playing we don't continue.
                        while (mainCanvasBehavior.videoPlayer.isPlaying) 
                        {
                            yield return null;
                        }

                        // Afterward we load all main game values.
                        CustomCampaignSceneSwitcher.BackToMainGame(false);
                    }
                    else // If not, we show the default one.
                    {
                        yield return new WaitForSeconds((float)mainCanvasBehavior.videoPlayer.clip.length);
                    }
                }

                // Disabled in Custom Campaign
                if (SteamManager.Initialized
                    && !GlobalVariables.isXmasDLC
                    && !CustomCampaignGlobal.InCustomCampaign)
                {
                    SteamUserStats.SetAchievement("GameFinished");

                    if (AchievedHundredPercentAccuracyRating == null)
                    {
                        LoggingHelper.ReflectionError(nameof(AchievedHundredPercentAccuracyRating));
                        yield break;
                    }

                    // OLD: mainCanvasBehavior.AchievedHundredPercentAccuracyRating()
                    if ((bool)AchievedHundredPercentAccuracyRating.Invoke(mainCanvasBehavior, null))
                    {
                        SteamUserStats.SetAchievement("PerfectGame");
                        LoggingHelper.DebugLog("[UNITY] PerfectGame Achievement unlocked.");
                    }

                    SteamUserStats.StoreStats();
                }

                yield return new WaitForSeconds(2f);

                mainCanvasBehavior.ExitToStartMenu();
            }
        }
    }
}