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
    }
}