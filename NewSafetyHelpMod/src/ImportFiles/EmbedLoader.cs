using JetBrains.Annotations;
using NewSafetyHelp.Audio;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.ImportFiles
{
    public static class EmbedLoader
    {
        [CanBeNull] public static RichAudioClip ClockFivePercent;
        [CanBeNull] public static RichAudioClip ClockHalfTime;
        [CanBeNull] public static RichAudioClip ClockStart;

        [CanBeNull] public static RichAudioClip KeyboardSound01;
        [CanBeNull] public static RichAudioClip KeyboardSound02;
        [CanBeNull] public static RichAudioClip KeyboardSound03;
        [CanBeNull] public static RichAudioClip KeyboardSound04;
        [CanBeNull] public static RichAudioClip KeyboardSound05;

        [CanBeNull] public static Sprite ClockBase;
        [CanBeNull] public static Sprite ClockHand;

        [CanBeNull] public static Sprite AdminIcon;

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
                        ClockFivePercent = AudioImport.CreateRichAudioClip(audioClip, "clock_10_percent.wav");
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
                        ClockHalfTime = AudioImport.CreateRichAudioClip(audioClip, "clock_half_time.wav");
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
                        ClockStart = AudioImport.CreateRichAudioClip(audioClip, "clock_start.wav");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockStart)}' audio clip.");
                    }
                },
                "clock_start.wav", true);

            /*
             * Keyboard Audio
             */

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSound01 = AudioImport.CreateRichAudioClip(audioClip, "keyboard01.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(KeyboardSound01)}' audio clip.");
                    }
                },
                "keyboard01.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSound02 = AudioImport.CreateRichAudioClip(audioClip, "keyboard02.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(KeyboardSound02)}' audio clip.");
                    }
                },
                "keyboard02.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSound03 = AudioImport.CreateRichAudioClip(audioClip, "keyboard03.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(KeyboardSound03)}' audio clip.");
                    }
                },
                "keyboard03.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSound04 = AudioImport.CreateRichAudioClip(audioClip, "keyboard04.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(KeyboardSound04)}' audio clip.");
                    }
                },
                "keyboard04.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSound05 = AudioImport.CreateRichAudioClip(audioClip, "keyboard05.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(KeyboardSound05)}' audio clip.");
                    }
                },
                "keyboard05.mp3", true);

            /*
             * Images
             */

            ClockBase = ImageImport.LoadEmbeddedImage("clock_base.png");
            ClockHand = ImageImport.LoadEmbeddedImage("clock_hand.png");

            AdminIcon = ImageImport.LoadEmbeddedImage("admin_icon.png");

            LoggingHelper.DebugLog("Finished the starting of the embed loading coroutines.");
        }
    }
}