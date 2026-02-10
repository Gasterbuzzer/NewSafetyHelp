namespace NewSafetyHelp.CustomCampaignPatches.Helper
{
    public class AccuracyType
    {
        // If this caller is an accuracy caller, this is the required accuracy.
        public float RequiredAccuracy = -1; 
        
        // If this caller looks for the day accuracy (false) or if the global accuracy (true).
        public bool UseTotalAccuracy = false;
        
        // How it should be checked for.
        public AccuracyHelper.CheckOptions AccuracyCheck = AccuracyHelper.CheckOptions.NoneSet;
    }
}