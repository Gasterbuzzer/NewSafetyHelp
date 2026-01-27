using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomDesktop;
using Steamworks;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CallerPatches.UI
{
    public static class MainCanvasPatches
    {
        // Cached animator lookups.
        private static readonly int Glitch = Animator.StringToHash("glitch");
        private static readonly int Shake = Animator.StringToHash("shake");

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "WriteDayString")]
        public static class WriteDayStringPatch
        {
            /// <summary>
            /// Patches the main canvas day string function to use custom day strings.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param> 
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref string __result)
            {
                List<string> defaultDayNames = new List<string>() {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
                
                if (!GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.arcadeMode)
                    {
                        __result = "Arcade Mode";
                    }

                    __result = defaultDayNames[GlobalVariables.currentDay - 1];

                }
                else if (GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign)
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
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign Values
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign currentCustomCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (currentCustomCampaign != null)
                    {
                        string dayString;
                        
                        // Campaign find campaign.
                        if (currentCustomCampaign.CampaignDayStrings.Count > 0)
                        {
                            if (GlobalVariables.currentDay > currentCustomCampaign.CampaignDayStrings.Count 
                                || currentCustomCampaign.CampaignDays > currentCustomCampaign.CampaignDayStrings.Count)
                            {
                                MelonLogger.Warning("WARNING: Amount of day strings does not correspond with the max amount of days for the custom campaign. Using default values. ");
                                dayString = defaultDayNames[(GlobalVariables.currentDay - 1) % defaultDayNames.Count];
                            }
                            else
                            {
                                dayString = currentCustomCampaign.CampaignDayStrings[(GlobalVariables.currentDay - 1) % currentCustomCampaign.CampaignDayStrings.Count];
                            }
                        }
                        else
                        {
                            dayString = defaultDayNames[(GlobalVariables.currentDay - 1) % defaultDayNames.Count];
                        }
                        
                        bool foundDayStrings = false;
                        List<string> daysStrings = CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.DayTitleStrings, ref foundDayStrings,
                            v => v != null && v.Count > 0);
                        
                        bool foundUnlockDays = false;
                        List<int> unlockDays = CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.UnlockDays, ref foundUnlockDays,
                            v => v != null && v.Count > 0);
                        
                        // Modifier
                        if (foundDayStrings && daysStrings != null)
                        {
                            if (daysStrings.Count > 0)
                            {
                                if (unlockDays != null                              // If conditional days, but we don't have enough day strings for amount of unlocked days. (And only if the campaign didn't provide one)
                                    && daysStrings.Count != unlockDays.Count 
                                    && string.IsNullOrEmpty(dayString))
                                {
                                    MelonLogger.Warning("WARNING: Amount of day strings does not correspond with the max amount of days for the custom campaign. Using default values. ");
                                    dayString = defaultDayNames[(GlobalVariables.currentDay - 1) % defaultDayNames.Count];
                                }
                                else
                                {
                                    if (unlockDays == null) // General Days, we simply display what we can.
                                    {
                                        if (currentCustomCampaign.CampaignDays > daysStrings.Count)
                                        {
                                            MelonLogger.Warning("WARNING: Amount of day strings does not correspond with the max amount of days for the custom campaign. Using modulated values. ");
                                        }
                                        
                                        dayString = daysStrings[(GlobalVariables.currentDay - 1) % daysStrings.Count]; // We simply pick what best fits.
                                    }
                                    else // Not General (Conditional Modifier)
                                    {
                                        if (daysStrings.Count != unlockDays.Count) // If we don't have enough to show.
                                        {
                                            MelonLogger.Warning("WARNING: Amount of day strings does not correspond with the max amount of days for the custom campaign. Using modulated values. ");
                                            dayString = daysStrings[(GlobalVariables.currentDay - 1) % daysStrings.Count];
                                        }
                                        else // We do have enough days.
                                        {
                                            int indexUnlockDay = unlockDays.IndexOf(GlobalVariables.currentDay);
                                        
                                            if (indexUnlockDay != -1)
                                            {
                                                dayString = daysStrings[indexUnlockDay];
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                        
                        if (string.IsNullOrEmpty(dayString)) // If empty, we provide a default one.
                        {
                            dayString = defaultDayNames[(GlobalVariables.currentDay - 1) % defaultDayNames.Count];
                        }
                        
                        if (!string.IsNullOrEmpty(dayString)) // Update if not empty. It should if nothing went wrong always work.
                        {
                            __result = dayString;
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Was unable of finding the current campaign. Defaulting to default values.");
                        
                        __result = defaultDayNames[GlobalVariables.currentDay - 1];
                    }
                    
                }
                else
                {
                    __result = "Default";
                }
                
                return false; // Skip function with false.
            }
        }
        
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "StartSoftwareRoutine")]
        public static class SoftwareRoutinePatches
        {
            /// <summary>
            /// Patches start software routine to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine of function to be called after wards </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                EndDayRoutinePatch.IsDayEnding = false; // Reset it, if not reset yet.
                
                __result = StartSoftwareRoutine(__instance);
                
                return false; // Skip function with false.
            }

            private static IEnumerator StartSoftwareRoutine(MainCanvasBehavior __instance)
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
                
                if (!GlobalVariables.arcadeMode && GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign)
                {
                  mainCanvasBehavior.trialScreen.SetActive(true);
                  mainCanvasBehavior.postProcessVolume.profile = mainCanvasBehavior.scaryProcessProfile;
                }
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign Last Day
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
                  
                  if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign)
                  {
                    mainCanvasBehavior.cameraAnimator.SetTrigger(Glitch);
                    GlobalVariables.fade.FadeIn();
                    
                    yield return new WaitForSeconds(0.2f);
                    
                    GlobalVariables.fade.FadeOut();
                  }
                  else if (CustomCampaignGlobal.InCustomCampaign) // Just Skip
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
                
                if (!GlobalVariables.arcadeMode && GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign)
                {
                  yield return new WaitForSeconds(0.4f);
                  
                  mainCanvasBehavior.cameraAnimator.SetTrigger(Glitch);
                  GlobalVariables.fade.FadeIn();
                  
                  yield return new WaitForSeconds(0.2f);
                  
                  GlobalVariables.fade.FadeOut();
                  GlobalVariables.musicControllerScript.StartTrialMusic();
                }
                else if (CustomCampaignGlobal.InCustomCampaign)
                {
                    // Skip
                }
                
                if (GlobalVariables.arcadeMode)
                {
                  mainCanvasBehavior.callTimer.SetActive(true);
                  
                  yield return new WaitForSeconds(1f);
                  
                  GlobalVariables.fade.FadeOut();
                }
                
                // Custom Enables
                if (CustomCampaignGlobal.InCustomCampaign)
                {

                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign is null! Unable of enabling skip call button.");
                    }
                    else if (customCampaign.AlwaysSkipCallButton)
                    {
                        CustomDesktopHelper.getCallSkipButton().SetActive(true);
                    }
                }
                
                GlobalVariables.callerControllerScript.StartCallRoutine();
                GlobalVariables.introIsPlaying = false;
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "EndDayRoutine")]
        public static class EndDayRoutinePatch
        {
            // To avoid duplicate day ending.
            public static bool IsDayEnding;

            /// <summary>
            /// Patches the EndDayRoutine coroutine to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called after wards. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                #if DEBUG
                    MelonLogger.Msg("DEBUG: Calling EndDayRoutine.");
                #endif
                
                __result = EndDayRoutineChanged(__instance);
                
                return false; // Skip function with false.
            }
            
            private static IEnumerator EndDayRoutineChanged(MainCanvasBehavior __instance)
            {
                if (IsDayEnding)
                {
                    #if DEBUG
                        MelonLogger.Msg("DEBUG: Skipping EndDayRoutine.");
                    #endif
                    
                    yield break;
                }

                IsDayEnding = true;
                
                MainCanvasBehavior mainCanvasBehavior = __instance;
                mainCanvasBehavior.clockedOut = false;
                
                yield return new WaitForSeconds(5f);
                
                mainCanvasBehavior.inputBlocker.SetActive(false);
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.correctSound);
                GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                mainCanvasBehavior.softwareStartupPanel.SetActive(true);
                mainCanvasBehavior.clockInPanel.SetActive(true);
                mainCanvasBehavior.logoPanel.SetActive(false);
                mainCanvasBehavior.clockOutElements.SetActive(true);
                mainCanvasBehavior.clockOutButton.SetActive(true);
                mainCanvasBehavior.clockInElements.SetActive(false);
                
                IsDayEnding = false;
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

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    PlayerPrefs.SetFloat("SavedDayScore" + GlobalVariables.currentDay.ToString(), GlobalVariables.callerControllerScript.GetScore());
                }
                else // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: customCampaign was null. Catastrophic failure!");
                        yield break;
                    }

                    customCampaign.SavedDayScores[GlobalVariables.currentDay] = GlobalVariables.callerControllerScript.GetScore();
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

                if (!CustomCampaignGlobal.InCustomCampaign)
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
                    
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign is null. Catastrophic failure!");
                        yield break;
                    }
                    
                    customCampaign.CurrentDay = GlobalVariables.currentDay;
                    customCampaign.SavedCurrentCaller = GlobalVariables.callerControllerScript.currentCallerID + 1;
                    customCampaign.CurrentPermissionTier  = GlobalVariables.entryUnlockScript.currentTier;
                    
                    List<bool> flagArray = new List<bool>();
                    
                    // Create missing values.
                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray.Add(false);
                    }
                    
                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        if (GlobalVariables.callerControllerScript.callers[index] != null) // Sanity check in case there were some unset callers.
                        {
                            flagArray[index] = GlobalVariables.callerControllerScript.callers[index].answeredCorrectly;
                        }
                    }
                    
                    customCampaign.SavedCallersCorrectAnswer = flagArray;
                    customCampaign.SavedCallerArrayLength = GlobalVariables.callerControllerScript.callers.Length;
                }
                
                GlobalVariables.saveManagerScript.SaveGameProgress();
                
                yield return null;
                
                #if DEBUG
                    MelonLogger.Msg("DEBUG: Ending the EndDayRoutine.");
                #endif
                
                mainCanvasBehavior.ExitToMenu();
                
                mainCanvasBehavior.StartCoroutine(mainCanvasBehavior.StartSoftwareRoutine());
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "EndingCutsceneRoutine")]
        public static class EndingCutsceneRoutinePatch
        {
            /// <summary>
            /// Patches the EndingCutsceneRoutine coroutine to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called after wards. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                __result = endingCutsceneRoutineChanged(__instance);
                
                return false; // Skip function with false.
            }
            
            private static IEnumerator endingCutsceneRoutineChanged(MainCanvasBehavior __instance)
            {
                MainCanvasBehavior mainCanvasBehavior = __instance;

                if (Camera.main == null)
                {
                    MelonLogger.Error("ERROR: Camera was null. Catastrophic failure!");
                    yield break;
                }
                
                if (mainCanvasBehavior.videoPlayer.isPlaying || Camera.main.gameObject.GetComponent<Animator>().GetBool(Shake))
                {
                    MelonLogger.Msg("INFO: Ending cutscene is already playing. Not calling again.");
                    yield break;
                }
                
                if (!GlobalVariables.isXmasDLC)
                {
                    
                    Camera.main.gameObject.GetComponent<Animator>().SetBool(Shake, true);
                    mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(GlobalVariables.UISoundControllerScript.screenShakeLoop, GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));
                    
                    yield return new WaitForSeconds(6f);
                    
                    mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));
                    GlobalVariables.musicControllerScript.StopTrialMusic();
                }

                if (!CustomCampaignGlobal.InCustomCampaign)
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
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    customCampaign.SavedGameFinished = 1;
                    customCampaign.SavedGameFinishedDisplay = 1;
                    
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
                    
                    customCampaign.SavedCallersCorrectAnswer = flagArray;
                    customCampaign.SavedCallerArrayLength = GlobalVariables.callerControllerScript.callers.Length;
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
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.endClip;
                    if (GlobalVariables.isXmasDLC)
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.xmasEndClip;
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign was null. Catastrophic failure!");
                        yield break;
                    }

                    if (!string.IsNullOrEmpty(customCampaign.EndCutsceneVideoName)) // If provided
                    {
                        mainCanvasBehavior.videoPlayer.url = customCampaign.EndCutsceneVideoName;
                    }
                    else // If not, we show the default one.
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.endClip;
                    }
                }
                
                mainCanvasBehavior.videoPlayer.Play();

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    yield return new WaitForSeconds((float) mainCanvasBehavior.videoPlayer.clip.length);
                }
                else // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    if (!string.IsNullOrEmpty(customCampaign.EndCutsceneVideoName)) // If provided
                    {
                        // Get video length and then wait for it.
                        mainCanvasBehavior.videoPlayer.Prepare();
                        
                        while (mainCanvasBehavior.videoPlayer.isPlaying) // While playing we don't continue.
                        {
                            yield return null;
                        }
                        
                        // Afterward we load all main game values.
                        CustomDesktopHelper.backToMainGame(false);
                    }
                    else // If not, we show the default one.
                    {
                        yield return new WaitForSeconds((float) mainCanvasBehavior.videoPlayer.clip.length);
                    }
                }
                
                if (SteamManager.Initialized && !GlobalVariables.isXmasDLC &&!CustomCampaignGlobal.InCustomCampaign) // Disable Achievement in Custom Campaign
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
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "IsNetworkDown")]
        public static class IsNetworkDownPatch
        {
            /// <summary>
            /// Patches the network down patch to also check for custom callers.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> If to down the network. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref bool __result)
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
                    if (!CustomCampaignGlobal.InCustomCampaign) // Not in custom campaign, could be main or DLC.
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
                    else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                    {
                        CallerModel.CustomCCaller customCCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);
                        
                        if (customCCaller == null)
                        {
                            MelonLogger.Error("ERROR: Custom campaign caller was null. Unable of checking for downed network parameter. Calling original function.");
                            return true;
                        }

                        if (customCCaller.DownedNetworkCaller) // This is set to true if the caller is allowed to down the network.
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
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "GameOverCutsceneRoutine")]
        public static class GameOverCutsceneRoutinePatch
        {
            /// <summary>
            /// Patches the game over cutscene coroutine to also be able to play custom game over cutscenes.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to run. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                __result = GameOverCutsceneRoutineChanged(__instance);
                
                return false; // Skip function with false.
            }

            private static IEnumerator GameOverCutsceneRoutineChanged(MainCanvasBehavior __instance)
            {
                MainCanvasBehavior mainCanvasBehavior = __instance;
                
                FieldInfo _shakeAnimationString = typeof(MainCanvasBehavior).GetField("shakeAnimationString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_shakeAnimationString == null)
                {
                    MelonLogger.Error("ERROR: shakeAnimationString is null! Catastrophic failure!");
                    yield break;
                }
                
                mainCanvasBehavior.cameraAnimator.SetBool((string) _shakeAnimationString.GetValue(__instance), true); // mainCanvasBehavior.shakeAnimationString
                
                mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(GlobalVariables.UISoundControllerScript.screenShakeLoop,
                    GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.7f));
                
                GlobalVariables.fade.FadeIn(6f);
                
                if (GlobalVariables.musicControllerScript.myTrialMusicSource.isPlaying)
                {
                    GlobalVariables.musicControllerScript.StopTrialMusic();
                }
                
                yield return new WaitForSeconds(6f);
                
                mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(GlobalVariables.UISoundControllerScript.myScreenShakeLoopingSource, 0.3f));
                
                yield return new WaitForSeconds(1f);
                
                mainCanvasBehavior.cutsceneCanvas.SetActive(true);

                if (!CustomCampaignGlobal.InCustomCampaign) // Not in custom campaign
                {
                    mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.gameOverClip;
                
                    if (GlobalVariables.isXmasDLC)
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.xmasGameOverClip;
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign was null. Catastrophic failure!");
                        yield break;
                    }

                    if (!string.IsNullOrEmpty(customCampaign.GameOverCutsceneVideoName)) // If provided
                    {
                        mainCanvasBehavior.videoPlayer.url = customCampaign.GameOverCutsceneVideoName;
                    }
                    else // If not, we show the default one.
                    {
                        mainCanvasBehavior.videoPlayer.clip = mainCanvasBehavior.gameOverClip;
                    }
                }
                
                mainCanvasBehavior.videoPlayer.Play();
                
                yield return new WaitForSeconds(1f);
                
                GlobalVariables.fade.FadeOut(3f);

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Game
                {
                    yield return new WaitForSeconds((float) mainCanvasBehavior.videoPlayer.clip.length);
                }
                else // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign was null. Catastrophic failure!");
                        yield break;
                    }
                    
                    if (!string.IsNullOrEmpty(customCampaign.GameOverCutsceneVideoName)) // If provided
                    {
                        // Get video length and then wait for it.
                        mainCanvasBehavior.videoPlayer.Prepare();
                        
                        while (mainCanvasBehavior.videoPlayer.isPlaying) // While playing we don't continue.
                        {
                            yield return null;
                        }
                    }
                    else // If not, we show the default one.
                    {
                        yield return new WaitForSeconds((float) mainCanvasBehavior.videoPlayer.clip.length);
                    }
                }
                
                if (SteamManager.Initialized && !GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign) // Don't show fired achievement in custom campaign.
                {
                    SteamUserStats.SetAchievement("Fired");
                    SteamUserStats.StoreStats();
                }
                
                GlobalVariables.fade.FadeIn(2f);
                
                yield return new WaitForSeconds(2f);
                
                mainCanvasBehavior.RestartDay();
            }
        }
        
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "LoadCallerAnswers")]
        public static class LoadCallerAnswersPatch
        {
            /// <summary>
            /// Patches the load caller answers to gracefully accept null values.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance)
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
                
                return false; // Skip function with false.
            }
            
        }
        
    }
}