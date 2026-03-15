using NewSafetyHelp.CustomCampaignPatches.Helper.AccuracyHelpers;

namespace NewSafetyHelp.CustomCampaignPatches.Helper.AccuracyModel
{
    public class EmailAccuracyType
    {
        // Day when this accuracy to be checked with.
        public int? CheckDay = null;
        
        // The accuracy to check against.
        public float RequiredAccuracy = -1;
        
        // How it should be checked for.
        public AccuracyHelper.CheckOptions AccuracyCheck = AccuracyHelper.CheckOptions.GreaterThanOrEqualTo;
    }
}