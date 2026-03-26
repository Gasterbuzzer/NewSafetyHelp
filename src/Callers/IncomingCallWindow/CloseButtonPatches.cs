using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Intermission;
using NewSafetyHelp.Callers.CallerHelpers;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.Callers.IncomingCallWindow
{
    public static class CloseButtonPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "CloseCallButton")]
        public static class CloseCallPatches
        {
            /// <summary>
            /// Patches the close call button to play cutscenes only when in main campaign.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(MethodBase __originalMethod, CallWindowBehavior __instance)
            {
                if (GlobalVariables.callerControllerScript.currentCallerID ==
                    GlobalVariables.callerControllerScript.callers.Length - 1)
                {
                    if (CustomCampaignGlobal.InCustomCampaign) // If we are not in the main campaign.
                    {
                        LoggingHelper.InfoLog("Playing custom ending cutscene.", consoleColor:ConsoleColor.Green);

                        GlobalVariables.callerControllerScript
                            .callers[GlobalVariables.callerControllerScript.currentCallerID].answeredCorrectly = true;
                        
                        CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                        if (customCampaign != null)
                        {
                            float dayScore = GlobalVariables.callerControllerScript.GetScore();

                            // No callers for that day, so we simply set it to 100%.
                            if (float.IsNaN(dayScore) || float.IsInfinity(dayScore))
                            {
                                dayScore = 100.0f;
                            }
                            
                            customCampaign.SavedDayScores[GlobalVariables.currentDay] = dayScore;
                                
                            LoggingHelper.DebugLog($"(Custom Ending) Saving day score of day '{GlobalVariables.currentDay}'. " +
                                                   $"With the score of '{customCampaign.SavedDayScores[GlobalVariables.currentDay]}'.");
                        }

                        GlobalVariables.mainCanvasScript.PlayEndingCutscene();
                    }
                    else // Main Campaign
                    {
                        GlobalVariables.callerControllerScript
                            .callers[GlobalVariables.callerControllerScript.currentCallerID].answeredCorrectly = true;

                        if (!GlobalVariables.isXmasDLC)
                        {
                            __instance.faeCarolRoot.SetActive(true);
                            __instance.currentCallRoot.SetActive(false);
                            GlobalVariables.callerControllerScript.StopCallAudio();
                            GlobalVariables.UISoundControllerScript.StopUISoundLooping();
                            GlobalVariables.callerControllerScript.StopLargeWindowRoutine();
                            GlobalVariables.UISoundControllerScript.PlayUISound(__instance.faeCarolClip);
                        }

                        GlobalVariables.mainCanvasScript.PlayEndingCutscene();
                    }
                }
                else
                {
                    GlobalVariables.callerControllerScript.SubmitAnswer();
                    GlobalVariables.UISoundControllerScript.StopUISoundLooping();
                    __instance.CloseButton(false, false);
                    GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript
                        .disconnect);

                    if (CustomCampaignGlobal.InCustomCampaign && !GlobalVariables.arcadeMode)
                    {
                        // If the next caller is the last, and we skip it (Consequence caller that we got right).
                        int checkResult = CallerSkipping.CheckIfAnyValidCallerLeft(GlobalVariables.callerControllerScript);
                        
                        if (checkResult > 0)
                        {
                            LoggingHelper.DebugLog("Calling end day routine from close button.");
                            
                            // In case the intermission music is playing, we stop it.
                            MelonCoroutines.Start(IntermissionMusicHelper.StopIntermissionMusic());
                            
                            GlobalVariables.callerControllerScript.currentCallerID += checkResult; // Increase caller ID, since we are skipping callers.
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript.EndDayRoutine());
                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false; // Skip original function.
                        }
                    }
                }

                return false; // Skip function with false.
            }
        }
    }
}