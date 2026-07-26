using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CustomTextFiles;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class TextProgramParsing
    {
        /// <summary>
        /// Creates a text file program from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        public static void CreateTextProgram(JObject jObjectParsed, string usermodFolderPath = "")
        {
            // Invalid JSON.
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath))
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a video. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomTextFile customTextFile = ParseTextFileProgram(ref jObjectParsed, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign customCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (customCampaign != null)
            {
                customCampaign.CustomTextProgramFiles.Add(customTextFile);
            }
            else
            {
                LoggingHelper.DebugLog(
                    "Found Text file program before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignTextFile.Add(customTextFile);
            }
        }

        private static CustomTextFile ParseTextFileProgram(ref JObject jObjectParsed, ref string customCampaignName)
        {
            string fileNameOnDesktop = "file.txt";

            string textFileContents = "Empty";

            int unlockDay = 0;

            /*
             * Priority by which the text file appears in order.
             * Higher Priority => Gets shown first.
             */
            int orderPriority = 0;

            VariableChanged<int> gameObjectOrder = new VariableChanged<int>
            {
                Data = 0
            };

            // New Accuracy Settings
            bool ignoreAccuracyChecks = false;
            List<GeneralAccuracyType> unlockAccuracy = null;

            // For this text file to appear, it may require some callers to be correct or false.
            List<CallerRequirement> unlockRequiredCallers = null;

            // If the player requires to finish the game first.
            // NOTE: It also requires the check to be true.
            bool unlockWhenGameFinished = false;

            ParsingHelper.TryAssign(jObjectParsed, "text_file_desktop_name", ref fileNameOnDesktop);

            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_attached", ref customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "text_file_unlock_day", ref unlockDay);

            ParsingHelper.TryAssign(jObjectParsed, "text_file_order_priority", ref orderPriority);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "text_file_desktop_position",
                ref gameObjectOrder);

            ParsingHelper.TryAssign(jObjectParsed, "text_file_contents", ref textFileContents);

            AccuracyParsingHelper.TryAssignListGeneralAccuracyType(jObjectParsed, ref unlockAccuracy,
                ref ignoreAccuracyChecks,
                "text_file_required_accuracy", "text_file_accuracy_days",
                "text_file_accuracy_check_type");

            CallerRequirementParsingHelper.TryAssignCallerRequirement(jObjectParsed, ref unlockRequiredCallers,
                "text_file_caller_requirement_ids",
                "text_file_caller_requirement_should_be_correct");

            ParsingHelper.TryAssign(jObjectParsed, "text_file_unlock_when_game_finished", ref unlockWhenGameFinished);

            return new CustomTextFile
            {
                FileNameOnDesktop = fileNameOnDesktop,
                CustomCampaignName = customCampaignName,

                TextFileContents = textFileContents,

                UnlockDay = unlockDay,

                OrderPriority = orderPriority,
                GameObjectOrder = gameObjectOrder,

                UnlockAccuracy = unlockAccuracy,
                UnlockRequiredCallers = unlockRequiredCallers,

                UnlockWhenGameFinished = unlockWhenGameFinished
            };
        }
    }
}