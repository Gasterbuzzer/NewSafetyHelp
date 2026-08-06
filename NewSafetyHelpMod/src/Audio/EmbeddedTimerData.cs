using JetBrains.Annotations;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.Audio
{
    public static class EmbeddedTimerData
    {
        [CanBeNull] public static RichAudioClip ClockFivePercent;
        [CanBeNull] public static RichAudioClip ClockHalfTime;
        [CanBeNull] public static RichAudioClip ClockStart;

        [CanBeNull] public static Sprite ClockBase;
        [CanBeNull] public static Sprite ClockHand;

        public static void Initialize()
        {
            /*
             * Audios
             */

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        ClockFivePercent = AudioImport.CreateRichAudioClip(audioClip);
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockFivePercent)}' audio clip.");
                    }
                },
                "clock_10_percent.wav", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        ClockHalfTime = AudioImport.CreateRichAudioClip(audioClip);
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockHalfTime)}' audio clip.");
                    }
                },
                "clock_half_time.wav", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        ClockStart = AudioImport.CreateRichAudioClip(audioClip);
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockStart)}' audio clip.");
                    }
                },
                "clock_start.wav", true);

            /*
             * Images
             */

            ClockBase = ImageImport.LoadEmbeddedImage("clock_base.png");
            ClockHand = ImageImport.LoadEmbeddedImage("clock_hand.png");

            LoggingHelper.DebugLog("Finished the starting of the embed loading coroutines.");
        }
    }
}