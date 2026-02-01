using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaign;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio.Music
{
    public static class MusicControllerPatches
    {
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
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

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
                            .Where(clip => clip.UnlockDay <= GlobalVariables.currentDay).ToList().Count;

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
                            .Where(clip => clip.UnlockDay <= GlobalVariables.currentDay).ToList().Count;

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
                    __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                }
                else // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error(
                            "ERROR: CustomCampaign is null! Cannot play random music. Using default logic.");
                        return true;
                    }

                    if (playCustomMusic)
                    {
                        List<CustomMusic> customMusicList = customCampaign.CustomMusic
                            .Where(clip => clip.UnlockDay <= GlobalVariables.currentDay).ToList();

                        if (customMusicList.Count > 0 && customMusicList[chosenMusicIndex].MusicClip != null)
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
                        __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                    }
                }

                return false; // Skip function with false.
            }
        }
    }
}