using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using TMPro;

namespace NewSafetyHelp.CustomCampaignSystem.Helper
{
    public static class AccuracyTextPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(AccuracyTextUpdate), "Start")]
        public static class AccuracyTextUpdateStartPatch
        {
            private static readonly FieldInfo MyText = typeof(AccuracyTextUpdate).GetField("myText",
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            /// <summary>
            /// Patches the start function to also allow to enable this object, if the custom campaign asks for it.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(AccuracyTextUpdate __instance)
            {
                if (MyText == null)
                {
                    LoggingHelper.ErrorLog("'myText' is null, unable of executing function." +
                                           " Calling original function.");
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

                    bool showDefaultAccuracyTextFound = false;
                    bool showDefaultUIAccuracyText = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.ShowDefaultUIAccuracyText,
                        ref showDefaultAccuracyTextFound,
                        specialPredicate: m => m.ShowDefaultUIAccuracyTextChanged);

                    if (showDefaultAccuracyTextFound)
                    {
                        if (!showDefaultUIAccuracyText)
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
    }
}