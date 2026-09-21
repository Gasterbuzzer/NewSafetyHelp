using System.Collections;
using System.Collections.Generic;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace NewSafetyHelp.Audio.Music.Arcade
{
    public static class ArcadeMusicHelper
    {
        private static GameObject arcadeMusicPlayer;
        private static AudioSource arcadeMusicPlayerAudioSource;

        private static int previousMusicIndex;

        /// <summary>
        /// Start the process of playing all the arcade music in the custom campaign arcade mode.
        /// </summary>
        /// <param name="customCampaign">Custom Campaign that is active.</param>
        /// <param name="customArcadeMusicList">List of all the custom arcade music.</param>
        /// <returns>Coroutine to run.</returns>
        public static IEnumerator PlayPassthroughMusicArcade(CustomCampaign customCampaign,
            List<CustomMusic> customArcadeMusicList)
        {
            LoggingHelper.DebugLog("Starting to play passthrough arcade music.", LoggingHelper.LoggingCategory.ARCADE);

            CreateArcadeMusicPlayer();

            bool playArcadeMusic = false;

            while (true)
            {
                int chosenMusicIndex = MusicHelper.ChoseArcadeMusic(customCampaign.ArcadeMusic.Count,
                    previousMusicIndex,
                    ref playArcadeMusic);

                if (!playArcadeMusic)
                {
                    yield break;
                }

                if (customArcadeMusicList.Count > 0
                    && chosenMusicIndex < customArcadeMusicList.Count
                    && customArcadeMusicList[chosenMusicIndex].MusicClip != null)
                {
                    previousMusicIndex = chosenMusicIndex;
                    PlayArcadeMusic(customArcadeMusicList[chosenMusicIndex].MusicClip.clip);
                }

                while (arcadeMusicPlayerAudioSource.isPlaying)
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Creates the arcade music player that will play the custom arcade music.
        /// </summary>
        private static void CreateArcadeMusicPlayer()
        {
            if (arcadeMusicPlayer != null)
            {
                Object.Destroy(arcadeMusicPlayerAudioSource);
                Object.Destroy(arcadeMusicPlayer);
            }

            arcadeMusicPlayer = new GameObject("ArcadeMusicPlayer");

            arcadeMusicPlayerAudioSource = arcadeMusicPlayer.AddComponent<AudioSource>();

            arcadeMusicPlayerAudioSource.playOnAwake = false;

            GameObject optionsPopup =
                GameObject.Find("MainCanvas").transform.Find("OptionsPopup").gameObject;

            AudioMixer audioMixer = optionsPopup.GetComponent<OptionsMenuBehavior>().masterMixer;

            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups("Music");

            if (audioMixerGroups.Length > 0)
            {
                arcadeMusicPlayerAudioSource.outputAudioMixerGroup = audioMixerGroups[0];
            }
            else
            {
                LoggingHelper.ErrorLog("Could not add custom arcade music player to music group.");
            }
        }

        /// <summary>
        /// Plays the arcade music on the arcade music audio source.
        /// </summary>
        /// <param name="musicToBePlayed">AudioClip containing the music to be played.</param>
        private static void PlayArcadeMusic(AudioClip musicToBePlayed)
        {
            if (arcadeMusicPlayerAudioSource == null)
            {
                return;
            }

            arcadeMusicPlayerAudioSource.Stop();

            arcadeMusicPlayerAudioSource.pitch = 1f;

            arcadeMusicPlayerAudioSource.clip = musicToBePlayed;
            arcadeMusicPlayerAudioSource.volume = 0.5f;
            arcadeMusicPlayerAudioSource.time = 0.0f;
            arcadeMusicPlayerAudioSource.Play();
        }
    }
}