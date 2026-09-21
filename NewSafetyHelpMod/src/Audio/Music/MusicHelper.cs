using System.Collections.Generic;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.Audio.Music
{
    public static class MusicHelper
    {
        /// <summary>
        /// Checks if a given clip is valid (meaning it holds the conditions required for it to play).
        /// </summary>
        /// <param name="clip">CustomMusic clip to be checked.</param>
        /// <returns>True: All conditions were met. False: A condition failed, not allowed to be played.</returns>
        public static bool IsValidCustomMusic(CustomMusic clip)
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

        /// <summary>
        /// Chooses a music index from a given list of clips.
        /// </summary>
        /// <param name="removeDefaultMusic">If to not account for the default base game music.</param>
        /// <param name="previousMusicIndex">The index of the previously played music clip.</param>
        /// <param name="customMusicAmount">Amount of custom music in custom campaign.</param>
        /// <param name="baseGameMusicAmount">Amount of music in the base game.</param>
        /// <returns>(int chosenMusicIndex, int amountOfClips):
        /// The first is the chosen index and the second is amount of clips that were accounted for.</returns>
        private static (int, int) ChooseMusicIndex(bool removeDefaultMusic, int previousMusicIndex,
            int customMusicAmount, int baseGameMusicAmount)
        {
            int amountOfClips = 0;

            if (!removeDefaultMusic)
            {
                amountOfClips += baseGameMusicAmount;
            }

            // We have custom music
            if (customMusicAmount > 0)
            {
                amountOfClips += customMusicAmount;
            }

            // First attempt.
            int chosenMusicIndex = Random.Range(0, amountOfClips);

            for (int musicChoosingAttempt = 0;
                 chosenMusicIndex == previousMusicIndex && musicChoosingAttempt < 3;
                 ++musicChoosingAttempt)
            {
                chosenMusicIndex = Random.Range(0, amountOfClips);
            }

            return (chosenMusicIndex, amountOfClips);
        }

        /// <summary>
        /// Chooses a random music index from the base game music list.
        /// </summary>
        /// <param name="previousMusicIndex">The index of the previously played music clip.</param>
        /// <returns>(int) Index of the music in the base game music list to be played.</returns>
        private static int ChoseRandomMusicIndexBaseGame(int previousMusicIndex)
        {
            int chosenMusicIndex = 0;

            for (int musicChoosingAttempt = 0;
                 chosenMusicIndex == previousMusicIndex && musicChoosingAttempt < 3;
                 ++musicChoosingAttempt)
            {
                chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
            }

            return chosenMusicIndex;
        }

        /// <summary>
        /// Chooses a music clip the combined pool of base game and custom music.
        /// </summary>
        /// <param name="removeDefaultMusic">If to not account for the default base game music.</param>
        /// <param name="baseGameMusicAmount">Amount of music in the base game.</param>
        /// <param name="customCampaignMusicAmount">Amount of custom music in the custom campaign.</param>
        /// <param name="previousMusicIndex">The index of the previously played music clip.</param>
        /// <param name="playCustomMusic">(Reference) If to play custom music
        /// (this gets enabled when the chosen index is from a custom music list)</param>
        /// <returns>(int) Index of the music to be played.
        /// Please mind, it's index based on the list chosen (so not combined index).</returns>
        public static int ChooseFromCombinedPool(bool removeDefaultMusic,
            int baseGameMusicAmount, int customCampaignMusicAmount,
            int previousMusicIndex, ref bool playCustomMusic)
        {
            (int chosenMusicIndex, int amountOfClips) =
                ChooseMusicIndex(removeDefaultMusic, previousMusicIndex, customCampaignMusicAmount,
                    baseGameMusicAmount);

            if (!removeDefaultMusic) // Don't remove default music.
            {
                if (chosenMusicIndex >= baseGameMusicAmount)
                {
                    playCustomMusic = true;
                    chosenMusicIndex -= baseGameMusicAmount;
                }
            }
            else // Remove default music
            {
                playCustomMusic = true;
            }

            bool capturedBoolPlayCustomMusic = playCustomMusic;

            LoggingHelper.DebugLog(() =>
                $"Chose to play the music track: '{chosenMusicIndex}' with the previous being '{previousMusicIndex}'. " +
                $"(From custom music? '{capturedBoolPlayCustomMusic}') " +
                $"(Amount of clips: '{amountOfClips}') " +
                $"(Total clips combined: '{baseGameMusicAmount + customCampaignMusicAmount}') " +
                $"(Remove default music? '{removeDefaultMusic}') " +
                $"(Current day: '{GlobalVariables.currentDay}').");

            if (playCustomMusic)
            {
                LoggingHelper.DebugLog($"Amount of custom music available: '{baseGameMusicAmount}'.");
            }

            return chosenMusicIndex;
        }

        /// <summary>
        /// Similar to ChooseFromCombinedPool(...) but makes sure that both the base game music and the custom music
        /// get a fair chance at playing (50% custom music and 50% base game music).
        /// </summary>
        /// <param name="removeDefaultMusic">If to not account for the default base game music.</param>
        /// <param name="baseGameMusicAmount">Amount of music in the base game.</param>
        /// <param name="customCampaignMusicAmount">Amount of custom music in the custom campaign.</param>
        /// <param name="previousMusicIndex">The index of the previously played music clip.</param>
        /// <param name="playCustomMusic">(Reference) If to play custom music
        /// (this gets enabled when the chosen index is from a custom music list)</param>
        /// <returns>(int) Index of the music to be played.
        /// Please mind, it's index based on the list chosen (so not combined index).</returns>
        public static int ChoseMusicIndexFairly(bool removeDefaultMusic, int baseGameMusicAmount,
            int customCampaignMusicAmount, int previousMusicIndex, ref bool playCustomMusic)
        {
            int chosenMusicIndex = 0;

            // Only custom music
            if (removeDefaultMusic)
            {
                chosenMusicIndex = ChooseFromCombinedPool(true,
                    baseGameMusicAmount, customCampaignMusicAmount, previousMusicIndex, ref playCustomMusic);
            }
            // Combined custom and normal music
            else if (customCampaignMusicAmount > 0)
            {
                // 0 or 1 (We choose if we play from the normal music playlist 0 or from the custom music playlist 1.)
                int whichMusicList = Random.Range(0, 2);

                switch (whichMusicList)
                {
                    // Base Game Music (Normal music playlist)
                    case 0:
                        chosenMusicIndex = ChoseRandomMusicIndexBaseGame(previousMusicIndex);
                        break;

                    // Custom Campaign Music (custom music playlist)
                    case 1:

                        playCustomMusic = true;

                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == previousMusicIndex && musicChoosingAttempt < 3;
                             ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, customCampaignMusicAmount);
                        }

                        break;
                }
            }
            // No custom music was provided, we use the default logic for choosing the music index.
            else
            {
                chosenMusicIndex = ChoseRandomMusicIndexBaseGame(previousMusicIndex);
            }

            return chosenMusicIndex;
        }

        /// <summary>
        /// Chooses a music clip for the arcade mode.
        /// </summary>
        /// <param name="customArcadeMusicAmount">Amount of arcade custom music in the custom campaign.</param>
        /// <param name="previousMusicIndex">The index of the previously played music clip.</param>
        /// <param name="playArcadeMusic">(Reference) If to play the arcade music.</param>
        /// <returns>(int) Index of the music to be played.</returns>
        public static int ChoseArcadeMusic(int customArcadeMusicAmount, int previousMusicIndex,
            ref bool playArcadeMusic)
        {
            int chosenMusicIndex = 0;

            if (customArcadeMusicAmount >= 0)
            {
                playArcadeMusic = true;

                for (int musicChoosingAttempt = 0;
                     chosenMusicIndex == previousMusicIndex && musicChoosingAttempt < 3;
                     ++musicChoosingAttempt)
                {
                    chosenMusicIndex = Random.Range(0, customArcadeMusicAmount);
                }
            }

            if (playArcadeMusic)
            {
                LoggingHelper.DebugLog(() =>
                    $"Chose to play the arcade music track: '{chosenMusicIndex}' with the previous being '{previousMusicIndex}'. " +
                    $"(Amount of arcade clips: '{customArcadeMusicAmount}') " +
                    $"(Current day: '{GlobalVariables.currentDay}').", LoggingHelper.LoggingCategory.ARCADE);
            }

            return chosenMusicIndex;
        }

        /// <summary>
        /// Plays music in the custom campaign.
        /// </summary>
        /// <param name="__instance">Instance of the music controller class.</param>
        /// <param name="customMusicList">List of all custom music available to be played.</param>
        /// <param name="customArcadeMusicList">List of all custom arcade music available to be played.</param>
        /// <param name="chosenMusicIndex">Index of the chosen music.</param>
        /// <param name="playCustomMusic">If to play music from the custom campaign list or from the base game.</param>
        /// <param name="playArcadeMusic">If to play arcade music from the custom campaign list-</param>
        /// <param name="removeDefaultMusic">If we remove the base game music.
        /// Ergo only play custom campaign music if this is enabled.</param>
        /// <param name="playThroughMusic">If the arcade music is play through, meaning it won't be stopped.</param>
        public static void PlayMusicInCustomCampaign(MusicController __instance, ref List<CustomMusic> customMusicList,
            ref List<CustomMusic> customArcadeMusicList, int chosenMusicIndex, bool playCustomMusic,
            bool playArcadeMusic, bool removeDefaultMusic, bool playThroughMusic)
        {
            if (GlobalVariables.arcadeMode
                && playArcadeMusic
                && !playThroughMusic)
            {
                if (customArcadeMusicList.Count > 0
                    && chosenMusicIndex < customArcadeMusicList.Count
                    && customArcadeMusicList[chosenMusicIndex].MusicClip != null)
                {
                    __instance.StartMusic(customArcadeMusicList[chosenMusicIndex].MusicClip);
                }

                return;
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
                    LoggingHelper.WarningLog("There is no music available or music clip is empty! " +
                                             "Possibly some music failed loading?");
                }
            }
            else if (!removeDefaultMusic)
            {
                if (GlobalVariables.musicControllerScript.onHoldMusicClips.Length >= chosenMusicIndex)
                {
                    __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                }
            }
        }
    }
}