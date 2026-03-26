using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.LoggingSystem;
using TMPro;

namespace NewSafetyHelp.CustomCampaignSystem.Helper
{
    public static class AccuracyTextPatches
    {
        private static readonly FieldInfo MyText = typeof(AccuracyTextUpdate).GetField("myText",
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        [HarmonyLib.HarmonyPatch(typeof(AccuracyTextUpdate), "Start")]
        public static class AccuracyTextUpdateStartPatch
        {
            /// <summary>
            /// Patches the start function to also allow to enable this object, if the custom campaign asks for it.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(AccuracyTextUpdate __instance)
            {
                if (MyText == null)
                {
                    LoggingHelper.ReflectionError(nameof(MyText));
                    return true;
                }
                
                // OLD: __instance.myText = ...;
                MyText.SetValue(__instance, __instance.GetComponent<TextMeshProUGUI>());
                
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }
                    
                    // Fix in case we have no valid callers, we simply set it to 100%.
                    if (MyText != null)
                    {
                        TextMeshProUGUI textMeshProUGUIComponent = MyText.GetValue(__instance) as TextMeshProUGUI;
                        if (textMeshProUGUIComponent != null 
                            && textMeshProUGUIComponent.text.Equals("ACCURACY RATE: 50%")
                            && GlobalVariables.callerControllerScript.callersToday <= 0)
                        {
                            textMeshProUGUIComponent.text = "ACCURACY RATE: 100%";
                        }
                    }
                    
                    (bool foundModifier, VariableChanged<bool> value) showDefaultUIAccuracyText = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.ShowDefaultUIAccuracyText, vCb => vCb.HasChanged);

                    if (showDefaultUIAccuracyText.foundModifier)
                    {
                        if (!showDefaultUIAccuracyText.value.Data)
                        {
                            __instance.gameObject.SetActive(false);
                        }
                        
                        return false;
                    }
                }
                
                // Base Game Logic
                if (!__instance.onlyDisplayWhenAccuracyOptionIsOn 
                    || GlobalVariables.saveManagerScript.savedAccuracyToggle != 0)
                {
                    return false;
                }
                
                __instance.gameObject.SetActive(false);
                
                return false; // Skip original
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "GetScore")]
        public static class CallerControllerGetScorePatch
        {
            /// <summary>
            /// Patches the GetScore function to not break if no correct callers exist.
            /// </summary>
            /// <param name="__result">Result of the function.</param>
            /// <returns>If to skip the function.</returns>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref float __result)
            {
                if (GlobalVariables.callerControllerScript.callersToday <= 0
                    && GlobalVariables.callerControllerScript.correctCallsToday <= 0)
                {
                    __result = 100.0f;
                    return false;
                }
                
                __result = (float) ((double) GlobalVariables.callerControllerScript.correctCallsToday / GlobalVariables.callerControllerScript.callersToday * 100.0);
                
                return false; // Skip original
            }
        }
    }
}