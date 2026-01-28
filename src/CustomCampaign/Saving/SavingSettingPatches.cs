using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Saving
{
    public static class SavingSettingPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "SaveOptions")]
        public static class SaveOptionsPatch
        {
            /// <summary>
            /// Changes the options save function to also handle the custom campaign if called in a custom campaign.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(SaveManagerBehavior __instance)
            {
                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
                    // Phobia Options
                    PlayerPrefs.SetInt("SavedSpiderToggle", __instance.savedSpiderToggle);
                    PlayerPrefs.SetInt("SavedInsectToggle", __instance.savedInsectToggle);
                    PlayerPrefs.SetInt("SavedDarkToggle", __instance.savedDarkToggle);
                    PlayerPrefs.SetInt("SavedHoleToggle", __instance.savedHoleToggle);
                    PlayerPrefs.SetInt("SavedWatchToggle", __instance.savedWatchToggle);
                    PlayerPrefs.SetInt("SavedDogToggle", __instance.savedDogToggle);
                    PlayerPrefs.SetInt("SavedTightToggle", __instance.savedTightToggle);
                    
                    // Dyslexia
                    PlayerPrefs.SetInt("SavedDyslexiaToggle", __instance.savedDyslexiaToggle);
                }
                
                if (GlobalVariables.isXmasDLC)
                {
                    MethodInfo saveXmasOptions = typeof(SaveManagerBehavior).GetMethod("SaveXmasOptions",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    if (saveXmasOptions == null)
                    {
                        MelonLogger.Error("ERROR: SaveXmasOptions method was not found. Calling original function.");
                        return true;
                    }

                    saveXmasOptions.Invoke(__instance, null); //__instance.SaveXmasOptions();
                }
                else
                {
                    PlayerPrefs.SetInt("SavedImmunityToggle", __instance.savedImmunityToggle);
                    PlayerPrefs.SetInt("SavedAccuracyToggle", __instance.savedAccuracyToggle);
                    PlayerPrefs.SetInt("SavedCallSkipToggle", __instance.savedCallSkipToggle);
                }

                __instance.savedScreenHeight = GlobalVariables.screenHeightSetting;
                __instance.savedScreenWidth = GlobalVariables.screenWidthSetting;
                PlayerPrefs.SetInt("SavedScreenHeight", __instance.savedScreenHeight);
                PlayerPrefs.SetInt("SavedScreenWidth", __instance.savedScreenWidth);

                __instance.savedRefreshRate = GlobalVariables.refreshRateSetting;
                PlayerPrefs.SetInt("SavedRefreshRate", __instance.savedRefreshRate);

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
                    // Text Settings
                    __instance.savedTextSizeMultiplier = GlobalVariables.textSizeMultiplier;
                    PlayerPrefs.SetFloat("SavedTextSizeMultiplier", __instance.savedTextSizeMultiplier);
                    
                    // Fullscreen
                    __instance.savedFullScreenToggle = __instance.BoolToInt(GlobalVariables.isFullScreen);
                    PlayerPrefs.SetInt("SavedFullScreenToggle", __instance.savedFullScreenToggle);
                    
                    // Screen effects
                    PlayerPrefs.SetInt("SavedCRTToggle", __instance.savedCRTToggle);
                    
                    // Volume
                    PlayerPrefs.SetFloat("SavedMusicVolume", __instance.savedMusicVolume);
                    PlayerPrefs.SetFloat("SavedSFXVolume", __instance.savedSFXVolume);
                    PlayerPrefs.SetFloat("SavedAmbienceVolume", __instance.savedAmbienceVolume);
                    
                    // Theme
                    PlayerPrefs.SetInt("SavedColorTheme", __instance.savedColorTheme);
                }
                else // Custom Campaign
                {
                    CustomCampaignOptionSaving.SaveCustomCampaignOptions();
                }

                PlayerPrefs.Save();

                return false; // Skip the original function
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "LoadSavedValues")]
        public static class LoadSavedValuesPatch
        {
            /// <summary>
            /// Patches the "LoadSaveValues" function to not use the save managers values and instead the custom campaigns ones.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(OptionsMenuBehavior __instance)
            {
                /*
                 * Default values to always be loaded.
                 *
                 */

                if (!GlobalVariables.saveManagerScript.hasLoaded)
                {
                    return false;
                }
                
                __instance.immunityToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedImmunityToggle);
                __instance.xmasImmunityToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedImmunityToggle);
                __instance.accuracyToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedAccuracyToggle);
                __instance.callSkipToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedCallSkipToggle);
               

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
                    // Phobia Options
                    __instance.spiderToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedSpiderToggle);
                    __instance.insectToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedInsectToggle);
                    __instance.darkToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDarkToggle);
                    __instance.holeToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedHoleToggle);
                    __instance.watchToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedWatchToggle);
                    __instance.tightToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedTightToggle);
                    __instance.dogToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDogToggle);
                    
                    // Text Options
                    __instance.dyslexiaToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDyslexiaToggle);
                    __instance.textSizeSlider.value = GlobalVariables.saveManagerScript.savedTextSizeMultiplier;
                    
                    // Screen Effects
                    __instance.crtToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedCRTToggle);
                    
                    // Volume
                    __instance.musicSlider.value = GlobalVariables.saveManagerScript.savedMusicVolume;
                    __instance.sfxSlider.value = GlobalVariables.saveManagerScript.savedSFXVolume;
                    __instance.masterSlider.value = GlobalVariables.saveManagerScript.savedAmbienceVolume;
                    
                    // Theme
                    __instance.colorDropdown.value = GlobalVariables.saveManagerScript.savedColorTheme;
                    __instance.ColorThemeChanged();
                }
                else // Custom Campaign
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null, unable of loading option values." +
                                          " Calling original function.");
                        return false;
                    }
                    
                    // Phobia Options
                    __instance.spiderToggle.isOn = customCampaign.SavedSpiderToggle;
                    __instance.insectToggle.isOn = customCampaign.SavedInsectToggle;
                    __instance.darkToggle.isOn = customCampaign.SavedDarkToggle;
                    __instance.holeToggle.isOn = customCampaign.SavedHoleToggle;
                    __instance.watchToggle.isOn = customCampaign.SavedWatchToggle;
                    __instance.tightToggle.isOn = customCampaign.SavedTightToggle;
                    __instance.dogToggle.isOn = customCampaign.SavedDogToggle;
                    
                    // Text Options
                    __instance.dyslexiaToggle.isOn = customCampaign.SavedDyslexiaToggle;
                    __instance.textSizeSlider.value = customCampaign.SavedTextSizeMultiplier;
                    
                    // Screen Effects
                    __instance.crtToggle.isOn = customCampaign.SavedCRTToggle;

                    // Volume
                    __instance.musicSlider.value = customCampaign.SavedMusicVolume;
                    __instance.sfxSlider.value = customCampaign.SavedSFXVolume;
                    __instance.masterSlider.value = customCampaign.SavedAmbienceVolume;
                    
                    // Theme
                    __instance.colorDropdown.value = customCampaign.ActiveTheme;
                    __instance.ColorThemeChanged();
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(LogoManager), "LoadVideoSettings")]
        public static class LoadVideoSettingsPatch
        {
            /// <summary>
            /// Patches the "LoadVideoSettings" function to not use the save managers values and instead the custom campaigns ones.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix()
            {
                if (GlobalVariables.saveManagerScript.savedScreenHeight == 0)
                {
                    return false;
                }

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    Screen.SetResolution(GlobalVariables.saveManagerScript.savedScreenHeight,
                        GlobalVariables.saveManagerScript.savedScreenWidth,
                        GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedFullScreenToggle));
                }
                else
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null. Unable to loading video settings. Calling original function.");
                        return true;
                    }
                    
                    Screen.SetResolution(GlobalVariables.saveManagerScript.savedScreenHeight,
                        GlobalVariables.saveManagerScript.savedScreenWidth,
                        customCampaign.SavedFullScreenToggle);
                }
                
                return false;
            }
        }
    }
}