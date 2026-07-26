using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen
{
    public class Computer3DScreen : CustomCampaignElementBase
    {
        /*
         * Properties
         */
        public bool InMainCampaign = false;

        /// <summary>
        /// Apply priority.
        /// The higher priority it will be applied first.
        /// </summary>
        public int ApplyPriority = 0;

        public VariableChanged<bool> SkipClickTime = new VariableChanged<bool>
        {
            Data = false
        };

        /*
         * Lights
         */

        public VariableChanged<Color> MainLightColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<bool> DisableMainLight = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<Color> SecondMainLightColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<bool> DisableSecondMainLight = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<Color> DeskLightColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<bool> DisableDeskLight = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<Color> KeyboardLightColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<bool> DisableKeyboardLight = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<Color> RightLightColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<bool> DisableRightLight = new VariableChanged<bool>
        {
            Data = false
        };

        /*
         * 3D Objects Settings
         */

        public VariableChanged<bool> DisableComputerScreen = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<bool> DisableKeyboard = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<bool> DisableTable = new VariableChanged<bool>
        {
            Data = false
        };

        /*
         * Camera Settings
         */

        public VariableChanged<Color> BackgroundColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<bool> DisablePostProcessing = new VariableChanged<bool>
        {
            Data = false
        };

        /*
         * Particle Settings
         */

        public VariableChanged<bool> DisableParticles = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<int> ParticleEmissionRate = new VariableChanged<int>
        {
            Data = 10
        };

        public VariableChanged<float> ParticleStartSize = new VariableChanged<float>
        {
            Data = 0.01f
        };

        public VariableChanged<Color> ParticleColor = new VariableChanged<Color>
        {
            Data = new Color()
        };

        public VariableChanged<Sprite> ParticleTexture = new VariableChanged<Sprite>
        {
            Data = null
        };

        /*
         * Title Settings
         */

        public VariableChanged<List<string>> TitleText = new VariableChanged<List<string>>
        {
            Data = null
        };

        public VariableChanged<Sprite> TitleLogo = new VariableChanged<Sprite>
        {
            Data = null
        };

        /*
         * 3D Screen Music
         */
        public string MusicPath = null;

        public RichAudioClip Music = null;

        public VariableChanged<bool> BringMusicCloser = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<bool> CenterMusic = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<float> MusicVolume = new VariableChanged<float>
        {
            Data = 0.07f
        };

        public VariableChanged<bool> DisableMusic = new VariableChanged<bool>
        {
            Data = false
        };

        /*
         * Special
         */

        public VariableChanged<bool> EnableBackgroundImage = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<Sprite> BackgroundImage = new VariableChanged<Sprite>
        {
            Data = null
        };
        
        public VariableChanged<bool> AddSun = new VariableChanged<bool>
        {
            Data = false
        };
    }
}