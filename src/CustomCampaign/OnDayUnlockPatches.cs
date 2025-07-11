using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using Object = System.Object;

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
            private static bool Prefix(MethodBase __originalMethod, OnDayUnlock __instance)
            {
                if (GlobalVariables.arcadeMode)
                {
                    return false;
                }
                
                if (!__instance.enableForArcadeMode)
                {
                    if (GlobalVariables.currentDay < __instance.unlockDay)
                    {
                        __instance.gameObject.SetActive(false);
                        
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Day to unlock has not been reached.. Disabling.\n");
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
                                                    $"{PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1).ToString()).ToString()}");
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
                                MelonLogger.Msg($"DEBUG: Saved day score of day {unlockDay}. The threshold is: {__instance.scoreThresholdToUnlock}.");
                            #endif
                            
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Custom Campaign the current saved day score is: {currentCampaign.savedDayScores[unlockDay]}");
                            #endif


                            
                            if (__instance.scoreThresholdToUnlock > 0.0f) // Has a set value other than the default.
                            {
                                if (currentCampaign.savedDayScores[unlockDay] < (double) __instance.scoreThresholdToUnlock)
                                {

                                    #if DEBUG
                                        MelonLogger.Msg($"DEBUG: This score is not enough. Disabling.\n");
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
                                MelonLogger.Msg($"DEBUG: Is unlocked due to beating the game or being in winter DLC.\n");
                            #endif
                            
                            return false;
                        }
                        else // If not enabled by beating the game or unlocked by 
                        {
                            
                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Didn't beat the game to unlock this or not in winter DLC. Disabling.\n");
                            #endif
                            
                            __instance.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Disabling. Made for Arcade.\n");
                    #endif
                    
                    __instance.gameObject.SetActive(false);
                }
                
                return false; // Skip the original function
            }
        }
    }
}