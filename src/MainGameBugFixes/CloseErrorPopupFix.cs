using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class CloseErrorPopupFix
    {
        [HarmonyLib.HarmonyPatch(typeof(GenericErrorPopupBehavior), "ExitButton")]
        public static class ExitButtonFix
        {

            /// <summary>
            /// Fixes the error popup to actually close instead of creating another error.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, GenericErrorPopupBehavior __instance)
            {
                // Close popup instead of creating another error popup.
                __instance.ConfirmButton();
                
                return false; // Skip the original function
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(GenericErrorPopupBehavior), "OnEnable")]
        public static class OnEnableFix
        {
            /// <summary>
            /// Fixes the error popup from having two buttons.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static void Prefix(MethodBase __originalMethod, GenericErrorPopupBehavior __instance)
            {
                GameObject closeButton = __instance.transform.Find("WindowsBar").Find("CloseButton").gameObject;

                if (closeButton != null && closeButton.GetComponents<Button>().Length >= 2)
                {
                    Object.Destroy(closeButton.GetComponent<Button>());
                }
            }
        }
    }
}