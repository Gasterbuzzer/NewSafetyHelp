using System.IO;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomVideos;
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
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a video. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomVideo customVideo = ParseVideo(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign.CustomCampaignModel.CustomCampaign foundCustomCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                foundCustomCampaign.AllDesktopVideos.Add(customVideo);
            }
            else
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found Video before the custom campaign was found / does not exist.");
                #endif

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

            ParsingHelper.TryAssign(jObjectParsed, "video_desktop_name", ref videoName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_campaign_attached", ref customCampaignName);
            ParsingHelper.TryAssign(jObjectParsed, "video_unlock_day", ref videoUnlockDay);

            if (jObjectParsed.TryGetValue("video_file_name", out var videoFileNameValue))
            {
                videoFilePath = jsonFolderPath + "\\" + (string)videoFileNameValue;
                string videoFileAlternativePath = usermodFolderPath + "\\" + (string)videoFileNameValue;

                if (string.IsNullOrEmpty(videoFilePath))
                {
                    MelonLogger.Warning("WARNING: Provided video path but name is empty. Unable to show show video.");
                }
                else if (!File.Exists(videoFilePath))
                {
                    if (!File.Exists(videoFileAlternativePath))
                    {
                        MelonLogger.Warning($"WARNING: Provided video {videoFilePath} does not exist.");
                    }
                    else
                    {
                        videoFilePath = videoFileAlternativePath;
                    }
                }
            }

            return new CustomVideo
            {
                desktopName = videoName,
                customCampaignName = customCampaignName,

                videoURL = videoFilePath,

                unlockDay = videoUnlockDay,
            };
        }
    }
}