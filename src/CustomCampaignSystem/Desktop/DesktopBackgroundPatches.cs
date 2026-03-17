using System.Collections.Generic;
using NewSafetyHelp.Callers.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Desktop
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
                    
                    (bool foundModifier, bool value) disableColorBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DisableColorBackground);
                    
                    (bool foundModifier, Color? value) modifierDesktopBackgroundColor = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DesktopBackgroundColor,
                        v => v != null);

                    if (disableColorBackground.foundModifier && disableColorBackground.value)
                    {
                        disableGreenColorBackground = true;
                    }

                    if (modifierDesktopBackgroundColor.foundModifier)
                    {
                        desktopBackgroundColor = modifierDesktopBackgroundColor.value;
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
                    
                    (bool foundModifier, List<Sprite> value) desktopBackgrounds = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DesktopBackgrounds,
                        v => v != null && v.Count > 0);
                    
                    (bool foundModifier, Sprite value) gameFinishedBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.GameFinishedBackground,
                        specialPredicate: cM => cM.GameFinishedBackgroundChanged);
                    
                    (bool foundModifier, List<int> value) unlockDays = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.UnlockDays,
                        v => v != null && v.Count > 0);
                    
                    // Valid backgrounds given.
                    if (desktopBackgrounds.foundModifier) 
                    {
                        // Game Finished
                        if (GlobalVariables.saveManagerScript.savedGameFinishedDisplay == 1 
                            || customCampaign.SavedGameFinishedDisplay == 1)
                        {
                            if (gameFinishedBackground.foundModifier)
                            {
                                // Check if we are allowed to change it.
                                // General Case. Always allowed.
                                if (!unlockDays.foundModifier 
                                    || unlockDays.value == null 
                                    || unlockDays.value.Count <= 0) 
                                {
                                    setBackgroundSprite = gameFinishedBackground.value;
                                }
                                else // Conditional (Days) Case:
                                {
                                    if (unlockDays.value.Contains(GlobalVariables.currentDay))
                                    {
                                        setBackgroundSprite = gameFinishedBackground.value;
                                    }
                                }
                            }
                        }
                        else // Not final day. 
                        {
                            // General Case:
                            if (!unlockDays.foundModifier || unlockDays.value == null)
                            {
                                // Valid amount of backgrounds.
                                if (desktopBackgrounds.value.Count > 0 
                                    && GlobalVariables.currentDay <= desktopBackgrounds.value.Count) 
                                {
                                    setBackgroundSprite = desktopBackgrounds.value[(GlobalVariables.currentDay - 1) % desktopBackgrounds.value.Count];
                                }
                                // The else statement is handled already above,
                                // so we don't need to override it accidentally.
                            }
                            else if (unlockDays.value.Count > 0) // Conditional (Days) Case:
                            {
                                for (int i = 0; i < unlockDays.value.Count; i++)
                                {
                                    if (GlobalVariables.currentDay == unlockDays.value[i])
                                    {
                                        setBackgroundSprite = desktopBackgrounds.value[i % desktopBackgrounds.value.Count];
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
                    
                    (bool foundModifier, List<string> value) animatedBackgrounds = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.AnimatedDesktopBackgrounds,
                        v => v != null && v.Count > 0);
                    
                    (bool foundModifier, bool value) removeBackgroundOnAnimatedBackground = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.BlackBackgroundOnAnimatedBackground);

                    if (animatedBackgrounds.foundModifier)
                    {
                        if (removeBackgroundOnAnimatedBackground.foundModifier 
                            && removeBackgroundOnAnimatedBackground.value)
                        {
                            __instance.myImage.sprite = null;

                            if (!modifierDesktopBackgroundColor.foundModifier)
                            {
                                __instance.myImage.color = Color.black;
                            }
                        }
                        
                        // General Case:
                        if (!unlockDays.foundModifier || unlockDays.value == null)
                        {
                            // We require a valid amount of backgrounds.
                            if (animatedBackgrounds.value.Count > 0 
                                && GlobalVariables.currentDay <= animatedBackgrounds.value.Count) 
                            {
                                AnimatedImageHelper.SetVideoUrl(
                                    animatedBackgrounds.value[(GlobalVariables.currentDay - 1) % animatedBackgrounds.value.Count],
                                    animatedVideoBackground
                                );
                            }
                        }
                        // Conditional (Days) Case:
                        else if (unlockDays.value.Count > 0) 
                        {
                            for (int i = 0; i < unlockDays.value.Count; i++)
                            {
                                if (GlobalVariables.currentDay == unlockDays.value[i])
                                {
                                    AnimatedImageHelper.SetVideoUrl(
                                        animatedBackgrounds.value[i % animatedBackgrounds.value.Count],
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