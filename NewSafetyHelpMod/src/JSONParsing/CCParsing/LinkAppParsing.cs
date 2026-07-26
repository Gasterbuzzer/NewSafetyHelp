using System;
using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.CustomCampaignSystem.LinkApps;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class LinkAppParsing
    {
        /// <summary>
        /// Creates a link app from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed">JSON Parsed</param>
        /// <param name="usermodFolderPath">Filepath to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateLinkApp(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a link app. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            LinkApp customLinkApp = ParseLinkApp(ref jObjectParsed,
                ref customCampaignName, usermodFolderPath, jsonFolderPath);

            // Add to correct campaign.
            CustomCampaign customCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (customCampaign != null)
            {
                customCampaign.LinkApps.Add(customLinkApp);
            }
            else
            {
                LoggingHelper.DebugLog("Found link app before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignLinkApps.Add(customLinkApp);
            }
        }

        private static LinkApp ParseLinkApp(ref JObject jObjectParsed, ref string customCampaignName,
            string usermodFolderPath, string jsonFolderPath)
        {
            /*
             * General Properties
             */

            // URL that is opened on click
            Uri linkAppClickURL = null;

            int linkAppPriority = 0;

            VariableChanged<int> gameObjectOrder = new VariableChanged<int>
            {
                Data = 0
            };

            VariableChanged<string> desktopName = new VariableChanged<string>
            {
                Data = "No Name Provided"
            };

            VariableChanged<Sprite> desktopIcon = new VariableChanged<Sprite>
            {
                Data = null
            };

            /*
             * Requirements
             */

            // Unlock
            bool unlockWhenGameFinished = false;

            int unlockDay = 0;

            // For this link app to appear, it may require some callers to be correct or false.
            List<CallerRequirement> unlockRequiredCallers = null;

            List<GeneralAccuracyType> unlockAccuracy = null;

            // --------------------------------------------------------------------------------------------------------

            ParsingHelper.TryAssign(jObjectParsed, "link_app_custom_campaign_name", ref customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "link_app_unlock_day", ref unlockDay);

            ParsingHelper.TryAssign(jObjectParsed, "link_app_unlock_when_game_finished", ref unlockWhenGameFinished);

            ParsingHelper.TryAssign(jObjectParsed, "link_app_priority", ref linkAppPriority);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "link_app_desktop_position", ref gameObjectOrder);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "link_app_desktop_name", ref desktopName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "link_app_desktop_icon", ref desktopIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "link_app_desktop_name", ref desktopName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "link_app_desktop_icon", ref desktopIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            /*
             * Unlock Requirements
             */
            CallerRequirementParsingHelper.TryAssignCallerRequirement(jObjectParsed, ref unlockRequiredCallers,
                "link_app_caller_requirement_ids",
                "link_app_caller_requirement_should_be_correct");

            bool useOldAccuracyChecks = true;
            AccuracyParsingHelper.TryAssignListGeneralAccuracyType(jObjectParsed, ref unlockAccuracy,
                ref useOldAccuracyChecks, "link_app_required_accuracy",
                "link_app_accuracy_days", "link_app_accuracy_check_type");

            /*
             * URL
             */
            URLParsingHelper.TryAssignURL(jObjectParsed, "link_app_click_url", ref linkAppClickURL);

            return new LinkApp
            {
                CustomCampaignName = customCampaignName,

                LinkAppClickURL = linkAppClickURL,

                LinkAppPriority = linkAppPriority,
                GameObjectOrder = gameObjectOrder,

                DesktopName = desktopName,
                DesktopIcon = desktopIcon,

                UnlockWhenGameFinished = unlockWhenGameFinished,

                UnlockDay = unlockDay,
                UnlockAccuracy = unlockAccuracy,

                UnlockRequiredCallers = unlockRequiredCallers,
            };
        }
    }
}