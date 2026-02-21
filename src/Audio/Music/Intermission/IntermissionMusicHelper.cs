using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.Audio.Music.Intermission
{
    public static class IntermissionMusicHelper
    {
        private static bool shouldPlayIntermissionMusic = true;
        private static object playIntermission;

        // Field Infos
        private static readonly FieldInfo MyMusicSource =
            typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Plays intermission music based on the given custom music.
        /// </summary>
        /// <param name="audioClip"> Audio Clip to be played as intermission music. </param>
        public static void PlayIntermissionMusic(CustomMusic audioClip = null)
        {
            if (MyMusicSource == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' was not found. Unable of changing StartMusic.");
                return;
            }

            AudioSource myMusicSourceCast = (AudioSource)MyMusicSource.GetValue(GlobalVariables.musicControllerScript);

            if (myMusicSourceCast == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic.");
                return;
            }

            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                LoggingHelper.CampaignNullError();
                return;
            }

            if (customCampaign.CustomIntermissionMusic.Count <= 0)
            {
                return;
            }

            shouldPlayIntermissionMusic = true;
            playIntermission =
                MelonCoroutines.Start(PlayIntermissionMusicLoop(myMusicSourceCast, audioClip, customCampaign));
        }

        /// <summary>
        /// Stops the intermission music coroutine from playing.
        /// </summary>
        public static void StopIntermissionMusicRoutine()
        {
            shouldPlayIntermissionMusic = false;

            if (playIntermission != null)
            {
                MelonCoroutines.Stop(playIntermission);
            }
        }

        /// <summary>
        /// Stops the intermission music from playing.
        /// </summary>
        public static void StopIntermissionMusic()
        {
            if (MyMusicSource == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' was not found. Unable of changing StartMusic.");
                return;
            }

            AudioSource myMusicSourceCast = (AudioSource)MyMusicSource.GetValue(GlobalVariables.musicControllerScript);

            if (myMusicSourceCast == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic.");
                return;
            }

            if (playIntermission != null)
            {
                shouldPlayIntermissionMusic = false;

                MelonCoroutines.Stop(playIntermission);

                myMusicSourceCast.Stop();

                myMusicSourceCast.loop = true;
            }
        }

        /// <summary>
        /// Actually plays the intermission music in a loop. The loop will stop when the stop function gets called.
        /// </summary>
        /// <param name="myMusicSourceCast"> AudioSource to stop the music for. </param>
        /// <param name="audioClip"> Music to played. </param>
        /// <param name="customCampaign"> Custom Campaign to find the music in. </param>
        /// <param name="shouldLoop"> If the music should loop or should stop after being played once. </param>
        private static IEnumerator PlayIntermissionMusicLoop(AudioSource myMusicSourceCast, CustomMusic audioClip,
            CustomCampaign customCampaign, bool shouldLoop = true)
        {
            bool wasProvidedNull = (audioClip == null);
            bool shouldPlayIntermission = false;

            while (shouldPlayIntermissionMusic)
            {
                if (audioClip == null || wasProvidedNull)
                {
                    audioClip = PickIntermissionMusic(ref shouldPlayIntermission, customCampaign);

                    // No valid music found. We stop.
                    if (!shouldPlayIntermission)
                    {
                        yield break;
                    }
                }

                myMusicSourceCast.loop = false;

                float startAfterSeconds = MusicStartRange(audioClip);
                float musicStopAfterSeconds = MusicEndRange(audioClip);

                LoggingHelper.DebugLog($"Intermission music playing with start of: '{startAfterSeconds}' with" +
                                       $"end range of '{musicStopAfterSeconds}'.");

                if (musicStopAfterSeconds - startAfterSeconds <= 0)
                {
                    MelonLogger.Warning("WARNING: Provided music ranges overlap and cause the music not to play." +
                                        " Unable to play music.");
                    yield break;
                }

                GlobalVariables.musicControllerScript.StartMusic(audioClip.MusicClip);

                myMusicSourceCast.time = startAfterSeconds;

                yield return new WaitForSeconds(musicStopAfterSeconds - startAfterSeconds);

                myMusicSourceCast.Stop();

                if (!shouldLoop)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Returns the music range where the music is supposed to be stopped based on the different types of end range.
        /// </summary>
        /// <param name="audioClip"> Music to played. </param>
        private static float MusicEndRange(CustomMusic audioClip)
        {
            if (audioClip.EndRange == null)
            {
                return audioClip.MusicClip.clip.length;
            }

            if (audioClip.EndRange.Count <= 0)
            {
                return audioClip.MusicClip.clip.length;
            }

            switch (audioClip.EndRange.Count)
            {
                case 1:
                    return audioClip.EndRange[0];

                case 2:
                    return Random.Range(audioClip.EndRange[0], audioClip.EndRange[1]);
            }

            if (audioClip.EndRange.Count > 2)
            {
                int randomIndex = Random.Range(0, audioClip.EndRange.Count);

                return audioClip.EndRange[randomIndex];
            }

            return audioClip.MusicClip.clip.length;
        }

        /// <summary>
        /// Returns the music range where the music is supposed
        /// to be started based on the different types of start range.
        /// </summary>
        /// <param name="audioClip"> Music to played. </param>
        private static float MusicStartRange(CustomMusic audioClip)
        {
            if (audioClip.StartRange == null)
            {
                return 0;
            }

            if (audioClip.StartRange.Count <= 0)
            {
                return 0;
            }

            switch (audioClip.StartRange.Count)
            {
                case 1:
                    return audioClip.StartRange[0];

                case 2:
                    return Random.Range(audioClip.StartRange[0], audioClip.StartRange[1]);
            }

            if (audioClip.StartRange.Count > 2)
            {
                int randomIndex = Random.Range(0, audioClip.StartRange.Count);

                return audioClip.StartRange[randomIndex];
            }

            return 0;
        }


        /// <summary>
        /// Returns a valid intermission music that is allowed to play for the current day.
        /// </summary>
        /// <returns>Returns null if none found and </returns>
        // ReSharper disable once RedundantAssignment
        private static CustomMusic PickIntermissionMusic(ref bool shouldPlayIntermission, CustomCampaign customCampaign)
        {
            shouldPlayIntermission = false;

            if (CustomCampaignGlobal.InCustomCampaign)
            {
                List<CustomMusic> validCustomMusic = customCampaign.CustomIntermissionMusic
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

                LoggingHelper.DebugLog(
                    $"Custom Intermission Music Available: {customCampaign.CustomIntermissionMusic.Count}. Valid: '{validCustomMusic.Count}'.");

                if (validCustomMusic.Count <= 0)
                {
                    return null;
                }
                else
                {
                    shouldPlayIntermission = true;
                    int randomIndex = Random.Range(0, validCustomMusic.Count);
                    return validCustomMusic[randomIndex];
                }
            }

            return null;
        }
    }
}