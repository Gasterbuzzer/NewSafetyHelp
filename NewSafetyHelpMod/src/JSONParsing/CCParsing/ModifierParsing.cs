using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
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

            /*
             * Cutscene Audio
             */
            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.FinalCutsceneAudioPath,
                clip =>
                {
                    customModifier.FinalCutsceneAudio.Data = clip;
                    customModifier.FinalCutsceneAudio.HasChanged = true;
                },
                jsonFolderPath, "final_cutscene_audio_name");

            /*
             * Timed Caller Audio
             */
            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.TimedCallerStartSoundPath,
                clip =>
                {
                    customModifier.TimedCallerStartSound.Data = clip;
                    customModifier.TimedCallerStartSound.HasChanged = true;
                },
                jsonFolderPath, "timed_caller_start_audio_name");

            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.TimedCallerHalfSoundPath,
                clip =>
                {
                    customModifier.TimedCallerHalfSound.Data = clip;
                    customModifier.TimedCallerHalfSound.HasChanged = true;
                },
                jsonFolderPath, "timed_caller_half_point_audio_name");

            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.TimedCallerCriticalSoundPath,
                clip =>
                {
                    customModifier.TimedCallerCriticalSound.Data = clip;
                    customModifier.TimedCallerCriticalSound.HasChanged = true;
                },
                jsonFolderPath, "timed_caller_critical_audio_name");

            /*
             * In Game Audio
             */

            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.ClockDayStartedAudioPath,
                clip =>
                {
                    customModifier.ClockDayStartedAudio.Data = clip;
                    customModifier.ClockDayStartedAudio.HasChanged = true;
                },
                jsonFolderPath, "in_game_clock_start_audio_name");

            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.DayStartedAudioPath,
                clip =>
                {
                    customModifier.DayStartedAudio.Data = clip;
                    customModifier.DayStartedAudio.HasChanged = true;
                },
                jsonFolderPath, "in_game_day_start_audio_name");

            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed, customModifier.InGameLogoFadeInAudioPath,
                clip =>
                {
                    customModifier.InGameLogoFadeInAudio.Data = clip;
                    customModifier.InGameLogoFadeInAudio.HasChanged = true;
                },
                jsonFolderPath, "in_game_logo_fade_in_sound");

            // Add to correct campaign.
            CustomCampaign customCampaign = CustomCampaignGlobal.GetNamedCustomCampaign(customCampaignName);

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
            /*
             * Modifier Conditions
             */
            List<int> unlockDays = null; // When the modifier is unlocked. If null, it is a general modifier.
            bool onlyIfGameFinished = false;

            /*
             * Username Section (Desktop username)
             */
            VariableChanged<string> username = new VariableChanged<string>
            {
                Data = string.Empty
            };

            /*
             * Date Section
             */

            VariableChanged<string> dateLabelOverwritten = new VariableChanged<string>
            {
                Data = "NO_DATE_SET"
            };

            /*
             * Background
             */
            List<Sprite> backgroundSprites = new List<Sprite>();

            VariableChanged<Sprite> gameFinishedBackgroundSprite = new VariableChanged<Sprite>();

            bool disableGreenColorBackground = false;
            Color? desktopBackgroundColor = null;

            // Animated Backgrounds
            List<string> animatedDesktopBackgrounds = new List<string>();
            bool blackBackgroundOnAnimatedBackground = false;

            VariableChanged<bool> animatedDesktopBackgroundShouldLoop = new VariableChanged<bool>
            {
                Data = true
            };

            /*
             * Desktop Logo
             */
            VariableChanged<Sprite> backgroundLogo = new VariableChanged<Sprite>();
            VariableChanged<bool> disableDesktopLogo = new VariableChanged<bool>
            {
                Data = false
            };
            VariableChanged<float> backgroundLogoTransparency = new VariableChanged<float>
            {
                Data = 0.2627f
            };

            /*
             * Programs on Desktop Section (Icons)
             */

            // Video Player
            VariableChanged<bool> videoPlayerDesktopIsWideMode = new VariableChanged<bool>
            {
                Data = false
            };

            // Main Program
            VariableChanged<string> renameMainGameDesktopIcon = new VariableChanged<string>
            {
                Data = string.Empty
            };
            VariableChanged<Sprite> mainGameDesktopIconSprite = new VariableChanged<Sprite>();

            // Mailbox Icon on Desktop
            VariableChanged<Sprite> mailboxIcon = new VariableChanged<Sprite>();
            VariableChanged<Sprite> applicationMailboxIcon = new VariableChanged<Sprite>();
            VariableChanged<string> mailboxRename = new VariableChanged<string>();
            VariableChanged<string> applicationMailboxTitle = new VariableChanged<string>();
            VariableChanged<bool> displayMailboxOnDesktop = new VariableChanged<bool>
            {
                Data = false
            };

            // Entry Browser Icon on Desktop
            VariableChanged<Sprite> entryBrowserIcon = new VariableChanged<Sprite>();
            VariableChanged<Sprite> applicationEntryBrowserIcon = new VariableChanged<Sprite>();
            VariableChanged<string> entryBrowserRename = new VariableChanged<string>();
            VariableChanged<string> applicationEntryBrowserTitle = new VariableChanged<string>();
            VariableChanged<bool> displayEntryBrowserOnDesktop = new VariableChanged<bool>
            {
                Data = false
            };

            // Options Icon on Desktop
            VariableChanged<Sprite> optionsIcon = new VariableChanged<Sprite>();
            VariableChanged<Sprite> applicationOptionsIcon = new VariableChanged<Sprite>();
            VariableChanged<string> optionsRename = new VariableChanged<string>();
            VariableChanged<string> applicationOptionsTitle = new VariableChanged<string>();

            // Artbook Icon on Desktop
            VariableChanged<Sprite> artbookIcon = new VariableChanged<Sprite>();
            VariableChanged<Sprite> applicationArtbookIcon = new VariableChanged<Sprite>();
            VariableChanged<string> artbookRename = new VariableChanged<string>();
            VariableChanged<string> applicationArtbookTitle = new VariableChanged<string>();
            List<ArtbookPage> artbookPages = new List<ArtbookPage>();
            VariableChanged<bool> displayArtbookOnDesktop = new VariableChanged<bool>
            {
                Data = false
            };

            // Arcade Icon on Desktop
            VariableChanged<Sprite> arcadeIcon = new VariableChanged<Sprite>();
            VariableChanged<string> arcadeRename = new VariableChanged<string>();
            VariableChanged<bool> displayArcadeOnDesktop = new VariableChanged<bool>
            {
                Data = false
            };

            // (Scorecard) Weekly Report Icon on Desktop
            VariableChanged<Sprite> scorecardIcon = new VariableChanged<Sprite>();
            VariableChanged<Sprite> applicationScorecardIcon = new VariableChanged<Sprite>();
            VariableChanged<string> scorecardRename = new VariableChanged<string>();
            VariableChanged<string> applicationScorecardTitle = new VariableChanged<string>();
            VariableChanged<bool> displayScorecardOnDesktop = new VariableChanged<bool>
            {
                Data = false
            };

            // Credits
            VariableChanged<string> desktopCredits = new VariableChanged<string>
            {
                Data = null
            };
            VariableChanged<string> creditsRename = new VariableChanged<string>();
            VariableChanged<bool> hideDesktopCredits = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Sprite> creditsIcon = new VariableChanged<Sprite>();

            // Not recommended to use, never use it. (Discord)
            // As such, it is not documented. 
            VariableChanged<bool> hideDiscordProgram = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * Special Desktop Options Section
             */
            List<string> dayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.

            /*
             * Final Cutscene
             */

            VariableChanged<bool> finalCutsceneFadeToBlack = new VariableChanged<bool>
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

            VariableChanged<bool> finalCutscenePreventClicks = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> finalCutsceneStopAudioAfterFade = new VariableChanged<bool>
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

            VariableChanged<RichAudioClip> finalCutsceneAudio = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<bool> finalCutsceneReturnTo3DScreen = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> finalCutsceneStayInCustomCampaign = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> finalCutsceneDisableSkippingKeys = new VariableChanged<bool>
            {
                Data = false
            };


            string finalCutsceneAudioPath = null;

            /*
             * Caller Section
             */

            VariableChanged<bool> disablePhoneStatic = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> useClockInsteadOfTimer = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<float> digitalClockTickRate = new VariableChanged<float>
            {
                Data = 1f
            };

            VariableChanged<float> analogClockTickRate = new VariableChanged<float>
            {
                Data = 0.05f
            };

            VariableChanged<string> timedCallerDisconnectedMessage = new VariableChanged<string>
            {
                Data = "TIMES UP!\nCALL DISCONNECTED"
            };

            VariableChanged<RichAudioClip> timedCallerStartSound = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<string> timedCallerStartSoundPath = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<RichAudioClip> timedCallerHalfSound = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<string> timedCallerHalfSoundPath = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<RichAudioClip> timedCallerCriticalSound = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<string> timedCallerCriticalSoundPath = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<Sprite> timedCallerBaseClock = new VariableChanged<Sprite>
            {
                Data = null
            };

            VariableChanged<Sprite> timedCallerClockHand = new VariableChanged<Sprite>
            {
                Data = null
            };

            VariableChanged<float> timedCallerProfileClockSize = new VariableChanged<float>
            {
                Data = 57.5f
            };

            VariableChanged<float> timedCallerProfileClockSizeMultiplier = new VariableChanged<float>
            {
                Data = 1f
            };

            VariableChanged<float> timedCallerProfileClockPadX = new VariableChanged<float>
            {
                Data = 5f
            };

            VariableChanged<float> timedCallerProfileClockPadY = new VariableChanged<float>
            {
                Data = 5f
            };

            /*
             * In Game Modifications
             */

            VariableChanged<Sprite> inGameProgramIcon = new VariableChanged<Sprite>
            {
                Data = null
            };

            VariableChanged<bool> inGameProgramIconCenter = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Sprite> inGamePhoneIcon = new VariableChanged<Sprite>
            {
                Data = null
            };

            VariableChanged<string> incomingCallTitle = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<string> incomingCallLabel = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<string> incomingCallAnswerButtonText = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<Sprite> incomingCallAnswerButtonImage = new VariableChanged<Sprite>
            {
                Data = null
            };

            VariableChanged<RichAudioClip> clockDayStartedAudio = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<string> clockDayStartedAudioPath = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<RichAudioClip> dayStartedAudio = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<string> dayStartedAudioPath = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<RichAudioClip> inGameLogoFadeInAudio = new VariableChanged<RichAudioClip>
            {
                Data = null
            };

            VariableChanged<string> inGameLogoFadeInAudioPath = new VariableChanged<string>
            {
                Data = null
            };

            VariableChanged<List<Sprite>> clockInLogoAnimation = new VariableChanged<List<Sprite>>
            {
                Data = new List<Sprite>()
            };

            VariableChanged<float> clockInLogoAnimationFadeDuration = new VariableChanged<float>
            {
                Data = 1.82f
            };

            VariableChanged<float> clockInLogoAnimationHoldDuration = new VariableChanged<float>
            {
                Data = 1.42f
            };

            /*
             * Cheats / Settings Section
             */

            // If to show the accuracy UI text string from the base game.
            VariableChanged<bool> showDefaultUIAccuracyText = new VariableChanged<bool>();

            // If to skip the initial desktop loading portion.
            bool disableDesktopLoading = false;

            VariableChanged<bool> selectPreviouslySelectedEntryInSubmitWindow = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * --------------------------------------------------------------------------------------------------------
             */

            /*
             * Modifier Conditions
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

            /*
             * Username
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_username_text", ref username);

            /*
             * Date Section
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "date_label_custom_text",
                ref dateLabelOverwritten);

            /*
             * Desktop Backgrounds
             */

            if (jObjectParsed.TryGetValue("desktop_backgrounds", out JToken customCampaignDesktopBackgrounds))
            {
                JArray backgroundNames = (JArray)customCampaignDesktopBackgrounds;

                foreach (JToken backgroundName in backgroundNames)
                {
                    if (string.IsNullOrEmpty(backgroundName.Value<string>()))
                    {
                        LoggingHelper.ErrorLog($"Did not find '{backgroundName.Value<string>()}'. " +
                                               "Adding no background.");
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

            VideoParsingHelper.TryAssignUrlListOrSingleUrl(jObjectParsed, "animated_desktop_backgrounds",
                ref animatedDesktopBackgrounds, jsonFolderPath, usermodFolderPath);

            ParsingHelper.TryAssign(jObjectParsed, "remove_background_with_animated_background",
                ref blackBackgroundOnAnimatedBackground);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "animated_desktop_background_should_loop",
                ref animatedDesktopBackgroundShouldLoop);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "game_finished_desktop_background",
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
                            LoggingHelper.ErrorLog("Provided color for desktop background is invalid! " +
                                                   "Make sure it's 3 or 4 values.");
                            break;
                    }
                }
            }

            /*
             * Desktop Logo
             */

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_logo_image_name",
                ref backgroundLogo, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_desktop_logo", ref disableDesktopLogo);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_logo_transparency",
                ref backgroundLogoTransparency);

            /*
             * Video player
             */
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "video_player_desktop_wide_mode",
                ref videoPlayerDesktopIsWideMode);

            /*
             * Main Game Desktop Program (Start Day Program)
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_main_game_desktop_icon",
                ref renameMainGameDesktopIcon);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "main_game_desktop_icon_path",
                ref mainGameDesktopIconSprite, jsonFolderPath, usermodFolderPath, customCampaignName);

            /*
             * Mailbox
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "show_mailbox_icon_on_desktop",
                ref displayMailboxOnDesktop);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_mailbox_image_name",
                ref mailboxIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_mailbox_program_image_name",
                ref applicationMailboxIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_mailbox_program",
                ref mailboxRename);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_mailbox_program_title",
                ref applicationMailboxTitle);

            /*
             * Entry Browser
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed,
                new List<string> { "entry_browser_state", "show_entry_browser_icon_on_desktop" },
                ref displayEntryBrowserOnDesktop);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_entry_browser_image_name",
                ref entryBrowserIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_entry_browser_program_image_name",
                ref applicationEntryBrowserIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_entry_browser_program",
                ref entryBrowserRename);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_entry_browser_program_title",
                ref applicationEntryBrowserTitle);

            /*
             * Options
             */

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_options_image_name",
                ref optionsIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_options_program_image_name",
                ref applicationOptionsIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_options_program",
                ref optionsRename);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_options_program_title",
                ref applicationOptionsTitle);

            /*
             * Artbook
             */


            ParsingHelper.TryAssignWithChangedBool(jObjectParsed,
                new List<string> { "artbook_state", "show_artbook_icon_on_desktop" },
                ref displayArtbookOnDesktop);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_artbook_image_name",
                ref artbookIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_artbook_program_image_name",
                ref applicationArtbookIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_artbook_program_title",
                ref applicationArtbookTitle);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_artbook_program",
                ref artbookRename);

            ArtbookParsingHelper.ParseArtbookPages(jObjectParsed, ref artbookPages, jsonFolderPath, usermodFolderPath);

            /*
             * Arcade
             */


            ParsingHelper.TryAssignWithChangedBool(jObjectParsed,
                new List<string> { "arcade_state", "show_arcade_icon_on_desktop" },
                ref displayArcadeOnDesktop);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_arcade_program",
                ref arcadeRename);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_arcade_image_name",
                ref arcadeIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            /*
             * Scorecard
             */

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_scorecard_image_name",
                ref scorecardIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_scorecard_program_image_name",
                ref applicationScorecardIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_scorecard_program",
                ref scorecardRename);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_scorecard_program_title",
                ref applicationScorecardTitle);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed,
                new List<string> { "scorecard_state", "show_scorecard_icon_on_desktop" },
                ref displayScorecardOnDesktop);

            /*
             * Credits
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "desktop_credits", ref desktopCredits);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "hide_desktop_credits", ref hideDesktopCredits);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "rename_credits_program",
                ref creditsRename);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "desktop_credits_image_name",
                ref creditsIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            /*
             * Not recommended
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "hide_discord_program", ref hideDiscordProgram);

            /*
             * Special Desktop Options Section
             */

            if (jObjectParsed.TryGetValue("campaign_day_names", out JToken customCampaignDaysNamesValue))
            {
                JArray customCampaignDays = (JArray)customCampaignDaysNamesValue;

                foreach (JToken campaignDay in customCampaignDays)
                {
                    dayTitleStrings.Add(campaignDay.Value<string>());
                }
            }

            /*
             * Final Cutscene
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_fade_to_black",
                ref finalCutsceneFadeToBlack);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_shake", ref finalCutsceneShake);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_glitch_sounds",
                ref finalCutsceneGlitchSounds);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_prevent_clicks",
                ref finalCutscenePreventClicks);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_fade_to_black_duration",
                ref finalCutsceneFadeDuration);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_extra_fade_to_black_duration",
                ref finalCutsceneFadePaddingDuration);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_stop_audio_after_fade",
                ref finalCutsceneStopAudioAfterFade);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_return_to_computer_start_menu",
                ref finalCutsceneReturnTo3DScreen);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_stay_in_custom_campaign",
                ref finalCutsceneStayInCustomCampaign);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "final_cutscene_disable_skipping",
                ref finalCutsceneDisableSkippingKeys);

            AudioParsingHelper.TryAssignAudioPath(jObjectParsed, "final_cutscene_audio_name",
                ref finalCutsceneAudioPath, jsonFolderPath, usermodFolderPath);

            /*
             * Caller Section
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_phone_static", ref disablePhoneStatic);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "use_clock_instead_of_timer",
                ref useClockInsteadOfTimer);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "caller_digital_clock_tick_rate",
                ref digitalClockTickRate);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "caller_analog_clock_tick_rate",
                ref analogClockTickRate);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "timed_caller_disconnect_message",
                ref timedCallerDisconnectedMessage);

            AudioParsingHelper.TryAssignAudioPathWithChangedBool(jObjectParsed, "timed_caller_start_audio_name",
                ref timedCallerStartSoundPath, jsonFolderPath, usermodFolderPath);

            AudioParsingHelper.TryAssignAudioPathWithChangedBool(jObjectParsed, "timed_caller_half_point_audio_name",
                ref timedCallerHalfSoundPath, jsonFolderPath, usermodFolderPath);

            AudioParsingHelper.TryAssignAudioPathWithChangedBool(jObjectParsed, "timed_caller_critical_audio_name",
                ref timedCallerCriticalSoundPath, jsonFolderPath, usermodFolderPath);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "timed_caller_base_clock_image_name",
                ref timedCallerBaseClock, jsonFolderPath, usermodFolderPath, customCampaignName);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "timed_caller_clock_hand_image_name",
                ref timedCallerClockHand, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "timed_caller_profile_clock_size",
                ref timedCallerProfileClockSize);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "timed_caller_profile_clock_size_multiplier",
                ref timedCallerProfileClockSizeMultiplier);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "timed_caller_profile_clock_vertical_spacing",
                ref timedCallerProfileClockPadY);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "timed_caller_profile_clock_horizontal_spacing",
                ref timedCallerProfileClockPadX);

            /*
             * In Game Modifications
             */

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "in_game_program_icon",
                ref inGameProgramIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "in_game_program_icon_center",
                ref inGameProgramIconCenter);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "in_game_phone_call_icon",
                ref inGamePhoneIcon, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "incoming_call_title",
                ref incomingCallTitle);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "incoming_call_label_text",
                ref incomingCallLabel);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "incoming_call_answer_button_text",
                ref incomingCallAnswerButtonText);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "incoming_call_answer_button_image",
                ref incomingCallAnswerButtonImage, jsonFolderPath, usermodFolderPath, customCampaignName);

            AudioParsingHelper.TryAssignAudioPathWithChangedBool(jObjectParsed, "in_game_clock_start_audio_name",
                ref clockDayStartedAudioPath, jsonFolderPath, usermodFolderPath);

            AudioParsingHelper.TryAssignAudioPathWithChangedBool(jObjectParsed, "in_game_day_start_audio_name",
                ref dayStartedAudioPath, jsonFolderPath, usermodFolderPath);

            AudioParsingHelper.TryAssignAudioPathWithChangedBool(jObjectParsed, "in_game_logo_fade_in_sound",
                ref inGameLogoFadeInAudioPath, jsonFolderPath, usermodFolderPath);
            
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "in_game_clock_in_logo_fade_duration",
                ref clockInLogoAnimationFadeDuration);
            
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "in_game_clock_in_logo_hold_duration",
                ref clockInLogoAnimationHoldDuration);

            ImageParsingHelper.TryAssignSpriteListOrSingleSpriteVariableChanged(jObjectParsed,
                "in_game_clock_in_logo_animation", ref clockInLogoAnimation, jsonFolderPath, usermodFolderPath);

            /*
             * Cheats / Settings
             */
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "show_accuracy_display",
                ref showDefaultUIAccuracyText);

            ParsingHelper.TryAssign(jObjectParsed, "skip_desktop_loading", ref disableDesktopLoading);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "submit_window_default_show_last_selected",
                ref selectPreviouslySelectedEntryInSubmitWindow);

            /*
             * Creating the modifier object.
             */
            return new CustomModifier
            {
                CustomCampaignName = customCampaignName,

                UnlockDays = unlockDays,
                OnlyIfGameFinished = onlyIfGameFinished,

                UsernameText = username,

                DateLabelOverwritten = dateLabelOverwritten,

                DesktopBackgrounds = backgroundSprites,
                GameFinishedBackground = gameFinishedBackgroundSprite,

                AnimatedDesktopBackgrounds = animatedDesktopBackgrounds,
                BlackBackgroundOnAnimatedBackground = blackBackgroundOnAnimatedBackground,
                AnimatedDesktopBackgroundShouldLoop = animatedDesktopBackgroundShouldLoop,

                DisableColorBackground = disableGreenColorBackground,
                DesktopBackgroundColor = desktopBackgroundColor,

                CustomBackgroundLogo = backgroundLogo,
                DisableDesktopLogo = disableDesktopLogo,
                BackgroundLogoTransparency = backgroundLogoTransparency,

                VideoPlayerDesktopIsWideMode = videoPlayerDesktopIsWideMode,

                RenameMainGameDesktopIcon = renameMainGameDesktopIcon,
                MainGameDesktopIcon = mainGameDesktopIconSprite,

                MailboxIcon = mailboxIcon,
                MailboxRename = mailboxRename,
                ApplicationMailboxTitle = applicationMailboxTitle,
                ApplicationMailboxIcon = applicationMailboxIcon,
                DisplayMailboxOnDesktop = displayMailboxOnDesktop,

                EntryBrowserIcon = entryBrowserIcon,
                EntryBrowserRename = entryBrowserRename,
                ApplicationEntryBrowserTitle = applicationEntryBrowserTitle,
                ApplicationEntryBrowserIcon = applicationEntryBrowserIcon,
                DisplayEntryBrowserOnDesktop = displayEntryBrowserOnDesktop,

                OptionsIcon = optionsIcon,
                OptionsRename = optionsRename,
                ApplicationOptionsTitle = applicationOptionsTitle,
                ApplicationOptionsIcon = applicationOptionsIcon,

                ArtbookIcon = artbookIcon,
                ArtbookRename = artbookRename,
                ArtbookPages = artbookPages,
                ApplicationArtbookTitle = applicationArtbookTitle,
                ApplicationArtbookIcon = applicationArtbookIcon,
                DisplayArtbookOnDesktop = displayArtbookOnDesktop,

                ArcadeIcon = arcadeIcon,
                ArcadeRename = arcadeRename,
                DisplayArcadeOnDesktop = displayArcadeOnDesktop,

                ScorecardIcon = scorecardIcon,
                ScorecardRename = scorecardRename,
                ApplicationScorecardTitle = applicationScorecardTitle,
                ApplicationScorecardIcon = applicationScorecardIcon,
                DisplayScorecardOnDesktop = displayScorecardOnDesktop,

                DesktopCredits = desktopCredits,
                CreditsRename = creditsRename,
                CreditsIcon = creditsIcon,
                HideDesktopCredits = hideDesktopCredits,

                HideDiscordProgram = hideDiscordProgram,

                DayTitleStrings = dayTitleStrings,

                FinalCutsceneFadeToBlack = finalCutsceneFadeToBlack,
                FinalCutsceneShake = finalCutsceneShake,
                FinalCutsceneGlitchSounds = finalCutsceneGlitchSounds,
                FinalCutscenePreventClicks = finalCutscenePreventClicks,
                FinalCutsceneFadeDuration = finalCutsceneFadeDuration,
                FinalCutsceneFadePaddingDuration = finalCutsceneFadePaddingDuration,
                FinalCutsceneAudioPath = finalCutsceneAudioPath,
                FinalCutsceneAudio = finalCutsceneAudio,
                FinalCutsceneStopAudioAfterFade = finalCutsceneStopAudioAfterFade,
                FinalCutsceneReturnTo3DScreen = finalCutsceneReturnTo3DScreen,
                FinalCutsceneStayInCustomCampaign = finalCutsceneStayInCustomCampaign,
                FinalCutsceneDisableSkippingKeys = finalCutsceneDisableSkippingKeys,

                DisablePhoneStatic = disablePhoneStatic,
                UseClockInsteadOfTimer = useClockInsteadOfTimer,
                DigitalClockTickRate = digitalClockTickRate,
                AnalogClockTickRate = analogClockTickRate,
                TimedCallerDisconnectedMessage = timedCallerDisconnectedMessage,
                TimedCallerStartSound = timedCallerStartSound,
                TimedCallerStartSoundPath = timedCallerStartSoundPath,
                TimedCallerHalfSound = timedCallerHalfSound,
                TimedCallerHalfSoundPath = timedCallerHalfSoundPath,
                TimedCallerCriticalSound = timedCallerCriticalSound,
                TimedCallerCriticalSoundPath = timedCallerCriticalSoundPath,
                TimedCallerBaseClock = timedCallerBaseClock,
                TimedCallerClockHand = timedCallerClockHand,
                TimedCallerProfileClockSize = timedCallerProfileClockSize,
                TimedCallerProfileClockSizeMultiplier = timedCallerProfileClockSizeMultiplier,
                TimedCallerProfileClockPadX = timedCallerProfileClockPadX,
                TimedCallerProfileClockPadY = timedCallerProfileClockPadY,

                InGameProgramIcon = inGameProgramIcon,
                InGameProgramIconCenter = inGameProgramIconCenter,
                InGamePhoneIcon = inGamePhoneIcon,
                ClockInLogoAnimation = clockInLogoAnimation,
                ClockInLogoAnimationFadeDuration = clockInLogoAnimationFadeDuration,
                ClockInLogoAnimationHoldDuration = clockInLogoAnimationHoldDuration,
                IncomingCallTitle = incomingCallTitle,
                IncomingCallLabel = incomingCallLabel,
                IncomingCallAnswerButtonText = incomingCallAnswerButtonText,
                IncomingCallAnswerButtonImage = incomingCallAnswerButtonImage,

                DayStartedAudio = dayStartedAudio,
                DayStartedAudioPath = dayStartedAudioPath,
                ClockDayStartedAudio = clockDayStartedAudio,
                ClockDayStartedAudioPath = clockDayStartedAudioPath,
                InGameLogoFadeInAudio = inGameLogoFadeInAudio,
                InGameLogoFadeInAudioPath = inGameLogoFadeInAudioPath,

                ShowDefaultUIAccuracyText = showDefaultUIAccuracyText,
                DisableDesktopLoading = disableDesktopLoading,
                SelectPreviouslySelectedEntryInSubmitWindow = selectPreviouslySelectedEntryInSubmitWindow
            };
        }
    }
}