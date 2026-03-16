using System;
using System.Collections.Generic;
using System.Diagnostics;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = System.Diagnostics.Debug;

namespace NewSafetyHelp.HelperFunctions
{
    public static class DebugHelper
    {
        /// <summary>
        /// Prints the caller of a given function.
        /// </summary>
        /// <param name="functionName">Name of the function, helps for printing.</param>
        /// <param name="functionDepth">How deep to look at the callers.</param>
        /// <param name="plusDepth">Any extra depth checks.</param>
        public static void CallerOfFunction(string functionName = "NO_FUNCTION_NAME_PROVIDED", int functionDepth = 3, int plusDepth = 2)
        {
            // Create a stack trace
            StackTrace stackTrace = new StackTrace();
            LoggingHelper.DebugLog("-----", consoleColor: ConsoleColor.Magenta);
            LoggingHelper.DebugLog("", consoleColor: ConsoleColor.Magenta);
            
            for (int i = functionDepth + plusDepth; i >= 0; i--)
            {
                // Get the calling method
                StackFrame callerFrame = stackTrace.GetFrame(i);
                var callerMethod = callerFrame.GetMethod();
                
                LoggingHelper.DebugLog($"{i}: '{functionName}' (FD: {functionDepth}) was called by: '{callerMethod.Name}'.",
                    consoleColor: ConsoleColor.Magenta);
            }
            
            LoggingHelper.DebugLog("-----", consoleColor: ConsoleColor.Magenta);
        }

        /// <summary>
        /// Prints all the names of a given location when pressed.
        /// It helps find GameObjects that absorb clicks.
        /// </summary>
        public static void PrintClickLocationNames()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> results = new List<RaycastResult>();

                EventSystem.current.RaycastAll(
                    pointerData,  // the pointer position to cast from
                    results       // populated list of everything hit, ordered front-to-back
                );

                foreach (RaycastResult result in results)
                {
                    LoggingHelper.DebugLog(() => 
                        $"Hit: {result.gameObject.name} | Depth: {result.depth} | Distance: {result.distance}");
                }
            }
        }
    }
}