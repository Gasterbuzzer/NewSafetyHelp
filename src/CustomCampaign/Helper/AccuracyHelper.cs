using MelonLoader;
using NewSafetyHelp.CallerPatches.CallerModel;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Helper
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
        private static float ComputeTotalCampaignAccuracy()
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
        private static float ComputeDayAccuracy()
        {
            return (float) GlobalVariables.callerControllerScript.correctCallsToday / GlobalVariables.callerControllerScript.callersToday;
        }

        /// <summary>
        /// Checks if the provided caller has to required accuracy to be allowed to be shown.
        /// </summary>
        /// <param name="currentCaller">Caller to be checked.</param>
        /// <returns>(True) Caller is allowed to call. (False) Caller is not allowed to call.</returns>
        public static bool CheckIfCallerIsToBeShown(CustomCCaller currentCaller)
        {
            foreach (AccuracyType accuracyType in currentCaller.AccuracyChecks)
            {
                float currentAccuracy = GetCorrectAccuracy(accuracyType.UseTotalAccuracy);
                
                #if DEBUG
                MelonLogger.Msg($"DEBUG: " +
                                $"Accuracy caller with current check '{accuracyType.AccuracyCheck.ToString()}' " +
                                $"and required accuracy '{accuracyType.RequiredAccuracy}'. " +
                                $"The current accuracy is: '{currentAccuracy}'.");
                #endif
                
                switch (accuracyType.AccuracyCheck)
                {
                    case CheckOptions.EqualTo:
                        if (!Mathf.Approximately(accuracyType.RequiredAccuracy, currentAccuracy))
                        {
                            return false;
                        }

                        break;

                    case CheckOptions.GreaterThanOrEqualTo:
                        if (!(currentAccuracy >= accuracyType.RequiredAccuracy))
                        {
                            return false;
                        }

                        break;

                    case CheckOptions.LessThanOrEqualTo:
                        if (!(currentAccuracy <= accuracyType.RequiredAccuracy))
                        {
                            return false;
                        }

                        break;
                    
                    case CheckOptions.NotEqualTo:
                        if (Mathf.Approximately(currentAccuracy, accuracyType.RequiredAccuracy))
                        {
                            return false;
                        }

                        break;

                    case CheckOptions.NoneSet:
                        break;
                }
            }
            
            #if DEBUG
            MelonLogger.Msg("DEBUG: Accuracy checks finished." +
                            $" All checks were true for caller '{currentCaller.CallerName}' " +
                            $"with '{currentCaller.AccuracyChecks.Count}' checks.");
            #endif
            
            // No check failed, we return true.
            return true;
        }

        /// <summary>
        /// Picks the correct accuracy that is needed.
        /// </summary>
        /// <param name="useTotalAccuracy">What accuracy to get.</param>
        /// <returns>Accuracy that the caller chose.</returns>
        public static float GetCorrectAccuracy(bool useTotalAccuracy)
        {
            if (useTotalAccuracy)
            {
                return ComputeTotalCampaignAccuracy();
            }
            
            return ComputeDayAccuracy();
        }
    }
}