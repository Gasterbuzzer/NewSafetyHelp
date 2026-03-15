using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignPatches.Helper;
using NewSafetyHelp.CustomCampaignPatches.Helper.AccuracyModel;
using NewSafetyHelp.Emails;
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

            // Image
            Sprite emailImage = null;
            
            string emailAnimatedVideo = null;

            // Unlock
            int emailUnlockDay = 0;
            
            float unlockThreshold = 0;
            
            int emailPriority = 0;
            
            // New Unlock System
            List<EmailAccuracyType> unlockAccuracy = null;
            bool useOldAccuracyChecks = true;

            ParsingHelper.TryAssign(jObjectParsed, "email_in_main_campaign", ref inMainCampaign);
            ParsingHelper.TryAssign(jObjectParsed, "email_custom_campaign_name", ref customCampaignName);
            ParsingHelper.TryAssign(jObjectParsed, "email_subject", ref emailSubject);
            ParsingHelper.TryAssign(jObjectParsed, "email_sender", ref emailSender);
            ParsingHelper.TryAssign(jObjectParsed, "email_body", ref emailBody);
            ParsingHelper.TryAssign(jObjectParsed, "email_unlock_day", ref emailUnlockDay);
            ParsingHelper.TryAssign(jObjectParsed, "email_priority", ref emailPriority);
            
            ParsingHelper.TryAssign(jObjectParsed, "email_unlock_threshold", ref unlockThreshold);
            
            ParsingHelper.TryAssignListEmailAccuracyType(jObjectParsed, ref unlockAccuracy, ref useOldAccuracyChecks);

            ParsingHelper.TryAssignSprite(jObjectParsed, "email_image", ref emailImage, jsonFolderPath,
                usermodFolderPath, customCampaignName);
            
            bool hasAnimatedVideo = ParsingHelper.TryAssignVideoPath(jObjectParsed, "email_animated_image",
                ref emailAnimatedVideo, jsonFolderPath, usermodFolderPath);

            return new CustomEmail
            {
                InMainCampaign = inMainCampaign,
                CustomCampaignName = customCampaignName,
                EmailSubject = emailSubject,
                SenderName = emailSender,
                EmailBody = emailBody,
                
                EmailPriority = emailPriority,

                UnlockDay = emailUnlockDay,
                UnlockThreshold = unlockThreshold,
                UnlockAccuracy = unlockAccuracy,
                UseOldAccuracyChecks = useOldAccuracyChecks,

                EmailImage = emailImage,
                
                EmailAnimatedVideo = emailAnimatedVideo,
                HasAnimatedVideo = hasAnimatedVideo
            };
        }
    }
}