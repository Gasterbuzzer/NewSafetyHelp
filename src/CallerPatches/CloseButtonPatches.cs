using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;

namespace NewSafetyHelp.CallerPatches
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
                    else
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
                }
                
                return false; // Skip function with false.
            }
        }
    }
}