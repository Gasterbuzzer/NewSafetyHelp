using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using UnityEngine;

namespace NewSafetyHelp.Audio.Music.Intermission
{
    public static class IntermissionMusicHelper
    {
        private static bool shouldPlayIntermissionMusic = true;
        private static object playIntermission;
        
        // Field Infos
        private static readonly FieldInfo MyMusicSource = typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);
        
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
                
            AudioSource myMusicSourceCast = (AudioSource) MyMusicSource.GetValue(GlobalVariables.musicControllerScript);

            if (myMusicSourceCast == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic.");
                return;
            }

            shouldPlayIntermissionMusic = true;
            playIntermission = MelonCoroutines.Start(PlayIntermissionMusicLoop(myMusicSourceCast, audioClip));
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
                
            AudioSource myMusicSourceCast = (AudioSource) MyMusicSource.GetValue(GlobalVariables.musicControllerScript);

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
        private static IEnumerator PlayIntermissionMusicLoop(AudioSource myMusicSourceCast, CustomMusic audioClip,
            bool shouldLoop = true)
        {
            bool wasProvidedNull = (audioClip == null);
            
            while (shouldPlayIntermissionMusic)
            {
                if (audioClip == null || wasProvidedNull)
                {
                    audioClip = PickIntermissionMusic();
                }
                
                myMusicSourceCast.loop = false;
                
                float musicStopAfterSeconds = MusicEndRange(audioClip);
                float startAfterSeconds = MusicStartRange(audioClip);
                
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
        public static CustomMusic PickIntermissionMusic()
        {
            if (CustomCampaignGlobal.InCustomCampaign)
            {
                CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (customCampaign == null)
                {
                    MelonLogger.Error("ERROR: 'customCampaign' could not be found. Unable to pick intermission music.");
                    return null;
                }
                
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

                if (validCustomMusic.Count <= 0)
                {
                    return null;
                }
                else
                {
                    int randomIndex = Random.Range(0, validCustomMusic.Count);
                    return validCustomMusic[randomIndex];
                }
            }
            
            return null;
        }
    }
}