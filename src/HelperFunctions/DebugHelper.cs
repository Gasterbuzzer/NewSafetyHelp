using System;
using System.Diagnostics;
using MelonLoader;

namespace NewSafetyHelp.HelperFunctions
{
    public static class DebugHelper
    {
        public static void CallerOfFunction(string functionName = "NO_FUNCTION_NAME_PROVIDED", int functionDepth = 3, int plusDepth = 2)
        {
            // Create a stack trace
            StackTrace stackTrace = new StackTrace();
            
            MelonLogger.Msg(ConsoleColor.Magenta, "-----");
            
            for (int i = functionDepth + plusDepth; i >= 0; i--)
            {
                // Get the calling method
                StackFrame callerFrame = stackTrace.GetFrame(i);
                var callerMethod = callerFrame.GetMethod();
                
                #if DEBUG
                    MelonLogger.Msg(ConsoleColor.Magenta, $"DEBUG: {i}: '{functionName}' (FD: {functionDepth}) was called by: '{callerMethod.Name}'.");
                #endif
            }
            
            MelonLogger.Msg(ConsoleColor.Magenta, "-----");
        }
    }
}