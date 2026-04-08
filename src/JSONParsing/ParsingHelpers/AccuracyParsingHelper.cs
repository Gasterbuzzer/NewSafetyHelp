using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class AccuracyParsingHelper
    {
        /// <summary>
        /// Attempts to parse the check option provided.
        /// </summary>
        /// <param name="accuracyCheckTypeString">String describing the accuracy tape.</param>
        private static AccuracyHelper.CheckOptions TryAssignSingleAccuracyType(string accuracyCheckTypeString)
        {
            if (!string.IsNullOrEmpty(accuracyCheckTypeString))
            {
                switch (accuracyCheckTypeString.ToLowerInvariant())
                {
                    case "equal":
                    case "eq": // Equal
                        return AccuracyHelper.CheckOptions.EqualTo;

                    case "":
                    case "no":
                    case "n":
                    case "none": // None
                        return AccuracyHelper.CheckOptions.NoneSet;

                    case "greaterorequal":
                    case "geq": // Greater than or equal to
                        return AccuracyHelper.CheckOptions.GreaterThanOrEqualTo;

                    case "lesserorequal":
                    case "lessorequal":
                    case "leq": // Less than or equal to
                        return AccuracyHelper.CheckOptions.LessThanOrEqualTo;

                    case "nequal":
                    case "notequal":
                    case "!equal":
                    case "!eq":
                    case "noteq":
                    case "neq": // Not equal to
                        return AccuracyHelper.CheckOptions.NotEqualTo;

                    default:
                        LoggingHelper.WarningLog("Provided accuracy check type" +
                                                 $" '{accuracyCheckTypeString}' is not in any known format." +
                                                 " Please double check.");
                        return AccuracyHelper.CheckOptions.NoneSet;
                }
            }

            LoggingHelper.WarningLog("Unable of parsing accuracy check type. Possible syntax problem?");
            return AccuracyHelper.CheckOptions.NoneSet;
        }

        /*
         * Const strings for assign list. This ensures more consistency.
         */

        private const string AccuracyCheckTypeString = "accuracy_check_type";
        private const string AccuracyRequiredString = "accuracy_required";
        private const string TotalAccuracyString = "use_total_accuracy";

        /// <summary>
        /// Attempts to parse the check option provided.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="target">Targets to write the value to.</param>
        public static bool TryAssignListAccuracyType(JObject jObjectParsed, ref List<CallerAccuracyType> target)
        {
            if (!jObjectParsed.TryGetValue(AccuracyCheckTypeString, out _))
            {
                return false;
            }

            if (target == null)
            {
                target = new List<CallerAccuracyType>();
            }

            List<bool> isTotalAccuracyList = new List<bool>();
            bool? providedSingleValueTA = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, TotalAccuracyString,
                ref isTotalAccuracyList);

            List<float> accuracyRequiredList = new List<float>();
            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, AccuracyRequiredString, ref accuracyRequiredList);

            List<string> accuracyCheckType = new List<string>();
            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, AccuracyCheckTypeString, ref accuracyCheckType);

            // It means we have no elements, or we simply failed parsing any. 
            // The error printed by the helper function will inform the user what was the cause. 
            // So here we simply need to return.
            if (accuracyCheckType.Count < 1)
            {
                LoggingHelper.ErrorLog("Provided accuracy lists are empty or could not be parsed. " +
                                       "Unable of parsing accuracy checks.");
                return false;
            }

            if (accuracyRequiredList.Count != accuracyCheckType.Count)
            {
                LoggingHelper.ErrorLog("Provided accuracy lists must all have equal length. " +
                                       "Unable of parsing accuracy checks.");
                return false;
            }

            if (isTotalAccuracyList.Count > accuracyCheckType.Count)
            {
                LoggingHelper.ErrorLog("Provided list of total accuracy is larger than available accuracy checks. " +
                                       "Unable of parsing accuracy checks.");
                return false;
            }

            for (int i = 0; i < accuracyCheckType.Count; i++)
            {
                CallerAccuracyType newCallerAccuracyType = new CallerAccuracyType();

                if (!string.IsNullOrEmpty(accuracyCheckType[i]))
                {
                    newCallerAccuracyType.AccuracyCheck = TryAssignSingleAccuracyType(accuracyCheckType[i]);
                }
                else
                {
                    LoggingHelper.WarningLog("Provided accuracy type is invalid. Defaulting to 'none'.");
                }

                if (providedSingleValueTA != null)
                {
                    if ((bool)providedSingleValueTA && isTotalAccuracyList.Count > 0)
                    {
                        newCallerAccuracyType.UseTotalAccuracy = isTotalAccuracyList[0];
                    }
                    else if (i < isTotalAccuracyList.Count)
                    {
                        newCallerAccuracyType.UseTotalAccuracy = isTotalAccuracyList[i];
                    }
                }

                newCallerAccuracyType.RequiredAccuracy = accuracyRequiredList[i];

                target.Add(newCallerAccuracyType);
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the check option for a given general accuracy type list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="isUsingOldSystem">(Ref Bool) if we found at least one element for the new system.</param>
        /// <param name="requiredAccuracyKey">Key for the required accuracy list or element.</param>
        /// <param name="accuracyDaysKey">Key for the different days that the accuracy checks for.</param>
        /// <param name="accuracyCheckTypesKey">Key for the different check types.</param>
        public static void TryAssignListGeneralAccuracyType(JObject jObjectParsed, ref List<GeneralAccuracyType> target,
            ref bool isUsingOldSystem,
            string requiredAccuracyKey = "email_required_accuracy", string accuracyDaysKey = "email_accuracy_days", 
            string accuracyCheckTypesKey = "email_accuracy_check_type")
        {
            isUsingOldSystem = true;

            if (!jObjectParsed.TryGetValue(requiredAccuracyKey, out _))
            {
                return;
            }

            if (target == null)
            {
                target = new List<GeneralAccuracyType>();
            }

            List<int> differentAccuracyDays = new List<int>();
            bool? hasOnlyOneAccuracyDay = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, accuracyDaysKey,
                ref differentAccuracyDays);

            List<float> accuracyRequiredList = new List<float>();
            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, requiredAccuracyKey, ref accuracyRequiredList);

            List<string> accuracyCheckType = new List<string>();
            bool? hasOnlyOneAccuracyCheckType = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed,
                accuracyCheckTypesKey, ref accuracyCheckType);

            if (accuracyRequiredList.Count != accuracyCheckType.Count)
            {
                if (accuracyRequiredList.Count < accuracyCheckType.Count)
                {
                    LoggingHelper.ErrorLog("Provided accuracy lists must all should have equal length. " +
                                           "Unable of parsing accuracy checks.");
                    return;
                }

                if (accuracyCheckType.Count >= 1)
                {
                    LoggingHelper.WarningLog("Provided accuracy lists must all should have equal length. " +
                                             "Adding any missing accuracy checks with 'geq' (greater or equal).");
                }

                while (accuracyCheckType.Count < accuracyRequiredList.Count)
                {
                    accuracyCheckType.Add("geq");
                }
            }

            if (accuracyCheckType.Count > 0
                || accuracyCheckType.Count > 0
                || differentAccuracyDays.Count > 0)
            {
                isUsingOldSystem = false;
            }

            for (int i = 0; i < accuracyCheckType.Count; i++)
            {
                GeneralAccuracyType newAccuracyType = new GeneralAccuracyType();

                if (hasOnlyOneAccuracyCheckType != null)
                {
                    if ((bool)hasOnlyOneAccuracyCheckType 
                        && accuracyCheckType.Count > 0)
                    {
                        newAccuracyType.AccuracyCheck = TryAssignSingleAccuracyType(accuracyCheckType[0]);
                    }
                    else if (i < accuracyCheckType.Count)
                    {
                        if (!string.IsNullOrEmpty(accuracyCheckType[i]))
                        {
                            newAccuracyType.AccuracyCheck = TryAssignSingleAccuracyType(accuracyCheckType[i]);
                        }
                        else
                        {
                            LoggingHelper.WarningLog("Provided general accuracy type is invalid. " +
                                                     "Defaulting to 'greater or equal'.");
                            newAccuracyType.AccuracyCheck = AccuracyHelper.CheckOptions.GreaterThanOrEqualTo;
                        }
                    }
                }
                
                if (hasOnlyOneAccuracyDay != null)
                {
                    if ((bool)hasOnlyOneAccuracyDay && differentAccuracyDays.Count > 0)
                    {
                        newAccuracyType.CheckDay = differentAccuracyDays[0];
                    }
                    else if (i < differentAccuracyDays.Count)
                    {
                        newAccuracyType.CheckDay = differentAccuracyDays[i];
                    }
                }

                newAccuracyType.RequiredAccuracy = accuracyRequiredList[i];

                target.Add(newAccuracyType);
            }
        }
    }
}