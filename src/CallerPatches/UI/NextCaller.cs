using MelonLoader;
using NewSafetyHelp.CallerPatches.IncomingCallWindow;
using NewSafetyHelp.CustomCampaign;

namespace NewSafetyHelp.CallerPatches.UI
{
    public static class NextCaller
    {
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "NextCallButton")]
        public static class NextCallButtonPatch
        {
            /// <summary>
            /// Patches the NextCallButton logic to handle custom campaign behavior and end-of-day conditions.
            /// When in a custom campaign, it checks if the next caller to be skipped is the last caller of the day
            /// and, if so, informs the player instead of silently skipping. It also prevents advancing when the
            /// day is ending (showing a message and closing the caller window), or otherwise stops current routines
            /// and opens the call window for the next caller, skipping the original method implementation.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MainCanvasBehavior __instance)
            {
                // Check for next caller that will be skipped.
                if (CustomCampaignGlobal.inCustomCampaign)
                {
                    // If the next caller is the last, and we skip it (Consequence caller that we got right).
                    if (CloseButtonPatches.isNextCallerTheLastDayCaller(GlobalVariables.callerControllerScript))
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
                                GlobalVariables.mainCanvasScript.CreateError("Day is ending. Please hold.");
                                GlobalVariables.mainCanvasScript.NoCallerWindow();
                                return false; // Skip original function.
                            }
                        }
                    }
                }
                
                if (GlobalVariables.callerControllerScript.IsLastCallOfDay() && !GlobalVariables.arcadeMode)
                {
                    GlobalVariables.mainCanvasScript.CreateError("Day is ending. Please hold.");
                    GlobalVariables.mainCanvasScript.NoCallerWindow();
                }
                else
                {
                    GlobalVariables.callerControllerScript.StopAllRoutines();
                    __instance.callWindow.SetActive(true);
                }

                return false; // Skip the original function
            }
        }
    }
}