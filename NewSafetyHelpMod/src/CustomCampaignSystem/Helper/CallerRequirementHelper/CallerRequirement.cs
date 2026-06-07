namespace NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper
{
    public class CallerRequirement
    {
        /// <summary>
        /// The caller ID (order number) that is required for the object to be shown.
        /// (Null => Not set)
        /// </summary>
        public int? CallerID = null;

        /// <summary>
        /// If the caller to be checked requires to have been marked as correct.
        /// (Either dynamic or answer based caller)
        /// </summary>
        public bool ShouldCallerBeCorrect = false;
    }
}