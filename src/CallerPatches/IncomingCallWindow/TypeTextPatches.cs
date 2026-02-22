using System.Collections;
using System.Reflection;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public static class TypeTextPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "TypeText", typeof(CallerProfile), typeof(bool))]
        public static class TypeTextPatch
        {

            /// <summary>
            /// Patches the type text function to not cut off letters at the end.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="profile"> Profile of the caller. </param>
            /// <param name="skip"> If to skip. (By default: False)</param>
            /// <param name="__result"> Result of the function. In this case a coroutine. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, CallWindowBehavior __instance, ref IEnumerator __result,
                ref CallerProfile profile, ref bool skip)
            {
                __result = TypeTextCoroutine(__instance, profile, skip);
                
                return false; // Skip function with false.
            }

            private static IEnumerator TypeTextCoroutine(CallWindowBehavior __instance, CallerProfile profile,
                bool skip)
            {
                yield return null;

                if (profile == null)
                {
                    LoggingHelper.WarningLog("Profile is null. Possibly missing something?");
                    yield break;
                }
                
                int characterCount = __instance.myTranscription.textInfo.characterCount;
                int counter = 0;
                float waitTime = 0;
                
                if (profile.callerClip != null && profile.callerClip.clip != null) // If we have a valid clip.
                {
                    waitTime = profile.callerClip.clip.length / characterCount;
                }
                else // No caller clip so we skip.
                {
                    skip = true;
                } 
                
                if (skip)
                {
                    counter = characterCount + 1;
                    __instance.myTranscription.maxVisibleCharacters = counter;
                }
                
                while (counter < characterCount + 1)
                {
                    __instance.myTranscription.maxVisibleCharacters = counter;
                    if (waitTime > 0.006)
                    {
                        ++counter;
                    }
                    else
                    {
                        counter += 2;
                    }
                    
                    if (waitTime > 0.005)
                    {
                        yield return new WaitForSecondsRealtime(waitTime);
                    }
                    else
                    {
                        yield return null;
                    }
                }

                __instance.myTranscription.maxVisibleCharacters = int.MaxValue; // Ensures no missing characters.
                
                if (profile.callerMonster != null)
                {
                    __instance.holdButton.SetActive(true);
                    
                    if (GlobalVariables.arcadeMode)
                    {
                        GlobalVariables.callerControllerScript.StartCallTimerRoutine();
                    }
                }
                else
                {
                    __instance.closeButton.SetActive(true);
                }
            }
        }
    }
}