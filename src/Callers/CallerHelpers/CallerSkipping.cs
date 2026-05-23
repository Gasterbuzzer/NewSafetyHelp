using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.Callers.CallerHelpers
{
    public static class CallerSkipping
    {
        /// <summary>
        /// Checks if for the next 'n' callers there is any caller that is valid and should be shown and if any of these should end the day.
        /// </summary>
        /// <param name="__instance">CallerController Instance</param>
        /// <returns> (If a valid caller was found) -1; (If all the callers are skipped between the last caller) </returns>
        public static int CheckIfAnyValidCallerLeft(CallerController __instance)
        {
            int callersSkipped = 0;
            
            for (int i = __instance.currentCallerID + 1; i < __instance.callers.Length; i++)
            {
                if (i < __instance.callers.Length) // Valid caller.
                {
                    CustomCCaller customCCallerFound = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(i);

                    // If the next caller does not exist or was not found, we simply say false.
                    // There might be valid callers after that one, but we are in an invalid state.
                    if (customCCallerFound == null)
                    {
                        return -1;
                    }

                    // Checks for seeing if the next caller is valid:
                    
                    // 1. Is an accuracy caller that will be shown.
                    
                    // 2. If any valid caller comes afterward. (One that cannot be skipped)
                    
                    // 3. Is a consequence caller that will be shown.

                    int currentIndexCopy = i;

                    LoggingHelper.DebugLog(() =>
                            $"Last caller of day (Caller ID: {currentIndexCopy}): '{customCCallerFound.LastDayCaller}'. " +
                            $"Next caller name (Caller ID: {currentIndexCopy}): '{customCCallerFound.CallerName}'. " +
                            $"Is a accuracy caller?: '{customCCallerFound.IsAccuracyCaller}'.",
                        LoggingHelper.LoggingCategory.SKIPPED_CALLER);

                    LoggingHelper.DebugLog(() =>
                            "Is ConsequenceProfile not null? " +
                            "(Meaning it's this current caller is a consequence caller): " +
                            $"'{GlobalVariables.callerControllerScript.callers[currentIndexCopy].callerProfile.consequenceCallerProfile != null}'." +
                            "\n" +
                            "Is this caller allowed to be called? " +
                            "(Meaning we got the answer wrong from the previous caller): " +
                            $"'{GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[currentIndexCopy].callerProfile.consequenceCallerProfile)}'." +
                            "\n" +
                            $"Is this caller the last one of the day? '{customCCallerFound.LastDayCaller}'.",
                        LoggingHelper.LoggingCategory.SKIPPED_CALLER);

                    // Consequence caller
                    bool isConsequenceCaller = false;
                    if (GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile != null)
                    {
                        isConsequenceCaller = true;
                        // This consequence caller is supposed to be called, since the player got the response wrong.
                        if (GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile))
                        {
                            return -1;
                        }
                        
                        callersSkipped++;
                        
                        // Caller is supposed to be skipped. So we simply give the amount to skip.
                        if (customCCallerFound.LastDayCaller)
                        {
                            return callersSkipped;
                        }
                    }
                    
                    // If accuracy caller
                    if (customCCallerFound.IsAccuracyCaller)
                    {
                        bool showCaller = AccuracyCallerHelper.CheckIfCallerIsToBeShown(customCCallerFound);
                        
                        // Accuracy caller that is supposed to be called, since its condition was fulfilled.
                        if (customCCallerFound.IsAccuracyCaller && showCaller)
                        {
                            return -1;
                        }
                        
                        callersSkipped++;

                        // Last caller of the day that is supposed to be skipped.
                        if (customCCallerFound.LastDayCaller)
                        {
                            if (!showCaller)
                            {
                                return callersSkipped;
                            }
                        }
                    }
                    
                    // If not a consequence caller or an accuracy caller, we simply return, since it's a normal caller.
                    if (!isConsequenceCaller && !customCCallerFound.IsAccuracyCaller)
                    {
                        return -1;
                    }
                }
            }

            // Nothing found.
            return -1;
        }
        
        /// <summary>
        /// Checks the next few callers of how many were skipped.
        /// Can be useful for finding the correct custom caller.
        /// </summary>
        /// <param name="__instance">CallerController Instance</param>
        /// <returns> (If none got skipped) -1; Else: The number of skipped callers. </returns>
        public static int GetCallersSkippedAmount(CallerController __instance)
        {
            int callersSkipped = 0;
            
            for (int i = __instance.currentCallerID + 1; i < __instance.callers.Length; i++)
            {
                if (i < __instance.callers.Length) // Valid caller.
                {
                    CustomCCaller customCCallerFound = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(i);

                    // If the next caller does not exist or was not found, we simply say false.
                    // There might be valid callers after that one, but we are in an invalid state.
                    if (customCCallerFound == null)
                    {
                        return callersSkipped;
                    }

                    // Checks for seeing if the next caller is valid:
                    
                    // 1. Is an accuracy caller that will be shown.
                    
                    // 2. If any valid caller comes afterward. (One that cannot be skipped)
                    
                    // 3. Is a consequence caller that will be shown.

                    int currentIndexCopy = i;

                    LoggingHelper.DebugLog(() =>
                            $"Last caller of day (Caller ID: {currentIndexCopy}): '{customCCallerFound.LastDayCaller}'. " +
                            $"Next caller name (Caller ID: {currentIndexCopy}): '{customCCallerFound.CallerName}'. " +
                            $"Is a accuracy caller?: '{customCCallerFound.IsAccuracyCaller}'.",
                        LoggingHelper.LoggingCategory.SKIPPED_CALLER);

                    LoggingHelper.DebugLog(() =>
                            "Is ConsequenceProfile not null? " +
                            "(Meaning it's this current caller is a consequence caller): " +
                            $"'{GlobalVariables.callerControllerScript.callers[currentIndexCopy].callerProfile.consequenceCallerProfile != null}'." +
                            "\n" +
                            "Is this caller allowed to be called? " +
                            "(Meaning we got the answer wrong from the previous caller): " +
                            $"'{GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[currentIndexCopy].callerProfile.consequenceCallerProfile)}'." +
                            "\n" +
                            $"Is this caller the last one of the day? '{customCCallerFound.LastDayCaller}'.",
                        LoggingHelper.LoggingCategory.SKIPPED_CALLER);

                    // Consequence caller
                    bool isConsequenceCaller = false;
                    if (GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile != null)
                    {
                        isConsequenceCaller = true;
                        // This consequence caller is supposed to be called, since the player got the response wrong.
                        if (GlobalVariables.callerControllerScript.CanReceiveConsequenceCall(GlobalVariables.callerControllerScript.callers[i].callerProfile.consequenceCallerProfile))
                        {
                            return callersSkipped;
                        }
                        
                        callersSkipped++;
                        
                        // Caller is supposed to be skipped. So we simply give the amount to skip.
                        if (customCCallerFound.LastDayCaller)
                        {
                            return callersSkipped;
                        }
                    }
                    
                    // If accuracy caller
                    if (customCCallerFound.IsAccuracyCaller)
                    {
                        bool showCaller = AccuracyCallerHelper.CheckIfCallerIsToBeShown(customCCallerFound);
                        
                        // Accuracy caller that is supposed to be called, since its condition was fulfilled.
                        if (customCCallerFound.IsAccuracyCaller && showCaller)
                        {
                            return callersSkipped;
                        }
                        
                        callersSkipped++;

                        // Last caller of the day that is supposed to be skipped.
                        if (customCCallerFound.LastDayCaller)
                        {
                            if (!showCaller)
                            {
                                return callersSkipped;
                            }
                        }
                    }
                    
                    // If not a consequence caller or an accuracy caller, we simply return, since it's a normal caller.
                    if (!isConsequenceCaller && !customCCallerFound.IsAccuracyCaller)
                    {
                        return callersSkipped;
                    }
                }
            }

            // Nothing found.
            return -1;
        }
    }
}