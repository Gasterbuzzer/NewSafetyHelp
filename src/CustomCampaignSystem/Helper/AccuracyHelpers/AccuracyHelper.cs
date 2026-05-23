using System.Collections.Generic;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

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
        /// Gets the accuracy of a provided day.
        /// </summary>
        /// <param name="unlockDay">Day to check for.</param>
        /// <returns>(Float?) If found, will return the score of that day. If not, it will return null.</returns>
        public static float? GetAccuracyOfDay(int? unlockDay)
        {
            if (unlockDay <= 0)
            {
                LoggingHelper.WarningLog("Provided accuracy of cutscene" +
                                         $"Unlock day '{unlockDay}' is invalid.");
                return null;
            }

            if (CustomCampaignGlobal.InCustomCampaign)
            {
                CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (customCampaign == null)
                {
                    return null;
                }

                if (customCampaign.SavedDayScores.Count > unlockDay)
                {
                    return customCampaign.SavedDayScores[(int)unlockDay] / 100.0f;
                }
            }
            else
            {
                if (PlayerPrefs.HasKey("SavedDayScore" + unlockDay))
                {
                    return PlayerPrefs.GetFloat("SavedDayScore" + unlockDay) / 100.0f;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the given Accuracy day can even be checked (unlock day is valid).
        /// </summary>
        /// <param name="accuracyType">Accuracy element.</param>
        /// <param name="loggingCategory">Logging category for debugging.</param>
        /// <returns>(Bool) True: Day is valid. False: Day is not reached yet.</returns>
        public static bool CheckIfDayValid(GeneralAccuracyType accuracyType,
            LoggingHelper.LoggingCategory loggingCategory = LoggingHelper.LoggingCategory.CUTSCENE)
        {
            int? unlockDay = accuracyType.CheckDay;

            if (unlockDay <= 0
                || unlockDay <= GlobalVariables.currentDay)
            {
                return true;
            }

            LoggingHelper.DebugLog($"Checking cutscene accuracy day of '{unlockDay}' " +
                                   $"on day '{GlobalVariables.currentDay}'. " +
                                   $"(Accuracy type check day: '{accuracyType.CheckDay}')",
                loggingCategory);

            return false;
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
            LoggingHelper.DebugLog("Checking caller requirements...", loggingCategory);

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

                    int callerID = (int)callerRequirement.CallerID;

                    CustomCCaller customCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(callerID);

                    if (customCaller == null)
                    {
                        LoggingHelper.WarningLog(
                            "Provided object has a caller requirement that could not be checked. " +
                            "Caller does not exist. " +
                            "Unable to check. Hiding GameObject.");

                        return false;
                    }

                    // We haven't reached that point yet. (Not the correct day for example)
                    if (callerID >= customCampaign.SavedCallersCorrectAnswer.Count)
                    {
                        LoggingHelper.DebugLog("Caller requirement skipped, we have not reached the caller yet.",
                            loggingCategory);
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