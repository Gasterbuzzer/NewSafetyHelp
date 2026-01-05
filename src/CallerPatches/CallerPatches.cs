using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.JSONParsing;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CallerPatches
{
    public static class CallerPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "Start", new Type[] { })]
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

                FieldInfo _lastDayNum = callerController.GetField("lastDayNum",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_lastDayNum == null)
                {
                    MelonLogger.Error("ERROR: CallerController.lastDayNum is null!");
                    return true;
                }

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    _lastDayNum.SetValue(__instance, __instance.mainGameLastDay);
                }
                else
                {
                    _lastDayNum.SetValue(__instance, CustomCampaignGlobal.getActiveCustomCampaign().campaignDays);
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
                    _lastDayNum.SetValue(__instance, __instance.xmasLastDay);
                    __instance.downedNetworkCalls = __instance.xmasDownedNetworkCalls;
                }


                /*
                 * Add Custom Callers / Campaign Callers.
                 */

                if (!CustomCampaignGlobal.inCustomCampaign) // If we are not in a custom campaign. (Main Campaign)
                {
                    foreach (KeyValuePair<int, CustomCallerExtraInfo> customCaller in GlobalParsingVariables.CustomCallersMainGame)
                    {
                        if (customCaller.Key < 0 || customCaller.Value == null) // Sanity check
                        {
                            MelonLogger.Error($"ERROR: Custom caller {customCaller.Key} is invalid!");
                            continue;
                        }

                        if (customCaller.Value.inCustomCampaign)
                        {
                            MelonLogger.Warning(
                                "WARNING: Custom Caller is marked as custom campaign but is also main campaign! Skipping.");
                            continue;
                        }

                        // Is Valid

                        CallerProfile callerProfile = ScriptableObject.CreateInstance<CallerProfile>();

                        if (!customCaller.Value.isCallerClipLoaded)
                        {
                            MelonLogger.Msg(
                                "INFO: Audio is still loading for this custom caller. It will be updated once the audio has been updated.");
                        }

                        callerProfile.callerName = customCaller.Value.callerName;
                        callerProfile.callTranscription = customCaller.Value.callTranscript;
                        callerProfile.callerPortrait = customCaller.Value.callerImage;
                        callerProfile.callerClip = customCaller.Value.callerClip;
                        callerProfile.increaseTier = customCaller.Value.callerIncreasesTier;

                        if (customCaller.Value.monsterNameAttached != "NO_MONSTER_NAME")
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, customCaller.Value.monsterNameAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided Monster name '{customCaller.Value.monsterNameAttached}' for custom caller {customCaller.Key} was not found! Thus will not have any monster entry.");
                                callerProfile.callerMonster = null;
                            }
                            else
                            {
                                callerProfile.callerMonster = foundMonster;
                            }
                        }
                        else if (customCaller.Value.monsterIDAttached >= 0) // Check for ID monster.
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, monsterID: customCaller.Value.monsterIDAttached);

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

                        if (customCaller.Value.consequenceCallerID >= 0)
                        {
                            if (__instance.callers[customCaller.Value.consequenceCallerID].callerProfile == null)
                            {
                                MelonLogger.Warning(
                                    "WARNING: Provided consequence caller but profile is null? Setting to null.");
                            }

                            callerProfile.consequenceCallerProfile =
                                __instance.callers[customCaller.Value.consequenceCallerID].callerProfile;
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

                    if (string.IsNullOrEmpty(CustomCampaignGlobal.currentCustomCampaignName)) // Invalid Custom Campaign
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is set to be true but no custom campaign is active!");
                        return true;
                    }
                    else if (!CustomCampaignGlobal.customCampaignsAvailable.Exists(scannedCampaign =>
                                 scannedCampaign.campaignName ==
                                 CustomCampaignGlobal.currentCustomCampaignName)) // Custom Campaign is not registered.
                    {
                        MelonLogger.Error(
                            "ERROR: Current Custom Campaign has not been properly setup! Stopping loading.");
                        return true;
                    }

                    CustomCampaignExtraInfo currentCustomCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    // Clear callers array with amount of campaign callers.
                    __instance.callers = new Caller[currentCustomCampaign.customCallersInCampaign.Count];

                    if (currentCustomCampaign.customCallersInCampaign.Count <= 0)
                    {
                        MelonLogger.Warning(
                            "WARNING: Custom Campaign has no custom caller assigned! Unexpected behavior will occur when in campaign.");
                    }

                    // Reference list for consequence caller (after adding all profiles).
                    Dictionary<int, int> listOfConsequenceCallers = new Dictionary<int, int>();

                    // Add all customCallers in Callers list.
                    foreach (CustomCallerExtraInfo customCallerCC in currentCustomCampaign.customCallersInCampaign)
                    {
                        CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                        newProfile.callerName = customCallerCC.callerName;
                        newProfile.callTranscription = customCallerCC.callTranscript;

                        // Clip
                        if (customCallerCC.callerClip == null)
                        {
                            if (AudioImport.currentLoadingAudios.Count > 0)
                            {
                                MelonLogger.Msg(
                                    $"INFO : Custom Caller '{customCallerCC.callerName}' is still loading its audio. Using fallback for now.");
                            }
                            else // No Loading Audio
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Custom Caller '{customCallerCC.callerName}' does not have any valid audio clip! Using fallback instead of real audio. ");
                            }

                            newProfile.callerClip = (RichAudioClip)getRandomClip.Invoke(__instance, new object[] { });
                        }
                        else
                        {
                            newProfile.callerClip = customCallerCC.callerClip;
                        }

                        // Sprite
                        if (customCallerCC.callerImage == null)
                        {
                            MelonLogger.Warning(
                                $"WARNING: Custom Caller '{(customCallerCC.callerName != null ? $"{customCallerCC.callerName}" : "")}' does not have any valid image / sprite. Using fallback for now.");

                            newProfile.callerPortrait = (Sprite)getRandomPicMethod.Invoke(__instance, new object[] { });
                        }
                        else
                        {
                            newProfile.callerPortrait = customCallerCC.callerImage;
                        }

                        // Adding Entry to Caller if valid.
                        if (customCallerCC.monsterNameAttached != "NO_MONSTER_NAME")
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, customCallerCC.monsterNameAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided Monster name '{customCallerCC.monsterNameAttached}' for custom caller {customCallerCC.callerName} was not found! Thus will not have any monster entry.");
                                newProfile.callerMonster = null;
                            }
                            else
                            {
                                newProfile.callerMonster = foundMonster;
                            }
                        }
                        else if (customCallerCC.monsterIDAttached >= 0) // Check for ID monster.
                        {
                            MonsterProfile foundMonster = EntryManager.EntryManager.FindEntry(
                                ref GameObject.Find("EntryUnlockController").GetComponent<EntryUnlockController>()
                                    .allEntries.monsterProfiles, monsterID: customCallerCC.monsterIDAttached);

                            if (foundMonster == null)
                            {
                                MelonLogger.Warning(
                                    $"WARNING: Provided monster ID for custom caller {customCallerCC.callerName} was not found! Thus will not have any monster entry.");
                                newProfile.callerMonster = null;
                            }
                            else
                            {
                                newProfile.callerMonster = foundMonster;
                            }
                        }

                        if (customCallerCC.consequenceCallerID >= 0) // We have a consequence caller ID provided.
                        {
                            listOfConsequenceCallers.Add(customCallerCC.orderInCampaign,
                                customCallerCC.consequenceCallerID); // Add for processing later.
                        }

                        // Increase Tier
                        newProfile.increaseTier = customCallerCC.callerIncreasesTier;

                        // Sanity check if we actually have a valid order provided.
                        if (customCallerCC.orderInCampaign < 0 || customCallerCC.orderInCampaign >=
                            currentCustomCampaign.customCallersInCampaign.Count)
                        {
                            MelonLogger.Error("ERROR: " +
                                              "Provided order is not valid! (Might be missing a caller(s) in between callers!)" +
                                              $" (Info: Provided Order: {customCallerCC.orderInCampaign}; " +
                                              $"CampaignSize: {currentCustomCampaign.customCallersInCampaign.Count})");
                        }
                        else
                        {
                            if (__instance.callers[customCallerCC.orderInCampaign] !=
                                null) // Adding to non-empty caller.
                            {
                                MelonLogger.Error("ERROR:" +
                                                  $" Provided caller {newProfile.callerName}" +
                                                  " has replaced a previous caller at " +
                                                  $"position {customCallerCC.orderInCampaign}! Reducing array size by 1 to compensate. Things might break!");

                                Array.Resize(ref __instance.callers, __instance.callers.Length - 1);
                            }

                            __instance.callers[customCallerCC.orderInCampaign] = new Caller
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
                                currentCustomCampaign.customCallersInCampaign
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

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "IsLastCallOfDay", new Type[] { })]
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

                if (!CustomCampaignGlobal.inCustomCampaign) // In main Campaign.
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
                else // Custom Campaign
                {
                    CustomCallerExtraInfo customCallerFound =
                        CustomCampaignGlobal.getCustomCallerFromActiveCampaign(__instance.currentCallerID);

                    if (customCallerFound == null)
                    {
                        MelonLogger.Error(
                            $"ERROR: Was unable of finding the current caller. Calling original. For ID: {__instance.currentCallerID}");

                        foreach (CustomCallerExtraInfo customCallerE in CustomCampaignGlobal.getActiveCustomCampaign()
                                     .customCallersInCampaign)
                        {
                            MelonLogger.Error($"{customCallerE.callerName} : {customCallerE.orderInCampaign}");
                        }

                        return true;
                    }

                    // If the last caller of the day, this will result in true.
                    __result = customCallerFound.lastDayCaller;

                    #if DEBUG
                    MelonLogger.Msg(
                        $"DEBUG: Last caller of day: '{__result}'. Caller name: '{customCallerFound.callerName}'.");

                    #endif
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "IncreaseTier", new Type[] { })]
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
                    !CustomCampaignGlobal.inCustomCampaign) // Patched to work better with custom campaigns.
                {
                    return false;
                }

                GlobalVariables.mainCanvasScript.CreateError(
                    "PERMISSIONS HAVE BEEN UPDATED. NEW ENTRIES NOW AVAILABLE.");

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "AnswerCaller", new Type[] { })]
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

                if (!CustomCampaignGlobal.inCustomCampaign) // Not in custom campaign.
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
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign
                {
                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Answering caller in custom campaign.");
                    #endif

                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: customCampaign is null. Calling original function.");
                        return true;
                    }

                    int[] callersTodayMainCampaign = new int[] { 3, 2, 3, 4, 5, 7 };

                    // Not Arcade Mode, is last call of day?, not DLC, threshold correct, current day is after day 1 and no save immunity.
                    if (!GlobalVariables.arcadeMode && __instance.IsLastCallOfDay() && !GlobalVariables.isXmasDLC &&
                        !__instance.ScoreIsPassing(customCampaign.gameOverThreshold) &&
                        GlobalVariables.currentDay > 1 && GlobalVariables.saveManagerScript.savedImmunityToggle == 0)
                    {
                        __instance.TriggerGameOver();
                    }
                    else if (!__instance.ScoreIsPassing(customCampaign.warningThreshold) &&
                             !(bool)_givenWarning.GetValue(__instance)) // !__instance.givenWarning
                    {
                        #if DEBUG
                        MelonLogger.Msg($"DEBUG: Caller (Warning) checks started.");
                        #endif

                        int callersTodayRequiredWarning;

                        if (GlobalVariables.currentDay <=
                            customCampaign.warningCallThresholdCallerAmounts
                                .Count) // We have enough information per day until the warning call appears.
                        {
                            callersTodayRequiredWarning =
                                customCampaign.warningCallThresholdCallerAmounts[GlobalVariables.currentDay - 1];
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
                            CustomCallerExtraInfo warningCallerToday = null;

                            // Try finding a warning caller.
                            if (customCampaign.customWarningCallersInCampaign.Count >
                                0) // We actually have any warning call to insert here.
                            {
                                if (customCampaign.customWarningCallersInCampaign.Exists(warningCaller =>
                                        warningCaller.warningCallDay <=
                                        -1)) // If we have warning caller without a day attached we use this one before trying to find a more fitting one.
                                {
                                    List<CustomCallerExtraInfo> allWarningCallsWithoutDay =
                                        customCampaign.customWarningCallersInCampaign.FindAll(warningCaller =>
                                            warningCaller.warningCallDay <= -1);

                                    if (allWarningCallsWithoutDay.Count > 0)
                                    {
                                        warningCallerToday =
                                            allWarningCallsWithoutDay
                                                [Random.Range(0, allWarningCallsWithoutDay.Count)]; // Choose a random one from the available list.
                                    }
                                }

                                // Try finding a warning call that is set for the current day.
                                List<CustomCallerExtraInfo> allWarningCallsForToday =
                                    customCampaign.customWarningCallersInCampaign.FindAll(warningCaller =>
                                        warningCaller.warningCallDay == GlobalVariables.currentDay);
                                if (allWarningCallsForToday.Count > 0)
                                {
                                    warningCallerToday =
                                        allWarningCallsForToday
                                            [Random.Range(0, allWarningCallsForToday.Count)]; // Choose a random one from the available list.
                                }
                            }

                            // If we found a warning call to replace it, we insert it here.
                            if (warningCallerToday != null)
                            {
                                #if DEBUG
                                MelonLogger.Msg(
                                    $"DEBUG: Warning caller found to replace! {warningCallerToday.callerName}.");
                                #endif

                                CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                                newProfile.callerName = warningCallerToday.callerName;
                                newProfile.callTranscription = warningCallerToday.callTranscript;

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

                                if (warningCallerToday.callerImage != null)
                                {
                                    newProfile.callerPortrait = warningCallerToday.callerImage;
                                }
                                else
                                {
                                    MelonLogger.Warning(
                                        "WARNING: Warning-Caller has no caller image, using random image.");

                                    newProfile.callerPortrait = (Sprite)getRandomPicMethod.Invoke(__instance, null);
                                }

                                if (warningCallerToday.callerClip != null)
                                {
                                    newProfile.callerClip = warningCallerToday.callerClip;
                                }
                                else
                                {
                                    if (AudioImport.currentLoadingAudios.Count > 0)
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

                                if (!string.IsNullOrEmpty(warningCallerToday.monsterNameAttached) ||
                                    warningCallerToday.monsterIDAttached != -1)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: A monster was provided for the warning caller, but warning callers do not use any entries! Will default to none.");
                                }

                                newProfile.callerMonster = null;


                                if (warningCallerToday.callerIncreasesTier)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: Increase tier was provided for a warning caller! It will be set to false!");
                                }

                                newProfile.increaseTier = false;


                                if (warningCallerToday.consequenceCallerID != -1)
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

                if (normalCallerAfterCheck) // Since we have duplicated copies of this, we just have a flag called if that section is called.
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
                __result = newCallRoutine(__instance, minTime, maxTime);

                return false; // Skip the original function
            }

            private static IEnumerator newCallRoutine(CallerController __instance, float minTime, float maxTime)
            {
                yield return new WaitForSeconds(Random.Range(minTime, maxTime));

                if (GlobalVariables.mainCanvasScript != null && GlobalVariables.mainCanvasScript.callWindow != null)
                {
                    GlobalVariables.mainCanvasScript.callWindow.SetActive(true);
                }
            }
        }
    }
}