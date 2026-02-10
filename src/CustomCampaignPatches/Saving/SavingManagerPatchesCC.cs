using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CustomCampaignPatches.Saving
{
    public static class SavingManagerPatchesCC
    {
        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "SaveGameProgress")]
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
                    MethodInfo saveXmasGameProgress = typeof(SaveManagerBehavior).GetMethod("SaveXmasGameProgress",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (saveXmasGameProgress == null)
                    {
                        MelonLogger.Error("ERROR: Method 'SaveXmasGameProgress' was null. Calling normal function.");
                        return true;
                    }

                    saveXmasGameProgress.Invoke(__instance, null); // __instance.SaveXmasGameProgress();
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

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "SaveGameFinished")]
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
                    MethodInfo saveXmasGameFinished = typeof(SaveManagerBehavior).GetMethod("SaveXmasGameFinished",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                    if (saveXmasGameFinished == null)
                    {
                        MelonLogger.Error("ERROR: Method 'SaveXmasGameFinished' was null. Calling normal function.");
                        return true;
                    }

                    saveXmasGameFinished.Invoke(__instance, null); // __instance.SaveXmasGameFinished();
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

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "Load")]
        public static class LoadPatch
        {
            /// <summary>
            /// Changes the load function to respect custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, SaveManagerBehavior __instance)
            {
                // We are loading now.
                __instance.hasLoaded = false;

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    // Load custom campaign values.
                    CustomCampaignSaving.LoadFromFileCustomCampaignInfo();
                }
                else if (GlobalVariables.isXmasDLC) // DLC
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

                /*
                 * Special loading. (Options that are saved differently)
                 */

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaignOptionSaving.LoadCustomCampaignOptions();
                }
                else if (!GlobalVariables.isXmasDLC) // Everything else, except for DLC, since the DLC handles itself.
                {
                    /*
                     * Cheat Options
                     */
                    
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
                    
                    /*
                     * Screen Settings
                     */
                    
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
                    
                    if (PlayerPrefs.HasKey("SavedRefreshRate"))
                    {
                        __instance.savedRefreshRate = PlayerPrefs.GetInt("SavedRefreshRate");
                    }
                    GlobalVariables.refreshRateSetting = __instance.savedRefreshRate;
                    
                    /*
                     * Phobias
                     */
                    
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
                    
                    if (PlayerPrefs.HasKey("SavedTightToggle"))
                    {
                        __instance.savedTightToggle = PlayerPrefs.GetInt("SavedTightToggle");
                    }
                    
                    /*
                     * Saved Values
                     */
                    
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
                    
                    /*
                     * Text options
                     */
                    
                    if (PlayerPrefs.HasKey("SavedDyslexiaToggle"))
                    {
                        __instance.savedDyslexiaToggle = PlayerPrefs.GetInt("SavedDyslexiaToggle");
                    }

                    GlobalVariables.dyslexiaMode = __instance.IntToBool(__instance.savedDyslexiaToggle);

                    if (PlayerPrefs.HasKey("SavedTextSizeMultiplier"))
                    {
                        __instance.savedTextSizeMultiplier = PlayerPrefs.GetFloat("SavedTextSizeMultiplier");
                    }

                    GlobalVariables.textSizeMultiplier = __instance.savedTextSizeMultiplier;
                    
                    /*
                     * Fullscreen Option
                     */
                    
                    if (PlayerPrefs.HasKey("SavedFullScreenToggle"))
                    {
                        __instance.savedFullScreenToggle = PlayerPrefs.GetInt("SavedFullScreenToggle");
                    }

                    GlobalVariables.isFullScreen = __instance.IntToBool(__instance.savedFullScreenToggle);
                    
                    /*
                     * Screen Effects
                     */
                    
                    if (PlayerPrefs.HasKey("SavedCRTToggle"))
                    {
                        __instance.savedCRTToggle = PlayerPrefs.GetInt("SavedCRTToggle", 1);
                    }
                    
                    /*
                     * Volume
                     */

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

                    /*
                     * Theme
                     */

                    if (PlayerPrefs.HasKey("SavedColorTheme"))
                    {
                        __instance.savedColorTheme = PlayerPrefs.GetInt("SavedColorTheme");
                    }

                    if ((bool)GlobalVariables.colorPaletteController)
                    {
                        GlobalVariables.colorPaletteController.UpdateColorTheme();
                    }

                    /*
                     * Saved Game Scores
                     */

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
                    
                    if (PlayerPrefs.HasKey("SavedArcadeScore"))
                    {
                        __instance.savedArcadeScore = PlayerPrefs.GetFloat("SavedArcadeScore");
                    }
                }

                // Finished Loading

                __instance.hasLoaded = true;

                if (__instance.PostLoadEvent == null)
                {
                    return false;
                }

                __instance.PostLoadEvent();

                return false; // Skip the original function
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveManagerBehavior), "DeleteGameProgress")]
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
    }
}