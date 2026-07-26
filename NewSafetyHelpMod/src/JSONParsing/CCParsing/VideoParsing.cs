using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class VideoParsing
    {
        /// <summary>
        /// Creates a video program from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateVideo(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a video. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomVideo customVideo = ParseVideo(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign customCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (customCampaign != null)
            {
                customCampaign.CustomVideos.Add(customVideo);
            }
            else
            {
                LoggingHelper.DebugLog("Found Video before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignVideos.Add(customVideo);
            }
        }

        private static CustomVideo ParseVideo(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            // Main
            string videoName = "";

            string videoFilePath = "";

            // Unlock
            int videoUnlockDay = 0;

            /*
             * Priority by which this video gets to be shown first on the desktop.
             * The higher the priority the earlier it appears on the desktop.
             */
            int orderPriority = 0;

            VariableChanged<int> gameObjectOrder = new VariableChanged<int>
            {
                Data = 0
            };

            // New Accuracy Settings
            List<GeneralAccuracyType> unlockAccuracy = null;
            bool ignoreAccuracyChecks = true;

            // For this email to appear, it may require some callers to be correct or false.
            List<CallerRequirement> unlockRequiredCallers = null;

            bool unlockWhenGameFinished = false;

            ParsingHelper.TryAssign(jObjectParsed, "video_desktop_name", ref videoName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_attached", ref customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "video_unlock_day", ref videoUnlockDay);

            ParsingHelper.TryAssign(jObjectParsed, "video_order_priority", ref orderPriority);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "video_desktop_position", ref gameObjectOrder);

            VideoParsingHelper.TryAssignVideoPath(jObjectParsed, "video_file_name", ref videoFilePath,
                jsonFolderPath, usermodFolderPath);

            AccuracyParsingHelper.TryAssignListGeneralAccuracyType(jObjectParsed, ref unlockAccuracy,
                ref ignoreAccuracyChecks,
                "video_required_accuracy", "video_accuracy_days",
                "video_accuracy_check_type");

            CallerRequirementParsingHelper.TryAssignCallerRequirement(jObjectParsed, ref unlockRequiredCallers,
                "video_caller_requirement_ids",
                "video_caller_requirement_should_be_correct");

            ParsingHelper.TryAssign(jObjectParsed, "video_ignore_accuracy_checks", ref ignoreAccuracyChecks);

            ParsingHelper.TryAssign(jObjectParsed, "video_unlock_when_game_finished", ref unlockWhenGameFinished);

            return new CustomVideo
            {
                DesktopName = videoName,
                CustomCampaignName = customCampaignName,

                VideoURL = videoFilePath,

                UnlockDay = videoUnlockDay,

                OrderPriority = orderPriority,
                GameObjectOrder = gameObjectOrder,

                IgnoreAccuracyChecks = ignoreAccuracyChecks,

                UnlockAccuracy = unlockAccuracy,
                UnlockRequiredCallers = unlockRequiredCallers,

                UnlockWhenGameFinished = unlockWhenGameFinished
            };
        }
    }
}