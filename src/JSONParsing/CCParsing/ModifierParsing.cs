using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;
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
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a modifier. Possible syntax mistake?");
                return;
            }

            // Campaign Values
            string customCampaignName = "";

            CustomModifier customModifier = ParseModifier(ref jObjectParsed, ref usermodFolderPath,
                ref jsonFolderPath, ref customCampaignName);

            // Add to correct campaign.
            CustomCampaign customCampaign =
                CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                    customCampaignSearch.CampaignName == customCampaignName);

            if (customCampaign != null)
            {
                if (customModifier.UnlockDays == null)
                {
                    if (!customModifier.OnlyIfGameFinished)
                    {
                        customCampaign.ModifierSources[0].Modifiers.Add(customModifier); // General
                    }
                    else
                    {
                        customCampaign.ModifierSources[1].Modifiers.Add(customModifier); // General Game Finished
                    }
                }
                else
                {
                    if (!customModifier.OnlyIfGameFinished)
                    {
                        customCampaign.ModifierSources[2].Modifiers.Add(customModifier); // Day
                    }
                    else
                    {
                        customCampaign.ModifierSources[3].Modifiers.Add(customModifier); // Day Game Finished
                    }
                }
            }
            else
            {
                LoggingHelper.DebugLog("Found modifier file before the custom campaign was found / does not exist.");

                GlobalParsingVariables.PendingCustomCampaignModifiers.Add(customModifier);
            }
        }

        /// <summary>
        /// Parses all keys for a modifier.
        /// </summary>
        /// <param name="jObjectParsed">Json Object.</param>
        /// <param name="usermodFolderPath">Usermod folder.</param>
        /// <param name="jsonFolderPath">Folder where the JSON is located at.</param>
        /// <param name="customCampaignName">Name of the custom campaign.</param>
        /// <returns>New CustomModifier</returns>
        private static CustomModifier ParseModifier(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName)
        {
            // When the modifier is unlocked. If null, it is a general modifier.
            List<int> unlockDays = null;
            
            bool onlyIfGameFinished = false;

            /*
             * Desktop Settings
             */
            string username = string.Empty;

            // Main Program
            string renameMainGameDesktopIcon = "";
            VariableChanged<Sprite> mainGameDesktopIconSprite = new VariableChanged<Sprite>();

            // Backgrounds
            List<Sprite> backgroundSprites = new List<Sprite>();
            
            VariableChanged<Sprite> gameFinishedBackgroundSprite = new VariableChanged<Sprite>();
            
            // If the final cutscene should fade to black.
            VariableChanged<bool> finalCutsceneFadeToBlack = new VariableChanged<bool>()
            {
                Data = true
            };
            
            VariableChanged<bool> finalCutsceneShake = new VariableChanged<bool>
            {
                Data = true
            };
            
            VariableChanged<bool> finalCutsceneGlitchSounds = new VariableChanged<bool>
            {
                Data = true
            };
            
            VariableChanged<float> finalCutsceneFadeDuration = new VariableChanged<float>
            {
                Data = 3f
            };
            
            VariableChanged<float> finalCutsceneFadePaddingDuration = new VariableChanged<float>
            {
                Data = 1f
            };
            
            bool disableGreenColorBackground = false;
            Color? desktopBackgroundColor = null;

            VariableChanged<Sprite> backgroundLogo = new VariableChanged<Sprite>();
            bool disableBackgroundLogo = false;
            float backgroundLogoTransparency = 0.2627f;

            // Animated Backgrounds
            List<string> animatedDesktopBackgrounds = new List<string>();
            bool blackBackgroundOnAnimatedBackground = false;

            // Icons
            // Mailbox Icon on Desktop
            VariableChanged<Sprite> mailBoxIcon = new VariableChanged<Sprite>();

            // Entry Browser Icon on Desktop
            VariableChanged<Sprite> entryBrowserIcon = new VariableChanged<Sprite>(); 
            
            // Options Icon on Desktop
            VariableChanged<Sprite> optionsIcon = new VariableChanged<Sprite>(); 
            
            // Artbook Icon on Desktop
            VariableChanged<Sprite> artbookIcon = new VariableChanged<Sprite>(); 
            
            // Arcade Icon on Desktop
            VariableChanged<Sprite> arcadeIcon = new VariableChanged<Sprite>(); 
            
            // Weekly Report Icon on Desktop
            VariableChanged<Sprite> scorecardIcon = new VariableChanged<Sprite>(); 

            // Credits
            string desktopCredits = null;
            
            VariableChanged<bool> hideDesktopCredits = new VariableChanged<bool>
            {
                Data = false
            };
            
            // Credits Icon on Desktop
            VariableChanged<Sprite> creditsIcon = new VariableChanged<Sprite>(); 

            // Desktop settings
            VariableChanged<bool> entryBrowserActive = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> scorecardActive = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> artbookActive = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> arcadeActive = new VariableChanged<bool>
            {
                Data = false
            };
            
            // Not recommended to use ever.
            VariableChanged<bool> hideDiscordProgram = new VariableChanged<bool>
            {
                Data = false
            }; 

            // Day Strings
            List<string> dayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.
            
            // Cheats
            // If to show the accuracy UI text string from the base game.
            VariableChanged<bool> showDefaultUIAccuracyText = new VariableChanged<bool>(); 
            bool disableDesktopLoading = false; // If to skip the initial desktop loading portion.

            /*
             * Modifier Parsing
             */

            ParsingHelper.TryAssign(jObjectParsed, "modifier_custom_campaign_attached",
                ref customCampaignName);
            
            ParsingHelper.TryAssign(jObjectParsed, "only_if_game_beaten", ref onlyIfGameFinished);

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

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "main_game_desktop_icon_path",
                ref mainGameDesktopIconSprite, jsonFolderPath, usermodFolderPath, customCampaignName);

            if (jObjectParsed.TryGetValue("desktop_backgrounds", out JToken customCampaignDesktopBackgrounds))
            {
                JArray backgroundNames = (JArray)customCampaignDesktopBackgrounds;

                foreach (JToken backgroundName in backgroundNames)
                {
                    if (string.IsNullOrEmpty(backgroundName.Value<string>()))
                    {
                        LoggingHelper.ErrorLog($"Did not find '{backgroundName.Value<string>()}'." +
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

            ParsingHelper.TryAssignUrlListOrSingleUrl(jObjectParsed, "animated_desktop_backgrounds",
                ref animatedDesktopBackgrounds, jsonFolderPath, usermodFolderPath);

            ParsingHelper.TryAssign(jObjectParsed, "remove_background_with_animated_background",
                ref blackBackgroundOnAnimatedBackground);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "game_finished_desktop_background",
                ref gameFinishedBackgroundSprite, jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_fade_to_black",
                ref finalCutsceneFadeToBlack);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_shake", ref finalCutsceneShake);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_glitch_sounds",
                ref finalCutsceneGlitchSounds);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_fade_to_black_duration",
                ref finalCutsceneFadeDuration);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_extra_fade_to_black_duration",
                ref finalCutsceneFadePaddingDuration);
            
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
                            LoggingHelper.ErrorLog("Provided color for desktop background is invalid! " +
                                                   "Make sure it's 3 or 4 values.");
                            break;
                    }
                }
            }

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_logo_image_name",
                ref backgroundLogo, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "disable_desktop_logo", ref disableBackgroundLogo);
            ParsingHelper.TryAssign(jObjectParsed, "desktop_logo_transparency", ref backgroundLogoTransparency);
            ParsingHelper.TryAssign(jObjectParsed, "desktop_credits", ref desktopCredits);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "hide_desktop_credits", ref hideDesktopCredits);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_credits_image_name",
                ref creditsIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_mailbox_image_name",
                ref mailBoxIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_entry_browser_image_name",
                ref entryBrowserIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_options_image_name",
                ref optionsIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_artbook_image_name",
                ref artbookIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_arcade_image_name",
                ref arcadeIcon, jsonFolderPath, usermodFolderPath, customCampaignName);
            
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "hide_discord_program", ref hideDiscordProgram);

            ParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_scorecard_image_name",
                ref scorecardIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            if (jObjectParsed.TryGetValue("campaign_day_names", out JToken customCampaignDaysNamesValue))
            {
                JArray customCampaignDays = (JArray)customCampaignDaysNamesValue;

                foreach (JToken campaignDay in customCampaignDays)
                {
                    dayTitleStrings.Add(campaignDay.Value<string>());
                }
            }

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "entry_browser_state", ref entryBrowserActive);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "scorecard_state", ref scorecardActive);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "artbook_state", ref artbookActive);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "arcade_state", ref arcadeActive);
            
            // Cheats
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "show_accuracy_display",
                ref showDefaultUIAccuracyText);
            
            ParsingHelper.TryAssign(jObjectParsed, "skip_desktop_loading", ref disableDesktopLoading);
            
            return new CustomModifier
            {
                CustomCampaignName = customCampaignName,

                UnlockDays = unlockDays,
                
                OnlyIfGameFinished = onlyIfGameFinished,

                UsernameText = username,

                RenameMainGameDesktopIcon = renameMainGameDesktopIcon,
                MainGameDesktopIcon = mainGameDesktopIconSprite,
                
                DesktopBackgrounds = backgroundSprites,
                
                GameFinishedBackground = gameFinishedBackgroundSprite,
                
                FinalCutsceneFadeToBlack = finalCutsceneFadeToBlack,
                FinalCutsceneShake = finalCutsceneShake,
                FinalCutsceneGlitchSounds = finalCutsceneGlitchSounds,
                FinalCutsceneFadeDuration = finalCutsceneFadeDuration,
                FinalCutsceneFadePaddingDuration = finalCutsceneFadePaddingDuration,
                
                DisableColorBackground = disableGreenColorBackground,
                DesktopBackgroundColor = desktopBackgroundColor,
                
                CustomBackgroundLogo = backgroundLogo,

                DisableDesktopLogo = disableBackgroundLogo,
                BackgroundLogoTransparency = backgroundLogoTransparency,

                AnimatedDesktopBackgrounds = animatedDesktopBackgrounds,
                BlackBackgroundOnAnimatedBackground = blackBackgroundOnAnimatedBackground,
                
                HideDiscordProgram = hideDiscordProgram,

                MailBoxIcon = mailBoxIcon,
                
                EntryBrowserIcon = entryBrowserIcon,
                
                OptionsIcon = optionsIcon,
                
                ArtbookIcon = artbookIcon,
                
                ArcadeIcon = arcadeIcon,
                
                ScorecardIcon = scorecardIcon,

                DesktopCredits = desktopCredits,
                CreditsIcon = creditsIcon,
                HideDesktopCredits = hideDesktopCredits,

                DayTitleStrings = dayTitleStrings,

                EntryBrowserActive = entryBrowserActive,

                ScorecardActive = scorecardActive,

                ArtbookActive = artbookActive,

                ArcadeActive = arcadeActive,
                
                ShowDefaultUIAccuracyText = showDefaultUIAccuracyText,
                
                DisableDesktopLoading = disableDesktopLoading
            };
        }
    }
}