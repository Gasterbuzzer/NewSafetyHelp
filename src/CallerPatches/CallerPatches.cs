using MelonLoader;
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
                            item.currentlySelected = false;

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

                // Replace information about the caller with a random entry with a 20% chance that wasn't called.
                float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);
                if (randomValue <= 0.2f) // Chance
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
                        profile.callerName = selected.Name;
                        profile.callTranscription = selected.callTranscript;

                        MelonLogger.Msg($"Info: Replaced the current caller with a custom caller: {profile.callerMonster.monsterName} with ID: {profile.callerMonster.monsterID}.");
                    }
                }

                __instance.currentCallerProfile = profile;
                GlobalVariables.mainCanvasScript.UpdateCallerInfo(profile);

                return false; // Skip the original function
            }
        }
    }
}
