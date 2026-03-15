using System;
using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;

namespace NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel
{
    public class ModifierSource
    {
        /// <summary>
        /// The actual modifiers.
        /// </summary>
        public List<CustomModifier> Modifiers = new List<CustomModifier>();

        /// <summary>
        /// Name of modifier.
        /// </summary>
        public string ModifierName = "NO_MODIFIER_NAME_GIVEN";
        
        /// <summary>
        /// Condition for the modifier to be used. (Like if the finished game tag is enabled)
        /// </summary>
        public Func<CustomCampaign, bool> SourceCondition = _ => true;
        
        /// <summary>
        /// Condition for the modifier to be added into.
        /// (Like requiring the unlock days to be empty (if it is general or not))
        /// </summary>
        public Func<CustomModifier, bool> RequirementForAddingToModifierList = _ => true;
        
        /// <summary>
        /// Extra condition for the modifier to be valid under selection.
        /// That means that we append this condition to all the other conditions.
        /// (Like if a day modifier needs the current day to be included in the modifiers unlock days)
        /// </summary>
        public Func<CustomModifier, bool> ModifierExtraSelectionCondition = _ => true;
    }
}