using System.Collections.Generic;
using System.Linq;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
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
        /// Tries to assign the target with the JSON value at the given key. If not found, it will not write.
        /// This version takes in a bool that updates to "true" if updated.
        /// This overload takes in a list of keys, it will search for all provided keys and assigned based on the last one found.
        /// </summary>
        /// <param name="jObjectParsed">JSON Object where the key is found.</param>
        /// <param name="keys">(List) Keys to be found.</param>
        /// <param name="target">Target to write the value to.</param>
        /// <typeparam name="T">Type of the target.</typeparam>
        public static void TryAssignWithChangedBool<T>(JObject jObjectParsed, List<string> keys, ref VariableChanged<T> target)
        {
            target.HasChanged = false;
            
            foreach (var singleKey in keys)
            {
                TryAssignWithChangedBool(jObjectParsed, singleKey, ref target);
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

        
    }
}