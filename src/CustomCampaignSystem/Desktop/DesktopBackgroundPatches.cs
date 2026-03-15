using System.Collections.Generic;
using NewSafetyHelp.CallerPatches.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignPatches.Desktop
{
    public static class DesktopBackgroundPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(DayNumSpriteSwapper), "Start")]
        public static class StartPatch
        {
            private static GameObject animatedVideoBackground;
            
            /// <summary>
            /// Original function replaces background based on the day.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(DayNumSpriteSwapper __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return true;
                    }
                    
                    // Create animated video player for animated backgrounds
                    animatedVideoBackground = AnimatedImageHelper.CreateAnimatedPortrait(__instance.gameObject,
                        true, true, true);
                    
                    // If to disable the green color overlay.

                    bool disableGreenColorBackground = false;
                    Color? desktopBackgroundColor = null;
                    
                    if (customCampaign.DisableGreenColorBackground)
                    {
                        disableGreenColorBackground = true;
                    }
                    
                    bool disableColorBackgroundFound = false;
                    bool disableColorBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DisableColorBackground,
                        ref disableColorBackgroundFound);
                    
                    bool desktopBackgroundColorFound = false;
                    Color? modifierDesktopBackgroundColor = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DesktopBackgroundColor,
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
                    
                    Sprite setBackgroundSprite;
                    
                    // If we finished the campaign.
                    if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay == 1 
                        || customCampaign.SavedGameFinishedDisplay == 1) 
                    {
                        if (customCampaign.GameFinishedBackground != null)
                        {
                            setBackgroundSprite = customCampaign.GameFinishedBackground;
                        }
                        else
                        {
                            setBackgroundSprite = __instance.gameFinishedSprite;
                        }
                    }
                    else // Current Day Background instead.
                    {
                        // We have backgrounds to replace.
                        if (customCampaign.BackgroundSprites.Count > 0 
                            && GlobalVariables.currentDay <= customCampaign.BackgroundSprites.Count) 
                        {
                            setBackgroundSprite = customCampaign.BackgroundSprites[GlobalVariables.currentDay - 1];
                        }
                        else
                        {
                            // Too many days for default image, we show first image.
                            if (GlobalVariables.currentDay > __instance.spritesPerDay.Length) 
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
                        c => c.DesktopBackgrounds,
                        ref desktopBackgroundsFound,
                        v => v != null && v.Count > 0);
                    
                    bool gameFinishedBackgroundFound = false;
                    Sprite gameFinishedBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.GameFinishedBackground,
                        ref gameFinishedBackgroundFound,
                        v => v != null);
                    
                    bool unlockDaysFound = false;
                    List<int> unlockDays = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.UnlockDays,
                        ref unlockDaysFound,
                        v => v != null && v.Count > 0);
                    
                    // Modifier
                    if (desktopBackgroundsFound
                        && desktopBackgrounds != null
                        && desktopBackgrounds.Count > 0) // Valid backgrounds given.
                    {
                        // Game Finished
                        if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay == 1 
                            || customCampaign.SavedGameFinishedDisplay == 1)
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
                    
                    bool animatedBackgroundsFound = false;
                    List<string> animatedBackgrounds = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.AnimatedDesktopBackgrounds,
                        ref animatedBackgroundsFound,
                        v => v != null && v.Count > 0);
                    
                    bool removeBackgroundOnAnimatedBackgroundFound = false;
                    bool removeBackgroundOnAnimatedBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.BlackBackgroundOnAnimatedBackground,
                        ref removeBackgroundOnAnimatedBackgroundFound);

                    if (animatedBackgroundsFound)
                    {
                        if (removeBackgroundOnAnimatedBackgroundFound && removeBackgroundOnAnimatedBackground)
                        {
                            __instance.myImage.sprite = null;

                            if (!desktopBackgroundColorFound)
                            {
                                __instance.myImage.color = Color.black;
                            }
                        }
                        
                        // General Case:
                        if (!unlockDaysFound || unlockDays == null)
                        {
                            // We require a valid amount of backgrounds.
                            if (animatedBackgrounds.Count > 0 
                                && GlobalVariables.currentDay <= animatedBackgrounds.Count) 
                            {
                                AnimatedImageHelper.SetVideoUrl(
                                    animatedBackgrounds[(GlobalVariables.currentDay - 1) % animatedBackgrounds.Count],
                                    animatedVideoBackground
                                );
                            }
                        }
                        else if (unlockDays.Count > 0) // Conditional (Days) Case:
                        {
                            for (int i = 0; i < unlockDays.Count; i++)
                            {
                                if (GlobalVariables.currentDay == unlockDays[i])
                                {
                                    AnimatedImageHelper.SetVideoUrl(
                                        animatedBackgrounds[i % animatedBackgrounds.Count],
                                        animatedVideoBackground);
                                }
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