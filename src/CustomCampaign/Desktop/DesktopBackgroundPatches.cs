using System;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Desktop
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
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null even though its enabled! Calling original function.");
                        return true;
                    }
                    
                    // If to disable the green color overlay.

                    bool disableGreenColorBackground = false;
                    Color? desktopBackgroundColor = null;
                    
                    if (customCampaign.disableGreenColorBackground)
                    {
                        disableGreenColorBackground = true;
                    }
                    
                    bool disableColorBackgroundFound = false;
                    bool disableColorBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.disableColorBackground,
                        ref disableColorBackgroundFound);
                    
                    bool desktopBackgroundColorFound = false;
                    Color? modifierDesktopBackgroundColor = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.desktopBackgroundColor,
                        ref desktopBackgroundColorFound,
                        v => v != null);

                    if (disableColorBackgroundFound && disableColorBackground)
                    {
                        disableGreenColorBackground = true;
                    }

                    if (desktopBackgroundColorFound && modifierDesktopBackgroundColor != null)
                    {
                        desktopBackgroundColor = modifierDesktopBackgroundColor;
                    }
                    
                    if (disableGreenColorBackground)
                    {
                        __instance.myImage.color = Color.white;
                    }
                    else if (desktopBackgroundColor != null)
                    {
                        __instance.myImage.color = (Color) desktopBackgroundColor;
                    }
                    
                    Sprite setBackgroundSprite = null;
                    
                    if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay == 1 || customCampaign.savedGameFinishedDisplay == 1) // If we finished the campaign.
                    {
                        if (customCampaign.gameFinishedBackground != null)
                        {
                            setBackgroundSprite = customCampaign.gameFinishedBackground;
                        }
                        else
                        {
                            setBackgroundSprite = __instance.gameFinishedSprite;
                        }
                    }
                    else // Current Day Background instead.
                    {
                        if (customCampaign.backgroundSprites.Count > 0 
                            && GlobalVariables.currentDay <= customCampaign.backgroundSprites.Count) // We have backgrounds to replace.
                        {
                            setBackgroundSprite = customCampaign.backgroundSprites[GlobalVariables.currentDay - 1];
                        }
                        else
                        {
                            if (GlobalVariables.currentDay > __instance.spritesPerDay.Length) // Too many days for default image, we show first image.
                            {
                                setBackgroundSprite = __instance.spritesPerDay[0];
                            }
                            else
                            {
                                setBackgroundSprite = __instance.spritesPerDay[GlobalVariables.currentDay - 1];
                            }
                        }
                    }

                    bool desktopBackgroundsFound = false;
                    List<Sprite> desktopBackgrounds = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.desktopBackgrounds,
                        ref desktopBackgroundsFound,
                        v => v != null && v.Count > 0);
                    
                    bool gameFinishedBackgroundFound = false;
                    Sprite gameFinishedBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.gameFinishedBackground,
                        ref gameFinishedBackgroundFound,
                        v => v != null);
                    
                    bool unlockDaysFound = false;
                    List<int> unlockDays = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.unlockDays,
                        ref unlockDaysFound,
                        v => v != null && v.Count > 0);
                    
                    // Modifier
                    if (desktopBackgroundsFound && desktopBackgrounds != null && desktopBackgrounds.Count > 0) // Valid backgrounds given.
                    {
                        // Game Finished
                        if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay == 1 
                            || customCampaign.savedGameFinishedDisplay == 1)
                        {
                            if (gameFinishedBackgroundFound && gameFinishedBackground != null)
                            {
                                // Check if we are allowed to change it.
                                if (!unlockDaysFound || unlockDays == null || unlockDays.Count <= 0) // General Case. Always allowed.
                                {
                                    setBackgroundSprite = gameFinishedBackground;
                                }
                                else // Conditional (Days) Case:
                                {
                                    if (unlockDays.Contains(GlobalVariables.currentDay))
                                    {
                                        setBackgroundSprite = gameFinishedBackground;
                                    }
                                }
                            }
                        }
                        else // Not final day. 
                        {
                            // General Case:
                            if (!unlockDaysFound || unlockDays == null)
                            {
                                if (desktopBackgrounds.Count > 0 
                                    && GlobalVariables.currentDay <= desktopBackgrounds.Count) // Valid amount of backgrounds.
                                {
                                    setBackgroundSprite = desktopBackgrounds[(GlobalVariables.currentDay - 1) % desktopBackgrounds.Count];
                                }
                                // The else statement is handled already above,
                                // so we don't need to override it accidentally.
                            }
                            else if (unlockDays.Count > 0) // Conditional (Days) Case:
                            {
                                for (int i = 0; i < unlockDays.Count; i++)
                                {
                                    if (GlobalVariables.currentDay == unlockDays[i])
                                    {
                                        setBackgroundSprite = desktopBackgrounds[i % desktopBackgrounds.Count];
                                    }
                                }
                            }
                        }
                    }

                    if (setBackgroundSprite != null)
                    {
                        __instance.myImage.sprite = setBackgroundSprite;
                    }
                    else // Fallback
                    {
                        __instance.myImage.sprite = __instance.spritesPerDay[0];
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