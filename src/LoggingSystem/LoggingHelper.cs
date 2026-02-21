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

        private static void Log(string loggingMessage,
            LoggingLevel loggingLevel = LoggingLevel.INFO,
            LoggingCategory loggingCategory = LoggingCategory.NONE,
            ConsoleColor? consoleColor = null)
        {
            if (loggingCategory != LoggingCategory.NONE)
            {
                switch (loggingCategory)
                {
                    case LoggingCategory.SKIPPED_CALLER:
                        if (!NewSafetyHelpMainClass.ShowSkippedCallerDebugLog.Value)
                        {
                            return;
                        }
                        break;
                    
                    case LoggingCategory.THEME:
                        if (!NewSafetyHelpMainClass.ShowThemeDebugLog.Value)
                        {
                            return;
                        }
                        break;
                    
                    case LoggingCategory.RINGTONE:
                        if (!NewSafetyHelpMainClass.ShowRingtoneDebugLog.Value)
                        {
                            return;
                        }
                        break;
                }
            }

            switch (loggingLevel)
            {
                case LoggingLevel.ERROR:
                    MelonLogger.Error("ERROR: " + loggingMessage);
                    break;

                case LoggingLevel.CRITICAL_ERROR:
                    MelonLogger.Msg(ConsoleColor.Red, "CRITICAL ERROR: " + loggingMessage);
                    break;

                case LoggingLevel.WARNING:
                    MelonLogger.Warning("WARNING: " + loggingMessage);
                    break;

                case LoggingLevel.INFO:
                    if (consoleColor != null)
                    {
                        MelonLogger.Msg((ConsoleColor) consoleColor, "INFO: " + loggingMessage);
                    }
                    else
                    {
                        MelonLogger.Msg("INFO: " + loggingMessage);
                    }
                    break;

                case LoggingLevel.DEBUG:
                    if (NewSafetyHelpMainClass.ShowDebugLogs.Value)
                    {
                        if (consoleColor != null)
                        {
                            MelonLogger.Msg((ConsoleColor) consoleColor, "DEBUG: " + loggingMessage);
                        }
                        else
                        {
                            MelonLogger.Msg("DEBUG: " + loggingMessage);
                        }
                    }

                    break;

                case LoggingLevel.DEBUG_WARNING:
                    if (NewSafetyHelpMainClass.ShowDebugLogs.Value)
                    {
                        if (consoleColor != null)
                        {
                            MelonLogger.Msg((ConsoleColor) consoleColor, "DEBUG WARNING: " + loggingMessage);
                        }
                        else
                        {
                            MelonLogger.Msg("DEBUG WARNING: " + loggingMessage);
                        }
                    }

                    break;
            }
        }

        public static void InfoLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE,
            ConsoleColor? consoleColor = null)
        {
            Log(loggingMessage, LoggingLevel.INFO, loggingCategory, consoleColor);
        }

        public static void DebugLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE,
            ConsoleColor? consoleColor = null)
        {
            if (!NewSafetyHelpMainClass.ShowDebugLogs.Value)
            {
                return;
            }
            
            Log(loggingMessage, LoggingLevel.DEBUG, loggingCategory, consoleColor);
        }

        public static void ErrorLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE)
        {
            Log(loggingMessage, LoggingLevel.ERROR, loggingCategory);
        }
        
        public static void CriticalErrorLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE)
        {
            Log(loggingMessage, LoggingLevel.CRITICAL_ERROR, loggingCategory);
        }

        public static void WarningLog(string loggingMessage, LoggingCategory loggingCategory = LoggingCategory.NONE)
        {
            Log(loggingMessage, LoggingLevel.WARNING, loggingCategory);
        }

        public static void CampaignNullError()
        {
            ErrorLog("Custom Campaign is null, even though custom campaign is active. " +
                     "Calling either original function or cancelling the current operation.");
        }
    }
}