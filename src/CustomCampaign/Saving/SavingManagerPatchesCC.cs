using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CustomCampaign.Saving
{
    public static class SavingManagerPatchesCC
    {
        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "SaveGameProgress", new Type[] { })]
        public static class SaveGameProgressPatch
        {
            /// <summary>
            /// Changes the save function to respect custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, SaveManagerBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign) // Use Custom Campaign saving.
                {
                    CustomCampaignSaving.SaveCustomCampaignInfo();
                }
                else if (GlobalVariables.isXmasDLC)
                {
                    MethodInfo _saveXmasGameProgress = typeof(SaveManagerBehavior).GetMethod("SaveXmasGameProgress",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_saveXmasGameProgress == null)
                    {
                        MelonLogger.Error("ERROR: Method 'SaveXmasGameProgress' was null. Calling normal function.");
                        return true;
                    }

                    _saveXmasGameProgress.Invoke(__instance, null); // __instance.SaveXmasGameProgress();
                }
                else
                {
                    PlayerPrefs.SetInt("SavedDay", __instance.savedDay);
                    PlayerPrefs.SetInt("SavedCurrentCaller", __instance.savedCurrentCaller);
                    PlayerPrefs.SetInt("SavedEntryTier", __instance.savedEntryTier);
                    PlayerPrefs.SetInt("SavedCallerArrayLength", __instance.savedCallerArrayLength);
                    __instance.SaveBoolArray("SavedCallerCorrectAnswer", __instance.savedCallerCorrectAnswers);
                    PlayerPrefs.Save();
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "SaveGameFinished", new Type[] { })]
        public static class SaveGameFinishedPatch
        {
            /// <summary>
            /// Changes the save function to respect custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, SaveManagerBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign) // Use Custom Campaign saving.
                {
                    CustomCampaignSaving.SaveCustomCampaignInfo();
                }
                else if (GlobalVariables.isXmasDLC)
                {
                    MethodInfo _saveXmasGameFinished = typeof(SaveManagerBehavior).GetMethod("SaveXmasGameFinished",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_saveXmasGameFinished == null)
                    {
                        MelonLogger.Error("ERROR: Method 'SaveXmasGameFinished' was null. Calling normal function.");
                        return true;
                    }

                    _saveXmasGameFinished.Invoke(__instance, null); // __instance.SaveXmasGameFinished();
                }
                else
                {
                    PlayerPrefs.SetInt("SavedGameFinished", __instance.savedGameFinished);
                    PlayerPrefs.SetInt("SavedGameFinishedDisplay", __instance.savedGameFinishedDisplay);
                    PlayerPrefs.Save();
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "Load", new Type[] { })]
        public static class LoadPatch
        {
            /// <summary>
            /// Changes the load function to respect custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, SaveManagerBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    __instance.hasLoaded = false;

                    // Load custom campaign values.
                    CustomCampaignSaving.LoadFromFileCustomCampaignInfo();
                    
                }
                else if (GlobalVariables.isXmasDLC)
                {
                    MethodInfo _loadXmas = typeof(SaveManagerBehavior).GetMethod("LoadXmas",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (_loadXmas == null)
                    {
                        MelonLogger.Error("ERROR: Method 'LoadXmas' was null. Calling normal function.");
                        return true;
                    }

                    _loadXmas.Invoke(__instance, null); // __instance.LoadXmas();
                }
                else
                {
                    __instance.hasLoaded = false;

                    if (PlayerPrefs.HasKey("SavedDay"))
                    {
                        __instance.savedDay = PlayerPrefs.GetInt("SavedDay");
                    }

                    GlobalVariables.currentDay = __instance.savedDay;

                    if (PlayerPrefs.HasKey("SavedCurrentCaller"))
                    {
                        __instance.savedCurrentCaller = PlayerPrefs.GetInt("SavedCurrentCaller");
                    }

                    if (PlayerPrefs.HasKey("SavedEntryTier"))
                    {
                        __instance.savedEntryTier = PlayerPrefs.GetInt("SavedEntryTier");
                    }

                    if (PlayerPrefs.HasKey("SavedCallerArrayLength"))
                    {
                        __instance.savedCallerArrayLength = PlayerPrefs.GetInt("SavedCallerArrayLength");
                    }

                    __instance.savedCallerCorrectAnswers = __instance.LoadBoolArray("SavedCallerCorrectAnswer",
                        __instance.savedCallerArrayLength);
                    
                                    // Options
                // These are allowed to be loaded without checking if in a custom campaign or not.
                if (PlayerPrefs.HasKey("SavedSpiderToggle"))
                {
                    __instance.savedSpiderToggle = PlayerPrefs.GetInt("SavedSpiderToggle");
                }

                if (PlayerPrefs.HasKey("SavedInsectToggle"))
                {
                    __instance.savedInsectToggle = PlayerPrefs.GetInt("SavedInsectToggle");
                }

                if (PlayerPrefs.HasKey("SavedDarkToggle"))
                {
                    __instance.savedDarkToggle = PlayerPrefs.GetInt("SavedDarkToggle");
                }

                if (PlayerPrefs.HasKey("SavedHoleToggle"))
                {
                    __instance.savedHoleToggle = PlayerPrefs.GetInt("SavedHoleToggle");
                }

                if (PlayerPrefs.HasKey("SavedWatchToggle"))
                {
                    __instance.savedWatchToggle = PlayerPrefs.GetInt("SavedWatchToggle");
                }

                if (PlayerPrefs.HasKey("SavedDogToggle"))
                {
                    __instance.savedDogToggle = PlayerPrefs.GetInt("SavedDogToggle");
                }

                if (PlayerPrefs.HasKey("SavedDyslexiaToggle"))
                {
                    __instance.savedDyslexiaToggle = PlayerPrefs.GetInt("SavedDyslexiaToggle");
                }

                GlobalVariables.dyslexiaMode = __instance.IntToBool(__instance.savedDyslexiaToggle);

                if (PlayerPrefs.HasKey("SavedCRTToggle"))
                {
                    __instance.savedCRTToggle = PlayerPrefs.GetInt("SavedCRTToggle", 1);
                }

                if (PlayerPrefs.HasKey("SavedMusicVolume"))
                {
                    __instance.savedMusicVolume = PlayerPrefs.GetFloat("SavedMusicVolume");
                }

                if (PlayerPrefs.HasKey("SavedSFXVolume"))
                {
                    __instance.savedSFXVolume = PlayerPrefs.GetFloat("SavedSFXVolume");
                }

                if (PlayerPrefs.HasKey("SavedAmbienceVolume"))
                {
                    __instance.savedAmbienceVolume = PlayerPrefs.GetFloat("SavedAmbienceVolume");
                }

                if (PlayerPrefs.HasKey("SavedTextSizeMultiplier"))
                {
                    __instance.savedTextSizeMultiplier = PlayerPrefs.GetFloat("SavedTextSizeMultiplier");
                }

                GlobalVariables.textSizeMultiplier = __instance.savedTextSizeMultiplier;

                if (PlayerPrefs.HasKey("SavedFullScreenToggle"))
                {
                    __instance.savedFullScreenToggle = PlayerPrefs.GetInt("SavedFullScreenToggle");
                }

                GlobalVariables.isFullScreen = __instance.IntToBool(__instance.savedFullScreenToggle);

                if (PlayerPrefs.HasKey("SavedScreenHeight"))
                {
                    __instance.savedScreenHeight = PlayerPrefs.GetInt("SavedScreenHeight");
                }

                GlobalVariables.screenHeightSetting = __instance.savedScreenHeight;

                if (PlayerPrefs.HasKey("SavedScreenWidth"))
                {
                    __instance.savedScreenWidth = PlayerPrefs.GetInt("SavedScreenWidth");
                }

                GlobalVariables.screenWidthSetting = __instance.savedScreenWidth;
                
                if (PlayerPrefs.HasKey("SavedImmunityToggle"))
                {
                    __instance.savedImmunityToggle = PlayerPrefs.GetInt("SavedImmunityToggle", 0);
                }

                if (PlayerPrefs.HasKey("SavedAccuracyToggle"))
                {
                    __instance.savedAccuracyToggle = PlayerPrefs.GetInt("SavedAccuracyToggle", 0);
                }

                if (PlayerPrefs.HasKey("SavedCallSkipToggle"))
                {
                    __instance.savedCallSkipToggle = PlayerPrefs.GetInt("SavedCallSkipToggle", 0);
                }

                if (PlayerPrefs.HasKey("SavedRefreshRate"))
                {
                    __instance.savedRefreshRate = PlayerPrefs.GetInt("SavedRefreshRate");
                }
                
                GlobalVariables.refreshRateSetting = __instance.savedRefreshRate;

                    // Custom Campaign Magic. The values for custom campaigns are loaded beforehand. So no need for else.
                    if (!CustomCampaignGlobal.InCustomCampaign)
                    {
                        if (PlayerPrefs.HasKey("SavedGameFinished"))
                        {
                            __instance.savedGameFinished = PlayerPrefs.GetInt("SavedGameFinished");
                        }

                        if (PlayerPrefs.HasKey("SavedGameFinishedDisplay"))
                        {
                            __instance.savedGameFinishedDisplay = PlayerPrefs.GetInt("SavedGameFinishedDisplay");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore1"))
                        {
                            __instance.savedDayScore1 = PlayerPrefs.GetFloat("SavedDayScore1");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore2"))
                        {
                            __instance.savedDayScore2 = PlayerPrefs.GetFloat("SavedDayScore2");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore3"))
                        {
                            __instance.savedDayScore3 = PlayerPrefs.GetFloat("SavedDayScore3");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore4"))
                        {
                            __instance.savedDayScore4 = PlayerPrefs.GetFloat("SavedDayScore4");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore5"))
                        {
                            __instance.savedDayScore5 = PlayerPrefs.GetFloat("SavedDayScore5");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore6"))
                        {
                            __instance.savedDayScore6 = PlayerPrefs.GetFloat("SavedDayScore6");
                        }

                        if (PlayerPrefs.HasKey("SavedDayScore7"))
                        {
                            __instance.savedDayScore7 = PlayerPrefs.GetFloat("SavedDayScore7");
                        }
                    }
                    
                    if (PlayerPrefs.HasKey("SavedArcadeScore"))
                    {
                        __instance.savedArcadeScore = PlayerPrefs.GetFloat("SavedArcadeScore");
                    }
                }

                /*
                 * Special loading. (Options that are saved differently)
                 */

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaignSaving.LoadCustomCampaignOptions();
                    
                    __instance.hasLoaded = true;

                    if (__instance.PostLoadEvent == null)
                    {
                        return false;
                    }

                    __instance.PostLoadEvent();
                }
                else // Everything else
                {
                    if (PlayerPrefs.HasKey("SavedColorTheme"))
                    {
                        __instance.savedColorTheme = PlayerPrefs.GetInt("SavedColorTheme");
                    }

                    if ((bool)GlobalVariables.colorPaletteController)
                    {
                        GlobalVariables.colorPaletteController.UpdateColorTheme();
                    }
                    
                    __instance.hasLoaded = true;
                    
                    if (__instance.PostLoadEvent == null)
                    {
                        return false;
                    }

                    __instance.PostLoadEvent();
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "DeleteGameProgress", new Type[] { })]
        public static class DeleteGameProgressPatch
        {
            /// <summary>
            /// Changes the delete save function to delete the custom campaign if called in a custom campaign.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, SaveManagerBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign) // Use Custom Campaign saving.
                {
                    CustomCampaignSaving.ResetCustomCampaignFile();
                    SceneManager.LoadScene("MainMenuScene"); // Reload Scene
                }
                else if (GlobalVariables.isXmasDLC)
                {
                    MethodInfo _deleteXmasGameProgress = typeof(SaveManagerBehavior).GetMethod("DeleteXmasGameProgress",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                    if (_deleteXmasGameProgress == null)
                    {
                        MelonLogger.Error(
                            "ERROR: DeleteXmasGameProgress method was not found. Calling default function.");
                        return true;
                    }

                    _deleteXmasGameProgress.Invoke(__instance, null); // __instance.DeleteXmasGameProgress();
                }
                else
                {
                    PlayerPrefs.DeleteKey("SavedDay");
                    __instance.savedDay = 1;

                    PlayerPrefs.DeleteKey("SavedCurrentCaller");
                    __instance.savedCurrentCaller = 0;

                    PlayerPrefs.DeleteKey("SavedEntryTier");
                    __instance.savedEntryTier = 1;

                    PlayerPrefs.DeleteKey("SavedDayScore1");
                    PlayerPrefs.DeleteKey("SavedDayScore2");
                    PlayerPrefs.DeleteKey("SavedDayScore3");
                    PlayerPrefs.DeleteKey("SavedDayScore4");
                    PlayerPrefs.DeleteKey("SavedDayScore5");
                    PlayerPrefs.DeleteKey("SavedDayScore6");
                    PlayerPrefs.DeleteKey("SavedDayScore7");

                    __instance.DeleteBoolArray("SavedCallerCorrectAnswer", __instance.savedCallerArrayLength);
                    __instance.savedCallerCorrectAnswers = null;

                    PlayerPrefs.DeleteKey("SavedCallerArrayLength");
                    __instance.savedCallerArrayLength = 0;

                    PlayerPrefs.DeleteKey("SavedGameFinishedDisplay");
                    __instance.savedGameFinishedDisplay = 0;

                    MelonLogger.Msg($"UNITY: Game Progress Save Data Deleted.");

                    SceneManager.LoadScene("MainMenuScene");
                }

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "SaveOptions", new Type[] { })]
        public static class SaveOptionsPatch
        {
            /// <summary>
            /// Changes the options save function to also handle the custom campaign if called in a custom campaign.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, SaveManagerBehavior __instance)
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

                PlayerPrefs.SetFloat("SavedMusicVolume", __instance.savedMusicVolume);
                PlayerPrefs.SetFloat("SavedSFXVolume", __instance.savedSFXVolume);
                PlayerPrefs.SetFloat("SavedAmbienceVolume", __instance.savedAmbienceVolume);

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
                    PlayerPrefs.SetInt("SavedColorTheme", __instance.savedColorTheme);
                }
                else // Custom Campaign
                {
                    CustomCampaignSaving.SaveCustomCampaignOptions();
                }

                PlayerPrefs.Save();

                return false; // Skip the original function
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "LoadSavedValues", new Type[] { })]
        public static class SaveResolutionInfoPatch
        {
            /// <summary>
            /// Patches the "LoadSaveValues" function to not use the save managers values and instead the custom campaigns ones.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, OptionsMenuBehavior __instance)
            {
                /*
                 * Default values to always be loaded.
                 *
                 */
                
                if (!GlobalVariables.saveManagerScript.hasLoaded)
                {
                    return false;
                }
                
                __instance.spiderToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedSpiderToggle);
                __instance.insectToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedInsectToggle);
                __instance.darkToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDarkToggle);
                __instance.holeToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedHoleToggle);
                __instance.watchToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedWatchToggle);
                __instance.tightToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedTightToggle);
                __instance.dogToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDogToggle);
                __instance.dyslexiaToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDyslexiaToggle);
                __instance.crtToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedCRTToggle);
                __instance.immunityToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedImmunityToggle);
                __instance.xmasImmunityToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedImmunityToggle);
                __instance.accuracyToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedAccuracyToggle);
                __instance.callSkipToggle.isOn = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedCallSkipToggle);
                __instance.musicSlider.value = GlobalVariables.saveManagerScript.savedMusicVolume;
                __instance.sfxSlider.value = GlobalVariables.saveManagerScript.savedSFXVolume;
                __instance.masterSlider.value = GlobalVariables.saveManagerScript.savedAmbienceVolume;
                __instance.textSizeSlider.value = GlobalVariables.saveManagerScript.savedTextSizeMultiplier;
                
                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
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

                    __instance.colorDropdown.value = customCampaign.activeTheme;
                    __instance.ColorThemeChanged();
                }
                
                return false; // Skip the original function
            }
        }
    }
}