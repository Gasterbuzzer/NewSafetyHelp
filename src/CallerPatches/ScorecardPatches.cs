using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedMember.Local

namespace NewSafetyHelp.CallerPatches
{
    public static class ScorecardPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(ScorecardBehavior), "PopulateList", new Type[] { })]
        public static class ScorecardPatch
        {

            /// <summary>
            /// Patches the scorecard function to respect custom callers correctly.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(MethodBase __originalMethod, ScorecardBehavior __instance)
            {
        
                Type scorecardBehavior = typeof(ScorecardBehavior);
                
                MethodInfo clearListMethod = scorecardBehavior.GetMethod("ClearList", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                            
                if (clearListMethod == null)
                {
                    MelonLogger.Error("ERROR: clearListMethod is null!");
                    return true;
                }
                
                // Clear List
                clearListMethod.Invoke(__instance, new object[] { });
                
                UnityEngine.Object.Instantiate<GameObject>(__instance.dayListing, __instance.contentHolder);
                
                int dayAmount = 1;
                
                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    foreach (Caller caller in GlobalVariables.callerControllerScript.callers)
                    {
                        if (caller.callerProfile.increaseTier)
                        {
                            ++dayAmount;
                            UnityEngine.Object.Instantiate<GameObject>(__instance.dayListing, __instance.contentHolder).GetComponentInChildren<TextMeshProUGUI>().text = "Day " + dayAmount.ToString();
                        }
                        else if (caller.callerProfile.callerMonster !=  null)
                        {
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.callerListing, __instance.contentHolder);
                            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = caller.callerProfile.callerName;
                            gameObject.GetComponentInChildren<Toggle>().isOn = caller.answeredCorrectly;
                        }
                    }
                }
                else // Custom Campaign
                {
                    int currentCallerIndex = 0;
                    
                    foreach (Caller caller in GlobalVariables.callerControllerScript.callers)
                    {
                        CustomCallerExtraInfo customCallerFound = CustomCampaignGlobal.getCustomCampaignCustomCallerByOrderID(currentCallerIndex);
                        
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding: {customCallerFound.callerName}.");
                        #endif
                        
                        if (customCallerFound.lastDayCaller)
                        {
                            if (caller.callerProfile.callerMonster !=  null) // Since custom callers could also be the last caller. We also create a profile for them.
                            {
                                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.callerListing, __instance.contentHolder);
                                gameObject.GetComponentInChildren<TextMeshProUGUI>().text = caller.callerProfile.callerName;
                                gameObject.GetComponentInChildren<Toggle>().isOn = caller.answeredCorrectly;
                            }
                            
                            ++dayAmount;

                            if (currentCallerIndex + 1 != GlobalVariables.callerControllerScript.callers.Length) // We are not the last caller.
                            {
                                UnityEngine.Object.Instantiate<GameObject>(__instance.dayListing, __instance.contentHolder).GetComponentInChildren<TextMeshProUGUI>().text = "Day " + dayAmount.ToString();  
                            }
                            
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Scorecard: Caller is last day caller.");
                            #endif
                        }
                        else if (caller.callerProfile.callerMonster !=  null)
                        {
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.callerListing, __instance.contentHolder);
                            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = caller.callerProfile.callerName;
                            gameObject.GetComponentInChildren<Toggle>().isOn = caller.answeredCorrectly;
                        }

                        currentCallerIndex++;
                    }
                    
                }
                
                return false; // Skip function with false.
            }
        }
    }
}