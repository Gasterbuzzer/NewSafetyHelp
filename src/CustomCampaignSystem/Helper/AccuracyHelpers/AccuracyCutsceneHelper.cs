using NewSafetyHelp.CustomCampaignSystem.CutsceneLogic;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers
{
    public static class AccuracyCutsceneHelper
    {
        /// <summary>
        /// Checks if all accuracy requirements are met.
        /// </summary>
        /// <param name="customCutscene">CustomCutscene to check for.</param>
        /// <returns>(Bool) True: Passed all checks; False: Failed a check.</returns>
        private static bool CheckAllAccuracyRequirements(CustomCutscene customCutscene)
        {
            // No accuracies given, we can say it will be shown, since no check exists.
            if (customCutscene.UnlockAccuracy == null)
            {
                return true;
            }

            LoggingHelper.DebugLog(() => "Checking specific accuracy requirement for a cutscene.",
                LoggingHelper.LoggingCategory.CUTSCENE);

            foreach (GeneralAccuracyType accuracyType in customCutscene.UnlockAccuracy)
            {
                // If the day of to unlock is even reached.
                if (!AccuracyHelper.CheckIfDayValid(accuracyType))
                {
                    LoggingHelper.DebugLog(() => "Accuracy day not reached.",
                        LoggingHelper.LoggingCategory.CUTSCENE);
                    return false;
                }

                float? currentAccuracyWithNull = AccuracyHelper.GetAccuracyOfDay(accuracyType.CheckDay);

                if (currentAccuracyWithNull == null)
                {
                    LoggingHelper.WarningLog("Unable of getting accuracy of a day. " +
                                             "Possible logic error?");
                    return false;
                }

                // Valid accuracy.
                float currentAccuracy = (float)currentAccuracyWithNull;

                int? checkDay;
                if (accuracyType.CheckDay != null)
                {
                    checkDay = accuracyType.CheckDay;
                }
                else
                {
                    LoggingHelper.ErrorLog("Provided custom cutscene check day is invalid! " +
                                           "Unable of checking requirement.");
                    return false;
                }
                
                
                LoggingHelper.DebugLog(() =>
                    $"The current accuracy is '{currentAccuracy}' of day '{checkDay}' " +
                    $"with check type: '{accuracyType.AccuracyCheck.ToString()}'. " +
                    $"With required accuracy of '{accuracyType.RequiredAccuracy}'.",
                    LoggingHelper.LoggingCategory.CUTSCENE);

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
        /// Checks if the provided custom cutscene has the accuracy to be allowed to be shown.
        /// </summary>
        /// <param name="customCutscene">Cutscene to be checked.</param>
        /// <returns>(True) Passed all checks. (False) Failed a check.</returns>
        public static bool CheckCutsceneAccuracy(CustomCutscene customCutscene)
        {
            LoggingHelper.DebugLog("Checking custom cutscene accuracy.",
                LoggingHelper.LoggingCategory.CUTSCENE);

            if (customCutscene.UnlockRequiredCallers != null
                && customCutscene.UnlockRequiredCallers.Count > 0)
            {
                if (!AccuracyHelper.CheckIfCallerRequirementsAreMet(customCutscene.UnlockRequiredCallers,
                        LoggingHelper.LoggingCategory.CUTSCENE))
                {
                    LoggingHelper.DebugLog("Cutscene will not be shown. The caller requirement was not met.",
                        LoggingHelper.LoggingCategory.CUTSCENE);
                    return false;
                }
            }

            if (!CheckAllAccuracyRequirements(customCutscene))
            {
                return false;
            }

            // No checks failed, so we return true.
            return true;
        }
    }
}