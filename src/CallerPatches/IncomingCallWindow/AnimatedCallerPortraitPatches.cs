using System.Collections;
using System.Reflection;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CallerPatches.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignPatches;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public class AnimatedCallerPortraitPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "UpdateCallerInfo")]
        public static class UpdateCallerInfoPatch
        {
            private static MethodInfo typeText = typeof(CallWindowBehavior).GetMethod("TypeText", BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo typeRoutine = typeof(CallWindowBehavior).GetField("typeRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            
            public static bool Prefix(CallWindowBehavior __instance)
            {
                CallerProfile currentCallerProfile = GlobalVariables.callerControllerScript.currentCallerProfile;
                
                __instance.myPortrait.sprite = currentCallerProfile.callerPortrait;

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCCaller currentCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);

                    if (currentCaller != null 
                        && currentCaller.CallerHasAnimatedPortrait)
                    {
                        MainCanvasEntry.SetVideoUrlCaller(currentCaller.CallerAnimatedPortraitURL);
                    }
                    else
                    {
                        MainCanvasEntry.RestoreCallerPortrait();
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
    }
}