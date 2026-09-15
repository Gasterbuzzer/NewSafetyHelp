using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio.Music
{
    public static class MusicControllerPatches
    {
        // Used for figuring out, what is playing, instead of guessing or searching.
        private static RichAudioClip currentMusicClip;

        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartRandomMusic")]
        public static class StartRandomMusicPatch
        {
            private static readonly FieldInfo PreviousHoldMusicIndex =
                typeof(MusicController).GetField("previousHoldMusicIndex",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            /// <summary>
            /// Patches the play random music to not play day 7 music in custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MusicController __instance)
            {
                // If in the main game and the current day is the 7th day.
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.currentDay == 7 && !GlobalVariables.arcadeMode)
                    {
                        return false;
                    }
                }

                int chosenMusicIndex = 0;
                bool playCustomMusic = false;

                if (PreviousHoldMusicIndex == null)
                {
                    LoggingHelper.ReflectionError(nameof(PreviousHoldMusicIndex));
                    return true;
                }

                List<CustomMusic> customMusicList = new List<CustomMusic>();

                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                             musicChoosingAttempt < 3;
                             ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }

                        PreviousHoldMusicIndex.SetValue(__instance,
                            chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                    else
                    {
                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                             musicChoosingAttempt < 3;
                             ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, GlobalVariables.currentDay);
                        }

                        PreviousHoldMusicIndex.SetValue(__instance,
                            chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                }
                else // Custom Campaign Music. Ignores the custom day logic
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }

                    customMusicList = customCampaign.CustomMusic
                        .Where(clip =>
                            {
                                if (clip.OnlyPlayOnUnlockDay)
                                {
                                    if (clip.UnlockDay <= 0)
                                    {
                                        return 1 == GlobalVariables.currentDay;
                                    }

                                    return clip.UnlockDay == GlobalVariables.currentDay;
                                }

                                return clip.UnlockDay <= GlobalVariables.currentDay;
                            }
                        ).ToList();

                    int customMusicAmount = customMusicList.Count;

                    if (customCampaign.AlwaysRandomMusic || GlobalVariables.currentDay > 4)
                    {
                        int amountOfClips = 0;

                        if (!customCampaign.RemoveDefaultMusic)
                        {
                            amountOfClips += __instance.onHoldMusicClips.Length;
                        }

                        if (customCampaign.CustomMusic.Count > 0) // We have custom music
                        {
                            amountOfClips += customMusicAmount;
                        }

                        chosenMusicIndex = Random.Range(0, amountOfClips); // Set it once.

                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                             musicChoosingAttempt < 3;
                             ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, amountOfClips);
                        }

                        if (!customCampaign.RemoveDefaultMusic) // Don't remove default music.
                        {
                            if (chosenMusicIndex >= __instance.onHoldMusicClips.Length)
                            {
                                playCustomMusic = true;
                                chosenMusicIndex -= __instance.onHoldMusicClips.Length;
                            }
                        }
                        else // Remove default music
                        {
                            playCustomMusic = true;
                        }

                        LoggingHelper.DebugLog(() =>
                            $"Chose to play the music track: '{chosenMusicIndex}' with the previous being '{(int)PreviousHoldMusicIndex.GetValue(__instance)}'. " +
                            $"(From custom music? '{playCustomMusic}') " +
                            $"(Amount of clips: '{amountOfClips}') " +
                            $"(Total clips: '{customCampaign.CustomMusic.Count}') " +
                            $"(Remove default music? '{customCampaign.RemoveDefaultMusic}') " +
                            $"(Current day: '{GlobalVariables.currentDay}').");

                        if (playCustomMusic)
                        {
                            LoggingHelper.DebugLog($"Amount of custom music available: '{customMusicAmount}'.");
                        }

                        PreviousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);
                    }
                    else
                    {
                        int amountOfClips = 0;

                        if (!customCampaign.RemoveDefaultMusic)
                        {
                            amountOfClips += __instance.onHoldMusicClips.Length;
                        }

                        if (customCampaign.CustomMusic.Count > 0) // We have custom music
                        {
                            amountOfClips += customMusicAmount;
                        }

                        if (customCampaign.RemoveDefaultMusic) // Only custom music
                        {
                            playCustomMusic = true;

                            for (int musicChoosingAttempt = 0;
                                 chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                                 musicChoosingAttempt < 3;
                                 ++musicChoosingAttempt)
                            {
                                chosenMusicIndex = Random.Range(0, amountOfClips);
                            }
                        }
                        else if (!customCampaign.RemoveDefaultMusic &&
                                 customMusicAmount > 0) // Combined custom and normal music
                        {
                            int whichMusicList = Random.Range(0, 2); // 0 or 1

                            switch (whichMusicList)
                            {
                                case 0: // Normal

                                    for (int musicChoosingAttempt = 0;
                                         chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                                         musicChoosingAttempt < 3;
                                         ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                                    {
                                        chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                                    }

                                    break;

                                case 1: // Custom Music

                                    playCustomMusic = true;

                                    for (int musicChoosingAttempt = 0;
                                         chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                                         musicChoosingAttempt < 3;
                                         ++musicChoosingAttempt)
                                    {
                                        chosenMusicIndex = Random.Range(0, amountOfClips);
                                    }

                                    break;
                            }
                        }
                        else if (!customCampaign.RemoveDefaultMusic && customMusicAmount <= 0) // Normal
                        {
                            for (int musicChoosingAttempt = 0;
                                 chosenMusicIndex == (int)PreviousHoldMusicIndex.GetValue(__instance) &&
                                 musicChoosingAttempt < 3;
                                 ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                            {
                                chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                            }
                        }

                        PreviousHoldMusicIndex.SetValue(__instance,
                            chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                }

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
                    if (GlobalVariables.musicControllerScript.onHoldMusicClips.Length >= chosenMusicIndex)
                    {
                        __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }

                    if (playCustomMusic)
                    {
                        if (customMusicList.Count > 0
                            && chosenMusicIndex < customMusicList.Count
                            && customMusicList[chosenMusicIndex].MusicClip != null)
                        {
                            __instance.StartMusic(customMusicList[chosenMusicIndex].MusicClip);
                        }
                        else
                        {
                            LoggingHelper.WarningLog(
                                "There is no music available or music clip is empty! Possibly failed loading?");
                        }
                    }
                    else if (!customCampaign.RemoveDefaultMusic)
                    {
                        if (GlobalVariables.musicControllerScript.onHoldMusicClips.Length >= chosenMusicIndex)
                        {
                            __instance.StartMusic(
                                GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                        }
                    }
                }

                return false; // Skip function with false.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartMusic", typeof(RichAudioClip))]
        public static class StartMusicPatch
        {
            private static readonly FieldInfo MyMusicSource =
                typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

            /// <summary>
            /// Patches the play music to take into consideration the downed caller.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="myMusicClip"> Music clip to play. </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MusicController __instance, ref RichAudioClip myMusicClip)
            {
                if (MyMusicSource == null)
                {
                    LoggingHelper.ReflectionError(nameof(MyMusicSource));
                    return true;
                }

                AudioSource myMusicSourceCast = (AudioSource)MyMusicSource.GetValue(__instance);

                myMusicSourceCast.pitch = 1f; // OLD: __instance.myMusicSource.pitch = 1f;

                myMusicSourceCast.clip = myMusicClip.clip; // OLD: __instance.myMusicSource.clip = myMusicClip.clip;

                myMusicSourceCast.volume =
                    myMusicClip.volume; // OLD: __instance.myMusicSource.volume = myMusicClip.volume;

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return true;
                    }

                    CustomCCaller activeCaller =
                        CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript
                            .currentCallerID);

                    if (activeCaller != null && activeCaller.DownedNetworkCaller)
                    {
                        myMusicSourceCast.pitch = 0.8f; // OLD: __instance.myMusicSource.pitch = 0.8f;
                    }

                    CustomMusic customMusic = CustomCampaignGlobal.GetCustomMusicFromActiveCampaign(myMusicClip);

                    if (customMusic != null
                        && !customMusic.IsIntermissionMusic
                        && customMusic.StartRange != null
                        && customMusic.StartRange.Count > 0)
                    {
                        float? chosenStart = RandomFromList.GetRandomFromList(customMusic.StartRange);

                        if (chosenStart != null)
                        {
                            myMusicSourceCast.time = (float)chosenStart;
                            LoggingHelper.DebugLog($"Chosen starting music offset of: '{chosenStart}'.");
                        }
                        else
                        {
                            myMusicSourceCast.time = 0.0f;
                        }
                    }
                    else
                    {
                        myMusicSourceCast.time = 0.0f;
                    }
                }
                else // Main Campaign
                {
                    foreach (int downedNetworkCall in GlobalVariables.callerControllerScript.downedNetworkCalls)
                    {
                        if (downedNetworkCall == GlobalVariables.callerControllerScript.currentCallerID)
                        {
                            myMusicSourceCast.pitch = 0.8f; // OLD: __instance.myMusicSource.pitch = 0.8f;
                        }
                    }

                    // OLD: __instance.myMusicSource.time = !(myMusicClip == __instance.onHoldMusicClips[1]) ? 0.0f : 19.6f;
                    if (myMusicClip != __instance.onHoldMusicClips[1])
                    {
                        myMusicSourceCast.time = 0.0f;
                    }
                    else
                    {
                        myMusicSourceCast.time = 19.6f;
                    }
                }

                // Store a reference to the clip for later checking or restoring.
                currentMusicClip = myMusicClip;

                myMusicSourceCast.Play(); // OLD: __instance.myMusicSource.Play();

                return false; // Do not call original function.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MusicController), "TurnDownHoldMusicWhileHazardProfileSampleIsPlayingRoutine")]
        public static class TurnDownHoldMusicWhileHazardProfileSampleIsPlayingRoutinePatch
        {
            private static readonly FieldInfo MyMusicSource =
                typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

            /// <summary>
            /// Patches the function to not crash if the provided music clip is not from the base game.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to play. </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MusicController __instance, ref IEnumerator __result)
            {
                if (MyMusicSource == null)
                {
                    LoggingHelper.ReflectionError(nameof(MyMusicSource));
                    return true;
                }

                AudioSource myMusicSourceCast = (AudioSource)MyMusicSource.GetValue(__instance);

                __result = TurnDownMusicIfEntryIsPlaying(myMusicSourceCast);

                return false; // Do not call original function.
            }

            private static IEnumerator TurnDownMusicIfEntryIsPlaying(AudioSource myMusicSourceCast)
            {
                if (myMusicSourceCast.isPlaying)
                {
                    myMusicSourceCast.volume = 0.02f;

                    while (GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.isPlaying)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (currentMusicClip != null)
                    {
                        myMusicSourceCast.volume = currentMusicClip.volume;
                    }
                }
            }
        }
    }
}