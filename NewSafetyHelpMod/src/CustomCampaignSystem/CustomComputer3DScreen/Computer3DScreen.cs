using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen
{
    public class Computer3DScreen : CustomCampaignElementBase
    {
        public bool InMainCampaign = false;

        /// <summary>
        /// Apply priority.
        /// The higher priority it will be applied first.
        /// </summary>
        public int ApplyPriority = 0;
        
        public VariableChanged<Color> MainLightColor = new VariableChanged<Color>
        {
            Data = new Color()
        };
    }
}