using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.Helper;
using NewSafetyHelp.JSONParsing;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CallerPatches
{
    public static class CallerPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "Start")]
        public static class AddCustomCampaign
        {
            /// <summary>
            /// Patch the start function to inject custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Original Method Caller. </param>
            /// <param name="__instance"> Function Caller Instance </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance)
            {
                #if DEBUG
                MelonLogger.Msg(ConsoleColor.Magenta, $"DEBUG: Called Start from the class CallerController.");
                #endif

                Type callerController = typeof(CallerController);

                // Original Code
                GlobalVariables.callerControllerScript = __instance;

                __instance.arcadeMode = GlobalVariables.arcadeMode;

                FieldInfo lastDayNum = callerController.GetField("lastDayNum",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (lastDayNum == null)
                {
                    MelonLogger.Error("ERROR: CallerController.lastDayNum is null!");
                    return true;
                }

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    lastDayNum.SetValue(__instance, __instance.mainGameLastDay);
                }
                else
                {
                    lastDayNum.SetValue(__instance, CustomCampaignGlobal.GetActiveCustomCampaign().CampaignDays);
                }


                FieldInfo _callerAudioSource = callerController.GetField("callerAudioSource",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_callerAudioSource == null)
                {
                    MelonLogger.Error("ERROR: CallerAudioSource is null!");
                    return true;
                }

                _callerAudioSource.SetValue(__instance, __instance.GetComponent<AudioSource>());

                if (__instance.arcadeMode)
                {
                    __instance.CreateCustomCaller();
                }

                if (GlobalVariables.isXmasDLC)
                {
                    __instance.callers = null;
                    __instance.callers = __instance.xmasCallers;
                    __instance.warningCall = __instance.xmasWarningCall;
                    __instance.gameOverCall = __instance.xmasGameOverCall;
                    lastDayNum.SetValue(__instance, __instance.xmasLastDay);
                    __instance.downedNetworkCalls = __instance.xmasDownedNetworkCalls;
                }


                /*
                 * Add Custom Callers / Campaign Callers.
                 */

                if (!CustomCampaignGlobal.InCustomCampaign) // If we are not in a custom campaign. (Main Campaign)
                {
                    foreach (KeyValuePair<int, CustomCCaller> customCaller in GlobalParsingVariables.CustomCallersMainGame)
                    {
                        if (customCaller.Key < 0 || customCaller.Value == null) // Sanity check
                        {
                            MelonLogger.Error($"ERROR: Custom caller {customCaller.Key} is invalid!");
                            continue;
                        }

                        if (customCaller.Value.InCustomCampaign)
                        {
                            MelonLogger.Warning(
                                "WARNING: Custom Caller is marked as custom campaign but is also main campaign! Skipping.");
                            continue;
                        }

                        // Is Valid

                        CallerProfile callerProfile = ScriptableObject.CreateInstance<CallerProfile>();

                        if (!customCaller.Value.IsCallerClipLoaded)
                        {
                            MelonLogger.Msg(
                                "INFO: Audio is still loading for this custom caller. It will be updated once the audio has been updated.");
                        }

                        callerProfile.callerName = customCaller.Value.CallerName;
                        callerProfile.callTranscription = customCaller.Value.CallTranscript;
                        callerProfile.callerPortrait = customCaller.Value.CallerImage;
                        callerProfile.callerClip = customCaller.Value.CallerClip;
                        callerProfile.increaseTier = customCaller.Value.CallerIncreasesTier;

                        if (customCaller.Value.MonsterNameAttached != "NO_MONSTER_NAME")
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, customCaller.Value.MonsterNameAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided Monster name '{customCaller.Value.MonsterNameAttached}' for custom caller {customCaller.Key} was not found! Thus will not have any monster entry.");
                                callerProfile.callerMonster = null;
                            }
                            else
                            {
                                callerProfile.callerMonster = foundMonster;
                            }
                        }
                        else if (customCaller.Value.MonsterIDAttached >= 0) // Check for ID monster.
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, monsterID: customCaller.Value.MonsterIDAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided monster ID for custom caller {customCaller.Key} was not found! Thus will not have any monster entry.");
                                callerProfile.callerMonster = null;
                            }
                            else
                            {
                                callerProfile.callerMonster = foundMonster;
                            }
                        }
                        else // No caller monster.
                        {
                            callerProfile.callerMonster = null;
                        }

                        if (customCaller.Value.ConsequenceCallerID >= 0)
                        {
                            if (__instance.callers[customCaller.Value.ConsequenceCallerID].callerProfile == null)
                            {
                                MelonLogger.Warning(
                                    "WARNING: Provided consequence caller but profile is null? Setting to null.");
                            }

                            callerProfile.consequenceCallerProfile =
                                __instance.callers[customCaller.Value.ConsequenceCallerID].callerProfile;
                        }
                        else
                        {
                            callerProfile.consequenceCallerProfile = null;
                        }

                        Caller newCustomCaller = new Caller
                        {
                            answeredCorrectly = false,
                            callerProfile = callerProfile
                        };

                        // Insert our custom caller.
                        __instance.callers[customCaller.Key] = newCustomCaller;
                    }
                }
                else // We are in a custom campaign.
                {
                    // Attempt to hijack caller list.

                    // Fallback for missing picture or audio.
                    MethodInfo getRandomPicMethod = callerController.GetMethod("PickRandomPic",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    MethodInfo getRandomClip = callerController.GetMethod("PickRandomClip",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (getRandomPicMethod == null || getRandomClip == null)
                    {
                        MelonLogger.Error("ERROR: getRandomPicMethod or getRandomClip is null!");
                        return true;
                    }

                    if (string.IsNullOrEmpty(CustomCampaignGlobal.CurrentCustomCampaignName)) // Invalid Custom Campaign
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is set to be true but no custom campaign is active!");
                        return true;
                    }
                    else if (!CustomCampaignGlobal.CustomCampaignsAvailable.Exists(scannedCampaign =>
                                 scannedCampaign.CampaignName ==
                                 CustomCampaignGlobal.CurrentCustomCampaignName)) // Custom Campaign is not registered.
                    {
                        MelonLogger.Error(
                            "ERROR: Current Custom Campaign has not been properly setup! Stopping loading.");
                        return true;
                    }

                    CustomCampaign.CustomCampaignModel.CustomCampaign currentCustomCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    // Clear callers array with amount of campaign callers.
                    __instance.callers = new Caller[currentCustomCampaign.CustomCallersInCampaign.Count];

                    if (currentCustomCampaign.CustomCallersInCampaign.Count <= 0)
                    {
                        MelonLogger.Warning(
                            "WARNING: Custom Campaign has no custom caller assigned! Unexpected behavior will occur when in campaign.");
                    }

                    // Reference list for consequence caller (after adding all profiles).
                    Dictionary<int, int> listOfConsequenceCallers = new Dictionary<int, int>();

                    // Add all customCallers in Callers list.
                    foreach (CustomCCaller customCallerCC in currentCustomCampaign.CustomCallersInCampaign)
                    {
                        CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                        newProfile.callerName = customCallerCC.CallerName;
                        newProfile.callTranscription = customCallerCC.CallTranscript;

                        // Clip
                        if (customCallerCC.CallerClip == null)
                        {
                            if (AudioImport.CurrentLoadingAudios.Count > 0)
                            {
                                MelonLogger.Msg(
                                    $"INFO : Custom Caller '{customCallerCC.CallerName}' is still loading its audio. Using fallback for now.");
                            }
                            else // No Loading Audio
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Custom Caller '{customCallerCC.CallerName}' does not have any valid audio clip! Using fallback instead of real audio. ");
                            }

                            newProfile.callerClip = (RichAudioClip)getRandomClip.Invoke(__instance, new object[] { });
                        }
                        else
                        {
                            newProfile.callerClip = customCallerCC.CallerClip;
                        }

                        // Sprite
                        if (customCallerCC.CallerImage == null)
                        {
                            MelonLogger.Warning(
                                $"WARNING: Custom Caller '{(customCallerCC.CallerName != null ? $"{customCallerCC.CallerName}" : "")}' does not have any valid image / sprite. Using fallback for now.");

                            newProfile.callerPortrait = (Sprite)getRandomPicMethod.Invoke(__instance, new object[] { });
                        }
                        else
                        {
                            newProfile.callerPortrait = customCallerCC.CallerImage;
                        }

                        // Adding Entry to Caller if valid.
                        if (customCallerCC.MonsterNameAttached != "NO_MONSTER_NAME")
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, customCallerCC.MonsterNameAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided Monster name '{customCallerCC.MonsterNameAttached}' for custom caller {customCallerCC.CallerName} was not found! Thus will not have any monster entry.");
                                newProfile.callerMonster = null;
                            }
                            else
                            {
                                newProfile.callerMonster = foundMonster;
                            }
                        }
                        else if (customCallerCC.MonsterIDAttached >= 0) // Check for ID monster.
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, monsterID: customCallerCC.MonsterIDAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided monster ID for custom caller {customCallerCC.CallerName} was not found! Thus will not have any monster entry.");
                                newProfile.callerMonster = null;
                            }
                            else
                            {
                                newProfile.callerMonster = foundMonster;
                            }
                        }

                        if (customCallerCC.ConsequenceCallerID >= 0) // We have a consequence caller ID provided.
                        {
                            listOfConsequenceCallers.Add(customCallerCC.OrderInCampaign,
                                customCallerCC.ConsequenceCallerID); // Add for processing later.
                        }

                        // Increase Tier
                        newProfile.increaseTier = customCallerCC.CallerIncreasesTier;

                        // Sanity check if we actually have a valid order provided.
                        if (customCallerCC.OrderInCampaign < 0 || customCallerCC.OrderInCampaign >=
                            currentCustomCampaign.CustomCallersInCampaign.Count)
                        {
                            MelonLogger.Error("ERROR: " +
                                              "Provided order is not valid! (Might be missing a caller(s) in between callers!)" +
                                              $" (Info: Provided Order: {customCallerCC.OrderInCampaign}; " +
                                              $"CampaignSize: {currentCustomCampaign.CustomCallersInCampaign.Count})");
                        }
                        else
                        {
                            if (__instance.callers[customCallerCC.OrderInCampaign] !=
                                null) // Adding to non-empty caller.
                            {
                                MelonLogger.Error("ERROR:" +
                                                  $" Provided caller {newProfile.callerName}" +
                                                  " has replaced a previous caller at " +
                                                  $"position {customCallerCC.OrderInCampaign}! Reducing array size by 1 to compensate. Things might break!");

                                Array.Resize(ref __instance.callers, __instance.callers.Length - 1);
                            }

                            __instance.callers[customCallerCC.OrderInCampaign] = new Caller
                            {
                                callerProfile = newProfile
                            };
                        }
                    }

                    // Add references for consequence caller.

                    if (listOfConsequenceCallers.Count > 0)
                    {
                        foreach (KeyValuePair<int, int> customCallerIDWithConsequenceCaller in listOfConsequenceCallers)
                        {
                            if (customCallerIDWithConsequenceCaller.Value <
                                currentCustomCampaign.CustomCallersInCampaign
                                    .Count) // We have a valid ConsequenceCaller ID.
                            {
                                // We check if the current consequence caller and the original caller exists.
                                if ((__instance.callers[customCallerIDWithConsequenceCaller.Key] != null) &&
                                    (__instance.callers[customCallerIDWithConsequenceCaller.Value] != null))
                                {
                                    // It exists
                                    __instance.callers[customCallerIDWithConsequenceCaller.Key].callerProfile
                                        .consequenceCallerProfile = __instance
                                        .callers[customCallerIDWithConsequenceCaller.Value].callerProfile;
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        "ERROR: Provided consequence caller cannot be created! Check if was created correctly! (Either original caller or the current consequence caller failed)");
                                }
                            }
                            else
                            {
                                MelonLogger.Error(
                                    "ERROR. Provided original caller for consequence caller does not exist! Check that you have the correct amount of callers!");
                            }
                        }
                    }
                }

                // Sanity check to prevent the callers from freezing up.
                if (__instance.callers.Length < 2)
                {
                    MelonLogger.Error(
                        "ERROR: Amount of callers is less than 2. It is highly recommended to have at least 2 to avoid any soft locks by the game.");
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "IsLastCallOfDay")]
        public static class LastCallOfDayPatch
        {
            /// <summary>
            /// Changes the function to also return if the last caller of the day to check for custom callers.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of original function. </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, ref bool __result)
            {
                Type callerController = typeof(CallerController);
                FieldInfo lastDayNumField = callerController.GetField("lastDayNum",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (lastDayNumField == null)
                {
                    MelonLogger.Error(
                        $"CallerController: lastDayNumField is null! Unable of checking if caller is last day.");
                    return true;
                }

                if (!CustomCampaignGlobal.InCustomCampaign) // In main Campaign.
                {
                    bool mainCampaignResult; // False

                    if (GlobalVariables.currentDay < (int)lastDayNumField.GetValue(__instance))
                    {
                        mainCampaignResult = __instance.callers[__instance.currentCallerID + 1].callerProfile
                            .increaseTier;
                    }
                    else
                    {
                        mainCampaignResult = __instance.callers[__instance.currentCallerID + 1] ==
                                             __instance.callers[__instance.callers.Length - 1];
                    }

                    __result = mainCampaignResult;
                }
                else if (CustomCampaignGlobal.InCustomCampaign)// Custom Campaign
                {
                    CustomCCaller customCCallerFound =
                        CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(__instance.currentCallerID);

                    if (customCCallerFound == null)
                    {
                        MelonLogger.Error(
                            $"ERROR: Was unable of finding the current caller. Calling original. For ID: {__instance.currentCallerID}");

                        foreach (CustomCCaller customCallerE in CustomCampaignGlobal.GetActiveCustomCampaign()
                                     .CustomCallersInCampaign)
                        {
                            MelonLogger.Error($"{customCallerE.CallerName} : {customCallerE.OrderInCampaign}");
                        }

                        return true;
                    }

                    // If the last caller of the day, this will result in true.
                    __result = customCCallerFound.LastDayCaller;

                    #if DEBUG
                    MelonLogger.Msg(
                        $"DEBUG: Last caller of day: '{__result}'. Caller name: '{customCCallerFound.CallerName}'.");

                    #endif
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "IncreaseTier")]
        public static class IncreaseTierPatch
        {
            /// <summary>
            /// Changes the function increase tier patch to work with better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, EntryUnlockController __instance)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Running increase tier!");
                #endif

                ++__instance.currentTier;

                // Get Private Methods (Original: this.OnIncreasedTierEvent(); )
                Type entryUnlockControllerType = typeof(EntryUnlockController);

                FieldInfo onIncreasedTierEvent = entryUnlockControllerType.GetField("OnIncreasedTierEvent",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (onIncreasedTierEvent != null)
                {
                    Delegate del = (Delegate)onIncreasedTierEvent.GetValue(__instance);

                    if (del != null)
                    {
                        del.DynamicInvoke(); // call with parameters if required by TierUpdate signature
                    }
                    else
                    {
                        #if DEBUG
                        MelonLogger.Msg("No subscribers for OnIncreasedTierEvent.");
                        #endif
                    }
                }
                else
                {
                    MelonLogger.Error("ERROR: Could not find backing field for OnIncreasedTierEvent.");
                }


                if (GlobalVariables.currentDay >= 7 &&
                    !CustomCampaignGlobal.InCustomCampaign) // Patched to work better with custom campaigns.
                {
                    return false;
                }

                GlobalVariables.mainCanvasScript.CreateError(
                    "PERMISSIONS HAVE BEEN UPDATED. NEW ENTRIES NOW AVAILABLE.");

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "AnswerCaller")]
        public static class AnswerCallerPatch
        {
            /// <summary>
            /// Patches answer caller to have more features for custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Called 'AnswerCaller' method.");
                #endif

                FieldInfo _givenWarning = typeof(CallerController).GetField("givenWarning",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                FieldInfo _firstCaller = typeof(CallerController).GetField("firstCaller",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                MethodInfo _answerDynamicCall = typeof(CallerController).GetMethod("AnswerDynamicCall",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

                if (_givenWarning == null || _answerDynamicCall == null || _firstCaller == null)
                {
                    MelonLogger.Error(
                        "ERROR: givenWarning or AnswerDynamicCall or firstCaller is null. Calling original function.");
                    return true;
                }

                bool normalCallerAfterCheck = false;

                if (!CustomCampaignGlobal.InCustomCampaign) // Not in custom campaign.
                {
                    if (GlobalVariables.isXmasDLC &&
                        GlobalVariables.cheerMeterScript.scoreDisplay * 100.0 <= __instance.xmasGameOverThreshold &&
                        GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                    {
                        __instance.TriggerGameOver();
                    }
                    else if (!GlobalVariables.arcadeMode && __instance.IsLastCallOfDay() &&
                             !GlobalVariables.isXmasDLC && !__instance.ScoreIsPassing(__instance.gameOverThreshold) &&
                             GlobalVariables.currentDay > 1 &&
                             GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                    {
                        __instance.TriggerGameOver();
                    }
                    else if (GlobalVariables.currentDay == 1 && __instance.callersToday == 3 &&
                             !__instance.ScoreIsPassing(__instance.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        _answerDynamicCall.Invoke(__instance,
                            new object[]
                                { __instance.warningCall }); // __instance.AnswerDynamicCall(__instance.warningCall);
                        _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);
                    }
                    else if (GlobalVariables.currentDay == 2 && __instance.callersToday == 2 &&
                             !__instance.ScoreIsPassing(__instance.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        _answerDynamicCall.Invoke(__instance,
                            new object[]
                                { __instance.warningCall }); // __instance.AnswerDynamicCall(__instance.warningCall);
                        _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);
                    }
                    else if (GlobalVariables.currentDay == 3 && __instance.callersToday == 3 &&
                             !__instance.ScoreIsPassing(__instance.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        _answerDynamicCall.Invoke(__instance,
                            new object[]
                                { __instance.warningCall }); // __instance.AnswerDynamicCall(__instance.warningCall);
                        _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);
                    }
                    else if (GlobalVariables.currentDay == 4 && __instance.callersToday == 4 &&
                             !__instance.ScoreIsPassing(__instance.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        _answerDynamicCall.Invoke(__instance,
                            new object[]
                                { __instance.warningCall }); // __instance.AnswerDynamicCall(__instance.warningCall);
                        _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);
                    }
                    else if (GlobalVariables.currentDay == 5 && __instance.callersToday == 5 &&
                             !__instance.ScoreIsPassing(__instance.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        _answerDynamicCall.Invoke(__instance,
                            new object[]
                                { __instance.warningCall }); // __instance.AnswerDynamicCall(__instance.warningCall);
                        _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);
                    }
                    else if (GlobalVariables.currentDay == 6 && __instance.callersToday == 7 &&
                             !__instance.ScoreIsPassing(__instance.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        _answerDynamicCall.Invoke(__instance,
                            new object[]
                                { __instance.warningCall }); // __instance.AnswerDynamicCall(__instance.warningCall);
                        _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);
                    }
                    else
                    {
                        normalCallerAfterCheck = true;
                    }
                }
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Answering caller in custom campaign.");
                    #endif

                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: customCampaign is null. Calling original function.");
                        return true;
                    }

                    int[] callersTodayMainCampaign = new int[] { 3, 2, 3, 4, 5, 7 };

                    // Not Arcade Mode, is last call of day?, not DLC, threshold correct, current day is after day 1 and no save immunity.
                    if (!GlobalVariables.arcadeMode && __instance.IsLastCallOfDay() && !GlobalVariables.isXmasDLC &&
                        !__instance.ScoreIsPassing(customCampaign.GameOverThreshold) &&
                        GlobalVariables.currentDay > 1 && GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                    {
                        __instance.TriggerGameOver();
                    }
                    else if (!__instance.ScoreIsPassing(customCampaign.WarningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: Caller (Warning) checks started.");
                        #endif

                        int callersTodayRequiredWarning;

                        if (GlobalVariables.currentDay <=
                            customCampaign.WarningCallThresholdCallerAmounts
                                .Count) // We have enough information per day until the warning call appears.
                        {
                            callersTodayRequiredWarning =
                                customCampaign.WarningCallThresholdCallerAmounts[GlobalVariables.currentDay - 1];
                        }
                        else
                        {
                            if (GlobalVariables.currentDay <= callersTodayMainCampaign.Length)
                            {
                                callersTodayRequiredWarning = callersTodayMainCampaign[GlobalVariables.currentDay - 1];
                            }
                            else
                            {
                                callersTodayRequiredWarning = 7; // If we go past the 6 calls. We default to 7.
                            }
                        }

                        #if DEBUG
                        MelonLogger.Msg(
                            $"DEBUG: Warning caller check for callers today required: {callersTodayRequiredWarning}." +
                            $" Current amount of callers: {__instance.callersToday}.");
                        #endif

                        if (__instance.callersToday ==
                            callersTodayRequiredWarning) // Now the warning call should appear.
                        {
                            CustomCCaller warningCCallerToday = null;

                            // Try finding a warning caller.
                            if (customCampaign.CustomWarningCallersInCampaign.Count >
                                0) // We actually have any warning call to insert here.
                            {
                                if (customCampaign.CustomWarningCallersInCampaign.Exists(warningCaller =>
                                        warningCaller.WarningCallDay <=
                                        -1)) // If we have warning caller without a day attached we use this one before trying to find a more fitting one.
                                {
                                    List<CustomCCaller> allWarningCallsWithoutDay =
                                        customCampaign.CustomWarningCallersInCampaign.FindAll(warningCaller =>
                                            warningCaller.WarningCallDay <= -1);

                                    if (allWarningCallsWithoutDay.Count > 0)
                                    {
                                        warningCCallerToday =
                                            allWarningCallsWithoutDay
                                                [Random.Range(0, allWarningCallsWithoutDay.Count)]; // Choose a random one from the available list.
                                    }
                                }

                                // Try finding a warning call that is set for the current day.
                                List<CustomCCaller> allWarningCallsForToday =
                                    customCampaign.CustomWarningCallersInCampaign.FindAll(warningCaller =>
                                        warningCaller.WarningCallDay == GlobalVariables.currentDay);
                                if (allWarningCallsForToday.Count > 0)
                                {
                                    warningCCallerToday =
                                        allWarningCallsForToday
                                            [Random.Range(0, allWarningCallsForToday.Count)]; // Choose a random one from the available list.
                                }
                            }

                            // If we found a warning call to replace it, we insert it here.
                            if (warningCCallerToday != null)
                            {
                                #if DEBUG
                                MelonLogger.Msg(
                                    $"DEBUG: Warning caller found to replace! {warningCCallerToday.CallerName}.");
                                #endif

                                CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                                newProfile.callerName = warningCCallerToday.CallerName;
                                newProfile.callTranscription = warningCCallerToday.CallTranscript;

                                // Fallback for missing picture or audio.
                                MethodInfo getRandomPicMethod = typeof(CallerController).GetMethod("PickRandomPic",
                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                                MethodInfo getRandomClip = typeof(CallerController).GetMethod("PickRandomClip",
                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                                if (getRandomPicMethod == null || getRandomClip == null)
                                {
                                    MelonLogger.Error(
                                        "ERROR: getRandomPicMethod or getRandomClip is null! Calling original function.");
                                    return true;
                                }

                                if (warningCCallerToday.CallerImage != null)
                                {
                                    newProfile.callerPortrait = warningCCallerToday.CallerImage;
                                }
                                else
                                {
                                    MelonLogger.Warning(
                                        "WARNING: Warning-Caller has no caller image, using random image.");

                                    newProfile.callerPortrait = (Sprite)getRandomPicMethod.Invoke(__instance, null);
                                }

                                if (warningCCallerToday.CallerClip != null)
                                {
                                    newProfile.callerClip = warningCCallerToday.CallerClip;
                                }
                                else
                                {
                                    if (AudioImport.CurrentLoadingAudios.Count > 0)
                                    {
                                        MelonLogger.Warning(
                                            "WARNING: Warning-Caller audio is still loading! Using fallback for now. If this happens often, please check if the audio is too large!");
                                    }
                                    else
                                    {
                                        MelonLogger.Warning(
                                            "WARNING: Warning-Caller has no audio! Using audio fallback. If you provided an audio but this error shows up, check for any errors before!");
                                    }

                                    newProfile.callerClip = (RichAudioClip)getRandomClip.Invoke(__instance, null);
                                }

                                if (!string.IsNullOrEmpty(warningCCallerToday.MonsterNameAttached) ||
                                    warningCCallerToday.MonsterIDAttached != -1)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: A monster was provided for the warning caller, but warning callers do not use any entries! Will default to none.");
                                }

                                newProfile.callerMonster = null;


                                if (warningCCallerToday.CallerIncreasesTier)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: Increase tier was provided for a warning caller! It will be set to false!");
                                }

                                newProfile.increaseTier = false;


                                if (warningCCallerToday.ConsequenceCallerID != -1)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: Warning callers cannot be consequence caller, ignoring option.");
                                }

                                newProfile.consequenceCallerProfile = null;

                                __instance.warningCall = newProfile;
                            }

                            // Insert warning caller.
                            _answerDynamicCall.Invoke(__instance,
                                new object[]
                                {
                                    __instance.warningCall
                                }); // __instance.AnswerDynamicCall(__instance.warningCall);
                            _givenWarning.SetValue(__instance, true); // __instance.givenWarning = true);   
                        }
                        else
                        {
                            normalCallerAfterCheck = true;
                        }
                    }
                    else
                    {
                        normalCallerAfterCheck = true;
                    }
                }

                // Since we have duplicated copies of this, we just have a flag called if that section is called.
                if (normalCallerAfterCheck) 
                {
                    if (!(bool)_firstCaller.GetValue(__instance) && !__instance.arcadeMode) // !__instance.firstCaller
                    {
                        ++__instance.currentCallerID;
                    }

                    if ((bool)_firstCaller.GetValue(__instance)) // __instance.firstCaller
                    {
                        _firstCaller.SetValue(__instance, false); // __instance.firstCaller = false;
                    }

                    __instance.UpdateCallerInfo();

                    if (!GlobalVariables.arcadeMode
                        && __instance.callers[__instance.currentCallerID].callerProfile.consequenceCallerProfile != null
                        && !__instance.CanReceiveConsequenceCall(__instance.callers[__instance.currentCallerID]
                            .callerProfile.consequenceCallerProfile))
                    {
                        #if DEBUG
                        MelonLogger.Msg(ConsoleColor.DarkMagenta, "DEBUG: Caller is dynamic caller. " +
                                                                  $"Marking as correct. (Last Caller? {__instance.IsLastCallOfDay()}) " +
                                                                  "Next caller!");
                        #endif

                        // This will skip the caller if the current caller is a consequence caller, and we don't need to show this caller.
                        // It will call itself and in the UpdateCallerInfo update the caller to the next caller.

                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;

                        // Here we insert a small check to see if this caller wants to end the day.
                        if (__instance.IsLastCallOfDay())
                        {
                            
                            #if DEBUG
                            MelonLogger.Msg(ConsoleColor.DarkYellow, "DEBUG: Calling end day routine from answer caller.");
                            #endif
                            
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript
                                .EndDayRoutine());
                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                            return false; // Skip original function.
                        }

                        // Next caller.
                        __instance.AnswerCaller();
                    }
                    else
                    {
                        // Custom Caller check before we continue.
                        if (CustomCampaignGlobal.InCustomCampaign)
                        {
                            if (CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(__instance.currentCallerID) !=
                                null)
                            {
                                // Accuracy Caller part.
                                CustomCCaller currentCaller =
                                    CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(__instance.currentCallerID);

                                if (currentCaller != null && currentCaller.IsAccuracyCaller)
                                {
                                    bool showCaller = AccuracyHelper.CheckIfCallerIsToBeShown(currentCaller);

                                    #if DEBUG
                                        MelonLogger.Msg($"DEBUG: Should the accuracy caller be shown? '{showCaller}'. ");
                                    #endif

                                    if (!showCaller)
                                    {
                                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;

                                        // Here we insert a small check to see if this caller wants to end the day.
                                        if (__instance.IsLastCallOfDay())
                                        {
                                            #if DEBUG
                                            MelonLogger.Msg(ConsoleColor.DarkYellow, "DEBUG: Calling end day routine from answer caller (accuracy caller).");
                                            #endif
                                            
                                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables
                                                .mainCanvasScript
                                                .EndDayRoutine());
                                            GlobalVariables.mainCanvasScript.NoCallerWindow();
                                            return false; // Skip original function.
                                        }

                                        // Next caller.
                                        __instance.AnswerCaller();

                                        return false;
                                    }
                                }
                            }
                        }
                        
                        if (GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.isPlaying)
                        {
                            GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                        }

                        __instance.PlayCallAudio();

                        FieldInfo _delayedLargeWindowDisplayRoutine =
                            typeof(CallerController).GetField("delayedLargeWindowDisplayRoutine",
                                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance |
                                BindingFlags.Public);
                        MethodInfo _waitTillCallEndRoutine = typeof(CallerController).GetMethod(
                            "WaitTillCallEndRoutine",
                            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                        if (_delayedLargeWindowDisplayRoutine == null || _waitTillCallEndRoutine == null)
                        {
                            MelonLogger.Error(
                                "ERROR: delayedLargeWindowDisplayRoutine or WaitTillCallEndRoutine is null. Calling original function.");
                            return true;
                        }

                        _delayedLargeWindowDisplayRoutine.SetValue(__instance,
                            __instance.StartCoroutine((IEnumerator)_waitTillCallEndRoutine.Invoke(__instance,
                                new object[]
                                {
                                    __instance.callers[__instance.currentCallerID].callerProfile.callerClip.clip.length
                                }))); // __instance.delayedLargeWindowDisplayRoutine = __instance.StartCoroutine(__instance.WaitTillCallEndRoutine(__instance.callers[__instance.currentCallerID].callerProfile.callerClip.clip.length));
                    }
                }

                return false; // Skip function with false.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "NewCallRoutine", typeof(float), typeof(float))]
        public static class NewCallRoutinePatch
        {
            /// <summary>
            /// The original function waits a bit before calling a new caller. It is patched to not error out after going back to the desktop.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            /// <param name="minTime"> Minimum time to wait. </param>
            /// <param name="maxTime"> Maximum time to wait. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance,
                // ReSharper disable once RedundantAssignment
                ref IEnumerator __result, ref float minTime, ref float maxTime)
            {
                __result = NewCallRoutine(__instance, minTime, maxTime);

                return false; // Skip the original function
            }

            private static IEnumerator NewCallRoutine(CallerController __instance, float minTime, float maxTime)
            {
                yield return new WaitForSeconds(Random.Range(minTime, maxTime));

                if (GlobalVariables.mainCanvasScript != null 
                    && GlobalVariables.mainCanvasScript.callWindow != null)
                {
                    GlobalVariables.mainCanvasScript.callWindow.SetActive(true);
                }
            }
        }
    }
}