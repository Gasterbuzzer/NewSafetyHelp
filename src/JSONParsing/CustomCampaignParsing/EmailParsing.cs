using System.IO;
using MelonLoader;
using NewSafetyHelp.Emails;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CustomCampaignParsing
{
    public static class EmailParsing
    {
        public static EmailExtraInfo ParseEmail(ref JObject jObjectParsed, ref string usermodFolderPath,
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
                inMainCampaign = (bool) emailInMainCampaignValue;
            }

            if (jObjectParsed.TryGetValue("email_custom_campaign_name", out var emailCustomCampaignNameValue))
            {
                customCampaignName = (string) emailCustomCampaignNameValue;
            }

            if (jObjectParsed.TryGetValue("email_subject", out var emailSubjectValue))
            {
                emailSubject = (string) emailSubjectValue;
            }

            if (jObjectParsed.TryGetValue("email_sender", out var emailSenderValue))
            {
                emailSender = (string) emailSenderValue;
            }

            if (jObjectParsed.TryGetValue("email_body", out var emailBodyValue))
            {
                emailBody = (string) emailBodyValue;
            }

            if (jObjectParsed.TryGetValue("email_unlock_day", out var emailUnlockDayValue))
            {
                emailUnlockDay = (int) emailUnlockDayValue;
            }

            if (jObjectParsed.TryGetValue("email_unlock_threshold", out var emailUnlockThresholdValue))
            {
                emailUnlockThreshold = (int) emailUnlockThresholdValue;
            }

            if (jObjectParsed.TryGetValue("email_image", out var emailImageValue))
            {
                string emailImagePath = (string) emailImageValue;

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
            
            return new EmailExtraInfo
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