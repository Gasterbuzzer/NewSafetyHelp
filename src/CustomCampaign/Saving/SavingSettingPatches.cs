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
                PlayerPrefs.SetInt("SavedSpiderToggle", __instance.savedSpiderToggle);
                PlayerPrefs.SetInt("SavedInsectToggle", __instance.savedInsectToggle);
                PlayerPrefs.SetInt("SavedDarkToggle", __instance.savedDarkToggle);
                PlayerPrefs.SetInt("SavedHoleToggle", __instance.savedHoleToggle);
                PlayerPrefs.SetInt("SavedWatchToggle", __instance.savedWatchToggle);
                PlayerPrefs.SetInt("SavedDogToggle", __instance.savedDogToggle);
                PlayerPrefs.SetInt("SavedDyslexiaToggle", __instance.savedDyslexiaToggle);

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

                __instance.savedFullScreenToggle = __instance.BoolToInt(GlobalVariables.isFullScreen);
                PlayerPrefs.SetInt("SavedFullScreenToggle", __instance.savedFullScreenToggle);

                __instance.savedScreenHeight = GlobalVariables.screenHeightSetting;
                __instance.savedScreenWidth = GlobalVariables.screenWidthSetting;
                PlayerPrefs.SetInt("SavedScreenHeight", __instance.savedScreenHeight);
                PlayerPrefs.SetInt("SavedScreenWidth", __instance.savedScreenWidth);

                __instance.savedRefreshRate = GlobalVariables.refreshRateSetting;
                PlayerPrefs.SetInt("SavedRefreshRate", __instance.savedRefreshRate);

                __instance.savedTextSizeMultiplier = GlobalVariables.textSizeMultiplier;
                PlayerPrefs.SetFloat("SavedTextSizeMultiplier", __instance.savedTextSizeMultiplier);

                PlayerPrefs.SetInt("SavedCRTToggle", __instance.savedCRTToggle);

                /*
                 * Specially handled settings.
                 */

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
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

                __instance.spiderToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedSpiderToggle);
                __instance.insectToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedInsectToggle);
                __instance.darkToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDarkToggle);
                __instance.holeToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedHoleToggle);
                __instance.watchToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedWatchToggle);
                __instance.tightToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedTightToggle);
                __instance.dogToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDogToggle);
                __instance.dyslexiaToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDyslexiaToggle);
                __instance.crtToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedCRTToggle);
                __instance.immunityToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedImmunityToggle);
                __instance.xmasImmunityToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedImmunityToggle);
                __instance.accuracyToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedAccuracyToggle);
                __instance.callSkipToggle.isOn =
                    GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedCallSkipToggle);
                __instance.textSizeSlider.value = GlobalVariables.saveManagerScript.savedTextSizeMultiplier;

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
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
    }
}