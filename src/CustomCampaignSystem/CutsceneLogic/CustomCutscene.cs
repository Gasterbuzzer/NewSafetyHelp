using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;

namespace NewSafetyHelp.CustomCampaignSystem.CutsceneLogic
{
    public class CustomCutscene : CustomCampaignElementBase
    {
        [CanBeNull] public string CutsceneVideoPath = null;
        
        [CanBeNull] public List<GeneralAccuracyType> UnlockAccuracy = null;
        [CanBeNull] public List<CallerRequirement> UnlockRequiredCallers = null;
        
        /// <summary>
        /// The apply priority of the cutscene.
        /// Higher values mean that this cutscene is lower in the list and if lower, the cutscenes position in the list
        /// is lower. 
        /// This affects how the cutscene is chosen.
        /// Since a valid cutscene will be picked based on the first that matches all conditions given.
        /// </summary>
        public int ApplyPriority = 0;
    }
}