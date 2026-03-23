using System.Globalization;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace NewSafetyHelp.CustomCampaignSystem.Desktop
{
    public static class OnDayUnlockPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OnDayUnlock), "OnEnable")]
        public static class OnEnablePatch
        {
            /// <summary>
            /// Changes the function to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(OnDayUnlock __instance)
            {
                if (GlobalVariables.arcadeMode)
                {
                    return false;
                }

                // If not meant for Arcade Mode.
                if (!__instance.enableForArcadeMode)
                {
                    // Special cases / exceptions:
                    if (CustomCampaignGlobal.InCustomCampaign)
                    {
                        string gameObjectName = __instance.gameObject.name;
                        
                        // For future reference: The switch outcome only matters if the modifier was applied.
                        // If true: It means that we wish for the GameObject to be left enabled.
                        // If false (And modifier is false): We wish to check further.
                        // If false (And modifier is true): We wish to disable the object.
                        bool modifierApplied = false;
                        bool switchOutcome = false;

                        switch (gameObjectName)
                        {
                            case "EntryBrowser-Executable":
                                switchOutcome = HandleEntryBrowserUnlocker(ref __instance, ref modifierApplied);
                                break;

                            case "Scorecard":
                                switchOutcome = HandleScorecardUnlocker(ref __instance, ref modifierApplied);
                                break;

                            case "Artbook-Executable":
                                switchOutcome = HandleArtbookUnlocker(ref __instance, ref modifierApplied);
                                break;

                            case "Arcade-Executable":
                                switchOutcome = HandleArcadeUnlocker(ref __instance, ref modifierApplied);
                                break;

                            case "DLC-Executable(Clone)":
                                switchOutcome = HandleCustomCampaignIconsTooEarly(ref __instance);
                                break;
                        }

                        if (switchOutcome)
                        {
                            return false;
                        }

                        if (modifierApplied) // Modifier is applied and switchOutcome was false.
                        {
                            __instance.gameObject.SetActive(false);
                            return false;
                        }
                    }
                    else if (HandleCustomCampaignIconsTooEarly(ref __instance))
                    {
                        return false;
                    }

                    // It means the unlock day hasn't been reached yet.
                    if (GlobalVariables.currentDay < __instance.unlockDay)
                    {
                        __instance.gameObject.SetActive(false);

                        LoggingHelper.DebugLog(() =>
                            $"Day to unlock ({__instance.unlockDay}) has not been reached. " +
                            $"Disabling this GameObject ('{__instance.gameObject.name}'). " +
                            "(Main and Custom Campaign). " +
                            $"Current day: {GlobalVariables.currentDay}.\n");
                    }
                    else // Unlock Day has been reached.
                    {
                        // Main Campaign
                        if (!CustomCampaignGlobal.InCustomCampaign) 
                        {
                            if (PlayerPrefs.HasKey("SavedDayScore" + (__instance.unlockDay - 1)))
                            {
                                if (PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1)) 
                                    < (double)__instance.scoreThresholdToUnlock)
                                {
                                    __instance.gameObject.SetActive(false);
                                }
                                else
                                {
                                    LoggingHelper.DebugLog(() =>
                                        $"[UNITY]: Email unlocked: {__instance.gameObject.name}| " +
                                        $"Day Checked: {(__instance.unlockDay - 1).ToString()}" +
                                        "| Day Score: " +
                                        $"{PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1)).ToString(CultureInfo.InvariantCulture)}"
                                        );
                                }
                            }
                        }
                        else // Custom Campaign
                        {
                            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                            if (customCampaign == null)
                            {
                                return true;
                            }

                            int unlockDay = __instance.unlockDay - 1;

                            if (unlockDay < 0)
                            {
                                unlockDay = 0;
                            }
                            
                            LoggingHelper.DebugLog(() =>
                                $"This GameObject (For GameObject: '{__instance.gameObject.name}') " +
                                $"unlocks on day: {__instance.unlockDay} (Current Day is {GlobalVariables.currentDay}). " +
                                $"The threshold is: {__instance.scoreThresholdToUnlock}. " +
                                $"The current score for the day {__instance.unlockDay-1} is {customCampaign.SavedDayScores[unlockDay]}. " +
                                $"Is threshold over 0? '{__instance.scoreThresholdToUnlock > 0.0f}'.");
                            
                            EmailListingBehavior emailComponent = __instance.gameObject.GetComponent<EmailListingBehavior>();
                            
                            LoggingHelper.DebugLog("Checking if GameObject is email. " +
                                                   $"Is email null? '{emailComponent == null}'",
                                LoggingHelper.LoggingCategory.EMAIL);

                            // Only emails have a threshold.
                            // This checks if we have an email or not.
                            if (emailComponent != null)
                            {
                                LoggingHelper.DebugLog("Found Email to be unlocked.",
                                    LoggingHelper.LoggingCategory.EMAIL);
                                    
                                CustomEmail email = CustomCampaignGlobal.GetCustomEmailFromActiveCampaign(emailComponent.myEmail);

                                // If we found an email, it means it is a custom email.
                                // If we don't find any email, it either isn't an email or from the main campaign.
                                if (email != null)
                                {
                                    // We check each condition.
                                    if (AccuracyEmailHelper.CheckIfEmailAccuracyType(email))
                                    {
                                        LoggingHelper.DebugLog("Email allowed to be shown.",
                                            LoggingHelper.LoggingCategory.EMAIL);
                                                
                                        return false;
                                    }
                                    else // Checks failed.
                                    {
                                        LoggingHelper.DebugLog("One of the checks failed," +
                                                               " deactivating email.",
                                            LoggingHelper.LoggingCategory.EMAIL);
                                        __instance.gameObject.SetActive(false);
                                        return false;
                                    }
                                }
                            }

                            // If the given game object wasn't an email, we use the old system:
                            if (__instance.scoreThresholdToUnlock > 0.0f)
                            {
                                // If the threshold was not reached (score too low).
                                if (customCampaign.SavedDayScores[unlockDay] < (double)__instance.scoreThresholdToUnlock)
                                {
                                    LoggingHelper.DebugLog(() =>
                                        $"The score {customCampaign.SavedDayScores[unlockDay]} for day {unlockDay} is not enough to unlock. " +
                                        $"Required for Score: '{__instance.scoreThresholdToUnlock}' for this GameObject. " +
                                        $"Disabling this GameObject '{__instance.gameObject.name}'.\n");

                                    __instance.gameObject.SetActive(false);
                                }
                                else // Threshold was enough.
                                {
                                    LoggingHelper.DebugLog(() =>
                                            $"[UNITY] GameObject unlocked: {__instance.gameObject.name}| " +
                                            $"Day Checked: {unlockDay.ToString()}| Day Score: " +
                                            $"{customCampaign.SavedDayScores[unlockDay]}.\n",
                                        LoggingHelper.LoggingCategory.EMAIL);
                                }
                            }
                            
                            // Now we check if the given GameObject is a video.
                            if (__instance.gameObject.TryGetComponent(out VideoExecutableFile _))
                            {
                                CustomVideo customVideo = CustomCampaignGlobal.GetCustomVideoFromActiveCampaign(__instance.gameObject);

                                if (customVideo != null)
                                {
                                    LoggingHelper.DebugLog($"Found video on desktop '{customVideo.DesktopName}'.",
                                        LoggingHelper.LoggingCategory.VIDEO);
                                    
                                    if (!customVideo.IgnoreAccuracyChecks)
                                    {
                                        if (AccuracyVideoHelper.CheckIfVideoAccuracyType(customVideo))
                                        {
                                            LoggingHelper.DebugLog("Video allowed to be shown.",
                                                LoggingHelper.LoggingCategory.VIDEO);
                                            return false;
                                        }
                                        else
                                        {
                                            LoggingHelper.DebugLog("One of the checks failed," +
                                                                   " deactivating video.",
                                                LoggingHelper.LoggingCategory.VIDEO);
                                            __instance.gameObject.SetActive(false);
                                            return false;
                                        }
                                    }
                                }
                            }
                        }

                        // This object does not require to beat the game,
                        // The save manager is valid (not null) or 
                        // We have beaten the game (finished it)
                        // Or if we are in the DLC, and it only unlocks for the DLC.
                        if (!__instance.beatGameUnlock || !(bool)GlobalVariables.saveManagerScript ||
                            GlobalVariables.saveManagerScript.savedGameFinished >= 1 ||
                            __instance.xmasUnlock && GlobalVariables.isXmasDLC)
                        {
                            LoggingHelper.DebugLog(() =>
                                $"GameObject '{__instance.gameObject.name}' is unlocked! " +
                                "This may be due to it always being unlocked or by beating the game or being in winter DLC. " +
                                $"BeatGameUnlock: '{__instance.beatGameUnlock}'. " +
                                $"SaveManagerScript: '{(bool)GlobalVariables.saveManagerScript}'. " +
                                $"SaveManagerScript Game Finished: '{GlobalVariables.saveManagerScript.savedGameFinished >= 1}'. " +
                                $"XmasUnlock: '{__instance.xmasUnlock && GlobalVariables.isXmasDLC}'.\n");
                            return false;
                        }
                        else // If any of the above criteria wasn't met.
                        {
                            LoggingHelper.DebugLog(() =>
                                "Didn't beat the game to unlock this or not in winter DLC." +
                                $" Disabling the GameObject '{__instance.gameObject.name}'." +
                                $" BeatGameUnlock: '{__instance.beatGameUnlock}'." +
                                $" SaveManagerScript: '{(bool)GlobalVariables.saveManagerScript}'." +
                                $" SaveManagerScript Game Finished: '{GlobalVariables.saveManagerScript.savedGameFinished >= 1}'." +
                                $" XmasUnlock: '{__instance.xmasUnlock && GlobalVariables.isXmasDLC}'.\n");

                            __instance.gameObject.SetActive(false);
                        }
                    }
                }
                else // Else means that it is made for Arcade.
                {
                    LoggingHelper.DebugLog("Disabling UI Icon. GameObject made for Arcade.");

                    __instance.gameObject.SetActive(false);
                }

                return false; // Skip the original function
            }
        }

        /// <summary>
        /// Handles the Entry Browser of the desktop icon for enabling or disabling based on custom campaign settings.
        /// </summary>
        /// <param name="__instance">Instance of the OnDayUnlock script. </param>
        /// <param name="modifierApplied">A bool that defines if a modifier was applied.
        /// (Needed for overwriting values, such as disabling and not just simply saying "Don't enable",
        /// but rather be "disable)</param>
        /// <returns>(Bool) Outcome of the handling. (If to enable or not)</returns>
        public static bool HandleEntryBrowserUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "EntryBrowser-Executable")
            {
                CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    LoggingHelper.CampaignNullError();
                    return false;
                }

                bool enableEntryBrowser = false;
                
                (bool foundModifier, bool value) entryBrowser = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.EntryBrowserActive,
                    specialPredicate: m => m.EntryBrowserChanged);

                // If always on. We just leave them on.
                if (currentCampaign.EntryBrowserAlwaysActive)
                {
                    enableEntryBrowser = true;
                }

                if (entryBrowser.foundModifier)
                {
                    modifierApplied = true;
                    enableEntryBrowser = entryBrowser.value;
                }

                return enableEntryBrowser;
            }

            return false; // If not set to unlock.
        }
        
        /// <summary>
        /// Handles the Scorecard of the desktop icon for enabling or disabling based on custom campaign settings.
        /// </summary>
        /// <param name="__instance">Instance of the OnDayUnlock script. </param>
        /// <param name="modifierApplied">A bool that defines if a modifier was applied.
        /// (Needed for overwriting values, such as disabling and not just simply saying "Don't enable",
        /// but rather be "disable)</param>
        /// <returns>(Bool) Outcome of the handling. (If to enable or not)</returns>
        public static bool HandleScorecardUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "Scorecard")
            {
                CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    LoggingHelper.CampaignNullError();
                    return false;
                }

                bool enableScorecard = false;
                
                (bool foundModifier, bool value) scorecard = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.ScorecardActive,
                    specialPredicate: m => m.ScorecardChanged);

                // If always on. We just leave them on.
                if (currentCampaign.ScorecardAlwaysActive)
                {
                    enableScorecard = true;
                }

                if (scorecard.foundModifier)
                {
                    modifierApplied = true;
                    enableScorecard = scorecard.value;
                }

                return enableScorecard;
            }

            return false; // If not set to unlock.
        }

        /// <summary>
        /// Handles the Artbook of the desktop icon for enabling or disabling based on custom campaign settings.
        /// </summary>
        /// <param name="__instance">Instance of the OnDayUnlock script. </param>
        /// <param name="modifierApplied">A bool that defines if a modifier was applied.
        /// (Needed for overwriting values, such as disabling and not just simply saying "Don't enable",
        /// but rather be "disable)</param>
        /// <returns>(Bool) Outcome of the handling. (If to enable or not)</returns>
        public static bool HandleArtbookUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "Artbook-Executable")
            {
                CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    LoggingHelper.CampaignNullError();
                    return false;
                }

                bool artBookEnabled = false;
                
                (bool foundModifier, bool value) artbook = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.ArtbookActive,
                    specialPredicate: m => m.ArtbookChanged);

                // If always on. We just leave them on.
                if (currentCampaign.ArtbookAlwaysActive)
                {
                    artBookEnabled = true;
                }

                if (artbook.foundModifier)
                {
                    modifierApplied = true;
                    artBookEnabled = artbook.value;
                }

                return artBookEnabled;
            }

            return false; // If not set to unlock.
        }

        /// <summary>
        /// Handles the Arcade of the desktop icon for enabling or disabling based on custom campaign settings.
        /// </summary>
        /// <param name="__instance">Instance of the OnDayUnlock script. </param>
        /// <param name="modifierApplied">A bool that defines if a modifier was applied.
        /// (Needed for overwriting values, such as disabling and not just simply saying "Don't enable",
        /// but rather be "disable)</param>
        /// <returns>(Bool) Outcome of the handling. (If to enable or not)</returns>
        public static bool HandleArcadeUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "Arcade-Executable")
            {
                CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    LoggingHelper.CampaignNullError();
                    return false;
                }

                bool arcadeEnabled = false;
                
                (bool foundModifier, bool value) arcade = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.ArcadeActive,
                    specialPredicate: m => m.ArcadeChanged);

                // If always on. We just leave them on.
                if (currentCampaign.ArcadeAlwaysActive)
                {
                    arcadeEnabled = true;
                }

                if (arcade.foundModifier)
                {
                    modifierApplied = true;
                    arcadeEnabled = arcade.value;
                }

                return arcadeEnabled;
            }

            return false; // If not set to unlock.
        }

        /// <summary>
        /// Handles the case if the DLC Icon appears too early. Since it may be disabled on accident.
        /// </summary>
        /// <param name="__instance">Instance of the OnDayUnlock script.</param>
        /// <returns>(Bool) If we avoid messing with the DLC icon.</returns>
        public static bool HandleCustomCampaignIconsTooEarly(ref OnDayUnlock __instance)
        {
            // If always on. We just leave them on.
            if (__instance.gameObject.name == "DLC-Executable(Clone)")
            {
                return true;
            }

            return false; // If not set to unlock.
        }
    }
}