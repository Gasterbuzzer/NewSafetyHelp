using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
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
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as music. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            MusicExtraInfo customMusic = ParseMusic(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add music clip
            if (jObjectParsed.ContainsKey("music_audio_clip_name"))
            {
                if (string.IsNullOrEmpty(customMusic.musicClipPath))
                {
                    MelonLogger.Warning(
                        $"WARNING: No valid music file given for file in {jsonFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(customMusic.musicClipPath))
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain '{customMusic.musicClipPath}'." +
                        " Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    customMusic.musicClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {customMusic.musicClipPath} for custom caller.");
                                }
                            },
                            customMusic.musicClipPath)
                    );
                }
            }

            // Add to correct campaign.
            CustomCampaignExtraInfo foundCustomCampaign =
                CustomCampaignGlobal.customCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.campaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                foundCustomCampaign.customMusic.Add(customMusic);
            }
            else
            {
                #if DEBUG
                MelonLogger.Msg("DEBUG: Found Music File before the custom campaign was found / does not exist.");
                #endif

                GlobalParsingVariables.missingCustomCampaignMusic.Add(customMusic);
            }
        }

        private static MusicExtraInfo ParseMusic(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            int unlockDay = 0; // When the music is unlocked. Mostly used for default game logic.

            string musicAudioPath = ""; // Audio Path to load audio from.

            if (jObjectParsed.TryGetValue("custom_campaign_attached", out var customCampaignNameValue))
            {
                customCampaignName = (string)customCampaignNameValue;
            }

            if (jObjectParsed.TryGetValue("music_audio_clip_name", out var musicAudioClipName))
            {
                if (!File.Exists(jsonFolderPath + "\\" + musicAudioClipName))
                {
                    if (!File.Exists(usermodFolderPath + "\\" + musicAudioClipName))
                    {
                        MelonLogger.Warning("WARNING: " +
                                            $"Could not find provided audio file for custom caller at '{jsonFolderPath}' for {musicAudioClipName}.");
                    }
                    else
                    {
                        musicAudioPath = usermodFolderPath + "\\" + musicAudioClipName;
                    }
                }
                else
                {
                    musicAudioPath = jsonFolderPath + "\\" + musicAudioClipName;
                }
            }

            if (jObjectParsed.TryGetValue("unlock_day", out var unlockDayValue))
            {
                unlockDay = (int)unlockDayValue;
            }

            return new MusicExtraInfo
            {
                customCampaignName = customCampaignName,

                musicClipPath = musicAudioPath,

                unlockDay = unlockDay,
            };
        }
    }
}