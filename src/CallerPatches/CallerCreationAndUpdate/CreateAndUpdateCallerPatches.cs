using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.JSONParsing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.CallerPatches.CallerCreationAndUpdate
{
    public static class CreateAndUpdateCallerPatches
    {
        
        /// <summary>
        /// Gets the consequence caller based on a provided profile from a caller list.
        /// </summary>
        /// <param name="profileToCheck"></param>
        /// <param name="callers"></param>
        /// <returns></returns>
        [CanBeNull]
        private static Caller GetConsequenceCaller(CallerProfile profileToCheck, ref Caller[] callers)
        {
            foreach (Caller caller in callers)
            {
                if (profileToCheck == caller.callerProfile)
                    return caller; // Returns the caller
            }

            return null;
        }
        
        // Patches the caller to have a custom caller clip in arcade mode.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CreateCustomCaller", new Type[] { })]
        public static class UpdateArcadeCallerAudio
        {
            /// <summary>
            /// Update the list when opening.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(CallerController __instance)
            {
                foreach (EntryExtraInfo item in GlobalParsingVariables.entriesExtraInfo)
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
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance, CallerProfile profile, ref IEnumerator __result)
            {
                __result = originalCaller(__instance, profile);
                return false; // Skip the original coroutine
            }

            /// <summary>
            /// Patches the IEnumerator to be with the custom Audio.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name= "profile"> Profile Parameter. </param>
            private static IEnumerator originalCaller(CallerController __instance,
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

                // Here we replace the clip.
                bool found = false;

                // We only check if the caller has any entry to begin with. We will need to handle arcade mode later or scrap that idea.
                // And only if not in a custom campaign.
                if (!CustomCampaignGlobal.inCustomCampaign &&
                    profile != null && profile.callerMonster != null &&
                    !__instance.arcadeMode)
                {
                    foreach (EntryExtraInfo item in GlobalParsingVariables.entriesExtraInfo)
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
            /// <param name="__instance"> Function Caller Instance </param>
            /// <param name="profile"> Caller Profile that called </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallerController __instance, ref CallerProfile profile)
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
                    foreach (EntryExtraInfo item in GlobalParsingVariables.entriesExtraInfo)
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

                // Not in custom campaign. This makes odd problems in custom campaigns.
                if (!CustomCampaignGlobal.inCustomCampaign) 
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

                            if (GlobalParsingVariables.entriesExtraInfo.Exists(item =>
                                    item.referenceProfileNameInternal ==
                                    callers.callerProfile.consequenceCallerProfile
                                        .name)) // IF the consequence caller has been replaced once.
                            {
                                MelonLogger.Msg($"INFO: Consequence Caller to be replaced found!");
                                EntryExtraInfo foundExtraInfo = GlobalParsingVariables.entriesExtraInfo.Find(item =>
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
                            GlobalParsingVariables.entriesExtraInfo.Find(item => item.Equals(entries[entrySelected]))
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
                                    $"INFO: Replaced the current caller ({profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}) with a custom caller:" +
                                    $" {selected.Name} with ID: {selected.ID}.");
                            }

                            // We store a reference to the caller for finding later if the consequence caller calls.
                            GlobalParsingVariables.entriesExtraInfo.Find(item => item.Equals(entries[entrySelected]))
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
    }
}