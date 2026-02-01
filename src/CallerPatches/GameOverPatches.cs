using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CustomCampaign;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.CallerPatches
{
    public static class GameOverPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "TriggerGameOver", new Type[] { })]
        public static class TriggerGameOverPatch
        {
            /// <summary>
            /// This function calls the GameOver phone call and triggers the game over cutscene. It is patched to be able to have custom GameOver Callers in custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Triggering GameOver Call + GameOver Cutscene.");
                #endif

                MethodInfo answerDynamicCall = typeof(CallerController).GetMethod("AnswerDynamicCall",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo triggerGameOver = typeof(CallerController).GetField("triggerGameOver",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

                if (answerDynamicCall == null || triggerGameOver == null)
                {
                    MelonLogger.Error(
                        "ERROR: AnswerDynamicCall or triggerGameOver is null. Calling original function.");
                    return true;
                }

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null. Calling original function.");
                        return true;
                    }

                    if (customCampaign.CustomGameOverCallersInCampaign.Count > 0)
                    {
                        CallerModel.CustomCCaller customCCallerGameOverChosen = null;

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
                            #if DEBUG
                            MelonLogger.Msg(
                                $"DEBUG: GameOver caller found to replace! {customCCallerGameOverChosen.CallerName}.");
                            #endif

                            CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                            newProfile.callerName = customCCallerGameOverChosen.CallerName;
                            newProfile.callTranscription = customCCallerGameOverChosen.CallTranscript;

                            // Fallback for missing picture or audio.
                            MethodInfo getRandomPicMethod = typeof(CallerController).GetMethod("PickRandomPic",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                            MethodInfo getRandomClip = typeof(CallerController).GetMethod("PickRandomClip",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                            if (getRandomPicMethod == null || getRandomClip == null)
                            {
                                MelonLogger.Error(
                                    "ERROR: getRandomPicMethod or getRandomClip is null! Calling original function.");
                                return true;
                            }

                            if (customCCallerGameOverChosen.CallerImage != null)
                            {
                                newProfile.callerPortrait = customCCallerGameOverChosen.CallerImage;
                            }
                            else
                            {
                                MelonLogger.Warning(
                                    "WARNING: GameOver-Caller has no caller image, using random image.");

                                newProfile.callerPortrait = (Sprite)getRandomPicMethod.Invoke(__instance, null);
                            }

                            if (customCCallerGameOverChosen.CallerClip != null)
                            {
                                newProfile.callerClip = customCCallerGameOverChosen.CallerClip;
                            }
                            else
                            {
                                if (AudioImport.CurrentLoadingAudios.Count > 0)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: GameOver-Caller audio is still loading! Using fallback for now. If this happens often, please check if the audio is too large!");
                                }
                                else
                                {
                                    MelonLogger.Warning(
                                        "WARNING: GameOver-Caller has no audio! Using audio fallback. If you provided an audio but this error shows up, check for any errors before!");
                                }

                                newProfile.callerClip = (RichAudioClip)getRandomClip.Invoke(__instance, null);
                            }

                            if (!string.IsNullOrEmpty(customCCallerGameOverChosen.MonsterNameAttached) ||
                                customCCallerGameOverChosen.MonsterIDAttached != -1)
                            {
                                MelonLogger.Warning(
                                    "WARNING: A monster was provided for the GameOver caller, but GameOver callers do not use any entries! Will default to none.");
                            }

                            newProfile.callerMonster = null;


                            if (customCCallerGameOverChosen.CallerIncreasesTier)
                            {
                                MelonLogger.Warning(
                                    "WARNING: Increase tier was provided for a GameOver caller! It will be set to false!");
                            }

                            newProfile.increaseTier = false;


                            if (customCCallerGameOverChosen.ConsequenceCallerID != -1)
                            {
                                MelonLogger.Warning(
                                    "WARNING: GameOver Callers cannot be consequence caller, ignoring option.");
                            }

                            newProfile.consequenceCallerProfile = null;

                            __instance.gameOverCall = newProfile; // Replace the GameOver caller
                        }
                    }
                }

                // If any custom caller was "injected", we can now call it.

                answerDynamicCall.Invoke(__instance,
                    new object[] { __instance.gameOverCall }); //__instance.AnswerDynamicCall(__instance.gameOverCall);

                triggerGameOver.SetValue(__instance, true); //__instance.triggerGameOver = true;

                return false; // Skip the original function
            }
        }
    }
}