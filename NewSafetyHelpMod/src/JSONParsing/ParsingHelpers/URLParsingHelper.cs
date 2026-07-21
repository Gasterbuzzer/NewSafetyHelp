using System;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class URLParsingHelper
    {
        /// <summary>
        /// Assigns a URL to be clicked on a given location.
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

            if (!URLVerification.SetClickURL(givenStringURL, ref target))
            {
                LoggingHelper.WarningLog($"Provided URL with the key '{key}' is invalid. Unable of setting url.");
                return false;
            }

            LoggingHelper.DebugLog($"Found click URL: '{target.AbsoluteUri.Substring(0, 10)}[...]'. " +
                                   "It has been marked as valid.");

            return true;
        }
    }
}