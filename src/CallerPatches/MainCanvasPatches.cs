using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using Steamworks;
using UnityEngine;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

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
            /// <param name="__result"> Result of the function. </param> 
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, ref string __result)
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
            /// <param name="__result"> Coroutine of function to be called after wards </param>
            // ReSharper disable once RedundantAssignment
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
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "EndDayRoutine", new Type[] { })]
        public static class EndDayRoutinePatch
        {

            /// <summary>
            /// Patches the EndDayRoutine coroutine to work better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called after wards. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Calling EndDayRoutine.");
                #endif
                
                __result = endDayRoutineChanged(__instance);
                
                return false; // Skip function with false.
            }
            
            public static IEnumerator endDayRoutineChanged(MainCanvasBehavior __instance)
            {
                MainCanvasBehavior mainCanvasBehavior = __instance;
                mainCanvasBehavior.clockedOut = false;
                
                yield return new WaitForSeconds(5f);
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.correctSound);
                GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                mainCanvasBehavior.softwareStartupPanel.SetActive(true);
                mainCanvasBehavior.clockInPanel.SetActive(true);
                mainCanvasBehavior.logoPanel.SetActive(false);
                mainCanvasBehavior.clockOutElements.SetActive(true);
                mainCanvasBehavior.clockOutButton.SetActive(true);
                mainCanvasBehavior.clockInElements.SetActive(false);

                while (!mainCanvasBehavior.clockedOut)
                {
                    yield return null;
                }
                    
                yield return new WaitForSeconds(6f);

                if (!GlobalVariables.isXmasDLC)
                {
                    
                    MethodInfo _unlockDailySteamAchievement = typeof(MainCanvasBehavior).GetMethod("UnlockDailySteamAchievement", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_unlockDailySteamAchievement == null)
                    {
                        MelonLogger.Error("ERROR: Method 'UnlockDailySteamAchievement' was null. Catastrophic failure!");
                        yield break;
                    }

                    _unlockDailySteamAchievement.Invoke(mainCanvasBehavior, null); // mainCanvasBehavior.UnlockDailySteamAchievement();;
                }
                    
                GlobalVariables.fade.FadeIn(2f);
                mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource));
                
                yield return new WaitForSeconds(2f);

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    PlayerPrefs.SetFloat("SavedDayScore" + GlobalVariables.currentDay.ToString(), GlobalVariables.callerControllerScript.GetScore());
                }
                else // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: customCampaign was null. Catastrophic failure!");
                        yield break;
                    }

                    customCampaign.savedDayScores[GlobalVariables.currentDay] = GlobalVariables.callerControllerScript.GetScore();
                }
               
                
                FieldInfo _progressDay = typeof(MainCanvasBehavior).GetField("progressDay", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_progressDay == null)
                {
                    MelonLogger.Error("ERROR: Field 'progressDay' was null. Catastrophic failure!");
                    yield break;
                }
                
                if (!(bool) _progressDay.GetValue(mainCanvasBehavior)) // !mainCanvasBehavior.progressDay
                {
                    ++GlobalVariables.currentDay;
                    _progressDay.SetValue(mainCanvasBehavior, true); // mainCanvasBehavior.progressDay = true;
                }

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    GlobalVariables.saveManagerScript.savedDay = GlobalVariables.currentDay;
                    GlobalVariables.saveManagerScript.savedCurrentCaller = GlobalVariables.callerControllerScript.currentCallerID + 1;
                    GlobalVariables.saveManagerScript.savedEntryTier = GlobalVariables.entryUnlockScript.currentTier;
                    
                    MethodInfo _saveCallerAnswers = typeof(MainCanvasBehavior).GetMethod("SaveCallerAnswers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_saveCallerAnswers == null)
                    {
                        MelonLogger.Error("ERROR: Method 'SaveCallerAnswers' was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    _saveCallerAnswers.Invoke(mainCanvasBehavior, null); // mainCanvasBehavior.SaveCallerAnswers();
                }
                else // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();
                    
                    customCampaign.currentDay = GlobalVariables.currentDay;
                    customCampaign.savedCurrentCaller = GlobalVariables.callerControllerScript.currentCallerID + 1;
                    customCampaign.currentPermissionTier  = GlobalVariables.entryUnlockScript.currentTier;
                    
                    List<bool> flagArray = new List<bool>();
                    
                    // Create missing values.
                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray.Add(false);
                    }

                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray[index] = GlobalVariables.callerControllerScript.callers[index].answeredCorrectly;
                    }
                    
                    customCampaign.savedCallersCorrectAnswer = flagArray;
                    customCampaign.savedCallerArrayLength = GlobalVariables.callerControllerScript.callers.Length;
                }
                
                GlobalVariables.saveManagerScript.SaveGameProgress();
                
                yield return null;
                
                mainCanvasBehavior.ExitToMenu();
                mainCanvasBehavior.StartCoroutine(mainCanvasBehavior.StartSoftwareRoutine());
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "EndingCutsceneRoutine", new Type[] { })]
        public static class EndingCutsceneRoutinePatch
        {

            /// <summary>
            /// Patches the EndingCutsceneRoutine coroutine to work better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called after wards. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, ref IEnumerator __result)
            {

                __result = endingCutsceneRoutineChanged(__instance);
                
                return false; // Skip function with false.
            }
            
            public static IEnumerator endingCutsceneRoutineChanged(MainCanvasBehavior __instance)
            {
                MainCanvasBehavior mainCanvasBehavior = __instance;
                
                if (!GlobalVariables.isXmasDLC)
                {

                    if (Camera.main == null)
                    {
                        MelonLogger.Error("ERROR: Camera was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    Camera.main.gameObject.GetComponent<Animator>().SetBool("shake", true);
                    mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(GlobalVariables.UISoundControllerScript.screenShakeLoop, GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));
                    
                    yield return new WaitForSeconds(6f);
                    
                    mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));
                    GlobalVariables.musicControllerScript.StopTrialMusic();
                }

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    GlobalVariables.saveManagerScript.savedGameFinished = 1;
                    GlobalVariables.saveManagerScript.savedGameFinishedDisplay = 1;
                
                    MethodInfo _saveCallerAnswers = typeof(MainCanvasBehavior).GetMethod("SaveCallerAnswers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_saveCallerAnswers == null)
                    {
                        MelonLogger.Error("ERROR: Method 'SaveCallerAnswers' was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    _saveCallerAnswers.Invoke(mainCanvasBehavior, null); // mainCanvasBehavior.SaveCallerAnswers();
                }
                else // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();
                    
                    customCampaign.savedGameFinished = 1;
                    customCampaign.savedGameFinishedDisplay = 1;
                    
                    List<bool> flagArray = new List<bool>();
                    
                    // Create missing values.
                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray.Add(false);
                    }

                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray[index] = GlobalVariables.callerControllerScript.callers[index].answeredCorrectly;
                    }
                    
                    customCampaign.savedCallersCorrectAnswer = flagArray;
                    customCampaign.savedCallerArrayLength = GlobalVariables.callerControllerScript.callers.Length;
                }
                
                // Works for both custom campaigns and main campaign.
                GlobalVariables.saveManagerScript.SaveGameProgress();
                GlobalVariables.saveManagerScript.SaveGameFinished();
                
                GlobalVariables.fade.FadeIn(3f);
                
                yield return new WaitForSeconds(4f);
                
                GlobalVariables.fade.FadeOut();
                mainCanvasBehavior.cutsceneCanvas.SetActive(true);
                
                yield return new WaitForSeconds(0.5f);
                
                // Inject custom end clip here.
                mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.endClip;
                if (GlobalVariables.isXmasDLC)
                {
                    mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.xmasEndClip;
                }
                mainCanvasBehavior.videoPlayer.Play();
                
                yield return new WaitForSeconds((float) mainCanvasBehavior.videoPlayer.clip.length);
                
                if (SteamManager.Initialized && !GlobalVariables.isXmasDLC)
                {
                    SteamUserStats.SetAchievement("GameFinished");
                    
                    MethodInfo _achievedHundredPercentAccuracyRating = typeof(MainCanvasBehavior).GetMethod("AchievedHundredPercentAccuracyRating", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_achievedHundredPercentAccuracyRating == null)
                    {
                        MelonLogger.Error("ERROR: Method 'AchievedHundredPercentAccuracyRating' was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    if ((bool) _achievedHundredPercentAccuracyRating.Invoke(mainCanvasBehavior, null)) // mainCanvasBehavior.AchievedHundredPercentAccuracyRating()
                    {
                        SteamUserStats.SetAchievement("PerfectGame");
                        MelonLogger.Msg("UNITY LOG: PerfectGame Achievement Unlocked.");
                    }
                    SteamUserStats.StoreStats();
                }
                
                yield return new WaitForSeconds(2f);
                
                mainCanvasBehavior.ExitToStartMenu();
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "IsNetworkDown", new Type[] { })]
        public static class IsNetworkDownPatch
        {
            /// <summary>
            /// Patches the network down patch to also check for custom callers.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> If to down the network. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, ref bool __result)
            {

                if (GlobalVariables.arcadeMode)
                {
                    if (GlobalVariables.callerControllerScript.downedNetworkCall)
                    {
                        __result = true;
                        return false;  // Skip function with false.
                    }
                }
                else
                {
                    if (!CustomCampaignGlobal.inCustomCampaign) // Not in custom campaign, could be main or DLC.
                    {
                        foreach (int downedNetworkCall in GlobalVariables.callerControllerScript.downedNetworkCalls)
                        {
                            if (downedNetworkCall == GlobalVariables.callerControllerScript.currentCallerID)
                            {
                                __result = true;
                                return false;  // Skip function with false.
                            }
                        }
                    }
                    else // Custom Campaign
                    {
                        CustomCallerExtraInfo customCaller = CustomCampaignGlobal.getCustomCampaignCustomCallerByOrderID(GlobalVariables.callerControllerScript.currentCallerID);
                        
                        if (customCaller == null)
                        {
                            MelonLogger.Error("ERROR: Custom campaign was null. Unable of checking for downed network parameter. Calling original function.");
                            return true;
                        }

                        if (customCaller.downedNetworkCaller) // This is set to true if the caller is allowed to down the network.
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                
                __result = false;
                return false; // Skip function with false.
            }
        }
    }
}