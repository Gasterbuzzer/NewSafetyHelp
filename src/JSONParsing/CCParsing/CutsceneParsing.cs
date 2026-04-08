using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CutsceneLogic;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class CutsceneParsing
    {
        /// <summary>
        /// Load a music from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateCutscene(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a cutscene. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomCutscene customCutscene = ParseCutscene(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign customCampaign = CustomCampaignGlobal.GetNamedCustomCampaign(customCampaignName);

            if (customCampaign != null)
            {
                customCampaign.CustomCutscenes.Add(customCutscene);
            }
            else
            {
                LoggingHelper.DebugLog("Found custom cutscene before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignCutscenes.Add(customCutscene);
            }
        }

        private static CustomCutscene ParseCutscene(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            string cutsceneVideoPath = null;

            bool unlockAccuracyUsingOldSystem = false;
            List<GeneralAccuracyType> unlockAccuracy = null;
            
            List<CallerRequirement> unlockRequiredCallers = null;

            ParsingHelper.TryAssign(jObjectParsed, "cutscene_custom_campaign_name", ref customCampaignName);

            VideoParsingHelper.TryAssignVideoPath(jObjectParsed, "custom_cutscene_video_file",
                ref cutsceneVideoPath, jsonFolderPath, usermodFolderPath);

            AccuracyParsingHelper.TryAssignListGeneralAccuracyType(jObjectParsed, ref unlockAccuracy,
                ref unlockAccuracyUsingOldSystem, 
                "custom_cutscene_required_accuracy",
                "custom_cutscene_accuracy_days",
                "custom_cutscene_accuracy_check_type");
            
            CallerRequirementParsingHelper.TryAssignCallerRequirement(jObjectParsed, ref unlockRequiredCallers,
                "custom_cutscene_caller_requirement_ids",
                "custom_cutscene_caller_requirement_should_be_correct");

            return new CustomCutscene
            {
                CustomCampaignName = customCampaignName,

                CutsceneVideoPath = cutsceneVideoPath,

                UnlockAccuracy = unlockAccuracy,
                UnlockRequiredCallers = unlockRequiredCallers
            };
        }
    }
}