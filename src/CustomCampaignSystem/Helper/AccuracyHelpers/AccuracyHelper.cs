using System.Collections.Generic;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers
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

        /// <summary>
        /// Checks if the given caller requirements list are met.
        /// </summary>
        /// <param name="callerRequirements"> List of all caller requirements to check. </param>
        /// <param name="loggingCategory">Logging category to check.</param>
        /// <returns>(Bool) True: All requirements met. False: A requirement was not met.</returns>
        public static bool CheckIfCallerRequirementsAreMet(List<CallerRequirement> callerRequirements, 
            LoggingHelper.LoggingCategory loggingCategory = LoggingHelper.LoggingCategory.EMAIL)
        {
            if (CustomCampaignGlobal.InCustomCampaign)
            {
                CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (customCampaign == null)
                {
                    return false;
                }

                foreach (CallerRequirement callerRequirement in callerRequirements)
                {
                    if (callerRequirement.CallerID == null)
                    {
                        LoggingHelper.WarningLog("Provided caller requirement is invalid. " +
                                                 "Unable to check. Hiding GameObject.");
                        return false;
                    }

                    int callerID = (int) callerRequirement.CallerID;
                    
                    CustomCCaller customCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(callerID);

                    if (customCaller == null)
                    {
                        LoggingHelper.WarningLog(
                            "Provided object has a caller requirement that could not be checked. " +
                            "Caller does not exist. " +
                            "Unable to check. Hiding GameObject.");

                        return false;
                    }

                    if (callerID >= customCampaign.SavedCallersCorrectAnswer.Count)
                    {
                        LoggingHelper.WarningLog(
                            "Provided caller requirement can't be met. " +
                            "Possibly missing a caller in between? " +
                            "Unable to check. Hiding GameObject.");
                        return false;
                    }

                    bool isMarkedCorrect = customCampaign.SavedCallersCorrectAnswer[callerID];
                    
                    LoggingHelper.DebugLog(() => "Caller Requirement for caller " +
                                           $"ID: '{callerID}' with requirement " +
                                           $"'{callerRequirement.ShouldCallerBeCorrect}'." +
                                           $" Is that caller marked as correct? '{isMarkedCorrect}'. ", 
                        loggingCategory);
                    
                    if (callerRequirement.ShouldCallerBeCorrect != isMarkedCorrect)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}