using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.ErrorDebugging
{
    public static class UnityLogHook
    {
        /// <summary>
        /// Handler for Unity log messages. Prints all unity logs to the MelonLoader console.
        /// </summary>
        /// <param name="logString">The message string</param>
        /// <param name="stackTrace">Stack trace if available</param>
        /// <param name="type">Log type (Log, Warning, Error, etc.)</param>
        public static void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    LoggingHelper.ErrorLog($"[Unity Error] {logString}\nStackTrace:\n{stackTrace}");
                    break;

                case LogType.Exception:
                    LoggingHelper.ErrorLog($"[Unity Error] {logString}\nStackTrace:\n{stackTrace}");
                    break;

                case LogType.Warning:
                    LoggingHelper.ErrorLog($"[Unity Warning] {logString}\nStackTrace:\n{stackTrace}");
                    break;

                default:
                    LoggingHelper.DebugLog($"[Unity Log] {logString}");
                    break;
            }
        }
    }
}