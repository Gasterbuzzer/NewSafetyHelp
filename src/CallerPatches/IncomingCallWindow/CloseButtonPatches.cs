using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public static class CloseButtonPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "CloseCallButton", new Type[] { })]
        public static class ScorecardPatch
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
                if (GlobalVariables.callerControllerScript.currentCallerID == GlobalVariables.callerControllerScript.callers.Length - 1)
                {
                    if (CustomCampaignGlobal.inCustomCampaign) // If we are not in the main campaign.
                    {
                        MelonLogger.Msg(ConsoleColor.Green, "INFO: Playing custom ending cutscene.");
                        
                        GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID].answeredCorrectly = true;
                        
                        GlobalVariables.mainCanvasScript.PlayEndingCutscene();
                    }
                    else // Main Campaign
                    {
                        GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID].answeredCorrectly = true;
                        
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
                    GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.disconnect);

                    if (CustomCampaignGlobal.inCustomCampaign)
                    {
                        // If the next caller is the last, and we skip it (Consequence caller that we got right).
                        if (isNextCallerTheLastDayCaller(GlobalVariables.callerControllerScript))
                        {
                            if (GlobalVariables.callerControllerScript.currentCallerID + 1 < GlobalVariables.callerControllerScript.callers.Length)
                            {
                                #if DEBUG
                                    MelonLogger.Msg($"DEBUG: Profile not null '{GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile != null}'." +
                                                  $" Can receive consequence: '{!GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile)}'.");
                                #endif
                                
                                if (!GlobalVariables.arcadeMode 
                                    && GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile != null 
                                    && !GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile))
                                {
                                    GlobalVariables.callerControllerScript.StopAllRoutines();
                                    GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript.EndDayRoutine());
                                    GlobalVariables.mainCanvasScript.NoCallerWindow();
                                    return false; // Skip original function.
                                }
                            }
                        }
                    }
                }
                
                return false; // Skip function with false.
            }
        }
        
        public static bool isNextCallerTheLastDayCaller(CallerController __instance)
        {
            CustomCallerExtraInfo customCallerFound =
                CustomCampaignGlobal.getCustomCallerFromActiveCampaign(__instance.currentCallerID+1);

            if (customCallerFound == null) // There might be no next caller or any errors. We simply ignore it then.
            {
                return false;
            }

            #if DEBUG
            MelonLogger.Msg(
                $"DEBUG: Last caller of day (next): '{customCallerFound.lastDayCaller}'." +
                $" Next caller name: '{customCallerFound.callerName}'.");
            #endif
                
            // If the last caller of the day, this will result in true.
            return customCallerFound.lastDayCaller;
        }
    }
}