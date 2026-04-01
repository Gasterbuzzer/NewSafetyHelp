using System;
using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.Emails;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class EmailParsing
    {
        /// <summary>
        /// Creates an email from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed">JSON Parsed</param>
        /// <param name="usermodFolderPath">Filepath to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateEmail(JObject jObjectParsed, string usermodFolderPath = "", string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a email. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";
            bool inMainCampaign = false;

            CustomEmail customEmail = ParseEmail(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath,
                ref customCampaignName, ref inMainCampaign);

            if (inMainCampaign)
            {
                GlobalParsingVariables.MainCampaignEmails.Add(customEmail);
            }
            else
            {
                // Add to correct campaign.
                CustomCampaign customCampaign =
                    CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.CampaignName == customCampaignName);

                if (customCampaign != null)
                {
                    customCampaign.Emails.Add(customEmail);
                }
                else
                {
                    LoggingHelper.DebugLog("Found Email before the custom campaign was found / does not exist.");

                    GlobalParsingVariables.PendingCustomCampaignEmails.Add(customEmail);
                }
            }
        }

        private static CustomEmail ParseEmail(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName, ref bool inMainCampaign)
        {
            // Main
            string emailSubject = "";
            string emailSender = "";
            string emailBody = "";

            // Url that is opened on click
            Uri emailClickUrl = null;
            
            // Image
            Sprite emailImage = null;
            
            string emailAnimatedVideo = null;

            // Unlock
            bool unlockWhenGameFinished = false;
            
            int emailUnlockDay = 0;
            
            float unlockThreshold = 0;
            
            int emailPriority = 0;
            
            // For this email to appear, it may require some callers to be correct or false.
            List<CallerRequirement> unlockRequiredCallers = null;
            
            // New Unlock System
            List<GeneralAccuracyType> unlockAccuracy = null;
            bool useOldAccuracyChecks = true;

            ParsingHelper.TryAssign(jObjectParsed, "email_in_main_campaign", ref inMainCampaign);
            ParsingHelper.TryAssign(jObjectParsed, "email_custom_campaign_name", ref customCampaignName);
            ParsingHelper.TryAssign(jObjectParsed, "email_subject", ref emailSubject);
            ParsingHelper.TryAssign(jObjectParsed, "email_sender", ref emailSender);
            ParsingHelper.TryAssign(jObjectParsed, "email_body", ref emailBody);
            ParsingHelper.TryAssign(jObjectParsed, "email_unlock_day", ref emailUnlockDay);
            ParsingHelper.TryAssign(jObjectParsed, "unlock_when_game_finished", ref unlockWhenGameFinished);
            ParsingHelper.TryAssign(jObjectParsed, "email_priority", ref emailPriority);
            
            CallerRequirementParsingHelper.TryAssignCallerRequirement(jObjectParsed, ref unlockRequiredCallers);
            
            ParsingHelper.TryAssign(jObjectParsed, "email_unlock_threshold", ref unlockThreshold);
            
            AccuracyParsingHelper.TryAssignListGeneralAccuracyType(jObjectParsed, ref unlockAccuracy, ref useOldAccuracyChecks);

            ImageParsingHelper.TryAssignSprite(jObjectParsed, "email_image", ref emailImage, jsonFolderPath,
                usermodFolderPath, customCampaignName);
            
            bool hasAnimatedVideo = VideoParsingHelper.TryAssignVideoPath(jObjectParsed, "email_animated_image",
                ref emailAnimatedVideo, jsonFolderPath, usermodFolderPath);
            
            URLParsingHelper.TryAssignURL(jObjectParsed, "email_click_url", ref emailClickUrl);

            return new CustomEmail
            {
                InMainCampaign = inMainCampaign,
                CustomCampaignName = customCampaignName,
                EmailSubject = emailSubject,
                SenderName = emailSender,
                EmailBody = emailBody,
                
                EmailClickURL = emailClickUrl,
                
                EmailPriority = emailPriority,
                
                UnlockWhenGameFinished = unlockWhenGameFinished,

                UnlockDay = emailUnlockDay,
                UnlockThreshold = unlockThreshold,
                UnlockAccuracy = unlockAccuracy,
                UseOldAccuracyChecks = useOldAccuracyChecks,
                
                UnlockRequiredCallers = unlockRequiredCallers,

                EmailImage = emailImage,
                
                EmailAnimatedVideo = emailAnimatedVideo,
                HasAnimatedVideo = hasAnimatedVideo
            };
        }
    }
}