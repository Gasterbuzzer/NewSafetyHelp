using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.LinkApps;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers
{
    public static class AccuracyLinkAppHelper
    {
        /// <summary>
        /// Checks if the given EmailAccuracyDay can even be checked (unlock day is valid).
        /// </summary>
        /// <returns>(Bool) True: Day is valid. False: Day is not reached yet.</returns>
        private static bool CheckIfDayValid(GeneralAccuracyType accuracyType, LinkApp linkApp)
        {
            int? unlockDay = accuracyType.CheckDay;

            if (accuracyType.CheckDay == null)
            {
                unlockDay = linkApp.UnlockDay - 1;
            }

            if (unlockDay <= 0
                || unlockDay <= GlobalVariables.currentDay)
            {
                return true;
            }

            LoggingHelper.DebugLog($"Checking accuracy day of '{unlockDay}' " +
                                   $"on day '{GlobalVariables.currentDay}'. " +
                                   $"(Accuracy type check day: '{accuracyType.CheckDay}')",
                LoggingHelper.LoggingCategory.LINK_APP);

            return false;
        }

        /// <summary>
        /// Gets the accuracy of a provided day.
        /// </summary>
        /// <param name="unlockDay">Day to check for.</param>
        /// <param name="linkApp">Link App to be checked. </param>
        /// <returns>(Float?) If found, will return the score of that day. If not, it will return null.</returns>
        private static float? GetAccuracyOfDay(int? unlockDay, LinkApp linkApp)
        {
            if (unlockDay == null)
            {
                unlockDay = linkApp.UnlockDay - 1;
            }

            if (unlockDay <= 0)
            {
                LoggingHelper.WarningLog("Unable of getting accuracy for any day that isn't the first. " +
                                         $"Unlock day '{unlockDay}' with unlock day of " +
                                         $"'{linkApp.UnlockDay}' is thus invalid.",
                    LoggingHelper.LoggingCategory.LINK_APP);
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
        /// Checks if all accuracy requirements are met.
        /// </summary>
        /// <param name="linkApp">Link app to be checked for.</param>
        /// <returns>(Bool) True: Passed all checks; False: Failed a check.</returns>
        private static bool CheckAllAccuracyRequirements(LinkApp linkApp)
        {
            // No accuracies given, we can say it will be shown, since no check exists.
            if (linkApp.UnlockAccuracy == null)
            {
                return true;
            }

            foreach (GeneralAccuracyType accuracyType in linkApp.UnlockAccuracy)
            {
                // If the day of to unlock is even reached.
                if (!CheckIfDayValid(accuracyType, linkApp))
                {
                    LoggingHelper.DebugLog(() => "Link App accuracy day not reached.",
                        LoggingHelper.LoggingCategory.LINK_APP);
                    return false;
                }

                float? currentAccuracyWithNull = GetAccuracyOfDay(accuracyType.CheckDay, linkApp);

                if (currentAccuracyWithNull == null)
                {
                    LoggingHelper.WarningLog("Unable of getting accuracy of a day. " +
                                             "Possible logic error? Not showing link app.");
                    return false;
                }

                // Valid accuracy.
                float currentAccuracy = (float)currentAccuracyWithNull;

                int? checkDay;
                if (accuracyType.CheckDay == null)
                {
                    checkDay = linkApp.UnlockDay - 1;
                }
                else
                {
                    checkDay = accuracyType.CheckDay;
                }

                LoggingHelper.DebugLog(() =>
                        $"The current accuracy is '{currentAccuracy}' of day '{checkDay}' " +
                        $"(Link App Unlock Day: '{linkApp.UnlockDay}') " +
                        $"with check type: '{accuracyType.AccuracyCheck.ToString()}'. " +
                        $"With required accuracy of '{accuracyType.RequiredAccuracy}'.",
                    LoggingHelper.LoggingCategory.LINK_APP);

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

            return true;
        }

        /// <summary>
        /// Checks if the provided custom link app has the accuracy to be allowed to be shown.
        /// </summary>
        /// <param name="linkApp">Link App to be checked.</param>
        /// <returns>(True) Passed all checks. (False) Failed a check.</returns>
        public static bool CheckIfLinkAppPassAccuracyChecks(LinkApp linkApp)
        {
            LoggingHelper.DebugLog($"Checking custom link app ('{linkApp.DesktopName.Data}') accuracy type.");

            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return true;
            }

            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                return false;
            }

            if (linkApp.UnlockDay > GlobalVariables.currentDay)
            {
                return false;
            }

            // If the link app is only allowed to be unlocked after the game has been finished, we check that first.
            if (linkApp.UnlockWhenGameFinished)
            {
                // Game has not been finished. So the checks fail.
                if (customCampaign.SavedGameFinished != 1
                    && customCampaign.SavedGameFinishedDisplay != 1)
                {
                    LoggingHelper.DebugLog("Link app will not be shown. Game has not been finished.",
                        LoggingHelper.LoggingCategory.LINK_APP);
                    return false;
                }
            }

            if (linkApp.UnlockRequiredCallers != null
                && linkApp.UnlockRequiredCallers.Count > 0)
            {
                if (!AccuracyHelper.CheckIfCallerRequirementsAreMet(linkApp.UnlockRequiredCallers,
                        LoggingHelper.LoggingCategory.LINK_APP))
                {
                    LoggingHelper.DebugLog("Link app will not be shown. A caller requirement was not met.",
                        LoggingHelper.LoggingCategory.LINK_APP);
                    return false;
                }
            }

            if (!CheckAllAccuracyRequirements(linkApp))
            {
                return false;
            }

            // No check failed, we return true.
            return true;
        }
    }
}