using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignPatches.Helper
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
        /// Computes the accuracy for the current day. (0-1 format) (Only counts for entry based callers)
        /// </summary>
        /// <returns></returns>
        private static float ComputeDayAccuracy()
        {
            if (GlobalVariables.callerControllerScript.callersToday <= 0)
            {
                return 1; // 100% correct, since no callers have happened until now.
            }

            return (float)GlobalVariables.callerControllerScript.correctCallsToday /
                   GlobalVariables.callerControllerScript.callersToday;
        }
        
        // Variable for the total day accuracy.
        public static int StartOfDayCallerID = 0;
        
        /// <summary>
        /// Computes the accuracy for the current day. (0-1 format). (Counts all caller types)
        /// </summary>
        /// <returns></returns>
        private static float ComputeTotalDayAccuracy()
        {
            int amountOfCallersToday = 0;
            int amountOfCorrectCallersToday = 0;

            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                LoggingHelper.CampaignNullError();
                return 0;
            }
            
            for (int i = StartOfDayCallerID; i < customCampaign.CustomCallersInCampaign.Count; i++)
            {
                CustomCCaller customCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(i);

                if (customCaller == null)
                {
                    LoggingHelper.WarningLog($"Missing caller found for caller order {i}. Not counting this caller.");
                    continue;
                }
                
                amountOfCallersToday++;
                
                if (GlobalVariables.callerControllerScript.callers[i].answeredCorrectly)
                {
                    amountOfCorrectCallersToday++;
                }
                
                // Last caller of the day, we can stop.
                if (customCaller.LastDayCaller)
                {
                    break;
                }
            }

            if (amountOfCallersToday <= 0)
            {
                return 1;
            }
            
            return (float) amountOfCorrectCallersToday / amountOfCallersToday;
        }

        /// <summary>
        /// Checks if the provided caller has to required accuracy to be allowed to be shown.
        /// </summary>
        /// <param name="currentCaller">Caller to be checked.</param>
        /// <returns>(True) Caller is allowed to call. (False) Caller is not allowed to call.</returns>
        public static bool CheckIfCallerIsToBeShown(CustomCCaller currentCaller)
        {
            LoggingHelper.InfoLog(() => "Checking if accuracy caller is to be shown " +
                                  $"({currentCaller.CallerName} with '{currentCaller.AccuracyChecks.Count}' checks). " +
                                  $"Current day accuracy is '{GetCorrectAccuracy(false)}'. " +
                                  $"Total current day accuracy is '{GetCorrectAccuracy(false, true)}'. " +
                                  $"Total accuracy is '{GetCorrectAccuracy(true)}'.",
                LoggingHelper.LoggingCategory.SKIPPED_CALLER);
            
            foreach (AccuracyType accuracyType in currentCaller.AccuracyChecks)
            {
                float currentAccuracy = GetCorrectAccuracy(accuracyType.UseTotalAccuracy, 
                    currentCaller.CountEveryCallerForLocalAccuracy);
                
                LoggingHelper.DebugLog(() => "DEBUG: Found" +
                                       $"Accuracy caller with current check '{accuracyType.AccuracyCheck.ToString()}' " +
                                       $"and required accuracy '{accuracyType.RequiredAccuracy}'. " +
                                       $"The current accuracy is: '{currentAccuracy}'.",
                    LoggingHelper.LoggingCategory.SKIPPED_CALLER);
                
                // The switch statements all look for the opposite of the current statement,
                // since it only matters if we fail one of them and not if all check are true.
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
            
            // No check failed, we return true.
            return true;
        }

        /// <summary>
        /// Picks the correct accuracy that is needed.
        /// </summary>
        /// <param name="useTotalAccuracy">What accuracy to get.</param>
        /// <param name="useTotalDayAccuracy">If the day accuracy should account for every type of caller and
        /// not just entry based callers. </param>
        /// <returns>Accuracy that the caller chose.</returns>
        public static float GetCorrectAccuracy(bool useTotalAccuracy, bool useTotalDayAccuracy = false)
        {
            if (useTotalAccuracy)
            {
                return ComputeTotalCampaignAccuracy();
            }

            if (useTotalDayAccuracy)
            {
                return ComputeTotalDayAccuracy();  
            }
            else
            {
                return ComputeDayAccuracy();  
            }
        }
    }
}