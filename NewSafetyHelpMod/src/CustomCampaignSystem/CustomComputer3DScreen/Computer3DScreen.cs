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
    }
}