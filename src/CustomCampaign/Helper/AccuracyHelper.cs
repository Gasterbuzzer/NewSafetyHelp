namespace NewSafetyHelp.CustomCampaign.Helper
{
    public static class AccuracyHelper
    {
        public static float ComputeTotalCampaignAccuracy()
        {
            float correctCallers = 0;
            
            foreach (var caller in GlobalVariables.callerControllerScript.callers)
            {
                if (caller.answeredCorrectly)
                {
                    correctCallers++;
                }
            }

            return correctCallers / GlobalVariables.callerControllerScript.callers.Length;
        }
        
        public static float ComputeDayAccuracy()
        {
            return (float) ((float) GlobalVariables.callerControllerScript.correctCallsToday / GlobalVariables.callerControllerScript.callersToday * 100.0);
        }
    }
}