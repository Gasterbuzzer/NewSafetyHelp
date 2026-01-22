using System.IO;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.Emails;
using NewSafetyHelp.ImportFiles;
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
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a email. Possible syntax mistake?");
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
                CustomCampaign.CustomCampaignModel.CustomCampaign foundCustomCampaign =
                    CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.CampaignName == customCampaignName);

                if (foundCustomCampaign != null)
                {
                    foundCustomCampaign.Emails.Add(customEmail);
                }
                else
                {
                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found Email before the custom campaign was found / does not exist.");
                    #endif

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

            // Unlock
            int emailUnlockDay = 0;
            int emailUnlockThreshold = 0;

            ParsingHelper.TryAssign(jObjectParsed, "email_in_main_campaign", ref inMainCampaign);
            ParsingHelper.TryAssign(jObjectParsed, "email_custom_campaign_name", ref customCampaignName);
            ParsingHelper.TryAssign(jObjectParsed, "email_subject", ref emailSubject);
            ParsingHelper.TryAssign(jObjectParsed, "email_sender", ref emailSender);
            ParsingHelper.TryAssign(jObjectParsed, "email_body", ref emailBody);
            ParsingHelper.TryAssign(jObjectParsed, "email_unlock_day", ref emailUnlockDay);
            ParsingHelper.TryAssign(jObjectParsed, "email_unlock_threshold", ref emailUnlockThreshold);

            ParsingHelper.TryAssignSprite(jObjectParsed, "email_image", ref emailImage, jsonFolderPath,
                usermodFolderPath, customCampaignName);

            return new CustomEmail
            {
                inMainCampaign = inMainCampaign,
                customCampaignName = customCampaignName,
                emailSubject = emailSubject,
                senderName = emailSender,
                emailBody = emailBody,

                unlockDay = emailUnlockDay,
                unlockThreshold = emailUnlockThreshold,

                emailImage = emailImage
            };
        }
    }
}