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
            /// Also adds custom music.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MusicController __instance)
            {
                // If in the main/base game and the current day is the 7th day.
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.currentDay == 7
                        && !GlobalVariables.arcadeMode)
                    {
                        return false;
                    }
                }

                if (PreviousHoldMusicIndex == null)
                {
                    LoggingHelper.ReflectionError(nameof(PreviousHoldMusicIndex));
                    return true;
                }

                // Variables used to decide the next music.
                int chosenMusicIndex = 0;
                bool playCustomMusic = false;
                bool playArcadeMusic = false;

                List<CustomMusic> customMusicList = new List<CustomMusic>();
                List<CustomMusic> customArcadeMusicList = new List<CustomMusic>();

                // Main/Base game
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        int previousHoldMusicIndex = (int)PreviousHoldMusicIndex.GetValue(__instance);

                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == previousHoldMusicIndex && musicChoosingAttempt < 3;
                             ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }

                        // ORIGINAL: __instance.previousHoldMusicIndex = index1;
                        PreviousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);
                    }
                    else
                    {
                        int previousHoldMusicIndex = (int)PreviousHoldMusicIndex.GetValue(__instance);

                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == previousHoldMusicIndex && musicChoosingAttempt < 3;
                             ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, GlobalVariables.currentDay);
                        }

                        // ORIGINAL: __instance.previousHoldMusicIndex = index1;
                        PreviousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);
                    }
                }
                else // Custom Campaign Music. Ignores the custom day logic
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }

                    if (GlobalVariables.arcadeMode)
                    {
                        if (!customCampaign.ArcadeMusicPlayThrough.Data)
                        {
                            LoggingHelper.DebugLog("Choosing arcade music for arcade mode.",
                                LoggingHelper.LoggingCategory.ARCADE);

                            customArcadeMusicList =
                                customCampaign.ArcadeMusic.Where(MusicHelper.IsValidCustomMusic).ToList();

                            int previousMusicIndex = (int)PreviousHoldMusicIndex.GetValue(__instance);

                            chosenMusicIndex = MusicHelper.ChoseArcadeMusic(customCampaign.ArcadeMusic.Count,
                                previousMusicIndex, ref playArcadeMusic);
                        }
                    }
                    else
                    {
                        customMusicList = customCampaign.CustomMusic.Where(MusicHelper.IsValidCustomMusic).ToList();

                        int customCampaignMusicAmount = customMusicList.Count;
                        int baseGameMusicAmount = __instance.onHoldMusicClips.Length;

                        int previousMusicIndex = (int)PreviousHoldMusicIndex.GetValue(__instance);

                        bool removeBaseGameMusic = customCampaign.RemoveDefaultMusic;

                        bool useRandomMusic = customCampaign.AlwaysRandomMusic || GlobalVariables.currentDay > 4;

                        if (useRandomMusic)
                        {
                            chosenMusicIndex = MusicHelper.ChooseFromCombinedPool(removeBaseGameMusic,
                                baseGameMusicAmount,
                                customCampaignMusicAmount, previousMusicIndex, ref playCustomMusic);
                        }
                        else // Fairly chosen but still random.
                        {
                            chosenMusicIndex = MusicHelper.ChoseMusicIndexFairly(removeBaseGameMusic,
                                baseGameMusicAmount,
                                customCampaignMusicAmount, previousMusicIndex, ref playCustomMusic);
                        }
                    }
                }

                // ORIGINAL: __instance.previousHoldMusicIndex = index1;
                PreviousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);

                // Main/Base Campaign Logic
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.musicControllerScript.onHoldMusicClips.Length >= chosenMusicIndex)
                    {
                        __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                    }
                }
                // Custom Campaign
                else
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }

                    MusicHelper.PlayMusicInCustomCampaign(__instance, ref customMusicList, ref customArcadeMusicList,
                        chosenMusicIndex, playCustomMusic, playArcadeMusic, customCampaign.RemoveDefaultMusic,
                        customCampaign.ArcadeMusicPlayThrough.Data);
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

                // ORIGINAL: __instance.myMusicSource.pitch = 1f;
                myMusicSourceCast.pitch = 1f;

                // ORIGINAL: __instance.myMusicSource.clip = myMusicClip.clip;
                myMusicSourceCast.clip = myMusicClip.clip;

                // ORIGINAL: __instance.myMusicSource.volume = myMusicClip.volume;
                myMusicSourceCast.volume = myMusicClip.volume;

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
                        // ORIGINAl: __instance.myMusicSource.pitch = 0.8f;
                        myMusicSourceCast.pitch = 0.8f;
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
                            // ORIGINAL: __instance.myMusicSource.pitch = 0.8f;
                            myMusicSourceCast.pitch = 0.8f;
                        }
                    }

                    // ORIGINAL:
                    // __instance.myMusicSource.time = !(myMusicClip == __instance.onHoldMusicClips[1]) ? 0.0f : 19.6f;
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

                // ORIGINAL: __instance.myMusicSource.Play();
                myMusicSourceCast.Play();

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

            /// <summary>
            /// Coroutine for turning down the music if an entry is playing.
            /// </summary>
            /// <param name="myMusicSourceCast">Audio source playing the music.</param>
            /// <returns>Coroutine to be used.</returns>
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