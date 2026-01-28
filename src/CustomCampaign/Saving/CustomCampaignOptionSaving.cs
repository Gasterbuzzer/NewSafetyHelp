using MelonLoader;

namespace NewSafetyHelp.CustomCampaign.Saving
{
    public static class CustomCampaignOptionSaving
    {
        /// <summary>
        /// Initializes the custom campaign options once.
        /// </summary>
        private static void initializeCustomCampaignOptionsOnce()
        {
            CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }

            if (currentCampaign.CampaignSaveCategory == null)
            {
                currentCampaign.CampaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.CampaignName +
                    currentCampaign.CampaignDesktopName + currentCampaign.CampaignDays);
            }
            
            /*
             * Phobias
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedSpiderToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedSpiderToggle",false);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedInsectToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedInsectToggle",false);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDarkToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedDarkToggle",false);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedHoleToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedHoleToggle",false);
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedWatchToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedWatchToggle",false);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDogToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedDogToggle",false);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedTightToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedTightToggle",false);
            }
            
            /*
             * Text options
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDyslexiaToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedDyslexiaToggle",false);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedTextSizeMultiplier") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedTextSizeMultiplier",1.0f);
            }
            
            /*
             * Fullscreen Option
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedFullScreenToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedFullScreenToggle",true);
            }
            
            /*
             * Screen Effects
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedCRTToggle") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedCRTToggle",true);
            }
            
            /*
             * Volume
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedMusicVolume") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedMusicVolume",1.0f);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedSFXVolume") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedSFXVolume",1.0f);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedAmbienceVolume") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedAmbienceVolume",1.0f);
            }
            
            /*
             * Theme
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedColorTheme") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedColorTheme", 0);
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("themeShownOnce") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("themeShownOnce", false);
            }

            MelonPreferences.Save();
        }

        /// <summary>
        /// Saves the campaign options that matter for the custom campaign.
        /// </summary>
        public static void SaveCustomCampaignOptions()
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called save custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }
            
            // Custom Campaigns
            if (currentCampaign.CampaignSaveCategory == null)
            {
                // We get the category if not set yet.
                // We use this painful category name to avoid any conflicts.
                currentCampaign.CampaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.CampaignName +
                    currentCampaign.CampaignDesktopName + currentCampaign.CampaignDays);
            }

            // Check if it was ever saved before. If yes, load and if not then we call save once.
            initializeCustomCampaignOptionsOnce();
            
            /*
             * Phobias
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedSpiderToggle") == null)
            {
                MelonPreferences_Entry<bool> savedSpiderToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedSpiderToggle",false);
                
                savedSpiderToggle.Value = currentCampaign.SavedSpiderToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedSpiderToggle").Value = currentCampaign.SavedSpiderToggle;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedInsectToggle") == null)
            {
                MelonPreferences_Entry<bool> savedInsectToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedInsectToggle",false);
                
                savedInsectToggle.Value = currentCampaign.SavedInsectToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedInsectToggle").Value = currentCampaign.SavedInsectToggle;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDarkToggle") == null)
            {
                MelonPreferences_Entry<bool> savedDarkToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedDarkToggle",false);
                
                savedDarkToggle.Value = currentCampaign.SavedDarkToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDarkToggle").Value = currentCampaign.SavedDarkToggle;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedHoleToggle") == null)
            {
                MelonPreferences_Entry<bool> savedHoleToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedHoleToggle",false);
                
                savedHoleToggle.Value = currentCampaign.SavedHoleToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedHoleToggle").Value = currentCampaign.SavedHoleToggle;
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedWatchToggle") == null)
            {
                MelonPreferences_Entry<bool> savedWatchToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedWatchToggle",false);
                
                savedWatchToggle.Value = currentCampaign.SavedWatchToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedWatchToggle").Value = currentCampaign.SavedWatchToggle;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDogToggle") == null)
            {
                MelonPreferences_Entry<bool> savedDogToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedDogToggle",false);
                
                savedDogToggle.Value = currentCampaign.SavedDogToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDogToggle").Value = currentCampaign.SavedDogToggle;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedTightToggle") == null)
            {
                MelonPreferences_Entry<bool> savedTightToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedTightToggle",false);
                
                savedTightToggle.Value = currentCampaign.SavedTightToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedTightToggle").Value = currentCampaign.SavedTightToggle;
            }
            
            /*
             * Text options
             */
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDyslexiaToggle") == null)
            {
                MelonPreferences_Entry<bool> savedDyslexiaToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedDyslexiaToggle",false);
                
                savedDyslexiaToggle.Value = currentCampaign.SavedDyslexiaToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDyslexiaToggle").Value = currentCampaign.SavedDyslexiaToggle;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedTextSizeMultiplier") == null)
            {
                MelonPreferences_Entry<float> savedTextSizeMultiplier = currentCampaign.CampaignSaveCategory.CreateEntry("savedTextSizeMultiplier",1.0f);

                savedTextSizeMultiplier.Value = currentCampaign.SavedTextSizeMultiplier;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<float>("savedTextSizeMultiplier").Value = currentCampaign.SavedTextSizeMultiplier;
            }
            
            /*
             * Fullscreen Option
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedFullScreenToggle") == null)
            {
                MelonPreferences_Entry<bool> savedFullScreenToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedFullScreenToggle",true);

                savedFullScreenToggle.Value = currentCampaign.SavedFullScreenToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedFullScreenToggle").Value = currentCampaign.SavedFullScreenToggle;
            }
            
            /*
             * Screen Effects
             */
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedCRTToggle") == null)
            {
                MelonPreferences_Entry<bool> savedCRTToggle = currentCampaign.CampaignSaveCategory.CreateEntry("savedCRTToggle",true);

                savedCRTToggle.Value = currentCampaign.SavedCRTToggle;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedCRTToggle").Value = currentCampaign.SavedCRTToggle;
            }
            
            /*
             * Volume
             */
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedMusicVolume") == null)
            {
                MelonPreferences_Entry<float> savedMusicVolume = 
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedMusicVolume",1.0f);

                savedMusicVolume.Value = currentCampaign.SavedMusicVolume;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<float>("savedMusicVolume").Value = currentCampaign.SavedMusicVolume;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedSFXVolume") == null)
            {
                MelonPreferences_Entry<float> savedSFXVolume = currentCampaign.CampaignSaveCategory.CreateEntry("savedSFXVolume",1.0f);
                
                savedSFXVolume.Value = currentCampaign.SavedSFXVolume;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<float>("savedSFXVolume").Value = currentCampaign.SavedSFXVolume;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<float>("savedAmbienceVolume") == null)
            {
                MelonPreferences_Entry<float> savedAmbienceVolume = currentCampaign.CampaignSaveCategory.CreateEntry("savedAmbienceVolume",1.0f);
                
                savedAmbienceVolume.Value = currentCampaign.SavedAmbienceVolume;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<float>("savedAmbienceVolume").Value = currentCampaign.SavedAmbienceVolume;
            }
            
            /*
             * Theme
             */
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedColorTheme") == null)
            {
                MelonPreferences_Entry<int> savedColorTheme = currentCampaign.CampaignSaveCategory.CreateEntry(
                    "savedColorTheme", 0);
                
                savedColorTheme.Value = currentCampaign.ActiveTheme;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedColorTheme").Value = currentCampaign.ActiveTheme;
            }
            
            if (currentCampaign.CampaignSaveCategory.GetEntry<bool>("themeShownOnce") == null)
            {
                MelonPreferences_Entry<bool> themeShownOnce = currentCampaign.CampaignSaveCategory.CreateEntry(
                    "themeShownOnce", false);
                
                themeShownOnce.Value = currentCampaign.DefaultThemeAppliedOnce;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<bool>("themeShownOnce").Value = currentCampaign.DefaultThemeAppliedOnce;
            }
            
            MelonPreferences.Save();

            MelonLogger.Msg($"INFO: Finished saving (Options) for the custom campaign {currentCampaign.CampaignName}.");
        }

        /// <summary>
        /// Loads the options for the current custom campaign.
        /// </summary>
        public static void LoadCustomCampaignOptions()
        {
            MelonLogger.Msg("INFO: Starting to load all custom campaign settings/options.");
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called load custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignModel.CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of loading.");
                return;
            }

            // Custom Campaigns
            // We use this painful category name to avoid any conflicts.
            if (currentCampaign.CampaignSaveCategory == null)
            {
                // We get the category if not set yet.
                currentCampaign.CampaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.CampaignName +
                    currentCampaign.CampaignDesktopName + currentCampaign.CampaignDays);
            }

            // Check if it was ever saved before. If yes, load and if not then we call save once.
            initializeCustomCampaignOptionsOnce();
            
            #if DEBUG
                MelonLogger.Msg($"DEBUG: Saved color themes ({currentCampaign.CampaignSaveCategory.GetEntry<int>("savedColorTheme").Value}).");
            #endif
            
            // Load all values first into the currentCampaign instance.
            
            /*
             * Phobia Options
             */
            currentCampaign.SavedSpiderToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedSpiderToggle").Value;
            currentCampaign.SavedInsectToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedInsectToggle").Value;
            currentCampaign.SavedDarkToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDarkToggle").Value;
            currentCampaign.SavedDogToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDogToggle").Value;
            currentCampaign.SavedWatchToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedWatchToggle").Value;
            currentCampaign.SavedHoleToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedHoleToggle").Value;
            currentCampaign.SavedTightToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedTightToggle").Value;
            
            /*
             * Text Options
             */
            currentCampaign.SavedDyslexiaToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedDyslexiaToggle").Value;
            currentCampaign.SavedTextSizeMultiplier = currentCampaign.CampaignSaveCategory.GetEntry<float>("savedTextSizeMultiplier").Value;
            
            /*
             * Fullscreen Option
             */
            currentCampaign.SavedFullScreenToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedFullScreenToggle").Value;
            
            /*
             * Screen Effects
             */
            currentCampaign.SavedCRTToggle = currentCampaign.CampaignSaveCategory.GetEntry<bool>("savedCRTToggle").Value;
            
            /*
             * Volume
             */
            currentCampaign.SavedMusicVolume = currentCampaign.CampaignSaveCategory.GetEntry<float>("savedMusicVolume").Value;
            currentCampaign.SavedSFXVolume = currentCampaign.CampaignSaveCategory.GetEntry<float>("savedSFXVolume").Value;
            currentCampaign.SavedAmbienceVolume = currentCampaign.CampaignSaveCategory.GetEntry<float>("savedAmbienceVolume").Value;
            
            /*
             * Theme
             */
            currentCampaign.ActiveTheme = currentCampaign.CampaignSaveCategory.GetEntry<int>("savedColorTheme").Value;
            
            // If we have applied the default theme at least once.
            currentCampaign.DefaultThemeAppliedOnce = currentCampaign.CampaignSaveCategory.GetEntry<bool>("themeShownOnce").Value;
            
            /*
             * Load the values into actual game values now.
             */
            
            /*
             * Phobia Options
             */
            GlobalVariables.saveManagerScript.savedSpiderToggle = currentCampaign.SavedSpiderToggle ? 1 : 0;
            GlobalVariables.saveManagerScript.savedInsectToggle = currentCampaign.SavedInsectToggle ? 1 : 0;
            GlobalVariables.saveManagerScript.savedDarkToggle = currentCampaign.SavedDarkToggle ? 1 : 0;
            GlobalVariables.saveManagerScript.savedDogToggle = currentCampaign.SavedDogToggle ? 1 : 0;
            GlobalVariables.saveManagerScript.savedWatchToggle = currentCampaign.SavedWatchToggle ? 1 : 0;
            GlobalVariables.saveManagerScript.savedHoleToggle = currentCampaign.SavedHoleToggle ? 1 : 0;
            GlobalVariables.saveManagerScript.savedTightToggle = currentCampaign.SavedTightToggle ? 1 : 0;
            
            /*
             * Text Options
             */
            GlobalVariables.saveManagerScript.savedDyslexiaToggle = currentCampaign.SavedDyslexiaToggle ? 1 : 0;
            GlobalVariables.dyslexiaMode = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedDyslexiaToggle);

            GlobalVariables.saveManagerScript.savedTextSizeMultiplier = currentCampaign.SavedTextSizeMultiplier;
            GlobalVariables.textSizeMultiplier = GlobalVariables.saveManagerScript.savedTextSizeMultiplier;
            
            /*
             * Fullscreen Option
             */
            GlobalVariables.saveManagerScript.savedFullScreenToggle = currentCampaign.SavedFullScreenToggle ? 1 : 0;
            GlobalVariables.isFullScreen = GlobalVariables.saveManagerScript.IntToBool(GlobalVariables.saveManagerScript.savedFullScreenToggle);
            
            /*
             * Screen Effects
             */
            GlobalVariables.saveManagerScript.savedCRTToggle = currentCampaign.SavedCRTToggle ? 1 : 0;
            
            /*
             * Volume
             */
            GlobalVariables.saveManagerScript.savedMusicVolume = currentCampaign.SavedMusicVolume;
            GlobalVariables.saveManagerScript.savedSFXVolume = currentCampaign.SavedSFXVolume;
            GlobalVariables.saveManagerScript.savedAmbienceVolume = currentCampaign.SavedAmbienceVolume;
            
            /*
             * Theme
             */
            GlobalVariables.saveManagerScript.savedColorTheme = currentCampaign.ActiveTheme;
            
            if ((bool)GlobalVariables.colorPaletteController)
            {
                GlobalVariables.colorPaletteController.UpdateColorTheme();
            }

            // Finished loading.
            MelonLogger.Msg("INFO: Finished loading all custom campaign settings/options.");
        }
    }
}