using System.Collections;
using System.Reflection;
using NewSafetyHelp.Audio;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using Steamworks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.EndingPatches
{
    public static class GameOverPatches
    {
        private static readonly MethodInfo AnswerDynamicCall = typeof(CallerController).GetMethod("AnswerDynamicCall",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo TriggerGameOver = typeof(CallerController).GetField("triggerGameOver",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
        
        private static readonly MethodInfo GetRandomPicMethod = typeof(CallerController).GetMethod("PickRandomPic",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo GetRandomClip = typeof(CallerController).GetMethod("PickRandomClip",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "TriggerGameOver")]
        public static class TriggerGameOverPatch
        {
            /// <summary>
            /// This function calls the GameOver phone call and triggers the game over cutscene.
            /// It is patched to be able to have custom GameOver Callers in custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance)
            {
                LoggingHelper.DebugLog("Triggering GameOver Call + GameOver Cutscene.");

                if (AnswerDynamicCall == null || TriggerGameOver == null)
                {
                    LoggingHelper.ReflectionError(nameof(AnswerDynamicCall),
                        nameof(TriggerGameOver));
                    return true;
                }

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }

                    if (customCampaign.CustomGameOverCallersInCampaign.Count > 0)
                    {
                        CustomCCaller customCCallerGameOverChosen = null;

                        if (customCampaign.CustomGameOverCallersInCampaign.Exists(customCaller =>
                                customCaller.GameOverCallDay <= -1))
                        {
                            // Will choose a random game over caller if all are set at -1.
                            customCCallerGameOverChosen =
                                customCampaign.CustomGameOverCallersInCampaign.FindAll(customCaller =>
                                    customCaller.GameOverCallDay <= -1)[
                                    Random.Range(0, customCampaign.CustomGameOverCallersInCampaign.Count)];
                        }

                        // If any exist that are valid for the current day, we instead replace it with those.
                        if (customCampaign.CustomGameOverCallersInCampaign.Exists(customCaller =>
                                customCaller.GameOverCallDay == GlobalVariables.currentDay))
                        {
                            customCCallerGameOverChosen =
                                customCampaign.CustomGameOverCallersInCampaign.FindAll(customCaller =>
                                    customCaller.GameOverCallDay == GlobalVariables.currentDay)[
                                    Random.Range(0, customCampaign.CustomGameOverCallersInCampaign.Count)];
                        }

                        // Create custom caller and then replace gameOverCall with it.
                        if (customCCallerGameOverChosen != null)
                        {
                            LoggingHelper.DebugLog("WE are replacing the gameover caller with: " +
                                                   $"{customCCallerGameOverChosen.CallerName}.");

                            CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                            newProfile.callerName = customCCallerGameOverChosen.CallerName;
                            newProfile.callTranscription = customCCallerGameOverChosen.CallTranscript;

                            if (GetRandomPicMethod == null || GetRandomClip == null)
                            {
                                LoggingHelper.ReflectionError(nameof(GetRandomPicMethod),
                                    nameof(GetRandomClip));
                                return true;
                            }

                            if (customCCallerGameOverChosen.CallerImage != null)
                            {
                                newProfile.callerPortrait = customCCallerGameOverChosen.CallerImage;
                            }
                            else
                            {
                                LoggingHelper.WarningLog("GameOver-Caller has no caller image, using random image.");

                                newProfile.callerPortrait = (Sprite)GetRandomPicMethod.Invoke(__instance, null);
                            }

                            if (customCCallerGameOverChosen.CallerClip != null)
                            {
                                newProfile.callerClip = customCCallerGameOverChosen.CallerClip;
                            }
                            else
                            {
                                if (AudioImport.CurrentLoadingAudios.Count > 0)
                                {
                                    LoggingHelper.WarningLog(
                                        "GameOver-Caller audio is still loading! Using fallback for now. " +
                                        "If this happens often, please check if the audio is too large!");
                                }
                                else
                                {
                                    LoggingHelper.WarningLog(
                                        "GameOver-Caller has no audio! Using audio fallback. " +
                                        "If you provided an audio but this error shows up, " +
                                        "check for any errors before!");
                                }

                                newProfile.callerClip = (RichAudioClip)GetRandomClip.Invoke(__instance, null);
                            }

                            if (!string.IsNullOrEmpty(customCCallerGameOverChosen.MonsterNameAttached) ||
                                customCCallerGameOverChosen.MonsterIDAttached != -1)
                            {
                                LoggingHelper.WarningLog(
                                    "A monster was provided for the GameOver caller, " +
                                    "but GameOver callers do not use any entries! Will default to none.");
                            }

                            newProfile.callerMonster = null;


                            if (customCCallerGameOverChosen.CallerIncreasesTier)
                            {
                                LoggingHelper.WarningLog(
                                    "Increase tier was provided for a GameOver caller! It will be set to false!");
                            }

                            newProfile.increaseTier = false;


                            if (customCCallerGameOverChosen.ConsequenceCallerID != -1)
                            {
                                LoggingHelper.WarningLog(
                                    "GameOver Callers cannot be consequence caller, ignoring option.");
                            }

                            newProfile.consequenceCallerProfile = null;

                            // Replace the GameOver caller
                            __instance.gameOverCall = newProfile;
                        }
                    }
                }

                // If any custom caller was "injected", we can now call it.

                // OLD: __instance.AnswerDynamicCall(__instance.gameOverCall);
                AnswerDynamicCall.Invoke(__instance, new object[] { __instance.gameOverCall }); 

                // OLD: __instance.triggerGameOver = true;
                TriggerGameOver.SetValue(__instance, true); 

                return false; // Skip the original function
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "GameOverCutsceneRoutine")]
        public static class GameOverCutsceneRoutinePatch
        {
            private static readonly FieldInfo ShakeAnimationString = typeof(MainCanvasBehavior).GetField(
                "shakeAnimationString",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            /// <summary>
            /// Patches the game over cutscene coroutine to also be able to play custom game over cutscenes.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to run. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                __result = GameOverCutsceneRoutineChanged(__instance);

                return false; // Skip function with false.
            }

            private static IEnumerator GameOverCutsceneRoutineChanged(MainCanvasBehavior __instance)
            {
                MainCanvasBehavior mainCanvasBehavior = __instance;

                if (ShakeAnimationString == null)
                {
                    LoggingHelper.ReflectionError(nameof(ShakeAnimationString));
                    yield break;
                }

                // Main Campaign
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    // OLD: mainCanvasBehavior.shakeAnimationString
                    mainCanvasBehavior.cameraAnimator.SetBool((string)ShakeAnimationString.GetValue(__instance), true);
                }
                else
                {
                    mainCanvasBehavior.cameraAnimator.SetBool((string)ShakeAnimationString.GetValue(__instance), true);
                }
                
                mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(
                    GlobalVariables.UISoundControllerScript.screenShakeLoop,
                    GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));

                GlobalVariables.fade.FadeIn(6f);

                if (GlobalVariables.musicControllerScript.myTrialMusicSource.isPlaying)
                {
                    GlobalVariables.musicControllerScript.StopTrialMusic();
                }

                yield return new WaitForSeconds(6f);

                mainCanvasBehavior.StartCoroutine(
                    GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(
                        GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));

                yield return new WaitForSeconds(1f);

                mainCanvasBehavior.cutsceneCanvas.SetActive(true);

                // Not in custom campaign
                if (!CustomCampaignGlobal.InCustomCampaign) 
                {
                    mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.gameOverClip;

                    if (GlobalVariables.isXmasDLC)
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.xmasGameOverClip;
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    if (!string.IsNullOrEmpty(customCampaign.GameOverCutsceneVideoName)) // If provided
                    {
                        mainCanvasBehavior.videoPlayer.url = customCampaign.GameOverCutsceneVideoName;
                    }
                    else // If not, we show the default one.
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.gameOverClip;
                    }
                }

                mainCanvasBehavior.videoPlayer.Play();

                yield return new WaitForSeconds(1f);

                GlobalVariables.fade.FadeOut(3f);

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Game
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

                    if (!string.IsNullOrEmpty(customCampaign.GameOverCutsceneVideoName)) // If provided
                    {
                        // Get video length and then wait for it.
                        mainCanvasBehavior.videoPlayer.Prepare();

                        while (mainCanvasBehavior.videoPlayer.isPlaying) // While playing we don't continue.
                        {
                            yield return null;
                        }
                    }
                    else // If not, we show the default one.
                    {
                        yield return new WaitForSeconds((float)mainCanvasBehavior.videoPlayer.clip.length);
                    }
                }

                // Don't show fired achievement in custom campaign.
                if (SteamManager.Initialized 
                    && !GlobalVariables.isXmasDLC 
                    && !CustomCampaignGlobal.InCustomCampaign) 
                {
                    SteamUserStats.SetAchievement("Fired");
                    SteamUserStats.StoreStats();
                }

                GlobalVariables.fade.FadeIn(2f);

                yield return new WaitForSeconds(2f);

                mainCanvasBehavior.RestartDay();
            }
        }
    }
}