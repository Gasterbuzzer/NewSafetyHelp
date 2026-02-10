using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using UnityEngine;

namespace NewSafetyHelp.Audio.Music.Intermission
{
    public static class IntermissionMusicHelper
    {
        private static bool ShouldPlayIntermissionMusic = true;
        private static object playIntermission;
        
        public static void PlayIntermissionMusic(CustomMusic audioClip)
        {
            FieldInfo myMusicSource = typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

            if (myMusicSource == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' was not found. Unable of changing StartMusic.");
                return;
            }
                
            AudioSource myMusicSourceCast = (AudioSource) myMusicSource.GetValue(GlobalVariables.musicControllerScript);

            if (myMusicSourceCast == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic.");
                return;
            }

            playIntermission = MelonCoroutines.Start(PlayIntermissionMusicLoop(myMusicSourceCast,
                7f, audioClip));
        }
        
        public static void StopIntermissionMusic()
        {
            FieldInfo myMusicSource = typeof(MusicController).GetField("myMusicSource", BindingFlags.NonPublic | BindingFlags.Instance);

            if (myMusicSource == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' was not found. Unable of changing StartMusic.");
                return;
            }
                
            AudioSource myMusicSourceCast = (AudioSource) myMusicSource.GetValue(GlobalVariables.musicControllerScript);

            if (myMusicSourceCast == null)
            {
                MelonLogger.Error("ERROR: 'myMusicSource' could not be cast. Unable of changing StartMusic.");
                return;
            }
            
            if (playIntermission != null)
            {
                MelonCoroutines.Stop(playIntermission);
                
                myMusicSourceCast.Stop();
            }
        }
        
        private static IEnumerator PlayIntermissionMusicLoop(AudioSource myMusicSourceCast,
            float stopMusicAfterSeconds, CustomMusic audioClip)
        {
            while (ShouldPlayIntermissionMusic)
            {
                GlobalVariables.musicControllerScript.StartMusic(audioClip.MusicClip);
            
                yield return new WaitForSeconds(stopMusicAfterSeconds);
            
                myMusicSourceCast.Stop();
            }
        }
    }
}