using System;
using System.Diagnostics;
using MelonLoader;

namespace NewSafetyHelp.HelperFunctions
{
    public static class DebugHelper
    {
        public static void CallerOfFunction(int functionDepth = 2)
        {
            // Create a stack trace
            StackTrace stackTrace = new StackTrace();

            // Get the calling method (1 frame up)
            StackFrame callerFrame = stackTrace.GetFrame(functionDepth);
            var callerMethod = callerFrame.GetMethod();

            #if DEBUG
                MelonLogger.Msg(ConsoleColor.Magenta, $"Was called by: '{callerMethod.Name}'.");
            #endif
        }
    }
}