namespace NewSafetyHelp.CustomCampaignPatches.Helper.AccuracyHelpers
{
    public static class AccuracyHelper
    {
        public enum CheckOptions
        {
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,
            EqualTo,
            NotEqualTo,
            NoneSet
        }
        
        /// <summary>
        /// Computes the total campaign accuracy. (0-1 format)
        /// </summary>
        /// <returns></returns>
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
    }
}