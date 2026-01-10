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

            if (jObjectParsed.TryGetValue("email_in_main_campaign", out var emailInMainCampaignValue))
            {
                inMainCampaign = (bool)emailInMainCampaignValue;
            }

            if (jObjectParsed.TryGetValue("email_custom_campaign_name", out var emailCustomCampaignNameValue))
            {
                customCampaignName = (string)emailCustomCampaignNameValue;
            }

            if (jObjectParsed.TryGetValue("email_subject", out var emailSubjectValue))
            {
                emailSubject = (string)emailSubjectValue;
            }

            if (jObjectParsed.TryGetValue("email_sender", out var emailSenderValue))
            {
                emailSender = (string)emailSenderValue;
            }

            if (jObjectParsed.TryGetValue("email_body", out var emailBodyValue))
            {
                emailBody = (string)emailBodyValue;
            }

            if (jObjectParsed.TryGetValue("email_unlock_day", out var emailUnlockDayValue))
            {
                emailUnlockDay = (int)emailUnlockDayValue;
            }

            if (jObjectParsed.TryGetValue("email_unlock_threshold", out var emailUnlockThresholdValue))
            {
                emailUnlockThreshold = (int)emailUnlockThresholdValue;
            }

            if (jObjectParsed.TryGetValue("email_image", out var emailImageValue))
            {
                string emailImagePath = (string)emailImageValue;

                if (!string.IsNullOrEmpty(emailImagePath))
                {
                    if (File.Exists(jsonFolderPath + "\\" + emailImagePath) ||
                        File.Exists(usermodFolderPath + "\\" + emailImagePath))
                    {
                        emailImage = ImageImport.LoadImage(jsonFolderPath + "\\" + emailImagePath,
                            usermodFolderPath + "\\" + emailImagePath);
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Email {emailImagePath} has image option provided but it could not be found! Not showing any image.");
                    }
                }
                else
                {
                    MelonLogger.Warning(
                        $"WARNING: Email at {jsonFolderPath} has image provided but it is empty! Not showing any image, if you don't want an image, do not use 'email_image'.");
                }
            }

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