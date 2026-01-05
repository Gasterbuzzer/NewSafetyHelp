using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CallerPatches.IncomingCallWindow;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.JSONParsing;
using UnityEngine;

namespace NewSafetyHelp.CallerPatches.Answer
{
    public static class ProvidingAnswer
    {
        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CheckCallerAnswer", new[] { typeof(MonsterProfile) })]
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
                    MelonLogger.Error("ERROR: CallerController.dynamicCaller is null!");
                    return true;
                }

                if (monsterID != null) // Monster Valid
                {
                    ++__instance.callersToday;

                    // Get replaced monster if valid
                    bool found = false;
                    MonsterProfile monsterToCheck = null;
                    foreach (EntryExtraInfo item in GlobalParsingVariables.EntriesMetadata)
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
                        MelonLogger.Msg(
                            $"INFO: The caller monster was: {__instance.callers[__instance.currentCallerID].callerProfile.callerMonster.monsterName}.");

                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: The previous caller was not replaced by any custom caller.");
                        #endif
                    }


                    if (monsterID == monsterToCheck) // If correct
                    {
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;
                        ++__instance.correctCallsToday;

                        // Debug Info in case the replacement worked.
                        if (found)
                        {
                            #if DEBUG
                            MelonLogger.Msg("DEBUG: Selected the correct replaced entry.");
                            #endif
                        }
                    }
                    else // If wrong
                    {
                        if (CustomCampaignGlobal.inCustomCampaign)
                        {
                            CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                            if (customCampaign == null)
                            {
                                MelonLogger.Error(
                                    "ERROR: CustomCampaign is null! Unable of checking if to skip checking the caller answer.");
                                __instance.callers[__instance.currentCallerID].answeredCorrectly = false;
                                return false;
                            }

                            if (customCampaign.skipCallersCorrectly)
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
                                    MelonLogger.Error("ERROR: triggerXmasLight is null!");
                                    return true;
                                }

                                triggerXmasLight.Invoke(__instance, new object[] { });

                                GlobalVariables.cheerMeterScript.UpdateMeterVisuals();
                            }

