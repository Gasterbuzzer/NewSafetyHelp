using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign
{
    public static class DesktopBackgroundPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(DayNumSpriteSwapper), "Start", new Type[] { })]
        public static class StartPatch
        {
            /// <summary>
            /// Original function replaces background based on the day.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(MethodBase __originalMethod, DayNumSpriteSwapper __instance)
            {

                if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null even though its enabled! Calling original function.");
                        return true;
                    }
                    
                    // If to disable the green color overlay.
                    if (customCampaign.disableGreenColorBackground)
                    {
                        __instance.myImage.color = Color.white;
                    }

                    if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay == 1 || customCampaign.savedGameFinishedDisplay == 1) // If we finished the campaign.
                    {
                        if (customCampaign.gameFinishedBackground != null)
                        {
                            __instance.myImage.sprite = customCampaign.gameFinishedBackground;
                        }
                        else
                        {
                            __instance.myImage.sprite = __instance.gameFinishedSprite;
                        }
                    }
                    else // Current Day Background instead.
                    {
                        if (customCampaign.backgroundSprites.Count > 0 && (GlobalVariables.currentDay <= customCampaign.backgroundSprites.Count)) // We have backgrounds to replace.
                        {
                            __instance.myImage.sprite = customCampaign.backgroundSprites[GlobalVariables.currentDay - 1];
                        }
                        else
                        {
                            if (GlobalVariables.currentDay > __instance.spritesPerDay.Length) // Too many days for default image, we show first image.
                            {
                                __instance.myImage.sprite = __instance.spritesPerDay[0];
                            }
                            else
                            {
                                __instance.myImage.sprite = __instance.spritesPerDay[GlobalVariables.currentDay - 1];
                            }
                        }
                    }
                }
                else // Main Game
                {
                    __instance.myImage.sprite = __instance.spritesPerDay[GlobalVariables.currentDay - 1];
                    
                    if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay != 1)
                    {
                        return false;
                    }
                    
                    __instance.myImage.sprite = __instance.gameFinishedSprite;
                }
                
                return false; // Skip the original function
            }
        }
    }
}