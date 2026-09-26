using System.Collections.Generic;
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

        public static readonly List<RichAudioClip> KeyboardSounds = new List<RichAudioClip>();

        [CanBeNull] public static Sprite ClockBase;
        [CanBeNull] public static Sprite ClockHand;

        [CanBeNull] public static Sprite AdminIcon;

        [CanBeNull] public static RichAudioClip AchievementSound;

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
                        ClockFivePercent = AudioImport.CreateRichAudioClip(audioClip, "clock_10_percent.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockFivePercent)}' audio clip.");
                    }
                },
                "clock_10_percent.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        ClockHalfTime = AudioImport.CreateRichAudioClip(audioClip, "clock_half_time.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockHalfTime)}' audio clip.");
                    }
                },
                "clock_half_time.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        ClockStart = AudioImport.CreateRichAudioClip(audioClip, "clock_start.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(ClockStart)}' audio clip.");
                    }
                },
                "clock_start.mp3", true);

            /*
             * Keyboard Audio
             */

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSounds.Add(AudioImport.CreateRichAudioClip(audioClip, "keyboard01.mp3"));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Failed to load embedded 'keyboard01.mp3' audio clip.");
                    }
                },
                "keyboard01.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSounds.Add(AudioImport.CreateRichAudioClip(audioClip, "keyboard02.mp3"));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Failed to load embedded 'keyboard02.mp3' audio clip.");
                    }
                },
                "keyboard02.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSounds.Add(AudioImport.CreateRichAudioClip(audioClip, "keyboard03.mp3"));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Failed to load embedded 'keyboard03.mp3' audio clip.");
                    }
                },
                "keyboard03.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSounds.Add(AudioImport.CreateRichAudioClip(audioClip, "keyboard04.mp3"));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Failed to load embedded 'keyboard04.mp3' audio clip.");
                    }
                },
                "keyboard04.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        KeyboardSounds.Add(AudioImport.CreateRichAudioClip(audioClip, "keyboard05.mp3"));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Failed to load embedded 'keyboard05.mp3' audio clip.");
                    }
                },
                "keyboard05.mp3", true);

            AudioImport.LoadEmbeddedAudio(
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        // Add the audio
                        AchievementSound = AudioImport.CreateRichAudioClip(audioClip, "achievement.mp3");
                    }
                    else
                    {
                        LoggingHelper.ErrorLog($"Failed to load embedded '{nameof(AchievementSound)}' audio clip.");
                    }
                },
                "achievement.mp3", true);

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