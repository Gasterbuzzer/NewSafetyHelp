using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CallerPatches.IncomingCallWindow;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.JSONParsing;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CallerPatches
{
    public static class CallerPatches
    {
        // Patches the caller to have a custom caller clip in arcade mode.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CreateCustomCaller", new Type[] { })]
        public static class UpdateArcadeCallerAudio
        {
            /// <summary>
            /// Update the list when opening.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static void Postfix(MethodBase __originalMethod, CallerController __instance)
            {
                foreach (EntryExtraInfo item in ParseJSONFiles.entriesExtraInfo)
                {
                    if (__instance.currentCustomCaller.callerMonster.monsterName == item.Name ||
                        __instance.currentCustomCaller.callerMonster.monsterID ==
                        item.ID) // We found an entry to replace the audio for.
                    {
                        __instance.currentCustomCaller.callerClip = item.callerClip;
                    }
                }

                if (__instance.currentCustomCaller.callerClip ==
                    null) // If we didn't find anything, we set it to a random clip.
                {
                    MethodInfo pickRandomClip = typeof(CallerController).GetMethod("PickRandomClip",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                    if (pickRandomClip == null)
                    {
                        MelonLogger.Error("ERROR: PickRandomClip couldn't be found in CallerController.");
                        return;
                    }

                    __instance.currentCustomCaller.callerClip =
                        (RichAudioClip)pickRandomClip.Invoke(__instance, null); // __instance.PickRandomClip()
                }
            }
        }

        // Patches the caller to have a custom audio in campaign.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PlayCallAudioRoutine", new[] { typeof(CallerProfile) })]
        public static class UpdateCampaignCallerAudio
        {
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, CallerProfile profile,
                ref IEnumerator __result)
            {
                __result = originalCaller(__originalMethod, __instance, profile);
                return false; // Skip the original coroutine
            }

            /// <summary>
            /// Patches the IEnumerator to be with the custom Audio.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name= "profile"> Profile Parameter. </param>
            private static IEnumerator originalCaller(MethodBase __originalMethod, CallerController __instance,
                CallerProfile profile)
            {
                if (profile == null && !__instance.arcadeMode)
                {
                    profile = __instance.callers[__instance.currentCallerID].callerProfile;
                }

                if (__instance.arcadeMode)
                {
                    profile = __instance.currentCustomCaller;
                }

                yield return new WaitForSeconds(__instance.playCallAudioDelayTime);

                // Get callerAudioSource
                Type callerController = typeof(CallerController);
                var callerAudioSourceGetter = callerController.GetField("callerAudioSource",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (callerAudioSourceGetter == null)
                {
                    MelonLogger.Error($"CallerController: callerAudioSourceGetter is null");
                    yield break;
                }

                AudioSource callerAudioSource = (AudioSource)callerAudioSourceGetter.GetValue(__instance);

                if (callerAudioSource == null)
                {
                    MelonLogger.Error($"CallerController: callerAudioSource is null");
                    yield break;
                }

                callerAudioSource.Stop();

                // Here we replace the clip-
                bool found = false;

                if (profile != null && profile.callerMonster != null &&
                    !__instance
                        .arcadeMode) // We only check if the caller has any entry to begin with. We will need to handle arcade mode later or scrap that idea.
                {
                    foreach (EntryExtraInfo item in ParseJSONFiles.entriesExtraInfo)
                    {
                        if (item.currentlySelected) // We found an entry to replace the audio for.
                        {
                            item.alreadyCalledOnce = true;

                            // We now check if we are allowed to save if the entry can be saved as already called.
                            if (!item.allowCallAgainOverRestart)
                            {
                                if (!NewSafetyHelpMainClass.persistantEntrySave.HasEntry(item.Name + item.callerName))
                                {
                                    // ReSharper disable once RedundantTypeArgumentsOfMethod
                                    NewSafetyHelpMainClass.persistantEntrySave.CreateEntry<bool>(
                                        item.Name + item.callerName, true);
                                }
                                else
                                {
                                    NewSafetyHelpMainClass.persistantEntrySave
                                        .GetEntry<bool>(item.Name + item.callerName).Value = true;
                                }
                            }
                            else
                            {
                                if (!NewSafetyHelpMainClass.persistantEntrySave.HasEntry(item.Name + item.callerName))
                                {
                                    // ReSharper disable once RedundantTypeArgumentsOfMethod
                                    NewSafetyHelpMainClass.persistantEntrySave.CreateEntry<bool>(
                                        item.Name + item.callerName, false); // Store it as false
                                }
                                else
                                {
                                    NewSafetyHelpMainClass.persistantEntrySave
                                        .GetEntry<bool>(item.Name + item.callerName).Value = false;
                                }
                            }

                            // Current selection is set to false once the answer for the caller has been submitted.

                            if (item.callerClip != null && item.callerClip.clip != null)
                            {
                                callerAudioSource.clip = item.callerClip.clip;
                            }

                            found = true;
                        }
                    }
                }

                // Else
                if (!found)
                {
                    if (profile != null && profile.callerMonster != null)
                    {
                        #if DEBUG
                        MelonLogger.Msg(
                            $"DEBUG: Monster Name: {profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}.");
                        #endif
                    }
                    else
                    {
                        MelonLogger.Msg("INFO: This caller does not have a monster entry. Thus not replaced.");
                    }

                    if (profile != null && profile.callerClip != null && profile.callerClip.clip != null)
                    {
                        callerAudioSource.clip = profile.callerClip.clip;
                    }
                }

                #if DEBUG
                MelonLogger.Msg(
                    $"DEBUG: Caller Audio File Name: {callerAudioSource.name} with {callerAudioSource.clip.name} and {callerAudioSource.clip.length}.");
                #endif

                if (profile != null && profile.callerClip != null)
                {
                    callerAudioSource.volume = profile.callerClip.volume;
                }

                if (callerAudioSource.clip != null)
                {
                    callerAudioSource.Play();
                }
            }
        }

        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "UpdateCallerInfo", new[] { typeof(CallerProfile) })]
        public static class UpdateCampaignCallerRandom
        {
            /// <summary>
            /// Patch for the caller info to be updated to a custom entry.
            /// </summary>
            /// <param name="__originalMethod"> Original Method Caller </param>
            /// <param name="__instance"> Function Caller Instance </param>
            /// <param name="profile"> Caller Profile that called </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance,
                ref CallerProfile profile)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: New caller is calling.");
                #endif

                if (profile == null)
                {
                    if (__instance.callers[__instance.currentCallerID] != null)
                    {
                        profile = __instance.callers[__instance.currentCallerID].callerProfile;
                    }
                    else
                    {
                        MelonLogger.Error("ERROR: Caller is null. Unable of calling. " +
                                          "You may have a duplicate caller (Meaning the same ID on two callers)!");
                        return false;
                    }
                }

                if (__instance.arcadeMode)
                {
                    profile = __instance.currentCustomCaller;
                }

                List<EntryExtraInfo> entries = new List<EntryExtraInfo>();

                bool replaceTrue = false;

                if (!CustomCampaignGlobal.inCustomCampaign && !__instance.arcadeMode)
                {
                    foreach (EntryExtraInfo item in ParseJSONFiles.entriesExtraInfo)
                    {
                        if (item.inMainCampaign && !item.alreadyCalledOnce &&
                            !item.currentlySelected) // Find a valid entry.
                        {
                            // Create Entry if not existent and if allowed

                            MelonPreferences_Entry<bool> entryAlreadyCalledBeforeEntry;

                            if (!NewSafetyHelpMainClass.persistantEntrySave.HasEntry(item.Name + item.callerName))
                            {
                                // ReSharper disable once RedundantTypeArgumentsOfMethod
                                entryAlreadyCalledBeforeEntry =
                                    NewSafetyHelpMainClass.persistantEntrySave.CreateEntry<bool>(
                                        item.Name + item.callerName, false);
                            }
                            else
                            {
                                entryAlreadyCalledBeforeEntry =
                                    NewSafetyHelpMainClass.persistantEntrySave.GetEntry<bool>(item.Name +
                                        item.callerName);
                            }

                            if (item.allowCallAgainOverRestart)
                            {
                                #if DEBUG
                                MelonLogger.Msg(
                                    $"INFO: Entry {item.Name} is allowed to be called again even if called once in the past.");
                                #endif

                                entryAlreadyCalledBeforeEntry.Value =
                                    false; // Reset the entry. If not allowed to store the value.
                            }

                            if (entryAlreadyCalledBeforeEntry.Value)
                            {
                                #if DEBUG
                                MelonLogger.Msg(
                                    $"INFO: Entry {item.Name} was already called once, so it will not be available for calling.");
                                #endif
                            }

                            if (Random.Range(0.0f, 1.0f) <= item.callerReplaceChance)
                            {
                                if (!item
                                        .allowCallAgainOverRestart) // We check if we already called once, if yes, we skip and if not we continue (setting is done later).
                                {
                                    if (!entryAlreadyCalledBeforeEntry.Value &&
                                        item.permissionLevel <=
                                        GlobalVariables
                                            .currentDay) // We never called it. And make sure we can actually access the callers' entry.
                                    {
                                        if (GlobalVariables.isXmasDLC) // If DLC
                                        {
                                            if (item.includeInDLC || item.onlyDLC) // Is allowed to be added?
                                            {
                                                entries.Add(item);
                                                replaceTrue = true;

                                                MelonLogger.Msg(
                                                    $"INFO: Saved Entry '{item.Name}' to not be called in the future.");

                                                entryAlreadyCalledBeforeEntry.Value = true;
                                            }
                                            else
                                            {
                                                MelonLogger.Msg(
                                                    $"INFO: Entry '{item.Name}' is not allowed to be called in DLC Mode.");
                                            }
                                        }
                                        else // Main Game
                                        {
                                            entries.Add(item);
                                            replaceTrue = true;

                                            MelonLogger.Msg(
                                                $"INFO: Saved Entry '{item.Name}' to not be called in the future.");

                                            entryAlreadyCalledBeforeEntry.Value = true;
                                        }
                                    }
                                    // Else we skip it.
                                }
                                else // We are allowed to ignore it.
                                {
                                    if (item.permissionLevel <=
                                        GlobalVariables
                                            .currentDay) // Make sure we can actually access the callers' entry.
                                    {
                                        if (GlobalVariables.isXmasDLC) // If DLC
                                        {
                                            if (item.includeInDLC || item.onlyDLC) // Is allowed to be added?
                                            {
                                                entries.Add(item);
                                                replaceTrue = true;
                                            }
                                        }
                                        else // Main Game
                                        {
                                            entries.Add(item);
                                            replaceTrue = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!CustomCampaignGlobal.inCustomCampaign) // Not in custom campaign. This makes odd problems in custom campaigns.
                {
                    if (profile != null && !__instance.arcadeMode &&
                        profile.consequenceCallerProfile !=
                        null) // We are a consequence caller. (Since we don't replace, and we don't have a caller monster.)
                    {
                        MelonLogger.Msg($"INFO: Current caller is Consequence Caller.");
                        Caller callers = GetConsequenceCaller(profile, ref __instance.callers);

                        if (callers != null) // Caller is valid.
                        {
                            MelonLogger.Msg($"Consequence Caller name: {callers.callerProfile.name}");

                            if (ParseJSONFiles.entriesExtraInfo.Exists(item =>
                                    item.referenceProfileNameInternal ==
                                    callers.callerProfile.consequenceCallerProfile
                                        .name)) // IF the consequence caller has been replaced once.
                            {
                                MelonLogger.Msg($"INFO: Consequence Caller to be replaced found!");
                                EntryExtraInfo foundExtraInfo = ParseJSONFiles.entriesExtraInfo.Find(item =>
                                    item.referenceProfileNameInternal ==
                                    callers.callerProfile.consequenceCallerProfile.name);

                                if (foundExtraInfo == null)
                                {
                                    MelonLogger.Error($"INFO: Did not find replacement caller.");
                                    return true;
                                }

                                // It was replaced once, so we also change the consequence caller info.
                                profile.callTranscription = foundExtraInfo.consequenceTranscript;
                                profile.callerName = foundExtraInfo.consequenceName;
                                profile.callerPortrait = foundExtraInfo.consequenceCallerImage;
                                profile.callerClip = foundExtraInfo.consequenceCallerClip;

                                MelonLogger.Msg(
                                    $"INFO: Replaced the current caller transcript with: {profile.callTranscription}.");
                            }
                        }
                        else
                        {
                            MelonLogger.Error($"INFO: Did not find initial caller.");
                        }
                    }
                    // Replace information about the caller with a random entry
                    else if (replaceTrue && profile != null && profile.callerMonster != null &&
                             !__instance.arcadeMode &&
                             profile.consequenceCallerProfile ==
                             null) // If any entry won the chance to replace this call, replace it.
                    {
                        if (entries.Count > 0) // We actually found at least one.
                        {
                            // We are not a consequence caller.
                            // Select one randomly.
                            int entrySelected = Random.Range(0, entries.Count - 1);

                            // Audio check
                            ParseJSONFiles.entriesExtraInfo.Find(item => item.Equals(entries[entrySelected]))
                                .currentlySelected = true;

                            // Get a "copy"
                            EntryExtraInfo selected = entries[entrySelected];

                            // Replace caller with custom caller
                            profile.callerName = selected.callerName;

                            if (selected.callerImage != null) // If Image provided
                            {
                                profile.callerPortrait = selected.callerImage;
                            }

                            profile.callTranscription = selected.callTranscript;

                            if (profile != null && profile.callerMonster != null)
                            {
                                MelonLogger.Msg(
                                    $"INFO: Replaced the current caller ({profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}) with a custom caller: {selected.Name} with ID: {selected.ID}.");
                            }

                            // We store a reference to the caller for finding later if the consequence caller calls.
                            ParseJSONFiles.entriesExtraInfo.Find(item => item.Equals(entries[entrySelected]))
                                .referenceProfileNameInternal = profile.name;
                        }
                    }
                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Finished handling the caller replacement.");
                    #endif
                }

                __instance.currentCallerProfile = profile;
                GlobalVariables.mainCanvasScript.UpdateCallerInfo(profile);

                return false; // Skip the original function
            }
        }

        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CheckCallerAnswer", new[] { typeof(MonsterProfile) })]
        public static class ReplaceAnswerWithReplacedAnswer
        {
            /// <summary>
            /// Patch the caller answer check to be the custom caller/entry.
            /// </summary>
            /// <param name="__originalMethod"> Original Method Caller. </param>
            /// <param name="__instance"> Function Caller Instance </param>
            /// <param name="monsterID"> Monster Profile that was selected </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance,
                ref MonsterProfile monsterID)
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
                    foreach (EntryExtraInfo item in ParseJSONFiles.entriesExtraInfo)
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

        /// <summary>
        /// Gets the consequence caller based on a provided profile from a caller list.
        /// </summary>
        /// <param name="profileToCheck"></param>
        /// <param name="callers"></param>
        /// <returns></returns>
        [CanBeNull]
        public static Caller GetConsequenceCaller(CallerProfile profileToCheck, ref Caller[] callers)
        {
            foreach (Caller caller in callers)
            {
                if (profileToCheck == caller.callerProfile)
                    return caller; // Returns the caller
            }

            return null;
        }

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
                    foreach (KeyValuePair<int, CustomCallerExtraInfo> customCaller in ParseJSONFiles
                                 .customCallerMainGame)
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

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "SubmitAnswer", new[] { typeof(MonsterProfile) })]
        public static class SubmitAnswerPatch
        {
            /// <summary>
            /// Changes the function to work with better with custom campaigns. By also increasing tier on last call if available.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="monsterID"> Reference to parameter having the monster ID. </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance,
                ref MonsterProfile monsterID)
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
                        && !__instance.CanReceiveConsequenceCall(__instance.callers[__instance.currentCallerID].callerProfile.consequenceCallerProfile))
                    {

                        #if DEBUG
                            MelonLogger.Msg(ConsoleColor.DarkMagenta, "DEBUG: Caller is dynamic caller. " +
                                                                      "Marking as correct. " +
                                                                      "Next caller!");
                        #endif
                        
                        // This will skip the caller if the current caller is a consequence caller, and we don't need to show this caller.
                        // It will call itself and in the UpdateCallerInfo update the caller to the next caller.
                        
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;
                        
                        // Here we insert a small check to see if this caller wants to end the day.
                        if (__instance.IsLastCallOfDay())
                        {
                            GlobalVariables.mainCanvasScript.StartCoroutine(GlobalVariables.mainCanvasScript.EndDayRoutine());
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

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "TriggerGameOver", new Type[] { })]
        public static class TriggerGameOverPatch
        {
            /// <summary>
            /// This function calls the GameOver phone call and triggers the game over cutscene. It is patched to be able to have custom GameOver Callers in custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance)
            {
                #if DEBUG
                MelonLogger.Msg($"DEBUG: Triggering GameOver Call + GameOver Cutscene.");
                #endif

                MethodInfo answerDynamicCall = typeof(CallerController).GetMethod("AnswerDynamicCall",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo triggerGameOver = typeof(CallerController).GetField("triggerGameOver",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

                if (answerDynamicCall == null || triggerGameOver == null)
                {
                    MelonLogger.Error(
                        "ERROR: AnswerDynamicCall or triggerGameOver is null. Calling original function.");
                    return true;
                }

                if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null. Calling original function.");
                        return true;
                    }

                    if (customCampaign.customGameOverCallersInCampaign.Count > 0)
                    {
                        CustomCallerExtraInfo customCallerGameOverChosen = null;

                        if (customCampaign.customGameOverCallersInCampaign.Exists(customCaller =>
                                customCaller.gameOverCallDay <= -1))
                        {
                            // Will choose a random game over caller if all are set at -1.
                            customCallerGameOverChosen =
                                customCampaign.customGameOverCallersInCampaign.FindAll(customCaller =>
                                    customCaller.gameOverCallDay <= -1)[
                                    Random.Range(0, customCampaign.customGameOverCallersInCampaign.Count)];
                        }

                        // If any exist that are valid for the current day, we instead replace it with those.
                        if (customCampaign.customGameOverCallersInCampaign.Exists(customCaller =>
                                customCaller.gameOverCallDay == GlobalVariables.currentDay))
                        {
                            customCallerGameOverChosen =
                                customCampaign.customGameOverCallersInCampaign.FindAll(customCaller =>
                                    customCaller.gameOverCallDay == GlobalVariables.currentDay)[
                                    Random.Range(0, customCampaign.customGameOverCallersInCampaign.Count)];
                        }

                        // Create custom caller and then replace gameOverCall with it.
                        if (customCallerGameOverChosen != null)
                        {
                            #if DEBUG
                            MelonLogger.Msg(
                                $"DEBUG: GameOver caller found to replace! {customCallerGameOverChosen.callerName}.");
                            #endif

                            CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                            newProfile.callerName = customCallerGameOverChosen.callerName;
                            newProfile.callTranscription = customCallerGameOverChosen.callTranscript;

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

                            if (customCallerGameOverChosen.callerImage != null)
                            {
                                newProfile.callerPortrait = customCallerGameOverChosen.callerImage;
                            }
                            else
                            {
                                MelonLogger.Warning(
                                    "WARNING: GameOver-Caller has no caller image, using random image.");

                                newProfile.callerPortrait = (Sprite)getRandomPicMethod.Invoke(__instance, null);
                            }

                            if (customCallerGameOverChosen.callerClip != null)
                            {
                                newProfile.callerClip = customCallerGameOverChosen.callerClip;
                            }
                            else
                            {
                                if (AudioImport.currentLoadingAudios.Count > 0)
                                {
                                    MelonLogger.Warning(
                                        "WARNING: GameOver-Caller audio is still loading! Using fallback for now. If this happens often, please check if the audio is too large!");
                                }
                                else
                                {
                                    MelonLogger.Warning(
                                        "WARNING: GameOver-Caller has no audio! Using audio fallback. If you provided an audio but this error shows up, check for any errors before!");
                                }

                                newProfile.callerClip = (RichAudioClip)getRandomClip.Invoke(__instance, null);
                            }

                            if (!string.IsNullOrEmpty(customCallerGameOverChosen.monsterNameAttached) ||
                                customCallerGameOverChosen.monsterIDAttached != -1)
                            {
                                MelonLogger.Warning(
                                    "WARNING: A monster was provided for the GameOver caller, but GameOver callers do not use any entries! Will default to none.");
                            }

                            newProfile.callerMonster = null;


                            if (customCallerGameOverChosen.callerIncreasesTier)
                            {
                                MelonLogger.Warning(
                                    "WARNING: Increase tier was provided for a GameOver caller! It will be set to false!");
                            }

                            newProfile.increaseTier = false;


                            if (customCallerGameOverChosen.consequenceCallerID != -1)
                            {
                                MelonLogger.Warning(
                                    "WARNING: GameOver Callers cannot be consequence caller, ignoring option.");
                            }

                            newProfile.consequenceCallerProfile = null;

                            __instance.gameOverCall = newProfile; // Replace the GameOver caller
                        }
                    }
                }

                // If any custom caller was "injected", we can now call it.

                answerDynamicCall.Invoke(__instance,
                    new object[] { __instance.gameOverCall }); //__instance.AnswerDynamicCall(__instance.gameOverCall);

                triggerGameOver.SetValue(__instance, true); //__instance.triggerGameOver = true;

                return false; // Skip the original function
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

            public static IEnumerator newCallRoutine(CallerController __instance, float minTime, float maxTime)
            {
                yield return new WaitForSeconds(Random.Range(minTime, maxTime));

                if (GlobalVariables.mainCanvasScript != null && GlobalVariables.mainCanvasScript.callWindow != null)
                {
                    GlobalVariables.mainCanvasScript.callWindow.SetActive(true);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PickRandomClip")]
        public static class PickRandomClipPatch
        {
            /// <summary>
            /// The original function picks a random caller clip. Since missing callers may cause issues or errors. We now inform the user and prevent the issue.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance,
                // ReSharper disable once RedundantAssignment
                ref RichAudioClip __result)
            {
                if (__instance.randomCallerClips.Length <= 0)
                {
                    MelonLogger.Warning("WARNING: No caller audio available for caller!");
                    __result = null;
                    return false;
                }

                int num = Random.Range(0, __instance.randomCallerClips.Length);
                __result = __instance.randomCallerClips[num];

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PickRandomPic")]
        public static class PickRandomPicPatch
        {
            /// <summary>
            /// The original function picks a random caller picture. Since missing callers may cause issues or errors. We now inform the user and prevent the issue.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, ref Sprite __result)
            {
                if (__instance.randomCallerPics.Length <= 0)
                {
                    MelonLogger.Warning("WARNING: No image available for caller!");
                    __result = null;
                    return false;
                }

                int num = Random.Range(0, __instance.randomCallerPics.Length);
                __result = __instance.randomCallerPics[num];

                return false; // Skip the original function
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "NextCallButton")]
        public static class NextCallButtonPatch
        {
            /// <summary>
            /// Patches the NextCallButton logic to handle custom campaign behavior and end-of-day conditions.
            /// When in a custom campaign, it checks if the next caller to be skipped is the last caller of the day
            /// and, if so, informs the player instead of silently skipping. It also prevents advancing when the
            /// day is ending (showing a message and closing the caller window), or otherwise stops current routines
            /// and opens the call window for the next caller, skipping the original method implementation.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance)
            {
                // Check for next caller that will be skipped.
                if (CustomCampaignGlobal.inCustomCampaign)
                {
                    // If the next caller is the last, and we skip it (Consequence caller that we got right).
                    if (CloseButtonPatches.isNextCallerTheLastDayCaller(GlobalVariables.callerControllerScript))
                    {
                        if (GlobalVariables.callerControllerScript.currentCallerID + 1 < GlobalVariables.callerControllerScript.callers.Length)
                        {
                            #if DEBUG
                            MelonLogger.Msg($"DEBUG: Profile not null '{GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile != null}'." +
                                              $" Can receive consequence: '{!GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile)}'.");
                            #endif
                            
                            if (!GlobalVariables.arcadeMode 
                                && GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile != null 
                                && !GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[GlobalVariables.callerControllerScript.currentCallerID + 1].callerProfile.consequenceCallerProfile))
                            {
                                GlobalVariables.mainCanvasScript.CreateError("Day is ending. Please hold.");
                                GlobalVariables.mainCanvasScript.NoCallerWindow();
                                return false; // Skip original function.
                            }
                        }
                    }
                }
                
                if (GlobalVariables.callerControllerScript.IsLastCallOfDay() && !GlobalVariables.arcadeMode)
                {
                    GlobalVariables.mainCanvasScript.CreateError("Day is ending. Please hold.");
                    GlobalVariables.mainCanvasScript.NoCallerWindow();
                }
                else
                {
                    GlobalVariables.callerControllerScript.StopAllRoutines();
                    __instance.callWindow.SetActive(true);
                }

                return false; // Skip the original function
            }
        }
    }
}