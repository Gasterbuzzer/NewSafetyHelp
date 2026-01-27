using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.Helper;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
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
                        MelonLogger.Msg(ConsoleColor.Green, "INFO: Playing custom ending cutscene.");

                        GlobalVariables.callerControllerScript
                            .callers[GlobalVariables.callerControllerScript.currentCallerID].answeredCorrectly = true;

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
                        int checkResult = CheckIfAnyValidCallerLeft(GlobalVariables.callerControllerScript);
                        
                        if (checkResult > 0)
                        {
                            #if DEBUG
                            MelonLogger.Msg(ConsoleColor.DarkYellow, "DEBUG: Calling end day routine from close button.");
                            #endif
                            
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

        /// <summary>
        /// Checks if for the next 'n' callers there is any caller that is valid and should be shown and if any of these should end the day.
        /// </summary>
        /// <param name="__instance">CallerController Instance</param>
        /// <returns> (If a valid caller was found) -1; (If all the callers are skipped between the last caller) </returns>
        public static int CheckIfAnyValidCallerLeft(CallerController __instance)
        {
            int callersSkipped = 0;
            
            for (int i = __instance.currentCallerID + 1; i < __instance.callers.Length; i++)
            {
                if (i < __instance.callers.Length) // Valid caller.
                {
                    CallerModel.CustomCCaller customCCallerFound = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(i);

                    // If the next caller does not exist or was not found, we simply say false.
                    // There might be valid callers after that one, but we are in an invalid state.
                    if (customCCallerFound == null)
                    {
                        return -1;
                    }

                    // Checks for seeing if the next caller is valid:
                    
                    // 1. Is an accuracy caller that will be shown.
                    
                    // 2. If any valid caller comes afterward. (One that cannot be skipped)
                    
                    // 3. Is a consequence caller that will be shown.
                    
                    #if DEBUG
                    MelonLogger.Msg(ConsoleColor.DarkMagenta,
                        $"DEBUG: Last caller of day (Caller ID: {i}): '{customCCallerFound.LastDayCaller}'." +
                        $" Next caller name (Caller ID: {i}): '{customCCallerFound.CallerName}'." +
                        $" Is a accuracy caller?: '{customCCallerFound.IsAccuracyCaller}'.");
                    #endif

                    #if DEBUG
                    MelonLogger.Msg(ConsoleColor.DarkMagenta,
                        "DEBUG: Is ConsequenceProfile not null? (Meaning it's this current caller is a consequence caller):" +
                        $" '{GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile != null}'" +
                        ".\n" +
                        " Is this caller allowed to be called? (Meaning we got the answer wrong from the previous caller): " +
                        $"'{GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile)}'" +
                        ".\n " +
                        $"Is this caller the last one of the day? '{customCCallerFound.LastDayCaller}'.");
                    #endif

                    // Consequence caller
                    bool isConsequenceCaller = false;
                    if (GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile != null)
                    {
                        isConsequenceCaller = true;
                        // This consequence caller is supposed to be called, since the player got the response wrong.
                        if (GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile))
                        {
                            return -1;
                        }
                        
                        callersSkipped++;
                        
                        // Caller is supposed to be skipped. So we simply give the amount to skip.
                        if (customCCallerFound.LastDayCaller)
                        {
                            return callersSkipped;
                        }
                    }
                    
                    // If accuracy caller
                    if (customCCallerFound.IsAccuracyCaller)
                    {
                        bool showCaller = AccuracyHelper.CheckIfCallerIsToBeShown(customCCallerFound);
                        
                        // Accuracy caller that is supposed to be called, since its condition was fulfilled.
                        if (customCCallerFound.IsAccuracyCaller && showCaller)
                        {
                            return -1;
                        }
                        
                        callersSkipped++;

                        // Last caller of the day that is supposed to be skipped.
                        if (customCCallerFound.LastDayCaller)
                        {
                            if (!showCaller)
                            {
                                return callersSkipped;
                            }
                        }
                    }
                    
                    // If not a consequence caller or an accuracy caller, we simply return, since it's a normal caller.
                    if (!isConsequenceCaller && !customCCallerFound.IsAccuracyCaller)
                    {
                        return -1;
                    }
                }
            }

            // Nothing found.
            return -1;
        }
    }
}