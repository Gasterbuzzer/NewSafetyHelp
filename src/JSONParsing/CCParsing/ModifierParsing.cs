using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static NewSafetyHelp.CustomThemes.ColorHelper;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class ModifierParsing
    {
        /// <summary>
        /// Load a modifier from a JSON file.
        /// </summary>
        /// <param name="jObjectParsed"> JObject parsed. </param>
        /// <param name="usermodFolderPath">Path to JSON file.</param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateModifier(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                MelonLogger.Error("ERROR: Provided JSON could not be parsed as a modifier. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomModifier customModifier = ParseModifier(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign.CustomCampaignModel.CustomCampaign foundCustomCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (foundCustomCampaign != null)
            {
                if (customModifier.unlockDays == null)
                {
                    foundCustomCampaign.CustomModifiersGeneral.Add(customModifier);
                }
                else
                {
                    foundCustomCampaign.CustomModifiersDays.Add(customModifier);
                }
            }
            else
            {
                #if DEBUG
                MelonLogger.Msg("DEBUG: Found modifier file before the custom campaign was found / does not exist.");
                #endif

                GlobalParsingVariables.PendingCustomCampaignModifiers.Add(customModifier);
            }
        }

        private static CustomModifier ParseModifier(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            // When the modifier is unlocked. If null, it is a general modifier.
            List<int> unlockDays = null;

            /*
             * Desktop Settings
             */
            string username = string.Empty;

            // Main Program
            string renameMainGameDesktopIcon = "";
            Sprite mainGameDesktopIconSprite = null;

            // Backgrounds
            List<Sprite> backgroundSprites = new List<Sprite>();
            Sprite gameFinishedBackgroundSprite = null;
            bool disableGreenColorBackground = false;
            Color? desktopBackgroundColor = null;

            Sprite backgroundLogo = null;
            bool disableBackgroundLogo = false;
            float backgroundLogoTransparency = 0.2627f;
            
            // Icons
            Sprite mailBoxIcon = null; // Mail Box Icon on Desktop
            Sprite entryBrowserIcon = null; // Entry Browser Icon on Desktop
            Sprite optionsIcon = null; // Options Icon on Desktop
            Sprite artbookIcon = null; // Artbook Icon on Desktop
            Sprite arcadeIcon = null; // Arcade Icon on Desktop
            Sprite scorecardIcon = null; // Weekly Report Icon on Desktop

            // Credits
            string desktopCredits = null;
            Sprite creditsIcon = null; // Credits Icon on Desktop

            // Desktop settings
            bool entryBrowserActive = false;
            // If this setting was changed at all. Is used when checking.
            // If this is true and the "active" is false, it will disable the entry browser for example.
            bool entryBrowserChanged = false;

            bool scorecardActive = false;
            bool scorecardChanged = false; // See entryBrowserChanged for explanation.

            bool artbookActive = false;
            bool artbookChanged = false; // See entryBrowserChanged for explanation.

            bool arcadeActive = false;
            bool arcadeChanged = false; // See entryBrowserChanged for explanation.
            
            bool hideDiscordProgramChanged = false;
            bool hideDiscordProgram = false; // For those who want more immersion. Should not be recommended.

            // Day Strings
            List<string> dayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.

            /*
             * Modifier Parsing
             */

            ParsingHelper.TryAssign(jObjectParsed, "modifier_custom_campaign_attached",
                ref customCampaignName);

            if (jObjectParsed.TryGetValue("unlock_day", out JToken unlockDayValue))
            {
                if (unlockDayValue.Type == JTokenType.Integer)
                {
                    unlockDays = new List<int> { unlockDayValue.Value<int>() };
                }
                else if (unlockDayValue.Type == JTokenType.Array)
                {
                    unlockDays = new List<int>();

                    foreach (JToken unlockDayToken in (JArray)unlockDayValue)
                    {
                        unlockDays.Add(unlockDayToken.Value<int>());
                    }
                }
            }

            ParsingHelper.TryAssign(jObjectParsed, "desktop_username_text", ref username);

            ParsingHelper.TryAssign(jObjectParsed, "rename_main_game_desktop_icon",
                ref renameMainGameDesktopIcon);

            ParsingHelper.TryAssignSprite(jObjectParsed, "main_game_desktop_icon_path", ref mainGameDesktopIconSprite,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            if (jObjectParsed.TryGetValue("desktop_backgrounds", out JToken customCampaignDesktopBackgrounds))
            {
                JArray backgroundNames = (JArray)customCampaignDesktopBackgrounds;

                foreach (JToken backgroundName in backgroundNames)
                {
                    if (string.IsNullOrEmpty(backgroundName.Value<string>()))
                    {
                        MelonLogger.Error($"ERROR: Did not find '{backgroundName.Value<string>()}'." +
                                          " Adding no background.");
                        backgroundSprites.Add(null);
                    }
                    else
                    {
                        backgroundSprites.Add(
                            ImageImport.LoadImage(jsonFolderPath + "\\" + backgroundName.Value<string>(),
                                usermodFolderPath + "\\" + backgroundName.Value<string>()));
                    }
                }
            }
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "game_finished_desktop_background",
                ref gameFinishedBackgroundSprite, jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssign(jObjectParsed, "disable_green_color_on_desktop", ref disableGreenColorBackground);

            if (jObjectParsed.TryGetValue("desktop_background_color", out var _desktopBackgroundColor))
            {
                if (_desktopBackgroundColor.Type == JTokenType.Array)
                {
                    List<float> desktopBackgroundColorList = new List<float>();

                    foreach (JToken desktopBackgroundColorToken in (JArray)_desktopBackgroundColor)
                    {
                        desktopBackgroundColorList.Add(desktopBackgroundColorToken.Value<float>());
                    }

                    switch (desktopBackgroundColorList.Count)
                    {
                        case 3:
                            desktopBackgroundColor = new Color(GetConvertedColorFloat(desktopBackgroundColorList[0]),
                                GetConvertedColorFloat(desktopBackgroundColorList[1]),
                                GetConvertedColorFloat(desktopBackgroundColorList[2]));
                            break;

                        case 4:
                            desktopBackgroundColor = new Color(GetConvertedColorFloat(desktopBackgroundColorList[0]),
                                GetConvertedColorFloat(desktopBackgroundColorList[1]),
                                GetConvertedColorFloat(desktopBackgroundColorList[2]),
                                GetConvertedColorFloat(desktopBackgroundColorList[3]));
                            break;

                        default:
                            MelonLogger.Error("ERROR: " +
                                              "Provided color for desktop background is invalid! " +
                                              "Make sure its 3 or 4 values.");
                            break;
                    }
                }
            }

            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_logo_image_name", ref backgroundLogo,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "disable_desktop_logo", ref disableBackgroundLogo);
            ParsingHelper.TryAssign(jObjectParsed, "desktop_logo_transparency", ref backgroundLogoTransparency);
            ParsingHelper.TryAssign(jObjectParsed, "desktop_credits", ref desktopCredits);
            
            ParsingHelper.TryAssignWithBool(jObjectParsed, "hide_discord_program", ref hideDiscordProgram,
                ref hideDiscordProgramChanged);

            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_credits_image_name", ref creditsIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_mailbox_image_name", ref mailBoxIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_entry_browser_image_name", ref entryBrowserIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_options_image_name", ref optionsIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_artbook_image_name", ref artbookIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_arcade_image_name", ref arcadeIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignSprite(jObjectParsed, "desktop_scorecard_image_name", ref scorecardIcon,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            if (jObjectParsed.TryGetValue("campaign_day_names", out JToken customCampaignDaysNamesValue))
            {
                JArray customCampaignDays = (JArray)customCampaignDaysNamesValue;

                foreach (JToken campaignDay in customCampaignDays)
                {
                    dayTitleStrings.Add(campaignDay.Value<string>());
                }
            }

            ParsingHelper.TryAssignWithBool(jObjectParsed, "entry_browser_state", ref entryBrowserActive,
                ref entryBrowserChanged);
            
            ParsingHelper.TryAssignWithBool(jObjectParsed, "scorecard_state", ref scorecardActive,
                ref scorecardChanged);
            
            ParsingHelper.TryAssignWithBool(jObjectParsed, "artbook_state", ref artbookActive,
                ref artbookChanged);
            
            ParsingHelper.TryAssignWithBool(jObjectParsed, "arcade_state", ref arcadeActive,
                ref arcadeChanged);

            return new CustomModifier
            {
                customCampaignName = customCampaignName,

                unlockDays = unlockDays,

                usernameText = username,

                renameMainGameDesktopIcon = renameMainGameDesktopIcon,
                mainGameDesktopIcon = mainGameDesktopIconSprite,

                desktopBackgrounds = backgroundSprites,
                gameFinishedBackground = gameFinishedBackgroundSprite,
                disableColorBackground = disableGreenColorBackground,
                desktopBackgroundColor = desktopBackgroundColor,

                customBackgroundLogo = backgroundLogo,
                disableDesktopLogo = disableBackgroundLogo,
                backgroundLogoTransparency = backgroundLogoTransparency,
                
                HideDiscordProgramChanged = hideDiscordProgramChanged,
                HideDiscordProgram = hideDiscordProgram,
                
                MailBoxIcon = mailBoxIcon,
                EntryBrowserIcon = entryBrowserIcon,
                OptionsIcon = optionsIcon,
                ArtbookIcon = artbookIcon,
                ArcadeIcon = arcadeIcon,
                ScorecardIcon = scorecardIcon,

                DesktopCredits = desktopCredits,
                CreditsIcon = creditsIcon,

                dayTitleStrings = dayTitleStrings,

                entryBrowserActive = entryBrowserActive,
                entryBrowserChanged = entryBrowserChanged,

                scorecardActive = scorecardActive,
                scorecardChanged = scorecardChanged,

                artbookActive = artbookActive,
                artbookChanged = artbookChanged,

                arcadeActive = arcadeActive,
                arcadeChanged = arcadeChanged
            };
        }
    }
}