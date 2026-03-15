using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignPatches.Helper.AccuracyModel;
using NewSafetyHelp.Emails;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignPatches.Helper.AccuracyHelpers
{
    public static class AccuracyEmailHelper
    {
        /// <summary>
        /// Checks if the given EmailAccuracyDay can even be checked (unlock day is valid).
        /// </summary>
        /// <returns>(Bool) True: Day is valid. False: Day is not reached yet.</returns>
        public static bool CheckIfDayValid(EmailAccuracyType accuracyType, CustomEmail email)
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
        public static float? GetAccuracyOfDay(int? unlockDay, CustomEmail email)
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
                    return customCampaign.SavedDayScores[(int) unlockDay] / 100.0f;
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
                float currentAccuracy = (float) currentAccuracyWithNull;
                
                LoggingHelper.DebugLog($"The current accuracy is '{currentAccuracy}' of day '{accuracyType.CheckDay}' " +
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