                            // Debug Info in case the replacement worked.
                            if (found)
                            {
                                MelonLogger.Msg("INFO: Selected the wrong replaced entry.");
                            }
                        }
                    }
                }
                else if
                    (!(bool)dynamicCaller
                         .GetValue(__instance)) // Monster not provided and a dynamic caller. So we set it to true.
                {
                    __instance.callers[__instance.currentCallerID].answeredCorrectly = true;

                    MelonLogger.Msg("INFO: Dynamic Caller. No replacement possible. Always correct.");
                }

                dynamicCaller.SetValue(__instance, false);

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "SubmitAnswer", new[] { typeof(MonsterProfile) })]
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
                FieldInfo _onCallConcluded = typeof(CallerController).GetField("OnCallConcluded",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (_onCallConcluded == null)
                {
                    MelonLogger.Error("ERROR: OnCallConcluded is null. Calling original function.");
                    return true;
                }
                else // _onCallConcluded != null
                {
                    Delegate del = (Delegate)_onCallConcluded.GetValue(null); // Since its static.

                    if (del != null)
                    {
                        del.DynamicInvoke(); // CallerController.OnCallConcluded();
                    }
                    else
                    {
                        #if DEBUG
                        MelonLogger.Msg(
                            "DEBUG WARNING: OnCallConcluded has no subscribers unable of executing. Ignoring.");
                        #endif
                    }
                }

                // Some reflection.
                MethodInfo _newCallRoutine = typeof(CallerController).GetMethod("NewCallRoutine",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (_newCallRoutine == null)
                {
                    MelonLogger.Error("ERROR: NewCallRoutine is null. Calling original function.");
                    return true;
                }

                IEnumerator newCallRoutine = (IEnumerator)_newCallRoutine.Invoke(__instance, new object[] { 5f, 10f });
                IEnumerator newCallRoutineDefaultValues =
                    (IEnumerator)_newCallRoutine.Invoke(__instance, new object[] { 5f, 30f });


                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.disconnect);

                FieldInfo _callerAudioSource = typeof(CallerController).GetField("callerAudioSource",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (_callerAudioSource == null)
                {
                    MelonLogger.Error("ERROR: callerAudioSource is null. Calling original function.");
                    return true;
                }

                FieldInfo _triggerGameOver = typeof(CallerController).GetField("triggerGameOver",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                if (_triggerGameOver == null)
                {
                    MelonLogger.Error("ERROR: triggerGameOver is null. Calling original function.");
                    return true;
                }

                AudioSource callerAudioSource = (AudioSource)_callerAudioSource.GetValue(__instance);
                callerAudioSource.Stop(); // __instance.callerAudioSource.Stop();

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

                        __instance.StartCoroutine(
                            newCallRoutine); // this.StartCoroutine(this.NewCallRoutine(maxTime: 10f));

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
                            MelonLogger.Error(
                                "ERROR: ColorLifeImages or CameraShake or ArcadeFailureRoutine is null. Calling original function.");
                            return true;
                        }

                        _colorLifeImages.Invoke(__instance, null); // __instance.ColorLifeImages();

                        IEnumerator cameraShake = (IEnumerator)_cameraShake.Invoke(__instance, new object[] { 0.25f });

                        if (cameraShake != null)
                        {
                            __instance.StartCoroutine(
                                cameraShake); // __instance.StartCoroutine(__instance.CameraShake(0.25f));
                        }

                        if (__instance.currentStrikes >= __instance.strikesToFailure)
                        {
                            IEnumerator arcadeFailureRoutine =
                                (IEnumerator)_arcadeFailureRoutine.Invoke(__instance, null);
                            __instance.StartCoroutine(
                                arcadeFailureRoutine); // __instance.StartCoroutine(__instance.ArcadeFailureRoutine());
                        }
                        else
                        {
                            __instance.StartCoroutine(
                                newCallRoutine); //__instance.StartCoroutine(__instance.NewCallRoutine(maxTime: 10f));
                        }
                    }
                }
                else if ((bool)_triggerGameOver.GetValue(__instance)) // __instance.triggerGameOver
                {
                    GlobalVariables.mainCanvasScript.PlayGameOverCutscene();
                }
                else // Main Game
                {
                    MethodInfo _checkCallerAnswer = typeof(CallerController).GetMethod("CheckCallerAnswer",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

                    if (_checkCallerAnswer == null)
                    {
                        MelonLogger.Error("ERROR: CheckCallerAnswer is null. Calling original function.");
                        return true;
                    }

                    _checkCallerAnswer.Invoke(__instance,
                        new object[] { monsterID }); // __instance.CheckCallerAnswer(monsterID);

                    // Before checking, it is the last call of the day, we check if we can increase the tier.

                    #if DEBUG
                    MelonLogger.Msg(
                        $"DEBUG: Increase tier? (For: {__instance.callers[__instance.currentCallerID].callerProfile.callerName}) {__instance.callers[__instance.currentCallerID].callerProfile.increaseTier}");
                    #endif

                    if (__instance.callers[__instance.currentCallerID].callerProfile.increaseTier)
                    {
                        __instance.IncreaseTier();
                    }

                    if (__instance.IsLastCallOfDay())
                    {
                        if (!GlobalVariables.isXmasDLC && !__instance.ScoreIsPassing(__instance.gameOverThreshold) &&
                            GlobalVariables.currentDay > 1 &&
                            GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                        {
                            __instance.StartCoroutine(
                                newCallRoutineDefaultValues); //__instance.StartCoroutine(__instance.NewCallRoutine());

                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false;
                        }

                        if (GlobalVariables.isXmasDLC &&
                            GlobalVariables.cheerMeterScript.scoreDisplay * 100.0 <= __instance.xmasGameOverThreshold &&
                            GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                        {
                            __instance.StartCoroutine(
                                newCallRoutineDefaultValues); //__instance.StartCoroutine(__instance.NewCallRoutine());

                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false;
                        }

                        FieldInfo _lastDayNum = typeof(CallerController).GetField("lastDayNum",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                        if (_lastDayNum == null)
                        {
                            MelonLogger.Error("ERROR: lastDayNum is null. Calling original function.");
                            return true;
                        }

                        if (GlobalVariables.currentDay < (int)_lastDayNum.GetValue(__instance)) //__instance.lastDayNum
                        {
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript
                                .EndDayRoutine()); //  GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript.EndDayRoutine());

                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false;
                        }
                    }
                    
                    // Checks if we need to end the day, in case the next caller gets skipped.
                    if (CustomCampaignGlobal.inCustomCampaign
                        && !GlobalVariables.arcadeMode)
                    {
                        int checkResult = CloseButtonPatches.checkIfAnyValidCallerLeft(GlobalVariables.callerControllerScript);
                        if (checkResult > 0)
                        {
                            GlobalVariables.callerControllerScript.currentCallerID += checkResult; // Increase caller ID, since we are skipping callers.
                        
                            // Start the end day routine and stop any caller. And we end the day.
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript
                                .EndDayRoutine());
                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false; // Skip original function.
                        }
                    }

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
    }
}