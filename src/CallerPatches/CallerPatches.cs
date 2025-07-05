using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MelonLoader;
using NewSafetyHelp.EntryManager;
using NewSafetyHelp.JSONParsing;
using UnityEngine;

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
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            private static void Postfix(MethodBase __originalMethod, CallerController __instance)
            {

                foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
                {
                    if (__instance.currentCustomCaller.callerMonster.monsterName == item.Name || __instance.currentCustomCaller.callerMonster.monsterID == item.ID) // We found an entry to replace the audio for.
                    {
                        __instance.currentCustomCaller.callerClip = item.callerClip;
                    }
                }
            }
        }

        // Patches the caller to have a custom audio in campaign.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PlayCallAudioRoutine", new Type[] { typeof(CallerProfile) })]
        public static class UpdateCampaignCallerAudio
        {
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, CallerProfile profile, ref IEnumerator __result)
            {
                __result = originalCaller(__originalMethod, __instance, profile);
                return false; // Skip the original coroutine
            }

            /// <summary>
            /// Patches the IEnumerator to be with the custom Audio.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            private static IEnumerator originalCaller(MethodBase __originalMethod, CallerController __instance, CallerProfile profile)
            {
  
                if (profile == null)
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
                var callerAudioSourceGetter = callerController.GetField("callerAudioSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (callerAudioSourceGetter == null)
                {
                    MelonLogger.Error($"CallerController: callerAudioSourceGetter is null");
                    yield break;
                }

                AudioSource callerAudioSource = (AudioSource) callerAudioSourceGetter.GetValue(__instance);

                callerAudioSource.Stop();

                // Clip replace
                bool found = false;
                if (profile != null && profile.callerMonster != null && !__instance.arcadeMode) // We only check if the caller has any entry to begin with. We will need to handle arcade mode later or scrap that idea.
                {
                    foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
                    {
                        if (item.currentlySelected) // We found an entry to replace the audio for.
                        {
                            item.alreadyCalledOnce = true;

                            // We now check if we are allowed to save if the entry can be saved as already called.
                            if (!item.allowCallAgainOverRestart)
                            {
                                if (!NewSafetyHelpMainClass.persistantEntrySave.HasEntry(item.Name + item.callerName))
                                {
                                    NewSafetyHelpMainClass.persistantEntrySave.CreateEntry<bool>(item.Name + item.callerName, true);
                                }
                                else
                                {
                                    NewSafetyHelpMainClass.persistantEntrySave.GetEntry<bool>(item.Name + item.callerName).Value = true;
                                }
                            }
                            else
                            {
                                if (!NewSafetyHelpMainClass.persistantEntrySave.HasEntry(item.Name + item.callerName))
                                {
                                    NewSafetyHelpMainClass.persistantEntrySave.CreateEntry<bool>(item.Name + item.callerName, false); // Store it as false
                                }
                                else
                                {
                                    NewSafetyHelpMainClass.persistantEntrySave.GetEntry<bool>(item.Name + item.callerName).Value = false;
                                }
                            }

                            // Current selection is set to false once the answer for the caller has been submitted.

                            callerAudioSource.clip = item.callerClip.clip;
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
                        MelonLogger.Msg($"DEBUG: Monster Name: {profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}.");
                        #endif
                    }
                    else
                    {
                        MelonLogger.Msg("INFO: This caller does not have a monster entry. Thus not replaced.");
                    }

                    callerAudioSource.clip = profile.callerClip.clip;
                }

                #if DEBUG
                MelonLogger.Msg($"DEBUG: Caller Audio File Name: {callerAudioSource.name} with {callerAudioSource.clip.name} and {callerAudioSource.clip.length}.");
                #endif

                callerAudioSource.volume = profile.callerClip.volume;
                callerAudioSource.Play();
            }
        }

        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "UpdateCallerInfo", new Type[] { typeof(CallerProfile)})]
        public static class UpdateCampaignCallerRandom
        {
            /// <summary>
            /// Patch for the caller info to be updated to a custom entry.
            /// </summary>
            /// <param name="__originalMethod"> Original Method Caller </param>
            /// <param name="__instance"> Function Caller Instance </param>
            /// <param name="profile"> Caller Profile that called </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, ref CallerProfile profile)
            {

                #if DEBUG
                MelonLogger.Msg($"DEBUG: New caller is calling.");
                #endif

                if (profile == null)
                {
                    profile = __instance.callers[__instance.currentCallerID].callerProfile;
                }

                if (__instance.arcadeMode)
                {
                    profile = __instance.currentCustomCaller;
                }

                List<EntryExtraInfo> entries = new List<EntryExtraInfo>();

                bool replaceTrue = false;

                if (!__instance.arcadeMode)
                {
                    foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
                    {
                        if (item.inCampaign && !item.alreadyCalledOnce && !item.currentlySelected) // Find a valid entry.
                        {

                            // Create Entry if not existent and if allowed

                            MelonPreferences_Entry<bool> entryAlreadyCalledBeforeEntry = null;

                            if (!NewSafetyHelpMainClass.persistantEntrySave.HasEntry(item.Name + item.callerName))
                            {
                                entryAlreadyCalledBeforeEntry = NewSafetyHelpMainClass.persistantEntrySave.CreateEntry<bool>(item.Name + item.callerName, false);
                            }
                            else
                            {
                                entryAlreadyCalledBeforeEntry = NewSafetyHelpMainClass.persistantEntrySave.GetEntry<bool>(item.Name + item.callerName);
                            }

                            if (item.allowCallAgainOverRestart)
                            {

                                MelonLogger.Msg($"INFO: Entry {item.Name} is allowed to be called again even if called once in the past.");

                                entryAlreadyCalledBeforeEntry.Value = false; // Reset the entry. If not allowed to store the value.
                            }

                            if (entryAlreadyCalledBeforeEntry.Value)
                            {
                                MelonLogger.Msg($"INFO: Entry {item.Name} was already called once, so it will not be available for calling.");
                            }

                            if (UnityEngine.Random.Range(0.0f, 1.0f) <= item.callerReplaceChance)
                            {
                                if (!item.allowCallAgainOverRestart) // We check if we already called once, if yes, we skip and if not we continue (setting is done later).
                                {
                                    if (!entryAlreadyCalledBeforeEntry.Value && item.permissionLevel <= GlobalVariables.currentDay) // We never called it. And make sure we can actually access the callers' entry.
                                    {

                                        if (GlobalVariables.isXmasDLC) // If DLC
                                        {
                                            if (item.includeInDLC || item.onlyDLC) // Is allowed to be added?
                                            {
                                                entries.Add(item);
                                                replaceTrue = true;

                                                MelonLogger.Msg($"INFO: Saved Entry '{item.Name}' to not be called in the future.");

                                                entryAlreadyCalledBeforeEntry.Value = true;
                                            }
                                            else
                                            {
                                                MelonLogger.Msg($"INFO: Entry '{item.Name}' is not allowed to be called in DLC Mode.");
                                            }
                                        }
                                        else // Main Game
                                        {
                                            entries.Add(item);
                                            replaceTrue = true;

                                            MelonLogger.Msg($"INFO: Saved Entry '{item.Name}' to not be called in the future.");

                                            entryAlreadyCalledBeforeEntry.Value = true;
                                        }
                                    }
                                    // Else we skip it.
                                }
                                else // We are allowed to ignore it.
                                {
                                    if (item.permissionLevel <= GlobalVariables.currentDay) // Make sure we can actually access the callers' entry.
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

                if (profile != null && !__instance.arcadeMode && profile.consequenceCallerProfile != null) // We are a consequence caller. (Since we don't replace, and we don't have a caller monster.)
                {
                    MelonLogger.Msg($"INFO: Current caller is Consequence Caller.");
                    Caller callers = CallerPatches.GetConsequenceCaller(profile, ref __instance.callers);
                    
                    if (callers != null)// Caller is valid.
                    {
                        MelonLogger.Msg($"Consequence Caller name: {callers.callerProfile.name}");
                        
                        if (ParseMonster.entriesExtraInfo.Exists(item => item.referenceProfileNameInternal == callers.callerProfile.consequenceCallerProfile.name)) // IF the consequence caller has been replaced once.
                        {
                            MelonLogger.Msg($"INFO: Consequence Caller to be replaced found!");
                            EntryExtraInfo foundExtraInfo = ParseMonster.entriesExtraInfo.Find(item => item.referenceProfileNameInternal == callers.callerProfile.consequenceCallerProfile.name);

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

                            MelonLogger.Msg($"INFO: Replaced the current caller transcript with: {profile.callTranscription}.");
                        }
                    }
                    else
                    {
                        MelonLogger.Error($"INFO: Did not find initial caller.");
                    }
                }
                // Replace information about the caller with a random entry
                else if (replaceTrue && profile != null && profile.callerMonster != null && !__instance.arcadeMode && profile.consequenceCallerProfile == null) // If any entry won the chance to replace this call, replace it.
                {
                    if (entries.Count > 0) // We actually found at least one.
                    {
                            // We are not a consequence caller.
                            // Select one randomly.
                            int entrySelected = UnityEngine.Random.Range(0, entries.Count - 1);

                            // Audio check
                            ParseMonster.entriesExtraInfo.Find(item => item.Equals(entries[entrySelected])).currentlySelected = true;

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
                                MelonLogger.Msg($"INFO: Replaced the current caller ({profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}) with a custom caller: {selected.Name} with ID: {selected.ID}.");
                            }

                            // We store a reference to the caller for finding later if the consequence caller calls.
                            ParseMonster.entriesExtraInfo.Find(item => item == entries[entrySelected]).referenceProfileNameInternal = profile.name;
                    }
                }

                #if DEBUG
                MelonLogger.Msg($"DEBUG: Finished handling the caller replacement.");
                #endif

                __instance.currentCallerProfile = profile;
                GlobalVariables.mainCanvasScript.UpdateCallerInfo(profile);


                return false; // Skip the original function
            }
        }

        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CheckCallerAnswer", new Type[] { typeof(MonsterProfile) })]
        public static class ReplaceAnswerWithReplacedAnswer
        {
            /// <summary>
            /// Patch the caller answer check to be the custom caller/entry.
            /// </summary>
            /// <param name="__originalMethod"> Original Method Caller </param>
            /// <param name="__instance"> Function Caller Instance </param>
            /// <param name="monsterID"> Monster Profile that was selected </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, ref MonsterProfile monsterID)
            {

                // Get triggerGameOver
                Type callerController = typeof(CallerController);
                FieldInfo dynamicCaller = callerController.GetField("dynamicCaller", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

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
                    foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
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
                        MelonLogger.Msg($"INFO: Current entry was not replaced. The actual caller monster was: {__instance.callers[__instance.currentCallerID].callerProfile.callerMonster.monsterName}");
                    }


                    if (monsterID == monsterToCheck) // If correct
                    {
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;
                        ++__instance.correctCallsToday;

                        // Debug Info in case the replacement worked.
                        if (found)
                        {
                            MelonLogger.Msg("INFO: Selected the correct replaced entry.");
                        }
                    }
                    else // If wrong
                    {
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = false;
                        if (GlobalVariables.isXmasDLC) // If wrong and DLC
                        {
                            // Get TriggerXMAS Lights
                            MethodInfo triggerXmasLight = callerController.GetMethod("TriggerXmasLight", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                            
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
                else if (!(bool)dynamicCaller.GetValue(__instance)) // Monster not provided and a dynamic caller. So we set it to true.
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

        [HarmonyLib.HarmonyPatch(typeof(CallerController), "Start", new Type[] {})]
        public static class AddCustomCampaign
        {
            /// <summary>
            /// Patch the start function to inject custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Original Method Caller </param>
            /// <param name="__instance"> Function Caller Instance </param>
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Called Start from CallerController.");
                #endif

                Type callerController = typeof(CallerController);
                
                // Original Code
                GlobalVariables.callerControllerScript = __instance;
                
                __instance.arcadeMode = GlobalVariables.arcadeMode;
                
                FieldInfo _lastDayNum = callerController.GetField("lastDayNum", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_lastDayNum == null)
                {
                    MelonLogger.Error("ERROR: CallerController.lastDayNum is null!");
                    return true;
                }
                
                _lastDayNum.SetValue(__instance, __instance.mainGameLastDay);
                
                FieldInfo _callerAudioSource = callerController.GetField("callerAudioSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

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
                    __instance.callers = (Caller[]) null;
                    __instance.callers = __instance.xmasCallers;
                    __instance.warningCall = __instance.xmasWarningCall;
                    __instance.gameOverCall = __instance.xmasGameOverCall;
                    _lastDayNum.SetValue(__instance, __instance.xmasLastDay);
                    __instance.downedNetworkCalls = __instance.xmasDownedNetworkCalls;
                }
                
                // Attempt to hijack caller list.
                
                CallerProfile newProfile = ScriptableObject.CreateInstance<CallerProfile>();

                newProfile.callerName = "Jeff";
                newProfile.callTranscription = "We live jeff";
                
                MethodInfo getRandomPicMethod = callerController.GetMethod("PickRandomPic", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                
                MethodInfo getRandomClip = callerController.GetMethod("PickRandomClip", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (getRandomPicMethod == null || getRandomClip == null)
                {
                    MelonLogger.Error("ERROR: getRandomPicMethod or getRandomClip is null!");
                    return true;
                }
                
                newProfile.callerPortrait = (Sprite) getRandomPicMethod.Invoke(__instance, new object[] { });
                
                newProfile.callerClip = (RichAudioClip) getRandomClip.Invoke(__instance, new object[] { });
                

                __instance.callers = new Caller[2];
                __instance.callers[0] = new Caller
                {
                    callerProfile = newProfile
                };

                __instance.callers[1] = new Caller
                {
                    callerProfile = newProfile
                };

                return false; // Skip the original function
            }
        }
    }
}
