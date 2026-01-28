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
            
            // Volume
            
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
            
            // Theme
            
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