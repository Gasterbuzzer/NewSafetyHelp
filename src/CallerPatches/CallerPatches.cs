﻿using MelonLoader;
using NewSafetyHelp.src.AudioHandler;
using NewSafetyHelp.src.EntryManager;
using NewSafetyHelp.src.JSONParsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace NewSafetyHelp.src.CallerPatches
{
    public class CallerPatches
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

        // Patches the caller to have a custom audio in campaing.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "PlayCallAudioRoutine", new Type[] { typeof(CallerProfile) })]
        public static class UpdateCampaingCallerAudio
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

                AudioSource callerAudioSource = (AudioSource) callerAudioSourceGetter.GetValue(__instance);

                callerAudioSource.Stop();

                // Clip replace
                bool found = false;
                foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
                {
                    if (item.currentlySelected && !item.alreadyCalledOnce) // We found an entry to replace the audio for.
                    {
                            item.alreadyCalledOnce = true;
                                
                            // We used say that the current selection gets removed however, since we need to check upon 
                            //item.currentlySelected = false;

                            callerAudioSource.clip = item.callerClip.clip;
                            found = true;
                    }
                }

                // Else
                if (!found)
                {
                    MelonLogger.Msg($"Debug: Monster Name: {profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}.");
                    callerAudioSource.clip = profile.callerClip.clip;
                } 

                callerAudioSource.volume = profile.callerClip.volume;
                callerAudioSource.Play();
            }
        }

        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "UpdateCallerInfo", new Type[] { typeof(CallerProfile)})]
        public static class UpdateCampaignCallerRandom
        {
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, ref CallerProfile profile)
            {
                if (profile == null)
                {
                    profile = __instance.callers[__instance.currentCallerID].callerProfile;
                }

                else if (__instance.arcadeMode)
                {
                    profile = __instance.currentCustomCaller;
                }

                // Replace information about the caller with a random entry with a 10% chance that wasn't called.
                if (UnityEngine.Random.Range(0.0f, 1.0f) <= 0.1f) // Chance
                {
                    EntryExtraInfo selected = null;

                    List<EntryExtraInfo> entries = new List<EntryExtraInfo>();

                    foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
                    {
                        if (item.inCampaign && !item.alreadyCalledOnce && !item.currentlySelected) // Find a valid entry.
                        {
                            entries.Add(item);
                        }
                    }

                    if (entries.Count > 0) // We actually found atleast one.
                    {

                        // Select one randomly.
                        int entrySelected = UnityEngine.Random.Range(0, entries.Count - 1);

                        // Audio check
                        ParseMonster.entriesExtraInfo.Find(item => item == entries[entrySelected]).currentlySelected = true;

                        // Get a "copy"
                        selected = entries[entrySelected];

                        // Replace caller with custom caller
                        profile.callerName = selected.callerName;

                        if (selected.callerImage != null) // If Image provided
                        {
                            profile.callerPortrait = selected.callerImage;
                        }

                        profile.callTranscription = selected.callTranscript;

                        MelonLogger.Msg($"Info: Replaced the current caller with a custom caller: {profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}.");
                    }
                }

                __instance.currentCallerProfile = profile;
                GlobalVariables.mainCanvasScript.UpdateCallerInfo(profile);

                return false; // Skip the original function
            }
        }

        // Patches the caller to replace it with another with random chance.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CheckCallerAnswer", new Type[] { typeof(MonsterProfile) })]
        public static class ReplaceAnswerWithReplacedAnswer
        {
            private static bool Prefix(MethodBase __originalMethod, CallerController __instance, ref MonsterProfile monsterID)
            {

                // Get triggerGameOver
                Type callerController = typeof(CallerController);
                FieldInfo dynamicCaller = callerController.GetField("dynamicCaller", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

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
                        MelonLogger.Msg($"Info: Current entry was not replaced. The actual caller monster was: {__instance.callers[__instance.currentCallerID].callerProfile.callerMonster.monsterName}");
                    }


                    if (monsterID == monsterToCheck) // If correct
                    {
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = true;
                        ++__instance.correctCallsToday;

                        // Debug Info incase the replacement worked.
                        if (found)
                        {
                            MelonLogger.Msg("Info: Selected the correct replaced entry.");
                        }
                    }
                    else // If wrong
                    {
                        __instance.callers[__instance.currentCallerID].answeredCorrectly = false;
                        if (GlobalVariables.isXmasDLC) // If wrong and DLC
                        {
                            // Get TriggerXMAS Lights
                            MethodInfo triggerXmasLight = callerController.GetMethod("TriggerXmasLight", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                            triggerXmasLight.Invoke(__instance, new object[] { });

                            GlobalVariables.cheerMeterScript.UpdateMeterVisuals();
                        }

                        // Debug Info incase the replacement worked.
                        if (found)
                        {
                            MelonLogger.Msg("Info: Selected the wrong replaced entry.");
                        }
                    }
                }
                else if (!(bool)dynamicCaller.GetValue(__instance)) // Monster not provided and a dynamic caller. So we set it to true.
                {
                    __instance.callers[__instance.currentCallerID].answeredCorrectly = true;

                    MelonLogger.Msg("Info: Dynamic Caller. No replacement possible. Always correct.");
                }

                dynamicCaller.SetValue(__instance, false);

                return false; // Skip the original function
            }
        }
    }
}
