using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Intermission;
using NewSafetyHelp.CallerPatches.IncomingCallWindow;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.JSONParsing;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.CallerPatches.Answer
{
    public static class ProvidingAnswer
    {
        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CheckCallerAnswer", typeof(MonsterProfile))]
        public static class ReplaceAnswerWithReplacedAnswer
        {
            /// <summary>
            /// Patch the caller answer check to be the custom caller/entry.
            /// </summary>
            /// <param name="__instance"> Function Caller Instance </param>
            /// <param name="monsterID"> Monster Profile that was selected </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance, ref MonsterProfile monsterID)
            {
                // Get DynamicCaller
                Type callerController = typeof(CallerController);
                FieldInfo dynamicCaller = callerController.GetField("dynamicCaller",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (dynamicCaller == null)
                {
                    LoggingHelper.ErrorLog("CallerController.dynamicCaller is null!");
                    return true;
                }

                if (monsterID != null) // Monster Valid
                {
                    ++__instance.callersToday;

                    // Get replaced monster if valid
                    bool found = false;
                    MonsterProfile monsterToCheck = null;
                    foreach (EntryMetadata item in GlobalParsingVariables.EntriesMetadata)
                    {
                        if (item.currentlySelected) // We found an entry to replace the audio for.
                        {
                            // Now we unselect the item for new calls to allow replacing.
                            item.currentlySelected = false;

                            monsterToCheck = item.referenceCopyEntry;

                            found = true;
                        }
                    }

                    if (!found) // We do not replace, so we default back to the current caller.
                    {
                        monsterToCheck = __instance.callers[__instance.currentCallerID].callerProfile.callerMonster;
                        LoggingHelper.InfoLog("The caller monster was:" +
                                              $" {__instance.callers[__instance.currentCallerID].callerProfile.callerMonster.monsterName}.");
                        LoggingHelper.DebugLog("The previous caller was not replaced by any custom caller.");
                    }

                    if (monsterID == monsterToCheck) // If correct
                    {
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;
                        ++__instance.correctCallsToday;

                        // Debug Info in case the replacement worked.
                        if (found)
                        {
                            LoggingHelper.DebugLog("Selected the correct replaced entry.");
                        }
                    }
                    else // If wrong
                    {
                        if (CustomCampaignGlobal.InCustomCampaign)
                        {
                            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                            if (customCampaign == null)
                            {
                                LoggingHelper.CampaignNullError();
                                __instance.callers[__instance.currentCallerID].answeredCorrectly = false;
                                return false;
                            }

                            if (customCampaign.SkipCallersCorrectly)
                            {
                                __instance.callers[__instance.currentCallerID].answeredCorrectly = true;
                                ++__instance.correctCallsToday;
                            }
                            else
                            {
                                __instance.callers[__instance.currentCallerID].answeredCorrectly = false;
                            }
                        }
                        else
                        {
                            __instance.callers[__instance.currentCallerID].answeredCorrectly = false;
                            if (GlobalVariables.isXmasDLC) // If wrong and DLC
                            {
                                // Get TriggerXMAS Lights
                                MethodInfo triggerXmasLight = callerController.GetMethod("TriggerXmasLight",
                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                                if (triggerXmasLight == null)
                                {
                                    LoggingHelper.ErrorLog("triggerXmasLight is null! Calling original function.");
                                    return true;
                                }

                                triggerXmasLight.Invoke(__instance, new object[] { });

                                GlobalVariables.cheerMeterScript.UpdateMeterVisuals();
                            }

                            // Debug Info in case the replacement worked.
                            if (found)
                            {
                                LoggingHelper.InfoLog("Selected the wrong replaced entry.");
                            }
                        }
                    }
                }
                else if
                    (!(bool)dynamicCaller
                         .GetValue(__instance)) // Monster not provided and a dynamic caller. So we set it to true.
                {
                    __instance.callers[__instance.currentCallerID].answeredCorrectly = true;

                    if (!CustomCampaignGlobal.InCustomCampaign)
                    {
                        LoggingHelper.DebugLog("INFO: Dynamic Caller. No replacement possible. Always correct.");
                    }
                }

                dynamicCaller.SetValue(__instance, false);

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "SubmitAnswer", typeof(MonsterProfile))]
        public static class SubmitAnswerPatch
        {
            /// <summary>
            /// Changes the function to work with better with custom campaigns. By also increasing tier on last call if available.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="monsterID"> Reference to parameter having the monster ID. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance, ref MonsterProfile monsterID)
            {
                LoggingHelper.DebugLog("SubmitAnswer Called.", LoggingHelper.LoggingCategory.SKIPPED_CALLER);
                
                FieldInfo onCallConcluded = typeof(CallerController).GetField("OnCallConcluded",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                
                if (onCallConcluded == null)
                {
                    LoggingHelper.ErrorLog("OnCallConcluded is null. Calling original function.");
                    return true;
                }
                else // _onCallConcluded != null
                {
                    Delegate del = (Delegate) onCallConcluded.GetValue(null); // Since its static.

                    if (del != null)
                    {
                        // Old: CallerController.OnCallConcluded();
                        del.DynamicInvoke(); 
                    }
                    else
                    {
                        LoggingHelper.DebugLog("[INFO] OnCallConcluded has no subscribers unable of executing." +
                                               " Ignoring.");

                    }
                }

                // Some reflection.
                MethodInfo newCallRoutine = typeof(CallerController).GetMethod("NewCallRoutine",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (newCallRoutine == null)
                {
                    LoggingHelper.ErrorLog("NewCallRoutine is null. Calling original function.");
                    return true;
                }

                IEnumerator newCallRoutineTenValue = (IEnumerator)newCallRoutine.Invoke(__instance, new object[] { 5f, 10f });
                IEnumerator newCallRoutineDefaultValues = (IEnumerator)newCallRoutine.Invoke(__instance, new object[] { 5f, 30f });
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.disconnect);

                FieldInfo _callerAudioSource = typeof(CallerController).GetField("callerAudioSource",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (_callerAudioSource == null)
                {
                    LoggingHelper.ErrorLog("callerAudioSource is null. Calling original function.");
                    return true;
                }

                FieldInfo triggerGameOver = typeof(CallerController).GetField("triggerGameOver",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (triggerGameOver == null)
                {
                    LoggingHelper.ErrorLog("triggerGameOver is null. Calling original function.");
                    return true;
                }

                AudioSource callerAudioSource = (AudioSource)_callerAudioSource.GetValue(__instance);
                // OLD: __instance.callerAudioSource.Stop();
                callerAudioSource.Stop(); 

                if (__instance.arcadeMode)
                {
                    ++__instance.currentArcadeCallTotal;

                    if (__instance.callTimer > __instance.callTimerMinimum)
                    {
                        __instance.callTimer -= 5;
                    }

                    if (monsterID == __instance.currentCustomCaller.callerMonster)
                    {
                        __instance.CreateCustomCaller();

                        // OLD: this.StartCoroutine(this.NewCallRoutine(maxTime: 10f));
                        __instance.StartCoroutine(newCallRoutineTenValue); 

                        GlobalVariables.mainCanvasScript.NoCallerWindow();

                        ++__instance.currentArcadeCombo;

                        float num = __instance.currentArcadeCombo;
                        if (num > 10.0)
                        {
                            num = 10f;
                        }

                        __instance.playerScore += __instance.pointsPerCall * num;
                        if (__instance.currentArcadeCombo <= __instance.highestArcadeCombo)
                        {
                            return false;
                        }

                        __instance.highestArcadeCombo = __instance.currentArcadeCombo;
                    }
                    else
                    {
                        __instance.currentArcadeCombo = 0;
                        __instance.CreateCustomCaller();
                        GlobalVariables.mainCanvasScript.NoCallerWindow();
                        ++__instance.currentStrikes;

                        MethodInfo _colorLifeImages = typeof(CallerController).GetMethod("ColorLifeImages",
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                        MethodInfo _cameraShake = typeof(CallerController).GetMethod("CameraShake",
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                        MethodInfo _arcadeFailureRoutine = typeof(CallerController).GetMethod("ArcadeFailureRoutine",
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

                        if (_colorLifeImages == null || _cameraShake == null || _arcadeFailureRoutine == null)
                        {
                            LoggingHelper.ErrorLog("ColorLifeImages or CameraShake or ArcadeFailureRoutine is null." +
                                                   " Calling original function.");
                            return true;
                        }

                        // OLD: __instance.ColorLifeImages();
                        _colorLifeImages.Invoke(__instance, null); 

                        IEnumerator cameraShake = (IEnumerator)_cameraShake.Invoke(__instance, new object[] { 0.25f });

                        if (cameraShake != null)
                        {
                            // OLD: __instance.StartCoroutine(__instance.CameraShake(0.25f));
                            __instance.StartCoroutine(cameraShake);
                        }

                        if (__instance.currentStrikes >= __instance.strikesToFailure)
                        {
                            IEnumerator arcadeFailureRoutine =
                                (IEnumerator)_arcadeFailureRoutine.Invoke(__instance, null);
                            // OLD: __instance.StartCoroutine(__instance.ArcadeFailureRoutine());
                            __instance.StartCoroutine(arcadeFailureRoutine); 
                        }
                        else
                        {
                            // OLD: __instance.StartCoroutine(__instance.NewCallRoutine(maxTime: 10f));
                            __instance.StartCoroutine(newCallRoutineTenValue); 
                        }
                    }
                }
                else if ((bool)triggerGameOver.GetValue(__instance)) // __instance.triggerGameOver
                {
                    GlobalVariables.mainCanvasScript.PlayGameOverCutscene();
                }
                else // Not Arcade Mode
                {
                    MethodInfo checkCallerAnswer = typeof(CallerController).GetMethod("CheckCallerAnswer",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

                    if (checkCallerAnswer == null)
                    {
                        LoggingHelper.ErrorLog("CheckCallerAnswer is null. Calling original function.");
                        return true;
                    }

                    // OLD: __instance.CheckCallerAnswer(monsterID);
                    checkCallerAnswer.Invoke(__instance, new object[] { monsterID }); 

                    // Before checking, it is the last call of the day, we check if we can increase the tier.

                    LoggingHelper.DebugLog("Increase tier?" +
                                           $" (For: {__instance.callers[__instance.currentCallerID].callerProfile.callerName})" +
                                           $" {__instance.callers[__instance.currentCallerID].callerProfile.increaseTier}");

                    if (__instance.callers[__instance.currentCallerID].callerProfile.increaseTier)
                    {
                        __instance.IncreaseTier();
                    }

                    if (__instance.IsLastCallOfDay())
                    {
                        if (!GlobalVariables.isXmasDLC 
                            && !__instance.ScoreIsPassing(__instance.gameOverThreshold) 
                            && GlobalVariables.currentDay > 1 
                            && GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                        {
                            // In case we are not in the DLC and our score is not passing,
                            // we show the last game over caller.
                            
                            //__instance.StartCoroutine(__instance.NewCallRoutine());
                            __instance.StartCoroutine(newCallRoutineDefaultValues); 

                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false;
                        }

                        if (GlobalVariables.isXmasDLC &&
                            GlobalVariables.cheerMeterScript.scoreDisplay * 100.0 <= __instance.xmasGameOverThreshold 
                            && GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                        {
                            // We are in the DLC and our cheer score is less than the threshold,
                            // we show the last game over caller.
                            
                            //__instance.StartCoroutine(__instance.NewCallRoutine());
                            __instance.StartCoroutine(newCallRoutineDefaultValues); 

                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false;
                        }

                        FieldInfo lastDayNum = typeof(CallerController).GetField("lastDayNum",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                        if (lastDayNum == null)
                        {
                            LoggingHelper.ErrorLog("lastDayNum is null. Calling original function.");
                            return true;
                        }

                        if (GlobalVariables.currentDay < (int)lastDayNum.GetValue(__instance)) //__instance.lastDayNum
                        {
                            // GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript.EndDayRoutine());
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript.EndDayRoutine());

                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false;
                        }
                    }
                    
                    
                    // (VERY IMPORTANT: AFTER THIS FUNCTION THE NEXT CALLER GETS CALLED. IF WE WISH TO PREVENT THAT
                    // WE NEED TO END THE DAY HERE OR SKIP THE FUNCTION)
                    // Checks if we need to end the day, in case the next caller gets skipped.
                    if (CustomCampaignGlobal.InCustomCampaign
                        && !GlobalVariables.arcadeMode)
                    {
                        int checkResult = CloseButtonPatches.CheckIfAnyValidCallerLeft(GlobalVariables.callerControllerScript);
                        if (checkResult > 0)
                        {
                            LoggingHelper.DebugLog("Calling end day routine from submit answer.",
                                consoleColor: ConsoleColor.DarkYellow);
                            
                            // In case the intermission music is playing, we stop it.
                            MelonCoroutines.Start(IntermissionMusicHelper.StopIntermissionMusic());
                            
                            // Increase caller ID, since we are skipping callers.
                            GlobalVariables.callerControllerScript.currentCallerID += checkResult;
                        
                            // Start the end day routine and stop any caller. And we end the day.
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript
                                .EndDayRoutine());
                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false; // Skip original function.
                        }
                        
                        CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                        if (customCampaign == null)
                        {
                            LoggingHelper.CampaignNullError();
                            return true;
                        }
    
                        // A dynamic caller. We can play intermission music.
                        if (monsterID == null)
                        {
                            if (customCampaign.CustomIntermissionMusic.Count >= 0)
                            {
                                IntermissionMusicHelper.PlayIntermissionMusic();
                            }
                        }
                    }

                    // Next caller after providing an answer.
                    if (__instance.currentCallerID + 1 < __instance.callers.Length)
                    {
                        __instance.StartCoroutine(
                            newCallRoutineDefaultValues); //__instance.StartCoroutine(__instance.NewCallRoutine()); 
                        GlobalVariables.mainCanvasScript.NoCallerWindow();
                    }

                    if (GlobalVariables.isXmasDLC && __instance.currentCallerID == __instance.callers.Length - 4)
                    {
                        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
                        GlobalVariables.mainCanvasScript.cameraAnimator.SetBool("xmasTension", true);
                    }
                    else
                    {
                        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
                        GlobalVariables.mainCanvasScript.cameraAnimator.SetBool("xmasTension", false);
                    }
                }
                
                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SubmitWindowBehavior), "SubmitRoutine")]
        public static class SubmitRoutinePatch
        {
            /// <summary>
            /// Changes the SubmitWindowBehavior to play intermission music after finishing.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to call </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(SubmitWindowBehavior __instance, ref IEnumerator __result)
            {
                __result = SubmitRoutine(__instance);

                return false; // Skip original function.
            }

            private static IEnumerator SubmitRoutine(SubmitWindowBehavior __instance)
            {
                SubmitWindowBehavior submitWindowBehavior = __instance;
                
                if (submitWindowBehavior.answerToSubmit == null)
                {
                    GlobalVariables.mainCanvasScript.CreateError("ERROR: INSUFFICIENT PERMISSIONS.");
                }
                else
                {
                    submitWindowBehavior.submitButton.SetActive(false);
                    submitWindowBehavior.loadingText.SetActive(true);
                    
                    if (GlobalVariables.arcadeMode)
                    {
                        GlobalVariables.callerControllerScript.StopCoroutine(GlobalVariables.callerControllerScript
                            .callTimerRoutine);
                    }
                    
                    yield return new WaitForSeconds(Random.Range(4, 6));
                    
                    GlobalVariables.callerControllerScript.SubmitAnswer(submitWindowBehavior.answerToSubmit);
                    
                    if (!GlobalVariables.arcadeMode)
                    {
                        GlobalVariables.mainCanvasScript.CreateError("INFO SUCCESSFULLY SENT TO CLIENT. GOOD JOB!");
                    }
                    
                    GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.correctSound);
                    
                    GlobalVariables.musicControllerScript.StopMusic();
                    GlobalVariables.UISoundControllerScript.StopUISoundLooping();
                    
                    submitWindowBehavior.submitButton.SetActive(true);
                    submitWindowBehavior.loadingText.SetActive(false);
                    submitWindowBehavior.gameObject.SetActive(false);
                }
                
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        yield break;
                    }
                        
                    if (customCampaign.CustomIntermissionMusic.Count >= 0)
                    {
                        IntermissionMusicHelper.PlayIntermissionMusic();
                    }
                }
            }
        }
    }
}