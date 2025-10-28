using System.IO;
using MelonLoader;
using NewSafetyHelp.CustomVideos;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.CustomCampaignParsing
{
    public static class VideoParsing
    {
        public static CustomVideoExtraInfo ParseVideo(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string customCampaignName)
        {
            // Main
            string videoName = "";

            string videoFilePath = "";

            // Unlock
            int videoUnlockDay = 0;

            if (jObjectParsed.TryGetValue("video_desktop_name", out var videoDesktopNameValue))
            {
                videoName = (string) videoDesktopNameValue;
            }

            if (jObjectParsed.TryGetValue("custom_campaign_attached", out var customCampaignAttachedValue))
            {
                customCampaignName = (string) customCampaignAttachedValue;
            }

            if (jObjectParsed.TryGetValue("video_unlock_day", out var videoUnlockDayValue))
            {
                videoUnlockDay = (int) videoUnlockDayValue;
            }

            if (jObjectParsed.TryGetValue("video_file_name", out var videoFileNameValue))
            {
                videoFilePath = usermodFolderPath + "\\" + (string) videoFileNameValue;

                if (string.IsNullOrEmpty(videoFilePath))
                {
                    MelonLogger.Warning("WARNING: Provided video path but name is empty. Unable to show show video.");
                }
                else if (!File.Exists(videoFilePath))
                {
                    MelonLogger.Warning($"WARNING: Provided video {videoFilePath} does not exist.");
                }
            }
            
            return new CustomVideoExtraInfo
            {
                desktopName = videoName,
                customCampaignName = customCampaignName,

                videoURL = videoFilePath,

                unlockDay = videoUnlockDay,
            };
        }
    }
}