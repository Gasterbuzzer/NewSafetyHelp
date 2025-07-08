using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using UnityEngine;

namespace NewSafetyHelp.CallerPatches
{
    public static class MainCanvasPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "WriteDayString", new Type[] { })]
        public static class ScorecardPatch
        {

            /// <summary>
            /// Patches the main canvas day string function to use custom day strings.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, string __result)
            {
                
                List<string> defaultDayNames = new List<string>() {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
                
                if (!GlobalVariables.isXmasDLC)
                {
                    if (GlobalVariables.arcadeMode)
                    {
                        __result = "Arcade Mode";
                    }

                    __result = defaultDayNames[GlobalVariables.currentDay];

                }
                else if (GlobalVariables.isXmasDLC)
                {
                    switch (GlobalVariables.currentDay)
                    {
                        case 1:
                            __result = "3 Days Until Christmas";
                            break;
                        
                        case 2:
                            __result = "2 Days Until Christmas";
                            break;
                        
                        case 3:
                            __result = "1 Day Until Christmas";
                            break;
                        
                        case 4:
                            __result = "Christmas Day";
                            break;
                    }
                }
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign Values
                {

                    CustomCampaignExtraInfo currentCustomCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();

                    if (currentCustomCampaign != null)
                    {
                        
                        if (currentCustomCampaign.campaignDayStrings.Count > 0)
                        {

                            if (GlobalVariables.currentDay > currentCustomCampaign.campaignDayStrings.Count || currentCustomCampaign.campaignDays > currentCustomCampaign.campaignDayStrings.Count)
                            {
                                MelonLogger.Warning("WARNING: Amount of day strings does not correspond with the max amount of days for the custom campaign. Using default values. ");
                                __result = defaultDayNames[GlobalVariables.currentDay];
                            }
                            else
                            {
                                __result = currentCustomCampaign.campaignDayStrings[GlobalVariables.currentDay];
                            }
                        }
                        else
                        {
                            __result = defaultDayNames[GlobalVariables.currentDay];
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Was unable of finding the current campaign. Defaulting to default values.");
                        
                        __result = defaultDayNames[GlobalVariables.currentDay];
                    }
                    
                }
                else
                {
                    __result = "Default";
                }
                
                return false; // Skip function with false.
            }
        }
        
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "StartSoftwareRoutine", new Type[] { })]
        public static class SoftwareRoutinePatches
        {

            /// <summary>
            /// Patches start software routine to work better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                __result = StartSoftwareRoutine(__instance);
                
                return false; // Skip function with false.
            }


            public static IEnumerator StartSoftwareRoutine(MainCanvasBehavior __instance)
            {
                
                // Get Private Methods
                Type mainCanvasBehaviorType = typeof(MainCanvasBehavior);
                
                MethodInfo loadVarsMethod = mainCanvasBehaviorType.GetMethod("LoadVars", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                MethodInfo populateEntriesListMethod = mainCanvasBehaviorType.GetMethod("PopulateEntriesList", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                MethodInfo writeDayStringMethod = mainCanvasBehaviorType.GetMethod("WriteDayString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (loadVarsMethod == null || populateEntriesListMethod == null || writeDayStringMethod == null)
                {
                    MelonLogger.Error("ERROR: loadVarsMethod or populateEntriesListMethod or writeDayStringMethod is null!");
                    yield break;
                }
                
                MainCanvasBehavior mainCanvasBehavior = __instance;
                
                yield return null;

                loadVarsMethod.Invoke(mainCanvasBehavior, null);
                populateEntriesListMethod.Invoke(mainCanvasBehavior, null);
                
                if (!GlobalVariables.arcadeMode && GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.inCustomCampaign)
                {
                  mainCanvasBehavior.trialScreen.SetActive(true);
                  mainCanvasBehavior.postProcessVolume.profile = mainCanvasBehavior.scaryProcessProfile;
                }
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign Last Day
                {
                    // Currently just skips it.
                }

                if (GlobalVariables.isXmasDLC && (bool)(UnityEngine.Object)GlobalVariables.cheerMeterScript)
                {
                    GlobalVariables.cheerMeterScript.UpdateMeterVisuals();
                }
                  
                GlobalVariables.introIsPlaying = true;
                mainCanvasBehavior.clockedIn = false;
                GlobalVariables.callerControllerScript.callersToday = 0;
                GlobalVariables.callerControllerScript.correctCallsToday = 0;
                
                if (!GlobalVariables.arcadeMode)
                {
                  GlobalVariables.fade.FadeIn(1f, (string) writeDayStringMethod.Invoke(mainCanvasBehavior, null));
                }
                else
                {
                  GlobalVariables.fade.FadeIn(1f);
                  mainCanvasBehavior.arcadeStartPanel.SetActive(true);
                  GlobalVariables.fade.FadeOut(1f);
                }
                
                if (!GlobalVariables.arcadeMode)
                {
                  yield return new WaitForSeconds(6f);
                  
                  mainCanvasBehavior.softwareStartupPanel.SetActive(true);
                  mainCanvasBehavior.clockInPanel.SetActive(false);
                  mainCanvasBehavior.logoPanel.SetActive(false);
                  GlobalVariables.fade.FadeOut(1f);
                  
                  yield return new WaitForSeconds(1f);
                  
                  mainCanvasBehavior.logoPanel.SetActive(true);
                  mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(GlobalVariables.UISoundControllerScript.computerFanSpin, GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource));
                  
                  yield return new WaitForSeconds(6f);
                  
                  GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.correctSound);
                  
                  if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.inCustomCampaign)
                  {
                    mainCanvasBehavior.cameraAnimator.SetTrigger("glitch");
                    GlobalVariables.fade.FadeIn();
                    
                    yield return new WaitForSeconds(0.2f);
                    
                    GlobalVariables.fade.FadeOut();
                  }
                  else if (CustomCampaignGlobal.inCustomCampaign) // Just Skip
                  {
                      // Skip
                  }
                  
                  mainCanvasBehavior.logoPanel.SetActive(false);
                  mainCanvasBehavior.clockInPanel.SetActive(true);
                  mainCanvasBehavior.clockOutElements.SetActive(false);
                  mainCanvasBehavior.clockInElements.SetActive(true);
                  mainCanvasBehavior.clockInButton.SetActive(true);
                  
                  while (!mainCanvasBehavior.clockedIn)
                  {
                      yield return null;
                  }
                    
                  yield return new WaitForSeconds(5f);
                }
                else
                {
                    while (!mainCanvasBehavior.startArcadeMode)
                    {
                        yield return null;
                    }
                }
                
                mainCanvasBehavior.softwareStartupPanel.SetActive(false);
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.correctSound);
                
                if (!GlobalVariables.arcadeMode && GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.inCustomCampaign)
                {
                  yield return new WaitForSeconds(0.4f);
                  
                  mainCanvasBehavior.cameraAnimator.SetTrigger("glitch");
                  GlobalVariables.fade.FadeIn();
                  
                  yield return new WaitForSeconds(0.2f);
                  
                  GlobalVariables.fade.FadeOut();
                  GlobalVariables.musicControllerScript.StartTrialMusic();
                }
                else if (CustomCampaignGlobal.inCustomCampaign)
                {
                    // Skip
                }
                
                if (GlobalVariables.arcadeMode)
                {
                  mainCanvasBehavior.callTimer.SetActive(true);
                  
                  yield return new WaitForSeconds(1f);
                  
                  GlobalVariables.fade.FadeOut();
                }
                
                GlobalVariables.callerControllerScript.StartCallRoutine();
                GlobalVariables.introIsPlaying = false;
            }
        }
    }
}