using System.Collections.Generic;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class MusicParsing
    {
        /// <summary>
        /// Load a music from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateMusic(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as music. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomMusic customMusic = ParseMusic(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add music clip
            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customMusic.MusicClipPath,
                clip => customMusic.MusicClip = clip,
                jsonFolderPath, customMusic.CompressAudio, "music_audio_clip_name");

            // Add to correct campaign.
            CustomCampaign customCampaign = CustomCampaignGlobal.GetNamedCustomCampaign(customCampaignName);

            if (customCampaign != null)
            {
                if (customMusic.IsIntermissionMusic)
                {
                    customCampaign.CustomIntermissionMusic.Add(customMusic);
                }
                else
                {
                    customCampaign.CustomMusic.Add(customMusic);
                }
            }
            else
            {
                LoggingHelper.DebugLog("Found Music File before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignMusic.Add(customMusic);
            }
        }

        private static CustomMusic ParseMusic(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            int unlockDay = 0; // When the music is unlocked. Mostly used for default game logic.

            string musicAudioPath = ""; // Audio Path to load audio from.
            bool compressAudio = true;

            bool onlyPlayOnUnlockDay = false;

            bool isIntermissionMusic = false;

            List<float> startRange = new List<float>();
            List<float> endRange = new List<float>();

            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_attached", ref customCampaignName);

            AudioParsingHelper.TryAssignAudioPath(jObjectParsed, "music_audio_clip_name", ref musicAudioPath,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "music_compress_audio", ref compressAudio);

            ParsingHelper.TryAssign(jObjectParsed, "unlock_day", ref unlockDay);

            ParsingHelper.TryAssign(jObjectParsed, "only_play_on_unlock_day", ref onlyPlayOnUnlockDay);

            ParsingHelper.TryAssign(jObjectParsed, "is_intermission_music", ref isIntermissionMusic);

            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, "start_range", ref startRange);

            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, "end_range", ref endRange);

            return new CustomMusic
            {
                CustomCampaignName = customCampaignName,

                MusicClipPath = musicAudioPath,
                CompressAudio = compressAudio,

                UnlockDay = unlockDay,

                OnlyPlayOnUnlockDay = onlyPlayOnUnlockDay,

                IsIntermissionMusic = isIntermissionMusic,

                StartRange = startRange,
                EndRange = endRange
            };
        }
    }
}