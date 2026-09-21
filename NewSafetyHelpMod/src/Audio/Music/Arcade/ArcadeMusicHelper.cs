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
                    PLayArcadeMusic(customArcadeMusicList[chosenMusicIndex].MusicClip.clip);
                }

                while (arcadeMusicPlayerAudioSource.isPlaying)
                {
                    yield return null;
                }
            }
        }

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

        private static void PLayArcadeMusic(AudioClip musicToBePlayed)
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