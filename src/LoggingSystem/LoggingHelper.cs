using System;
using MelonLoader;

namespace NewSafetyHelp.LoggingSystem
{
    public static class LoggingHelper
    {
        public enum LoggingCategory
        {
            NONE,
            SKIPPED_CALLER,
            THEME,
            RINGTONE
        }

        private enum LoggingLevel
        {
            ERROR,
            CRITICAL_ERROR,
            WARNING,
            INFO,
            DEBUG,
            DEBUG_WARNING
        }

        /// <summary>
        /// Before we log, we check if the logging category is even enabled to show it.
        /// </summary>
        /// <param name="loggingCategory"> Category to check for. </param>
        /// <param name="checkDebugLog">Also check if debug logs are enabled.</param>
        /// <returns>(Bool) True: Category is enabled; False: Category is disabled.</returns>
        private static bool CheckLoggingCategory(LoggingCategory loggingCategory = LoggingCategory.NONE,
            bool checkDebugLog = false)
        {
            if (checkDebugLog)
            {
                if (!NewSafetyHelpMainClass.ShowDebugLogs.Value)
                {
                    return false;
                }
            }
            
            if (loggingCategory != LoggingCategory.NONE)
            {
                switch (loggingCategory)
                {
                    case LoggingCategory.SKIPPED_CALLER:
                        if (!NewSafetyHelpMainClass.ShowSkippedCallerDebugLog.Value)
                        {
                            return false;
                        }
                        break;
                    
                    case LoggingCategory.THEME:
                        if (!NewSafetyHelpMainClass.ShowThemeDebugLog.Value)
                        {
                            return false;
                        }
                        break;
                    
                    case LoggingCategory.RINGTONE:
                        if (!NewSafetyHelpMainClass.ShowRingtoneDebugLog.Value)
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Logs the given preconstructed string.
        /// </summary>
        /// <param name="loggingMessage"></param>
        /// <param name="loggingLevel"></param>
        /// <param name="consoleColor"></param>
        private static void Log(string loggingMessage,
            LoggingLevel loggingLevel = LoggingLevel.INFO,
            ConsoleColor? consoleColor = null)
        {
            switch (loggingLevel)
            {
                case LoggingLevel.ERROR:
                    MelonLogger.Error($"ERROR: {loggingMessage}");
                    break;

                case LoggingLevel.CRITICAL_ERROR:
                    MelonLogger.Msg(ConsoleColor.Red, $"CRITICAL ERROR: {loggingMessage}");
                    break;

                case LoggingLevel.WARNING:
                    MelonLogger.Warning($"WARNING: {loggingMessage}");
                    break;

                case LoggingLevel.INFO:
                    if (consoleColor != null)
                    {
                        MelonLogger.Msg((ConsoleColor) consoleColor, $"INFO: {loggingMessage}");
                    }
                    else
                    {
                        MelonLogger.Msg($"INFO: {loggingMessage}");
                    }
                    break;

                case LoggingLevel.DEBUG:
                    if (consoleColor != null)
                    {
                        MelonLogger.Msg((ConsoleColor)consoleColor, $"DEBUG: {loggingMessage}");
                    }
                    else
                    {
                        MelonLogger.Msg($"DEBUG: {loggingMessage}");
                    }

                    break;

                case LoggingLevel.DEBUG_WARNING:
                    if (consoleColor != null)
                    {
                        MelonLogger.Msg((ConsoleColor)consoleColor, $"DEBUG WARNING: {loggingMessage}");
                    }
                    else
                    {
                        MelonLogger.Msg($"DEBUG WARNING: {loggingMessage}");
                    }

                    break;
            }
        }

        /// <summary>
        /// Logs info messages to the console.
        /// </summary>
        /// <param name="loggingMessage">Message to log.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        /// <param name="consoleColor">Color to display in the console.</param>
        public static void InfoLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE,
            ConsoleColor? consoleColor = null)
        {
            if (CheckLoggingCategory(loggingCategory))
            {
                Log(loggingMessage, LoggingLevel.INFO, consoleColor);
            }
        }
        
        /// <summary>
        /// Logs info messages to the console.
        /// Overload: Provides a lambda function to avoid constructing the string if it is not known if to print yet.
        /// </summary>
        /// <param name="loggingMessageConstructor">Lambda function to construct logging message.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        /// <param name="consoleColor">Color to display in the console.</param>
        public static void InfoLog(Func<string> loggingMessageConstructor,
            LoggingCategory loggingCategory = LoggingCategory.NONE, ConsoleColor? consoleColor = null)
        {
            if (CheckLoggingCategory(loggingCategory))
            {
                string loggingMessage = loggingMessageConstructor();
                Log(loggingMessage, LoggingLevel.INFO, consoleColor);
            }
        }

        /// <summary>
        /// Logs a debug messages to the console. Only enabled if debug is enabled.
        /// </summary>
        /// <param name="loggingMessage">Message to log.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        /// <param name="consoleColor">Color to display in the console.</param>
        public static void DebugLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE,
            ConsoleColor? consoleColor = null)
        {
            if (CheckLoggingCategory(loggingCategory))
            {
                Log(loggingMessage, LoggingLevel.DEBUG, consoleColor);
            }
        }
        
        /// <summary>
        /// Logs a debug messages to the console. Only enabled if debug is enabled.
        /// Overload: Provides a lambda function to avoid constructing the string if it is not known if to print yet.
        /// </summary>
        /// <param name="loggingMessageConstructor">Lambda function to construct logging message.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        /// <param name="consoleColor">Color to display in the console.</param>
        public static void DebugLog(Func<string> loggingMessageConstructor,
            LoggingCategory loggingCategory = LoggingCategory.NONE, ConsoleColor? consoleColor = null)
        {
            if (CheckLoggingCategory(loggingCategory, true))
            {
                string loggingMessage = loggingMessageConstructor();
                
                Log(loggingMessage, LoggingLevel.DEBUG, consoleColor);
            }
        }

        /// <summary>
        /// Logs an error messages to the console.
        /// </summary>
        /// <param name="loggingMessage">Message to log.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        public static void ErrorLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE)
        {
            if (CheckLoggingCategory(loggingCategory))
            {
                Log(loggingMessage, LoggingLevel.ERROR);
            }
        }
        
        /// <summary>
        /// Logs a critical error messages to the console.
        /// </summary>
        /// <param name="loggingMessage">Message to log.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        public static void CriticalErrorLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE)
        {
            if (CheckLoggingCategory(loggingCategory))
            {
                Log(loggingMessage, LoggingLevel.CRITICAL_ERROR);
            }
        }

        /// <summary>
        /// Logs a warning messages to the console.
        /// </summary>
        /// <param name="loggingMessage">Message to log.</param>
        /// <param name="loggingCategory">If it belongs to a logging category, which one it is.</param>
        public static void WarningLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE)
        {
            if (CheckLoggingCategory(loggingCategory))
            {
                Log(loggingMessage, LoggingLevel.WARNING);
            }
        }

        /// <summary>
        /// Prints the default custom campaign is null.
        /// This is not supposed to happen, so this error informs the user that the normal operation can't continue.
        /// </summary>
        public static void CampaignNullError()
        {
            ErrorLog("Custom Campaign is null, even though custom campaign is active. " +
                     "Calling either original function or cancelling the current operation.");
        }
    }
}