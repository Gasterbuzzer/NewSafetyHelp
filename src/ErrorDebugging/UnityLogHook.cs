using MelonLoader;
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
            #if DEBUG
                switch (type)
                {
                    case LogType.Error:
                        MelonLogger.Error($"[Unity Error] {logString}\nStackTrace:\n{stackTrace}");
                        break;
                        
                    case LogType.Exception:
                        MelonLogger.Error($"[Unity Error] {logString}\nStackTrace:\n{stackTrace}");
                        break;
                    
                    case LogType.Warning:
                        MelonLogger.Warning($"[Unity Warning] {logString}");
                        break;
                    
                    default:
                        MelonLogger.Msg($"[Unity Log] {logString}");
                        break;
                }
            #endif
        }
    }
}