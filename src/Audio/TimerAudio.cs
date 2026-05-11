using JetBrains.Annotations;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.Audio
{
    public static class TimerAudio
    {
        [CanBeNull] public static RichAudioClip ClockFivePercent;
        [CanBeNull] public static RichAudioClip ClockHalfTime;
        [CanBeNull] public static RichAudioClip ClockStart;

        public static void Initialize()
        {
            // We delete all temp files and recreate them. (Makes sure we are up to date and doesn't leave residue).
            EmbedHelpers.DeleteTempFiles();

            // We now load all the audios.
            AudioImport.LoadEmbeddedAudio(
                (audioClip) =>
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
                "clock_10_percent.wav");

            AudioImport.LoadEmbeddedAudio(
                (audioClip) =>
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
                "clock_half_time.wav");

            AudioImport.LoadEmbeddedAudio(
                (audioClip) =>
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
                "clock_start.wav");

            LoggingHelper.DebugLog("Finished starting the embedded audio loading routines.");
        }
    }
}