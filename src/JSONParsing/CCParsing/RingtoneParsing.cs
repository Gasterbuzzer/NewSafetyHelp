using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CustomRingtone;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
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
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as ringtone. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomRingtone customRingtone = ParseRingtone(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add ringtone clip
            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customRingtone.RingtoneClipPath,
                clip => customRingtone.RingtoneClip = clip,
                jsonFolderPath, "ringtone_audio_clip_name");
            
            // Add to correct campaign.
            CustomCampaign customCampaign = CustomCampaignGlobal.GetNamedCustomCampaign(customCampaignName);
            
            if (customCampaign != null)
            {
                customCampaign.CustomRingtones.Add(customRingtone);
            }
            else
            {
                LoggingHelper.DebugLog("Found ringtone file before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignRingtones.Add(customRingtone);
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
            float playChance = 1.0f; // Chance for this Ringtone to play, only if set to append.

            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_attached", ref customCampaignName);

            AudioParsingHelper.TryAssignAudioPath(jObjectParsed, "ringtone_audio_clip_name", ref ringtoneAudioPath,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            // Unlock Day
            ParsingHelper.TryAssign(jObjectParsed, "unlock_day", ref unlockDay);

            if (unlockDay <= 0)
            {
                onlyOnUnlockDay = false;
            }
            
            ParsingHelper.TryAssign(jObjectParsed, "only_play_on_unlock_day", ref onlyOnUnlockDay);
            ParsingHelper.TryAssign(jObjectParsed, "is_glitched_version", ref isGlitchedVersion);
            
            ParsingHelper.TryAssign(jObjectParsed, "is_append_ringtone", ref appendRingtone);
            if (appendRingtone)
            {
                ParsingHelper.TryAssign(jObjectParsed, "ringtone_chance", ref playChance);
            }

            return new CustomRingtone
            {
                CustomCampaignName = customCampaignName,

                RingtoneClipPath = ringtoneAudioPath,

                UnlockDay = unlockDay,
                
                OnlyOnUnlockDay = onlyOnUnlockDay,
                
                IsGlitchedVersion = isGlitchedVersion,
                
                AppendRingtone = appendRingtone,
                PlayChance = playChance
            };
        }
    }
}