using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
                
                Object.Instantiate(__instance.dayListing, __instance.contentHolder);
                
                int dayAmount = 1;
                
                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    foreach (Caller caller in GlobalVariables.callerControllerScript.callers)
                    {
                        if (caller.callerProfile.increaseTier)
                        {
                            ++dayAmount;
                            Object.Instantiate(__instance.dayListing, __instance.contentHolder).GetComponentInChildren<TextMeshProUGUI>().text = "Day " + dayAmount.ToString();
                        }
                        else if (caller.callerProfile.callerMonster !=null)
                        {
                            GameObject gameObject = Object.Instantiate(__instance.callerListing, __instance.contentHolder);
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
                        if (caller == null)
                        {
                            MelonLogger.Warning($"WARNING: There seems to be an empty custom caller. Possible missing caller? (Index {currentCallerIndex})");
                            continue;
                        }
                        
                        CustomCallerExtraInfo customCallerFound = CustomCampaignGlobal.getCustomCallerFromActiveCampaign(currentCallerIndex);

                        if (customCallerFound == null)
                        {
                            MelonLogger.Warning($"WARNING: There seems to be an empty custom caller. Possible missing caller? (Index {currentCallerIndex})");
                            continue;
                        }
                        
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Adding: {customCallerFound.callerName}. (Info: {caller.answeredCorrectly})");
                        #endif
                        
                        if (customCallerFound.lastDayCaller)
                        {
                            if (caller.callerProfile.callerMonster !=  null) // Since custom callers could also be the last caller. We also create a profile for them.
                            {
                                GameObject gameObject = Object.Instantiate(__instance.callerListing, __instance.contentHolder);
                                gameObject.GetComponentInChildren<TextMeshProUGUI>().text = caller.callerProfile.callerName;
                                gameObject.GetComponentInChildren<Toggle>().isOn = caller.answeredCorrectly;
                            }
                            
                            ++dayAmount;

                            if (currentCallerIndex + 1 != GlobalVariables.callerControllerScript.callers.Length) // We are not the last caller.
                            {
                                Object.Instantiate(__instance.dayListing, __instance.contentHolder).GetComponentInChildren<TextMeshProUGUI>().text = "Day " + dayAmount.ToString();  
                            }
                            
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Scorecard: Caller is last day caller.");
                            #endif
                        }
                        else if (caller.callerProfile.callerMonster !=  null)
                        {
                            GameObject gameObject = Object.Instantiate(__instance.callerListing, __instance.contentHolder);
                            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = caller.callerProfile.callerName;
                            gameObject.GetComponentInChildren<Toggle>().isOn = caller.answeredCorrectly;
                        }

                        currentCallerIndex++;
                    }
                    
                }
                
                return false; // Skip function with false.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ScorecardBehavior), "LoadCallerAnswers", new Type[] { })]
        public static class LoadCallerAnswersPatch
        {

            /// <summary>
            /// Patches the scorecard load caller answers to gracefully handle null callers. (Broken caller list)
            /// </summary>
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(MethodBase __originalMethod, ScorecardBehavior __instance)
            {
                if (GlobalVariables.saveManagerScript.savedCallerCorrectAnswers.Length != GlobalVariables.callerControllerScript.callers.Length)
                {
                    return false;
                }
                
                for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                {
                    if (GlobalVariables.callerControllerScript.callers[index] != null)
                    {
                        GlobalVariables.callerControllerScript.callers[index].answeredCorrectly = GlobalVariables.saveManagerScript.savedCallerCorrectAnswers[index];
                    }
                }
                
                return false; // Skip original function with false.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ScorecardBehavior), "OnEnable", new Type[] { })]
        public static class OnEnablePatch
        {
            /// <summary>
            /// Patches the scorecard close button to not exist duplicated by removing the first instance.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            private static void Prefix(MethodBase __originalMethod, ScorecardBehavior __instance)
            {
                GameObject scorecardWindow = __instance.transform.gameObject;

                GameObject closeButton = scorecardWindow.transform.Find("WindowsBar").Find("CloseButton").gameObject;

                if (closeButton.GetComponents<Button>().Length >= 2)
                {
                    Object.Destroy(closeButton.GetComponent<Button>());
                }
            }
        }
    }
}