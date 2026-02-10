using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio.Music
{
    public static class MusicControllerPatches
    {
        // Used for figuring out, what is playing, instead of guessing or searching.
        private static RichAudioClip CurrentMusicClip;
        
        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartRandomMusic")]
        public static class StartRandomMusicPatch
        {
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

                FieldInfo _previousHoldMusicIndex = typeof(MusicController).GetField("previousHoldMusicIndex",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_previousHoldMusicIndex == null)
                {
                    MelonLogger.Error(
                        "ERROR: PreviousHoldMusicIndex is null. Unable of replacing MusicController StartNewRandomMusic. Calling original function.");
                    return true;
                }

                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
                             musicChoosingAttempt < 3;
                             ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }

                        _previousHoldMusicIndex.SetValue(__instance,
                            chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                    else
                    {
                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
                             musicChoosingAttempt < 3;
                             ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, GlobalVariables.currentDay);
                        }

                        _previousHoldMusicIndex.SetValue(__instance,
                            chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                }
                else // Custom Campaign Music. Ignores the day
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error(
                            "ERROR: CustomCampaign is null! Cannot play random music. Using default logic.");
                        return true;
                    }

                    if (customCampaign.AlwaysRandomMusic || GlobalVariables.currentDay > 4)
                    {
                        int amountOfClips = 0;

                        int customMusicAmount = customCampaign.CustomMusic
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
                            ).ToList().Count;

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
                             chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
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

                        #if DEBUG
                        MelonLogger.Msg("DEBUG: " +
                                        $"Chose to play the music track: {chosenMusicIndex} with the previous being {(int)_previousHoldMusicIndex.GetValue(__instance)}." +
                                        $" (From custom music? {playCustomMusic})" +
                                        $" (Amount of clips: {amountOfClips})" +
                                        $" (Total clips: {customCampaign.CustomMusic.Count})" +
                                        $" (Remove default music? {customCampaign.RemoveDefaultMusic})" +
                                        $" (Current day: {GlobalVariables.currentDay})");

                        if (playCustomMusic)
                        {
                            MelonLogger.Msg($"DEBUG: Amount of custom music: {customMusicAmount}.");
                        }
                        #endif

                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);
                    }
                    else
                    {
                        int amountOfClips = 0;

                        int customMusicAmount = customCampaign.CustomMusic
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
                                ).ToList().Count;

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
                                 chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
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
                                         chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
                                         musicChoosingAttempt < 3;
                                         ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                                    {
                                        chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                                    }

                                    break;

                                case 1: // Custom Music

                                    playCustomMusic = true;

                                    for (int musicChoosingAttempt = 0;
                                         chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
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
                                 chosenMusicIndex == (int)_previousHoldMusicIndex.GetValue(__instance) &&
                                 musicChoosingAttempt < 3;
                                 ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                            {
                                chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                            }
                        }

                        _previousHoldMusicIndex.SetValue(__instance,
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
                        MelonLogger.Error(
                            "ERROR: CustomCampaign is null! Cannot play random music. Using default logic.");
                        return true;
                    }

                    if (playCustomMusic)
                    {
                        List<CustomMusic> customMusicList = customCampaign.CustomMusic
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
                                
                            }).ToList();

                        if (customMusicList.Count > 0 
                            && customMusicList.Count >= chosenMusicIndex
                            && customMusicList[chosenMusicIndex].MusicClip != null)
                        {
                            __instance.StartMusic(customMusicList[chosenMusicIndex].MusicClip);
                        }
                        else
                        {
                            MelonLogger.Warning("WARNING: Music clip is empty! Possibly failed loading?");
                        }
                    }
                    else if (!customCampaign.RemoveDefaultMusic)
                    {
                        if (GlobalVariables.musicControllerScript.onHoldMusicClips.Length >= chosenMusicIndex)
                        {
                            __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                        }
                    }
                }

                return false; // Skip function with false.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartMusic", typeof(RichAudioClip))]
        public static class StartMusicPatch
        {
            /// <summary>
            /// Patches the play music to take into consideration the downed caller.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="myMusicClip"> Music clip to play. </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MusicController __instance, ref RichAudioClip myMusicClip)
            {
                FieldInfo myMusicSource = typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

                if (myMusicSource == null)
                {
                    MelonLogger.Error("ERROR: 'myMusicSource' was not found. Unable of changing StartMusic." +
                                      " Calling original function.");
                    return true;
                }
                
                AudioSource myMusicSourceCast = (AudioSource) myMusicSource.GetValue(__instance);

                if (myMusicSourceCast == null)
                {
                    MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic." +
                                      " Calling original function.");
                    return true;
                }

                myMusicSourceCast.pitch = 1f; // __instance.myMusicSource.pitch = 1f;
                
                myMusicSourceCast.clip = myMusicClip.clip; // __instance.myMusicSource.clip = myMusicClip.clip;
                
                myMusicSourceCast.volume = myMusicClip.volume; // __instance.myMusicSource.volume = myMusicClip.volume;

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null. Unable of changing StartMusic." +
                                          " Calling original.");
                        return true;
                    }

                    CustomCCaller activeCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID);

                    if (activeCaller != null && activeCaller.DownedNetworkCaller)
                    {
                        myMusicSourceCast.pitch = 0.8f; // __instance.myMusicSource.pitch = 0.8f;
                    }
                    
                    myMusicSourceCast.time = 0.0f;
                }
                else // Main Campaign
                {
                    foreach (int downedNetworkCall in GlobalVariables.callerControllerScript.downedNetworkCalls)
                    {
                        if (downedNetworkCall == GlobalVariables.callerControllerScript.currentCallerID)
                        {
                            myMusicSourceCast.pitch = 0.8f; // __instance.myMusicSource.pitch = 0.8f;
                        }
                    }
                    
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
                CurrentMusicClip = myMusicClip;
                
                myMusicSourceCast.Play(); // __instance.myMusicSource.Play();
                
                return false; // Do not call original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MusicController), "TurnDownHoldMusicWhileHazardProfileSampleIsPlayingRoutine")]
        public static class TurnDownHoldMusicWhileHazardProfileSampleIsPlayingRoutinePatch
        {
            /// <summary>
            /// Patches the function to not crash if the provided music clip is not from the base game.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to play. </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MusicController __instance, ref IEnumerator __result)
            {
                FieldInfo myMusicSource = typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

                if (myMusicSource == null)
                {
                    MelonLogger.Error("ERROR: 'myMusicSource' was not found. Unable of changing StartMusic." +
                                      " Calling original function.");
                    return true;
                }
                
                AudioSource myMusicSourceCast = (AudioSource) myMusicSource.GetValue(__instance);

                if (myMusicSourceCast == null)
                {
                    MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic." +
                                      " Calling original function.");
                    return true;
                }
                
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

                    if (CurrentMusicClip != null)
                    {
                        myMusicSourceCast.volume = CurrentMusicClip.volume; 
                    }
                }
            }
        }
    }
}