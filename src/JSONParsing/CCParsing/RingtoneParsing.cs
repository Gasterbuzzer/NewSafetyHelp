using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomRingtone;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class RingtoneParsing
    {
        /// <summary>
        /// Load a ringtone from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateRingtone(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as ringtone. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomRingtone customCustomRingtone = ParseRingtone(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add ringtone clip
            if (jObjectParsed.ContainsKey("ringtone_audio_clip_name"))
            {
                if (string.IsNullOrEmpty(customCustomRingtone.RingtoneClipPath))
                {
                    MelonLogger.Warning(
                        $"WARNING: No valid ringtone file given for file in {jsonFolderPath}.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(customCustomRingtone.RingtoneClipPath))
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain '{customCustomRingtone.RingtoneClipPath}'." +
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
                                    customCustomRingtone.RingtoneClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {customCustomRingtone.RingtoneClipPath}" +
                                        " for custom caller.");
                                }
                            },
                            customCustomRingtone.RingtoneClipPath)
                    );
                }
            }

            // Add to correct campaign.
            CustomCampaign.CustomCampaignModel.CustomCampaign foundCustomCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                foundCustomCampaign.CustomRingtones.Add(customCustomRingtone);
            }
            else
            {
                #if DEBUG
                MelonLogger.Msg("DEBUG: Found ringtone file before the custom campaign was found / does not exist.");
                #endif

                GlobalParsingVariables.PendingCustomCampaignRingtones.Add(customCustomRingtone);
            }
        }

        private static CustomRingtone ParseRingtone(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            int unlockDay = 0; // When the ringtone is unlocked.
            
            bool onlyOnUnlockDay = true; // If the ringtone should only play on the unlock day.

            string ringtoneAudioPath = ""; // Audio Path to load audio from.
            
            bool isGlitchedVersion = false;

            bool appendRingtone = false; // If this is an append ringtone caller.

            if (jObjectParsed.TryGetValue("custom_campaign_attached", out var customCampaignNameValue))
            {
                customCampaignName = customCampaignNameValue.Value<string>();
            }

            if (jObjectParsed.TryGetValue("ringtone_audio_clip_name", out var ringtoneAudioClipName))
            {
                if (!File.Exists(jsonFolderPath + "\\" + ringtoneAudioClipName))
                {
                    if (!File.Exists(usermodFolderPath + "\\" + ringtoneAudioClipName))
                    {
                        MelonLogger.Warning("WARNING: " +
                                            $"Could not find provided audio file for custom caller at '{jsonFolderPath}' for {ringtoneAudioClipName}.");
                    }
                    else
                    {
                        ringtoneAudioPath = usermodFolderPath + "\\" + ringtoneAudioClipName;
                    }
                }
                else
                {
                    ringtoneAudioPath = jsonFolderPath + "\\" + ringtoneAudioClipName;
                }
            }

            if (jObjectParsed.TryGetValue("unlock_day", out var unlockDayValue))
            {
                unlockDay = unlockDayValue.Value<int>();
            }

            if (unlockDay == 0)
            {
                onlyOnUnlockDay = false;
            }
            
            if (jObjectParsed.TryGetValue("only_play_on_unlock_day", out var onlyPlayOnUnlockDayValue))
            {
                onlyOnUnlockDay = onlyPlayOnUnlockDayValue.Value<bool>();
            }
            
            if (jObjectParsed.TryGetValue("is_glitched_version", out var isGlitchedVersionValue))
            {
                isGlitchedVersion = isGlitchedVersionValue.Value<bool>();
            }
            
            if (jObjectParsed.TryGetValue("is_append_ringtone", out var isAppendRingtoneValue))
            {
                appendRingtone = isAppendRingtoneValue.Value<bool>();
            }

            return new CustomRingtone()
            {
                CustomCampaignName = customCampaignName,

                RingtoneClipPath = ringtoneAudioPath,

                UnlockDay = unlockDay,
                
                OnlyOnUnlockDay = onlyOnUnlockDay,
                
                IsGlitchedVersion = isGlitchedVersion,
                
                AppendRingtone = appendRingtone
            };
        }
    }
}