using NewSafetyHelp.CallerPatches.CallerModel;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Helper
{
    public static class AccuracyHelper
    {
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
        
        /// <summary>
        /// Computes the accuracy for the current day. (0-1 format)
        /// </summary>
        /// <returns></returns>
        public static float ComputeDayAccuracy()
        {
            return (float) GlobalVariables.callerControllerScript.correctCallsToday / GlobalVariables.callerControllerScript.callersToday;
        }

        /// <summary>
        /// Checks if the provided caller has to required accuracy to be allowed to be shown.
        /// </summary>
        /// <param name="currentCaller">Caller to be checked.</param>
        /// <param name="currentAccuracy">Accuracy to test against.</param>
        /// <returns>(True) Caller is allowed to call. (False) Caller is not allowed to call.</returns>
        public static bool CheckIfCallerIsToBeShown(CustomCCaller currentCaller, float currentAccuracy)
        {
            switch (currentCaller.AccuracyCheck)
            {
                case CheckOptions.EqualTo:
                    if (Mathf.Approximately(currentCaller.RequiredAccuracy, currentAccuracy))
                    {
                        return true;
                    }

                    break;

                case CheckOptions.GreaterThanOrEqualTo:
                    if (currentAccuracy >= currentCaller.RequiredAccuracy)
                    {
                        return true;
                    }

                    break;

                case CheckOptions.LessThanOrEqualTo:
                    if (currentAccuracy <= currentCaller.RequiredAccuracy)
                    {
                        return true;
                    }

                    break;

                case CheckOptions.NoneSet:
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Picks the correct accuracy from the caller settings.
        /// </summary>
        /// <param name="currentCaller">Current caller and it's chosen accuracy.</param>
        /// <returns>Accuracy that the caller chose.</returns>
        public static float GetCorrectAccuracy(CustomCCaller currentCaller)
        {
            if (currentCaller.UseTotalAccuracy)
            {
                return ComputeTotalCampaignAccuracy();
            }
            else
            {
                return ComputeDayAccuracy();
            }
        }
    }
}