using System.Collections;
using System.Reflection;
using NewSafetyHelp.Callers.CallerModel;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.TimedCaller
{
    public static class TimedCallerPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PlayCallAudio", typeof(CallerProfile))]
        public static class PlayCallAudioPatch
        {
            private static Coroutine playCallAudioRoutine;
            
            private static readonly MethodInfo PlayCallAudioRoutineMethod = typeof(CallerController).GetMethod("PlayCallAudioRoutine",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            /// <summary>
            /// A patch that stores a reference to the started coroutine, so that later, a function may stop it.
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="profile"></param>
            /// <returns></returns>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance, CallerProfile profile)
            {
                IEnumerator playCallAudioWithProfile = (IEnumerator) PlayCallAudioRoutineMethod.Invoke(__instance, new object[] {profile});
                
                // OLD: __instance.PlayCallAudioRoutine(profile)
                playCallAudioRoutine = __instance.StartCoroutine(playCallAudioWithProfile);
                GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                
                return false; // Skip the original coroutine
            }

            /// <summary>
            /// Stops the call audio.
            /// </summary>
            public static void StopCallAudioRoutine()
            {
                if (playCallAudioRoutine != null)
                {
                    GlobalVariables.callerControllerScript.StopCoroutine(playCallAudioRoutine);
                }
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "CloseButton", typeof(bool), typeof(bool))]
        public static class HoldButtonClosePatch
        {
            /// <summary>
            /// Patches the hold buttons function (that closes the popup) to start timers for custom callers if necessary.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="playHoldSound">If to play the hold sound effect. ("Please Hold")</param>
            /// <param name="playHoldMusic">If to play the hold music (for callers with an entry).</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallWindowBehavior __instance, ref bool playHoldSound, ref bool playHoldMusic)
            {
                // Stop any active audio.
                GlobalVariables.callerControllerScript.StopCallAudio();
                
                // Stop any call coroutines (that are attempting to play the audio in a bit)
                PlayCallAudioPatch.StopCallAudioRoutine();
                
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
                
                GlobalVariables.callerControllerScript.StopCallAudio();
                
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