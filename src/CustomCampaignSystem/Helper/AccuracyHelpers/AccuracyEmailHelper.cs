using System.Collections.Generic;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.Emails;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers
{
    public static class AccuracyEmailHelper
    {
        /// <summary>
        /// Checks if the given EmailAccuracyDay can even be checked (unlock day is valid).
        /// </summary>
        /// <returns>(Bool) True: Day is valid. False: Day is not reached yet.</returns>
        private static bool CheckIfDayValid(EmailAccuracyType accuracyType, CustomEmail email)
        {
            int? unlockDay = accuracyType.CheckDay;

            if (accuracyType.CheckDay == null)
            {
                unlockDay = email.UnlockDay - 1;
            }

            if (unlockDay <= 0
                || unlockDay <= GlobalVariables.currentDay)
            {
                return true;
            }

            LoggingHelper.DebugLog($"Checking accuracy day of '{unlockDay}' " +
                                   $"on day '{GlobalVariables.currentDay}'. " +
                                   $"(Accuracy type check day: '{accuracyType.CheckDay}')",
                LoggingHelper.LoggingCategory.EMAIL);

            return false;
        }

        /// <summary>
        /// Gets the accuracy of a provided day.
        /// </summary>
        /// <param name="unlockDay">Day to check for.</param>
        /// <param name="email"> Email to be checked. </param>
        /// <returns>(Float?) If found, will return the score of that day. If not, it will return null.</returns>
        private static float? GetAccuracyOfDay(int? unlockDay, CustomEmail email)
        {
            if (unlockDay == null)
            {
                unlockDay = email.UnlockDay - 1;
            }

            if (unlockDay <= 0)
            {
                LoggingHelper.WarningLog("Unable of getting accuracy for any day that isn't the first. " +
                                         $"Unlock day '{unlockDay}' with unlock day of " +
                                         $"'{email.UnlockDay}' is thus invalid.");
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
        /// Checks if the given caller requirements list are met.
        /// </summary>
        /// <param name="callerRequirements"> List of all caller requirements to check. </param>
        /// <returns>(Bool) True: All requirements met. False: A requirement was not met.</returns>
        private static bool CheckIfCallerRequirementsAreMet(List<CallerRequirement> callerRequirements)
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
                        LoggingHelper.WarningLog("Provided email has an invalid caller requirement. " +
                                                 "Unable to check. Hiding email.");
                        return false;
                    }

                    int callerID = (int) callerRequirement.CallerID;
                    
                    CustomCCaller customCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(callerID);

                    if (customCaller == null)
                    {
                        LoggingHelper.WarningLog(
                            "Provided email has a caller requirement that could not be checked. " +
                            "Caller does not exist. " +
                            "Unable to check. Hiding email.");

                        return false;
                    }

                    if (callerID >= customCampaign.SavedCallersCorrectAnswer.Count)
                    {
                        LoggingHelper.WarningLog(
                            "Provided caller requirement can't be met. " +
                            "Possibly missing a caller in between? " +
                            "Unable to check. Hiding email.");
                        return false;
                    }

                    bool isMarkedCorrect = customCampaign.SavedCallersCorrectAnswer[callerID];
                    
                    if (callerRequirement.ShouldCallerBeCorrect != isMarkedCorrect)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the provided email has the accuracy to be allowed to be shown.
        /// Please note, if you have an email that uses the old system, then don't use this function.
        /// </summary>
        /// <param name="email">Email to be checked.</param>
        /// <returns>(True) Passed all checks. (False) Failed a check.</returns>
        public static bool CheckIfEmailAccuracyType(CustomEmail email)
        {
            LoggingHelper.DebugLog("Checking email accuracy type.", LoggingHelper.LoggingCategory.EMAIL);

            // If the email is only allowed to be unlocked after the game has been finished, we check that first.
            if (email.UnlockWhenGameFinished)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return false;
                    }

                    // Game has not been finished. So the checks fail.
                    if (customCampaign.SavedGameFinished != 1
                        && customCampaign.SavedGameFinishedDisplay != 1)
                    {
                        LoggingHelper.DebugLog("Email will not be shown. Game has not been finished.",
                            LoggingHelper.LoggingCategory.EMAIL);
                        return false;
                    }
                }
            }

            if (email.UnlockRequiredCallers != null
                && email.UnlockRequiredCallers.Count > 0)
            {
                if (!CheckIfCallerRequirementsAreMet(email.UnlockRequiredCallers))
                {
                    LoggingHelper.DebugLog("Email will not be shown. A caller requirement was not met.",
                        LoggingHelper.LoggingCategory.EMAIL);
                    return false;
                }
            }

            // No accuracies given, we can say it will be shown, since no check exists.
            if (email.UnlockAccuracy == null)
            {
                return true;
            }

            foreach (EmailAccuracyType accuracyType in email.UnlockAccuracy)
            {
                // If the day of to unlock is even reached.
                if (!CheckIfDayValid(accuracyType, email))
                {
                    LoggingHelper.DebugLog("Accuracy day not reached.", LoggingHelper.LoggingCategory.EMAIL);
                    return false;
                }

                float? currentAccuracyWithNull = GetAccuracyOfDay(accuracyType.CheckDay, email);

                if (currentAccuracyWithNull == null)
                {
                    LoggingHelper.WarningLog("Unable of getting accuracy of a day. " +
                                             "Possible logic error? Not showing email.");
                    return false;
                }

                // Valid accuracy.
                float currentAccuracy = (float)currentAccuracyWithNull;

                LoggingHelper.DebugLog(
                    $"The current accuracy is '{currentAccuracy}' of day '{accuracyType.CheckDay}' " +
                    $"(Email Unlock Day: '{email.UnlockDay}') " +
                    $"with check type: '{accuracyType.AccuracyCheck.ToString()}'. " +
                    $"With required accuracy of '{accuracyType.RequiredAccuracy}'.",
                    LoggingHelper.LoggingCategory.EMAIL);

                // The switch statements all look for the opposite of the current statement,
                // since it only matters if we fail one of them and not if all check are true.
                switch (accuracyType.AccuracyCheck)
                {
                    case AccuracyHelper.CheckOptions.EqualTo:
                        if (!Mathf.Approximately(accuracyType.RequiredAccuracy, currentAccuracy))
                        {
                            return false;
                        }

                        break;

                    case AccuracyHelper.CheckOptions.GreaterThanOrEqualTo:
                        if (!(currentAccuracy >= accuracyType.RequiredAccuracy))
                        {
                            return false;
                        }

                        break;

                    case AccuracyHelper.CheckOptions.LessThanOrEqualTo:
                        if (!(currentAccuracy <= accuracyType.RequiredAccuracy))
                        {
                            return false;
                        }

                        break;

                    case AccuracyHelper.CheckOptions.NotEqualTo:
                        if (Mathf.Approximately(currentAccuracy, accuracyType.RequiredAccuracy))
                        {
                            return false;
                        }

                        break;

                    case AccuracyHelper.CheckOptions.NoneSet:
                        break;
                }
            }

            // No check failed, we return true.
            return true;
        }
    }
}