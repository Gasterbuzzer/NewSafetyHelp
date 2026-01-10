using System;
using System.Globalization;
using System.Reflection;
using MelonLoader;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace NewSafetyHelp.CustomCampaign.Desktop
{
    public static class OnDayUnlockPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OnDayUnlock), "OnEnable", new Type[] { })]
        public static class OnEnablePatch
        {

            /// <summary>
            /// Changes the function to work better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(MethodBase __originalMethod, OnDayUnlock __instance)
            {
                if (GlobalVariables.arcadeMode)
                {
                    return false;
                }
                
                if (!__instance.enableForArcadeMode)
                {
                    
                    // Special cases / exceptions:
                    if (CustomCampaignGlobal.InCustomCampaign)
                    {
                        string gameObjectName = __instance.gameObject.name;

                        bool modifierApplied = false;
                        bool switchOutcome = false;

                        switch (gameObjectName)
                        {
                            case "EntryBrowser-Executable":
                                switchOutcome = handleEntryBrowserUnlocker(ref __instance, ref modifierApplied);
                                break;
                            
                            case "Scorecard":
                                switchOutcome = handleScorecardUnlocker(ref __instance, ref modifierApplied);
                                break;
                            
                            case "Artbook-Executable":
                                switchOutcome = handleArtbookUnlocker(ref __instance, ref modifierApplied);
                                break;
                            
                            case "Arcade-Executable":
                                switchOutcome = handleArcadeUnlocker(ref __instance, ref modifierApplied);
                                break;
                            
                            case "DLC-Executable(Clone)":
                                switchOutcome = handleCustomCampaignIconsTooEarly(ref __instance);
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
                    else if (handleCustomCampaignIconsTooEarly(ref __instance))
                    {
                        return false;
                    }
                    
                    if (GlobalVariables.currentDay < __instance.unlockDay)
                    {
                        __instance.gameObject.SetActive(false);
                        
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Day to unlock ({__instance.unlockDay}) has not been reached. Disabling this GameObject ('{__instance.gameObject.name}'). (Main and Custom Campaign). Current day: {GlobalVariables.currentDay}.\n");
                        #endif
                    }
                    else
                    {
                        if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                        {
                            if (PlayerPrefs.HasKey("SavedDayScore" + (__instance.unlockDay - 1).ToString()))
                            {
                                if (PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1).ToString()) < (double) __instance.scoreThresholdToUnlock)
                                {
                                    __instance.gameObject.SetActive(false);
                                }
                                else
                                {
                                    MelonLogger.Msg($"UNITY LOG: Email unlocked: {__instance.gameObject.name}| Day Checked: {(__instance.unlockDay - 1).ToString()}| Day Score: " +
                                                    $"{PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1).ToString()).ToString(CultureInfo.InvariantCulture)}");
                                }
                            }
                        }
                        else // Custom Campaign
                        {
                            CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                            if (currentCampaign == null)
                            {
                                MelonLogger.Error("ERROR: CustomCampaign is null in unlock script. This shouldn't happen as custom campaign is true.");
                                return true;
                            }
                            
                            int unlockDay = __instance.unlockDay - 1;

                            if (unlockDay < 0)
                            {
                                unlockDay = 0;
                            }
                            
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: This object unlock in day: {unlockDay} (Current Day is {GlobalVariables.currentDay}). The threshold is: {__instance.scoreThresholdToUnlock}. The current score for that day is {currentCampaign.SavedDayScores[unlockDay]}. (For GameObject: '{__instance.gameObject.name}')");
                            #endif
                            
                            if (__instance.scoreThresholdToUnlock > 0.0f) // Has a set value other than the default.
                            {
                                if (currentCampaign.SavedDayScores[unlockDay] < (double) __instance.scoreThresholdToUnlock)
                                {

                                    #if DEBUG
                                        MelonLogger.Msg($"DEBUG: The score {currentCampaign.SavedDayScores[unlockDay]} for day {unlockDay} is not enough to unlock. Required for Score: '{__instance.scoreThresholdToUnlock}' for this GameObject. Disabling this GameObject '{__instance.gameObject.name}'.\n");
                                    #endif
                                    
                                    __instance.gameObject.SetActive(false);
                                }
                                else
                                {
                                    MelonLogger.Msg($"UNITY LOG: Email unlocked: {__instance.gameObject.name}| Day Checked: {(unlockDay).ToString()}| Day Score: " +
                                                    $"{currentCampaign.SavedDayScores[unlockDay]}.\n");
                                }
                            }
                        }
                        
                        if (!__instance.beatGameUnlock || !(bool) GlobalVariables.saveManagerScript || GlobalVariables.saveManagerScript.savedGameFinished >= 1 || __instance.xmasUnlock && GlobalVariables.isXmasDLC)
                        {
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: GameObject '{__instance.gameObject.name}' is unlocked! This may be due to it always being unlocked or by beating the game or being in winter DLC. BeatGameUnlock: '{__instance.beatGameUnlock}'. SaveManagerScript: '{(bool) GlobalVariables.saveManagerScript}'. SaveManagerScript Game Finished: '{GlobalVariables.saveManagerScript.savedGameFinished >= 1}'. XmasUnlock: '{__instance.xmasUnlock && GlobalVariables.isXmasDLC}'.\n");
                            #endif
                            
                            return false;
                        }
                        else // If not enabled by beating the game or unlocked by 
                        {
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Didn't beat the game to unlock this or not in winter DLC. Disabling the GameObject '{__instance.gameObject.name}'. BeatGameUnlock: '{__instance.beatGameUnlock}'. SaveManagerScript: '{(bool) GlobalVariables.saveManagerScript}'. SaveManagerScript Game Finished: '{GlobalVariables.saveManagerScript.savedGameFinished >= 1}'. XmasUnlock: '{__instance.xmasUnlock && GlobalVariables.isXmasDLC}'.\n");
                            #endif
                            
                            __instance.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Disabling. GameObject made for Arcade.\n");
                    #endif
                    
                    __instance.gameObject.SetActive(false);
                }
                
                return false; // Skip the original function
            }
        }

        public static bool handleEntryBrowserUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "EntryBrowser-Executable")
            {
                CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    MelonLogger.Error("ERROR: CustomCampaign is null in unlock script. This shouldn't happen as custom campaign is true.");
                    return false;
                }
                
                bool enableEntryBrowser = false;
            
                bool entryBrowserFound = false;
                bool entryBrowser = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.entryBrowserActive,
                    ref entryBrowserFound,
                    specialPredicate: m => m.entryBrowserChanged);
            
                // If always on. We just leave them on.
                if (currentCampaign.EntryBrowserAlwaysActive)
                {
                    enableEntryBrowser = true;
                }

                if (entryBrowserFound)
                {
                    modifierApplied = true;
                    enableEntryBrowser = entryBrowser;
                }

                return enableEntryBrowser;
            }
            
            return false; // If not set to unlock.
        }
        
        public static bool handleScorecardUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "Scorecard")
            {
                CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    MelonLogger.Error("ERROR: CustomCampaign is null in unlock script. This shouldn't happen as custom campaign is true.");
                    return false;
                }
                
                bool enableScorecard = false;
            
                bool scorecardFound = false;
                bool scorecard = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.scorecardActive,
                    ref scorecardFound,
                    specialPredicate: m => m.scorecardChanged);
                
                // If always on. We just leave them on.
                if (currentCampaign.ScorecardAlwaysActive)
                {
                    enableScorecard = true;
                }
            
                if (scorecardFound)
                {
                    modifierApplied = true;
                    enableScorecard = scorecard;
                }

                return enableScorecard;
            }

            return false; // If not set to unlock.
        }
        
        public static bool handleArtbookUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "Artbook-Executable")
            {
                CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    MelonLogger.Error("ERROR: CustomCampaign is null in unlock script. This shouldn't happen as custom campaign is true.");
                    return false;
                }
                
                bool artBookEnabled = false;
            
                bool artbookFound = false;
                bool artbook = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.artbookActive,
                    ref artbookFound,
                    specialPredicate: m => m.artbookChanged);
                
                // If always on. We just leave them on.
                if (currentCampaign.ArtbookAlwaysActive)
                {
                    artBookEnabled = true;
                }
                
                if (artbookFound)
                {
                    modifierApplied = true;
                    artBookEnabled = artbook;
                }

                return artBookEnabled;
            }

            return false; // If not set to unlock.
        }
        
        public static bool handleArcadeUnlocker(ref OnDayUnlock __instance, ref bool modifierApplied)
        {
            if (__instance.gameObject.name == "Arcade-Executable")
            {
                CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (currentCampaign == null)
                {
                    MelonLogger.Error(
                        "ERROR: CustomCampaign is null in unlock script. This shouldn't happen as custom campaign is true.");
                    return false;
                }

                bool arcadeEnabled = false;
            
                bool arcadeFound = false;
                bool arcade = CustomCampaignGlobal.GetActiveModifierValue(
                    c => c.arcadeActive,
                    ref arcadeFound,
                    specialPredicate: m => m.arcadeChanged);
                
                // If always on. We just leave them on.
                if (currentCampaign.ArcadeAlwaysActive)
                {
                    arcadeEnabled = true;
                }
                
                if (arcadeFound)
                {
                    modifierApplied = true;
                    arcadeEnabled = arcade;
                }

                return arcadeEnabled;
            }

            return false; // If not set to unlock.
        }
        
        public static bool handleCustomCampaignIconsTooEarly(ref OnDayUnlock __instance)
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