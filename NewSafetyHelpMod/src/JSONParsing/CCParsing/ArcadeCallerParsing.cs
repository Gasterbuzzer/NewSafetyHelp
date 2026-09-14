using NewSafetyHelp.Audio;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.ArcadeCallerModule;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class ArcadeCallerParsing
    {
        /// <summary>
        /// Creates an arcade caller from a given JSON file.
        /// </summary>
        /// <param name="jObjectParsed">JSON Parsed</param>
        /// <param name="usermodFolderPath">Filepath to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateArcadeCaller(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog(
                    "Provided JSON could not be parsed as an arcade caller. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            ArcadeCaller arcadeCaller = ParseArcadeCaller(ref jObjectParsed,
                ref customCampaignName, usermodFolderPath, jsonFolderPath);

            // Custom Caller Audio Path (Later gets added with coroutine) 
            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed,
                arcadeCaller.CallerClipPath,
                clip =>
                {
                    arcadeCaller.CallerClip = clip;
                    arcadeCaller.IsCallerClipLoaded = true;

                    // We finished loading all audios.
                    // We call the start function again.
                    if (AudioImport.CurrentLoadingAudios.Count <= 0)
                    {
                        AudioImport.ReCallCallerListStart();
                    }
                },
                jsonFolderPath,
                arcadeCaller.CompressAudio,
                "arcade_caller_audio_clip_name");

            // Add to correct campaign.
            CustomCampaign customCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (customCampaign != null)
            {
                customCampaign.FixedArcadeCallers.Add(arcadeCaller);
            }
            else
            {
                LoggingHelper.DebugLog("Found an arcade caller before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignArcadeCaller.Add(arcadeCaller);
            }
        }

        private static ArcadeCaller ParseArcadeCaller(ref JObject jObjectParsed, ref string customCampaignName,
            string usermodFolderPath, string jsonFolderPath)
        {
            /*
             * General Properties
             */

            // Caller Basics
            string callerName = "NO_ARCADE_CALLER_NAME";
            string callTranscript = "NO_ARCADE_CALLER_TRANSCRIPT";
            VariableChanged<Sprite> callerImage = new VariableChanged<Sprite>
            {
                Data = null
            };

            int arcadeCallersRequired = 0;

            // Caller Audio
            string callerClipPath = "";
            bool compressAudio = true;

            // Animated Portrait
            string callerAnimatedPortraitURL = null;

            VariableChanged<bool> callerAnimatedPortraitShouldLoop = new VariableChanged<bool>
            {
                Data = true
            };

            // --------------------------------------------------------------------------------------------------------

            // Caller Basics
            ParsingHelper.TryAssign(jObjectParsed, "arcade_caller_custom_campaign_name",
                ref customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "arcade_caller_name", ref callerName);
            ParsingHelper.TryAssign(jObjectParsed, "arcade_caller_transcript", ref callTranscript);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "arcade_caller_image_name", ref callerImage,
                jsonFolderPath, usermodFolderPath);

            ParsingHelper.TryAssign(jObjectParsed, "arcade_caller_callers_required", ref arcadeCallersRequired);

            // Caller Audio
            AudioParsingHelper.TryAssignAudioPath(jObjectParsed, "arcade_caller_audio_clip_name",
                ref callerClipPath, jsonFolderPath, usermodFolderPath, callerName);

            ParsingHelper.TryAssign(jObjectParsed, "arcade_caller_compress_audio", ref compressAudio);

            // Animated Portrait
            bool callerHasAnimatedPortrait = VideoParsingHelper.TryAssignVideoPath(jObjectParsed,
                "arcade_caller_animated_portrait_name",
                ref callerAnimatedPortraitURL, jsonFolderPath, usermodFolderPath);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "arcade_caller_animated_portrait_should_loop",
                ref callerAnimatedPortraitShouldLoop);

            // --------------------------------------------------------------------------------------------------------

            return new ArcadeCaller
            {
                CustomCampaignName = customCampaignName,

                CallerName = callerName,
                CallTranscript = callTranscript,
                CallerImage = callerImage,

                ArcadeCallersRequired = arcadeCallersRequired,

                CallerClipPath = callerClipPath,
                CompressAudio = compressAudio,

                CallerAnimatedPortraitURL = callerAnimatedPortraitURL,
                CallerHasAnimatedPortrait = callerHasAnimatedPortrait,
                CallerAnimatedPortraitShouldLoop = callerAnimatedPortraitShouldLoop
            };
        }
    }
}