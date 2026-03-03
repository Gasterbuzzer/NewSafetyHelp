using System.Collections;
using System.Reflection;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CallerPatches.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public static class AnimatedCallerPortraitPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "UpdateCallerInfo")]
        public static class UpdateCallerInfoPatch
        {
            private static readonly MethodInfo typeText = typeof(CallWindowBehavior).GetMethod("TypeText", BindingFlags.NonPublic | BindingFlags.Instance);
            private static readonly FieldInfo typeRoutine = typeof(CallWindowBehavior).GetField("typeRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            
            /// <summary>
            /// Updates the caller info of the call window to the current caller.
            /// Plus I have added the animated portrait option.
            /// </summary>
            /// <param name="__instance"> Instance of the class calling the function. </param>
            /// <returns></returns>
            // ReSharper disable once UnusedMember.Global
            public static bool Prefix(CallWindowBehavior __instance)
            {
                if (typeText == null || typeRoutine == null)
                {
                    LoggingHelper.ErrorLog("'typeText' or 'typeRoutine' is null. Calling original function.");
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
                IEnumerator typeTextOfCaller = (IEnumerator) typeText.Invoke(__instance,
                    new object[] {currentCallerProfile, false});
                
                // OLD: __instance.typeRoutine = __instance.StartCoroutine(typeTextOfCaller);
                typeRoutine.SetValue(__instance, __instance.StartCoroutine(typeTextOfCaller));
                
                return false; // Skips original function.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "UpdateCallerInfo", typeof(CallerProfile))]
        public static class UpdateCallerInfoCornerPortraitPatch
        {
            private static readonly MethodInfo updateLayoutGroup = typeof(MainCanvasBehavior).GetMethod("UpdateLayoutGroup", BindingFlags.NonPublic | BindingFlags.Instance);

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
                if (updateLayoutGroup == null)
                {
                    LoggingHelper.ErrorLog("'updateLayoutGroup' is null. Calling original function.");
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
                IEnumerator updateTranscriptionLayoutGroup = (IEnumerator) updateLayoutGroup.Invoke(__instance,
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