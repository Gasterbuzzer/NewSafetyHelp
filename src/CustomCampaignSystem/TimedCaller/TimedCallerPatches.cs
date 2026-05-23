using NewSafetyHelp.Callers.CallerModel;

namespace NewSafetyHelp.CustomCampaignSystem.TimedCaller
{
    public static class TimedCallerPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "CloseButton", typeof(bool), typeof(bool))]
        public static class HoldButtonClosePatch
        {
            /// <summary>
            /// If the caller is already set on hold.
            /// </summary>
            public static bool IsInHold = true;
            
            /// <summary>
            /// Patches the hold buttons function (that closes the popup) to start timers for custom callers if necessary.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="playHoldSound">If to play the hold sound effect. ("Please Hold")</param>
            /// <param name="playHoldMusic">If to play the hold music (for callers with an entry).</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallWindowBehavior __instance, ref bool playHoldSound, ref bool playHoldMusic)
            {
                IsInHold = true;
                
                if (playHoldSound)
                {
                    GlobalVariables.UISoundControllerScript.PlayUISound(
                        GlobalVariables.UISoundControllerScript.holdPlease);
                }

                if (playHoldMusic)
                {
                    GlobalVariables.musicControllerScript.StartRandomMusic();
                    GlobalVariables.mainCanvasScript.largeCallerPortrait.gameObject.SetActive(false);
                }
                
                GlobalVariables.callerControllerScript.StopLargeWindowRoutine();
                
                __instance.gameObject.SetActive(false);
                
                // If we start the timer of the timed caller.
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCCaller currentCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);

                    if (currentCaller != null
                        && currentCaller.IsTimedCaller)
                    {
                        TimerCallerHelper.StartTimedCallerTimer(currentCaller.TimedCallerDuration);
                    }
                }
                
                // Moving the "StopCallAudio" lower, possibly fixes the timing mismatch of not stopping the caller audio.
                // Also updated the starting of the caller audio to not happen if halted.
                GlobalVariables.callerControllerScript.StopCallAudio();

                return false; // Skip function with false.
            }
        }
    }
}