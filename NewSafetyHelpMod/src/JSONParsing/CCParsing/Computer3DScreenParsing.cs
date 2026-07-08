using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class Computer3DScreenParsing
    {
        /// <summary>
        /// Creates a 3D computer screen from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath">Path to the location of the JSON.</param>
        public static void Create3DComputerScreen(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            // Invalid JSON.
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath))
            {
                LoggingHelper.ErrorLog(
                    "Provided JSON could not be parsed as a 3D computer screen. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            Computer3DScreen custom3DScreen = Parse3DComputerScreen(ref jObjectParsed, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign customCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (customCampaign != null)
            {
                customCampaign.CustomComputer3DScreens.Add(custom3DScreen);
            }
            else
            {
                LoggingHelper.DebugLog(
                    "Found 3D computer screen before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaign3DComputerScreens.Add(custom3DScreen);
            }
        }

        private static Computer3DScreen Parse3DComputerScreen(ref JObject jObjectParsed, ref string customCampaignName)
        {
            // Properties
            bool inMainCampaign = false;

            int applyPriority = 0;

            // Lights
            VariableChanged<Color> mainLightColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            // Properties
            ParsingHelper.TryAssign(jObjectParsed, "computer_3D_screen_custom_campaign_attached",
                ref customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "computer_3D_screen_in_main_campaign", ref inMainCampaign);

            ParsingHelper.TryAssign(jObjectParsed, "computer_3D_screen_priority", ref applyPriority);

            // Lights
            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_main_light_color", ref mainLightColor);

            return new Computer3DScreen
            {
                CustomCampaignName = customCampaignName,
                InMainCampaign = inMainCampaign,
                ApplyPriority = applyPriority,
                
                MainLightColor = mainLightColor
            };
        }
    }
}