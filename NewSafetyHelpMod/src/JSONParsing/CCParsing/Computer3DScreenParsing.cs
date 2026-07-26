using System.Collections.Generic;
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

            Computer3DScreen custom3DScreen = Parse3DComputerScreen(ref jObjectParsed, ref customCampaignName,
                usermodFolderPath, jsonFolderPath);

            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed,
                custom3DScreen.MusicPath,
                clip => { custom3DScreen.Music = clip; },
                jsonFolderPath, "computer_3D_screen_music_clip_name");

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

        private static Computer3DScreen Parse3DComputerScreen(ref JObject jObjectParsed, ref string customCampaignName,
            string usermodFolderPath, string jsonFolderPath)
        {
            /*
             * Properties
             */
            bool inMainCampaign = false;

            int applyPriority = 0;

            VariableChanged<bool> skipClickTime = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * Lights
             */

            VariableChanged<Color> mainLightColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<bool> disableMainLight = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Color> secondMainLightColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<bool> disableSecondMainLight = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Color> deskLightColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<bool> disableDeskLight = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Color> keyboardLightColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<bool> disableKeyboardLight = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Color> rightLightColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<bool> disableRightLight = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * 3D Objects Settings
             */

            VariableChanged<bool> disableComputerScreen = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> disableKeyboard = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> disableTable = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * Camera Settings
             */

            VariableChanged<Color> backgroundColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<bool> disablePostProcessing = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * Particle Settings
             */

            VariableChanged<bool> disableParticles = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<int> particleEmissionRate = new VariableChanged<int>
            {
                Data = 10
            };

            VariableChanged<float> particleStartSize = new VariableChanged<float>
            {
                Data = 0.01f
            };

            VariableChanged<Color> particleColor = new VariableChanged<Color>
            {
                Data = new Color()
            };

            VariableChanged<Sprite> particleTexture = new VariableChanged<Sprite>
            {
                Data = null
            };

            /*
             * Title Settings
             */

            VariableChanged<List<string>> titleText = new VariableChanged<List<string>>
            {
                Data = null
            };

            VariableChanged<Sprite> titleLogo = new VariableChanged<Sprite>
            {
                Data = null
            };

            /*
             * 3D Screen Music
             */

            string musicPath = null;

            VariableChanged<bool> bringMusicCloser = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<bool> centerMusic = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<float> musicVolume = new VariableChanged<float>
            {
                Data = 0.07f
            };

            VariableChanged<bool> disableMusic = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * Special
             */

            VariableChanged<bool> enableBackgroundImage = new VariableChanged<bool>
            {
                Data = false
            };

            VariableChanged<Sprite> backgroundImage = new VariableChanged<Sprite>
            {
                Data = null
            };

            VariableChanged<bool> addSun = new VariableChanged<bool>
            {
                Data = false
            };

            /*
             * Properties
             */
            ParsingHelper.TryAssign(jObjectParsed, "computer_3D_screen_custom_campaign_attached",
                ref customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "computer_3D_screen_in_main_campaign", ref inMainCampaign);

            ParsingHelper.TryAssign(jObjectParsed, "computer_3D_screen_priority", ref applyPriority);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "computer_3D_screen_skip_click_wait_time",
                ref skipClickTime);

            /*
             * Lights
             */
            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_main_light_color", ref mainLightColor);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_main_light",
                ref disableMainLight);

            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_second_main_light_color",
                ref secondMainLightColor);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_second_main_light",
                ref disableSecondMainLight);

            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_desk_light_color",
                ref deskLightColor);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_desk_light",
                ref disableDeskLight);

            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_keyboard_light_color",
                ref keyboardLightColor);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_keyboard_light",
                ref disableKeyboardLight);

            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_right_light_color",
                ref rightLightColor);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_right_light",
                ref disableRightLight);

            /*
             * 3D Objects Settings
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_screen_object",
                ref disableComputerScreen);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_keyboard_object",
                ref disableKeyboard);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_table_object",
                ref disableTable);

            /*
             * Camera Settings
             */

            ColorParsingHelper.ParseColor(jObjectParsed, "computer_3D_screen_background_color",
                ref backgroundColor);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_post_processing",
                ref disablePostProcessing);

            /*
             * Particle Settings
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_particles", ref disableParticles);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "particle_emission_rate", ref particleEmissionRate);
            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "particle_start_size", ref particleStartSize);

            ColorParsingHelper.ParseColor(jObjectParsed, "particle_color", ref particleColor);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "particle_image_name",
                ref particleTexture, jsonFolderPath, usermodFolderPath, customCampaignName);

            /*
             * Title Settings
             */

            ParsingHelper.TryAssignListOrSingleElementVariableChanged(jObjectParsed, "title_text", ref titleText);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "title_logo", ref titleLogo,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            /*
             * 3D Screen Music
             */

            AudioParsingHelper.TryAssignAudioPath(jObjectParsed, "computer_3D_screen_music_clip_name",
                ref musicPath, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "computer_3D_screen_music_move_closer",
                ref bringMusicCloser);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "computer_3D_screen_center_music",
                ref centerMusic);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "computer_3D_screen_music_volume",
                ref musicVolume);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "disable_computer_3D_screen_music",
                ref disableMusic);

            /*
             * Special
             */

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "enable_computer_3D_screen_background_image",
                ref enableBackgroundImage);

            ImageParsingHelper.TryAssignSpriteChanged(jObjectParsed, "computer_3D_screen_background_image",
                ref backgroundImage, jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssignWithChangedBool(jObjectParsed, "computer_3D_screen_add_sun", ref addSun);

            // Creating the object
            return new Computer3DScreen
            {
                CustomCampaignName = customCampaignName,
                InMainCampaign = inMainCampaign,
                ApplyPriority = applyPriority,

                SkipClickTime = skipClickTime,

                MainLightColor = mainLightColor,
                DisableMainLight = disableMainLight,

                SecondMainLightColor = secondMainLightColor,
                DisableSecondMainLight = disableSecondMainLight,

                DeskLightColor = deskLightColor,
                DisableDeskLight = disableDeskLight,

                KeyboardLightColor = keyboardLightColor,
                DisableKeyboardLight = disableKeyboardLight,

                RightLightColor = rightLightColor,
                DisableRightLight = disableRightLight,

                DisableComputerScreen = disableComputerScreen,
                DisableKeyboard = disableKeyboard,
                DisableTable = disableTable,

                BackgroundColor = backgroundColor,
                DisablePostProcessing = disablePostProcessing,

                DisableParticles = disableParticles,
                ParticleEmissionRate = particleEmissionRate,
                ParticleStartSize = particleStartSize,
                ParticleColor = particleColor,
                ParticleTexture = particleTexture,

                TitleText = titleText,
                TitleLogo = titleLogo,

                MusicPath = musicPath,
                BringMusicCloser = bringMusicCloser,
                CenterMusic = centerMusic,
                MusicVolume = musicVolume,
                DisableMusic = disableMusic,

                EnableBackgroundImage = enableBackgroundImage,
                BackgroundImage = backgroundImage,
                AddSun = addSun
            };
        }
    }
}