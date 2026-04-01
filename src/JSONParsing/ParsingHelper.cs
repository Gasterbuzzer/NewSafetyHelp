using System;
using System.Collections.Generic;
using System.Linq;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing
{
    public static class ParsingHelper
    {
        /// <summary>
        /// Checks if the JSON object contains any of the keys.
        /// </summary>
        /// <param name="keys">List of keys to check </param>
        /// <param name="json">JObject with the keys</param>
        /// <returns></returns>
        public static bool ContainsKeys(List<string> keys, JObject json)
        {
            return keys.Any(json.ContainsKey); // Checks if any of the keys is in the JSON via the flag ContainsKey
        }

        /// <summary>
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssign<T>(JObject jObjectParsed, string key, ref T target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            target = token.Value<T>();
        }

        /// <summary>
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// This version takes in a bool that updates to "true" if updated.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <param name="wasAssigned"> If the value was assigned. This is used in some parsing to allow both
        /// true and false and default values.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssignWithBool<T>(JObject jObjectParsed, string key, ref T target, ref bool wasAssigned)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                wasAssigned = false;
                return;
            }

            wasAssigned = true;
            target = token.Value<T>();
        }
        
        /// <summary>
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// This version takes in a bool that updates to "true" if updated.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssignWithChangedBool<T>(JObject jObjectParsed, string key, ref VariableChanged<T> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                target.HasChanged = false;
                return;
            }

            target.HasChanged = true;
            target.Data = token.Value<T>();
        }

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
            TryAssignListOrSingleElement(jObjectParsed, callerRequirementIDKey, ref callerRequirementIDs);

            List<bool> callerCorrectness = new List<bool>();
            bool? singleCorrectness =
                TryAssignListOrSingleElement(jObjectParsed, callerCorrectnessKey, ref callerCorrectness);

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

        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        /// <returns>(Bool) If the parsed value was an array (false) or a single element (true).
        /// Null if we failed parsing.</returns>
        public static bool? TryAssignListOrSingleElement<T>(JObject jObjectParsed, string key, ref List<T> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return null;
            }

            if (target == null)
            {
                target = new List<T>();
            }

            if (token.Type == JTokenType.Array)
            {
                foreach (JToken element in token)
                {
                    T value = element.Value<T>();
                    target.Add(value);
                }

                return false;
            }
            else
            {
                try
                {
                    T value = token.Value<T>();
                    target.Add(value);

                    return true;
                }
                catch
                {
                    LoggingHelper.ErrorLog($"For provided key '{key}' " +
                                           "we were unable of assigning any value, as the wrong value was given.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Attempts to parse the key for a list.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Targets to write the value to.</param>
        public static void TryAssignList<T>(JObject jObjectParsed, string key, ref List<T> target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return;
            }

            if (target == null)
            {
                target = new List<T>();
            }

            if (token.Type == JTokenType.Array)
            {
                foreach (JToken element in token)
                {
                    T value = element.Value<T>();
                    target.Add(value);
                }
            }
            else
            {
                LoggingHelper.ErrorLog($"Provided key '{key}' does not contain a list.");
            }
        }

        /// <summary>
        /// Assigns a URL to be clicked on an email attachment.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="key">Key to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        public static bool TryAssignURL(JObject jObjectParsed, string key, ref Uri target)
        {
            if (!jObjectParsed.TryGetValue(key, out var token))
            {
                return false;
            }

            string givenStringURL = token.Value<string>();

            if (!URLVerification.SetEmailClickURL(givenStringURL, ref target))
            {
                LoggingHelper.WarningLog($"Provided URL with the key '{key}' is invalid. Unable of setting url.");
                return false;
            }

            LoggingHelper.DebugLog($"Found email URL: '{target.AbsoluteUri.Substring(0, 10)}[...]'. " +
                                   "It has been marked as valid.");

            return true;
        }
    }
}