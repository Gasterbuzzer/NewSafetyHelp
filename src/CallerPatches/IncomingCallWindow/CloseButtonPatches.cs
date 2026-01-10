using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public static class CloseButtonPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "CloseCallButton", new Type[] { })]
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
        /// <returns> If we wound any caller that fits the above criteria. </returns>
        public static int CheckIfAnyValidCallerLeft(CallerController __instance)
        {
            int callersSkipped = 0;
            
            for (int i = __instance.currentCallerID + 1; i < __instance.callers.Length; i++)
            {
                if (i < __instance.callers.Length) // Valid caller.
                {
                    CallerModel.CustomCaller customCallerFound = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(i);

                    // If the next caller does not exist or was not found, we simply say false.
                    // There might be valid callers after that one, but we are in an invalid state.
                    if (customCallerFound == null)
                    {
                        return -1;
                    }

                    #if DEBUG
                    MelonLogger.Msg(
                        $"DEBUG: Last caller of day (Caller ID: {i}): '{customCallerFound.LastDayCaller}'." +
                        $" Next caller name (Caller ID: {i}): '{customCallerFound.CallerName}'.");
                    #endif

                    #if DEBUG
                    MelonLogger.Msg(
                        "DEBUG: Is ConsequenceProfile null? (Meaning it's this current caller is a consequence caller):" +
                        $" '{GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile != null}'" +
                        ".\n" +
                        " Is this caller allowed to be called? (Meaning we got the answer wrong from the previous caller): " +
                        $"'{GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile)}'" +
                        ".\n " +
                        $"Is this caller the last one of the day? '{customCallerFound.LastDayCaller}'.");
                    #endif

                    // Not a consequence caller. We can simply return false, since we have a valid caller.
                    if (GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile ==
                        null)
                    {
                        return -1;
                    }

                    callersSkipped++;

                    // If that caller ends the day.
                    if (customCallerFound.LastDayCaller
                        && !GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile))
                    {
                        return callersSkipped;
                    }
                }
            }

            // Nothing found.
            return -1;
        }
    }
}