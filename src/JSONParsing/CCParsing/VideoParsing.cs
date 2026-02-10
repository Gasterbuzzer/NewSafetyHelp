using MelonLoader;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
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
            CustomCampaign foundCustomCampaign =
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

            ParsingHelper.TryAssignVideoPath(jObjectParsed, "video_file_name", ref videoFilePath,
                jsonFolderPath, usermodFolderPath);

            return new CustomVideo
            {
                DesktopName = videoName,
                CustomCampaignName = customCampaignName,

                VideoURL = videoFilePath,

                UnlockDay = videoUnlockDay,
            };
        }
    }
}