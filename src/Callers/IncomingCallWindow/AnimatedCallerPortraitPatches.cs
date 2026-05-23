using System.Collections;
using System.Reflection;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.Callers.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.TimedCaller;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.Callers.IncomingCallWindow
{
    public static class AnimatedCallerPortraitPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "UpdateCallerInfo")]
        public static class UpdateCallerInfoPatch
        {
            private static readonly MethodInfo TypeText = typeof(CallWindowBehavior).GetMethod("TypeText", BindingFlags.NonPublic | BindingFlags.Instance);
            private static readonly FieldInfo TypeRoutine = typeof(CallWindowBehavior).GetField("typeRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            
            /// <summary>
            /// Updates the caller info of the call window to the current caller.
            /// Plus I have added the animated portrait option.
            /// </summary>
            /// <param name="__instance"> Instance of the class calling the function. </param>
            /// <returns></returns>
            // ReSharper disable once UnusedMember.Global
            public static bool Prefix(CallWindowBehavior __instance)
            {
                if (TypeText == null || TypeRoutine == null)
                {
                    LoggingHelper.ReflectionError(nameof(TypeText),
                        nameof(TypeRoutine));
                    return true;
                }
                
                CallerProfile currentCallerProfile = GlobalVariables.callerControllerScript.currentCallerProfile;
                
                __instance.myPortrait.sprite = currentCallerProfile.callerPortrait;

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCCaller currentCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);

                    if (currentCaller != null 
                        && currentCaller.CallerHasAnimatedPortrait)
                    {
                        MainCanvasEntry.SetVideoUrl(currentCaller.CallerAnimatedPortraitURL,
                            MainCanvasEntry.PortraitType.CALLER);
                    }
                    else
                    {
                        MainCanvasEntry.RestorePortrait(MainCanvasEntry.PortraitType.CALLER);
                    }
                }
                
                __instance.myName.text = "CURRENT CALLER: " + currentCallerProfile.callerName.ToUpper();
                __instance.myTranscription.text = currentCallerProfile.callTranscription;
                __instance.myTranscription.maxVisibleCharacters = 0;
                __instance.holdButton.SetActive(false);
                __instance.submitButton.SetActive(false);
                __instance.closeButton.SetActive(false);
                
                // OLD: __instance.TypeText(currentCallerProfile)
                IEnumerator typeTextOfCaller = (IEnumerator) TypeText.Invoke(__instance,
                    new object[] {currentCallerProfile, false});
                
                // OLD: __instance.typeRoutine = __instance.StartCoroutine(typeTextOfCaller);
                TypeRoutine.SetValue(__instance, __instance.StartCoroutine(typeTextOfCaller));
                
                return false; // Skips original function.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "UpdateCallerInfo", typeof(CallerProfile))]
        public static class UpdateCallerInfoCornerPortraitPatch
        {
            private static readonly MethodInfo UpdateLayoutGroup = 
                typeof(MainCanvasBehavior).GetMethod("UpdateLayoutGroup",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            
            /// <summary>
            /// Updates the caller info of the left upper corner to show the caller.
            /// Plus I have added the animated portrait option.
            /// </summary>
            /// <param name="__instance"> Instance of the class calling the function. </param>
            /// <param name="profile">Profile that will be shown.</param>
            /// <returns></returns>
            // ReSharper disable once UnusedMember.Global
            public static bool Prefix(MainCanvasBehavior __instance, ref CallerProfile profile)
            {
                if (UpdateLayoutGroup == null)
                {
                    LoggingHelper.ReflectionError(nameof(UpdateLayoutGroup));
                    return true;
                }
                
                __instance.callerNameText.text = "CURRENT CALLER: " + profile.callerName.ToUpper();
                
                __instance.callerPortrait.sprite = profile.callerPortrait;
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCCaller currentCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);

                    if (currentCaller != null 
                        && currentCaller.CallerHasAnimatedPortrait)
                    {
                        MainCanvasEntry.SetVideoUrl(currentCaller.CallerAnimatedPortraitURL,
                            MainCanvasEntry.PortraitType.CORNER_CALLER);
                    }
                    else
                    {
                        MainCanvasEntry.RestorePortrait(MainCanvasEntry.PortraitType.CORNER_CALLER);
                    }
                    
                    // For timed caller:
                    if (currentCaller != null
                        && currentCaller.IsTimedCaller)
                    {
                        TimerCallerHelper.ShowCallerTimerUI(__instance, currentCaller);
                    }
                    else
                    {
                        TimerCallerHelper.HideCallerTimerUI();
                    }
                }
                
                __instance.largeCallerPortrait.sprite = profile.callerPortrait;
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCCaller currentCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);

                    if (currentCaller != null 
                        && currentCaller.CallerHasAnimatedPortrait)
                    {
                        MainCanvasEntry.SetVideoUrl(currentCaller.CallerAnimatedPortraitURL,
                            MainCanvasEntry.PortraitType.LARGE_CALLER);
                    }
                    else
                    {
                        MainCanvasEntry.RestorePortrait(MainCanvasEntry.PortraitType.LARGE_CALLER);
                    }
                }
                
                __instance.callerTranscription.text = profile.callTranscription;
                
                // OLD: __instance.UpdateLayoutGroup(__instance.transcriptionLayoutGroup)
                IEnumerator updateTranscriptionLayoutGroup = (IEnumerator) UpdateLayoutGroup.Invoke(__instance,
                    new object[] { __instance.transcriptionLayoutGroup });
                
                __instance.StartCoroutine(updateTranscriptionLayoutGroup);
                
                return false; // Skips original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "NoCallerWindow")]
        public static class NoCallerWindowPatch
        {
            /// <summary>
            /// Updates the no caller function to not break with animated caller large portrait.
            /// </summary>
            /// <param name="__instance"> Instance of the class calling the function. </param>
            /// <returns>If this function skips the original.</returns>
            // ReSharper disable once UnusedMember.Global
            public static bool Prefix(MainCanvasBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    MainCanvasEntry.RestorePortrait(MainCanvasEntry.PortraitType.LARGE_CALLER);
                }
                
                __instance.largeCallerPortrait.sprite = __instance.noCallerSprite;
                __instance.largeCallerPortrait.gameObject.SetActive(true);
                
                __instance.callerNameText.text = "NO CURRENT CALLERS";
                __instance.callerTranscription.text = "";
                
                return false; // Skips original function.
            }
        }
    }
}