using System;
using System.Globalization;
using System.Reflection;
using MelonLoader;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace NewSafetyHelp.CustomCampaign
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
                    if (CustomCampaignGlobal.inCustomCampaign)
                    {
                        if (handleEntryBrowserUnlocker(ref __instance))
                        {
                            return false;
                        }
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
                        if (!CustomCampaignGlobal.inCustomCampaign) // Main Campaign
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
                            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();

                            if (currentCampaign == null)
                            {
                                MelonLogger.Error("ERROR: CustomCampaignExtraInfo is null in unlock script. This shouldn't happen as custom campaign is true.");
                                return true;
                            }
                            
                            int unlockDay = __instance.unlockDay - 1;

                            if (unlockDay < 0)
                            {
                                unlockDay = 0;
                            }
                            
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: This object unlock in day: {unlockDay} (Current Day is {GlobalVariables.currentDay}). The threshold is: {__instance.scoreThresholdToUnlock}. The current score for that day is {currentCampaign.savedDayScores[unlockDay]}. (For GameObject: '{__instance.gameObject.name}')");
                            #endif
                            
                            if (__instance.scoreThresholdToUnlock > 0.0f) // Has a set value other than the default.
                            {
                                if (currentCampaign.savedDayScores[unlockDay] < (double) __instance.scoreThresholdToUnlock)
                                {

                                    #if DEBUG
                                        MelonLogger.Msg($"DEBUG: The score {currentCampaign.savedDayScores[unlockDay]} for day {unlockDay} is not enough to unlock. Required for Score: '{__instance.scoreThresholdToUnlock}' for this GameObject. Disabling this GameObject '{__instance.gameObject.name}'.\n");
                                    #endif
                                    
                                    __instance.gameObject.SetActive(false);
                                }
                                else
                                {
                                    MelonLogger.Msg($"UNITY LOG: Email unlocked: {__instance.gameObject.name}| Day Checked: {(unlockDay).ToString()}| Day Score: " +
                                                    $"{currentCampaign.savedDayScores[unlockDay]}.\n");
                                }
                            }
                        }
                        
                        if (!__instance.beatGameUnlock || !(bool) GlobalVariables.saveManagerScript || GlobalVariables.saveManagerScript.savedGameFinished >= 1 || __instance.xmasUnlock && GlobalVariables.isXmasDLC)
                        {
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: GameObject '{__instance.gameObject.name}' is unlocked due to beating the game or being in winter DLC. BeatGameUnlock: '{__instance.beatGameUnlock}'. SaveManagerScript: '{(bool) GlobalVariables.saveManagerScript}'. SaveManagerScript Game Finished: '{GlobalVariables.saveManagerScript.savedGameFinished >= 1}'. XmasUnlock: '{__instance.xmasUnlock && GlobalVariables.isXmasDLC}'.\n");
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

        public static bool handleEntryBrowserUnlocker(ref OnDayUnlock __instance)
        {
            
            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: CustomCampaignExtraInfo is null in unlock script. This shouldn't happen as custom campaign is true.");
                return false;
            }
            
            // If always on. We just leave them on.
            if (currentCampaign.entryBrowserAlwaysActive && __instance.gameObject.name == "EntryBrowser-Executable")
            {
                                
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Entry Browser Executable is always enabled.");
                #endif
                                
                return true;
            }

            return false; // If not set to unlock.
        }
    }
}