using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class CallerRequirementParsingHelper
    {
        /// <summary>
        /// Attempts to parse the check option.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <param name="callerRequirementIDKey">Key to read the caller ID from.</param>
        /// <param name="callerCorrectnessKey">Key that describes if the caller is correct or wrong.</param>
        public static void TryAssignCallerRequirement(JObject jObjectParsed, ref List<CallerRequirement> target,
            string callerRequirementIDKey = "email_caller_requirement_ids",
            string callerCorrectnessKey = "email_caller_requirement_should_be_correct")
        {
            if (!jObjectParsed.TryGetValue(callerRequirementIDKey, out _))
            {
                return;
            }

            if (target == null)
            {
                target = new List<CallerRequirement>();
            }

            List<int> callerRequirementIDs = new List<int>();
            ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, callerRequirementIDKey, ref callerRequirementIDs);

            List<bool> callerCorrectness = new List<bool>();
            bool? singleCorrectness =
                ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, callerCorrectnessKey, ref callerCorrectness);

            if (callerCorrectness.Count > callerRequirementIDs.Count)
            {
                LoggingHelper.WarningLog("Too many caller correctness given, " +
                                         "the given caller requirement will not use all elements." +
                                         "If this is intentional, then no action is required.");
            }

            for (int i = 0; i < callerRequirementIDs.Count; i++)
            {
                CallerRequirement newCallerRequirement = new CallerRequirement
                {
                    CallerID = callerRequirementIDs[i]
                };

                if (singleCorrectness != null)
                {
                    if ((bool)singleCorrectness
                        && callerCorrectness.Count > 0)
                    {
                        newCallerRequirement.ShouldCallerBeCorrect = callerCorrectness[0];
                    }
                    else if (i < callerCorrectness.Count)
                    {
                        newCallerRequirement.ShouldCallerBeCorrect = callerCorrectness[i];
                    }
                }

                target.Add(newCallerRequirement);
            }
        }
    }
}