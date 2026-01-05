using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Saving
{
    public static class CustomCampaignSaving
    {
        /// <summary>
        /// Calls this if your values aren't loaded in yet.
        /// </summary>
        private static void initializeCustomCampaignOnce()
        {
            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }

            if (currentCampaign.campaignSaveCategory == null)
            {
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName +
                    currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedDays", 1);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedCurrentCaller", 0);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedEntryTier", 1);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedCallerArrayLength", 0);
            }
            
            for (int i = 0; i < currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") == null)
                {
                    currentCampaign.campaignSaveCategory.CreateEntry($"savedCallerCorrectAnswer{i}", false);
                }
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedGameFinished", 0);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedGameFinishedDisplay", 0);
            }
            
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") == null)
                {
                    currentCampaign.campaignSaveCategory.CreateEntry($"SavedDayScore{i}", 0.0f);
                }
            }

            MelonPreferences.Save();
        }

        /// <summary>
        /// Saves the information on the current custom campaign to MelonLoader preferences.
        /// Note: Saving is destructive if not called at the correct time. It's best to call it only after it has been loaded at least once.
        /// </summary>
        public static void saveCustomCampaignInfo()
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called save custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }

            // Custom Campaigns
            // We use this painful category name to avoid any conflicts.
            if (currentCampaign.campaignSaveCategory == null)
            {
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName +
                    currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }

            // Create Campaign Days Save Value
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                MelonPreferences_Entry<int> currentSavedDays =
                    currentCampaign.campaignSaveCategory.CreateEntry("savedDays", 1);
                currentSavedDays.Value = currentCampaign.currentDay;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays").Value = currentCampaign.currentDay;
            }
            
            // Current Caller Index
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller") == null)
            {
                MelonPreferences_Entry<int> savedCurrentCaller =
                    currentCampaign.campaignSaveCategory.CreateEntry("savedCurrentCaller", 0);
                savedCurrentCaller.Value = currentCampaign.savedCurrentCaller;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value =
                    currentCampaign.savedCurrentCaller;
            }

            // Current permission tier
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier") == null)
            {
                MelonPreferences_Entry<int> currentSavedTier =
                    currentCampaign.campaignSaveCategory.CreateEntry("savedEntryTier", 1);
                currentSavedTier.Value = currentCampaign.currentPermissionTier;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier").Value =
                    currentCampaign.currentPermissionTier;
            }

            // Correct answers.
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength") == null)
            {
                MelonPreferences_Entry<int> savedCallerArrayLength =
                    currentCampaign.campaignSaveCategory.CreateEntry("savedCallerArrayLength", 0);
                savedCallerArrayLength.Value = currentCampaign.savedCallerArrayLength;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value =
                    currentCampaign.savedCallerArrayLength;
            }

            // For each correct answer create the correct entry.
            for (int i = 0; i < currentCampaign.savedCallerArrayLength; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") == null)
                {
                    MelonPreferences_Entry<bool> savedCallerCorrectAnswers =
                        currentCampaign.campaignSaveCategory.CreateEntry($"savedCallerCorrectAnswer{i}", false);

                    if (currentCampaign.savedCallersCorrectAnswer.Count >
                        i) // If we have enough values for "i". It should be correct but who knows.
                    {
                        savedCallerCorrectAnswers.Value =
                            currentCampaign.savedCallersCorrectAnswer[i]; // Whatever value we have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Provided index {i} is not available. (savedCallerCorrectAnswer doesn't exist)");
                    }
                }
                else
                {
                    if (currentCampaign.savedCallersCorrectAnswer.Count >
                        i) // If we have enough values for "i". It should be but who knows.
                    {
                        currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value =
                            currentCampaign.savedCallersCorrectAnswer[i]; // Whatever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Provided index {i} is not available. (savedCallerCorrectAnswer exists)");
                    }
                }
            }

            // Special Values
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished") == null)
            {
                MelonPreferences_Entry<int> savedGameFinished =
                    currentCampaign.campaignSaveCategory.CreateEntry("savedGameFinished", 0);
                savedGameFinished.Value = currentCampaign.savedGameFinished;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished").Value =
                    currentCampaign.savedGameFinished;
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay") == null)
            {
                MelonPreferences_Entry<int> savedGameFinishedDisplay =
                    currentCampaign.campaignSaveCategory.CreateEntry("savedGameFinishedDisplay", 0);
                savedGameFinishedDisplay.Value = currentCampaign.savedGameFinishedDisplay;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value =
                    currentCampaign.savedGameFinishedDisplay;
            }

            // Day score
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") == null)
                {
                    MelonPreferences_Entry<float> savedCallerCorrectAnswers =
                        currentCampaign.campaignSaveCategory.CreateEntry($"SavedDayScore{i}", 0.0f);

                    if (currentCampaign.savedDayScores.Count >
                        i) // If we have enough values for "i". It should be but who knows.
                    {
                        savedCallerCorrectAnswers.Value =
                            currentCampaign.savedDayScores[i]; // Whatever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Provided index {i} is not available. (SavedDayScore doesn't exist)");
                    }
                }
                else
                {
                    if (currentCampaign.savedDayScores.Count >
                        i) // If we have enough values for "i". It should be but who knows.
                    {
                        currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}").Value =
                            currentCampaign.savedDayScores[i]; // Whatever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available. (SavedDayScore exists)");
                    }
                }
            }

            // We finished storing all important values. Now we save.
            MelonPreferences.Save();

            MelonLogger.Msg($"INFO: Finished saving custom campaign {currentCampaign.campaignName}.");
        }

        /// <summary>
        /// Loads the information on the current custom campaign to all possible values. Should realistically only be called once on custom campaign switch.
        /// </summary>
        public static void loadFromFileCustomCampaignInfo()
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called load custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of loading.");
                return;
            }

            // Custom Campaigns
            // We use this painful category name to avoid any conflicts.
            if (currentCampaign.campaignSaveCategory == null)
            {
                // We get the category if not set yet.
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName +
                    currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }

            // Check if it was ever saved before. If yes, load and if not then we call save once.
            initializeCustomCampaignOnce();

            // Load all values first into the currentCampaign Object.
            currentCampaign.currentDay = currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays").Value;
            currentCampaign.currentPermissionTier =
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier").Value;
            currentCampaign.savedCallerArrayLength =
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value;
            currentCampaign.savedCurrentCaller =
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value;

            // Special Values
            currentCampaign.savedGameFinished =
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished").Value;
            currentCampaign.savedGameFinishedDisplay =
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value;

            // Add all correct answers.
            for (int i = 0; i < currentCampaign.savedCallerArrayLength; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") != null)
                {
                    currentCampaign.savedCallersCorrectAnswer.Add(currentCampaign.campaignSaveCategory
                        .GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value);
                }
                else
                {
                    MelonLogger.Warning(
                        $"WARNING: While loading all saved caller answers, 'savedCallerCorrectAnswer{i}' does not exist! Setting to 0.0 for {i}.");
                    currentCampaign.savedCallersCorrectAnswer.Add(false);
                }
            }

            // Saved Day Scores
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") != null)
                {
                    currentCampaign.savedDayScores.Add(currentCampaign.campaignSaveCategory
                        .GetEntry<float>($"SavedDayScore{i}").Value);
                }
                else
                {
                    MelonLogger.Warning(
                        $"WARNING: While loading all saved caller answers, 'SavedDayScore{i}' does not exist! Setting to false for {i}.");
                    currentCampaign.savedDayScores.Add(0.0f);
                }
            }

            /*
             * Load the values into actual game values now.
             */

            GlobalVariables.saveManagerScript.savedDay = currentCampaign.currentDay;
            GlobalVariables.currentDay = currentCampaign.currentDay;

            GlobalVariables.saveManagerScript.savedCurrentCaller = currentCampaign.savedCurrentCaller;

            GlobalVariables.saveManagerScript.savedEntryTier = currentCampaign.currentPermissionTier;

            GlobalVariables.saveManagerScript.savedCallerArrayLength = currentCampaign.savedCallerArrayLength;

            // Add all saved answers.
            bool[] flagArray = new bool[currentCampaign.savedCallerArrayLength];

            for (int index = 0; index < currentCampaign.savedCallerArrayLength; ++index)
            {
                MelonPreferences_Entry<bool> entry =
                    currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{index}");

                if (entry == null)
                {
                    // If entry does not exist, create it with default value false.
                    entry = currentCampaign.campaignSaveCategory.CreateEntry($"savedCallerCorrectAnswer{index}", false);
                }

                flagArray[index] = entry.Value;
            }

            GlobalVariables.saveManagerScript.savedCallerCorrectAnswers = flagArray;

            // Special Values
            GlobalVariables.saveManagerScript.savedGameFinished = currentCampaign.savedGameFinished;
            GlobalVariables.saveManagerScript.savedGameFinishedDisplay = currentCampaign.savedGameFinishedDisplay;

            for (int i = 0; i < Mathf.Min(7, currentCampaign.campaignDays); ++i)
            {
                switch (i)
                {
                    case 0:
                        GlobalVariables.saveManagerScript.savedDayScore1 = currentCampaign.savedDayScores[i];
                        break;

                    case 1:
                        GlobalVariables.saveManagerScript.savedDayScore2 = currentCampaign.savedDayScores[i];
                        break;

                    case 2:
                        GlobalVariables.saveManagerScript.savedDayScore3 = currentCampaign.savedDayScores[i];
                        break;

                    case 3:
                        GlobalVariables.saveManagerScript.savedDayScore4 = currentCampaign.savedDayScores[i];
                        break;

                    case 4:
                        GlobalVariables.saveManagerScript.savedDayScore5 = currentCampaign.savedDayScores[i];
                        break;

                    case 5:
                        GlobalVariables.saveManagerScript.savedDayScore6 = currentCampaign.savedDayScores[i];
                        break;

                    case 6:
                        GlobalVariables.saveManagerScript.savedDayScore7 = currentCampaign.savedDayScores[i];
                        break;

                    default:
                        // No saved day available in the campaign save manager. Thus, we just ignore it.
                        continue;
                }
            }

            // Finished loading.
            MelonLogger.Msg("INFO: Finished loading all custom campaign values.");
        }

        /// <summary>
        /// Resets the stored custom campaign values to default values.
        /// </summary>
        public static void resetCustomCampaignFile()
        {
            MelonLogger.Msg("INFO: Resetting custom campaign file.");

            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called reset custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of resetting.");
                return;
            }

            // Custom Campaigns

            if (currentCampaign.campaignSaveCategory == null) // We haven't loaded it in?
            {
                loadFromFileCustomCampaignInfo();
            }

            if (currentCampaign.campaignSaveCategory == null)
            {
                MelonLogger.Error("ERROR: Tried resetting but save is still null!");
                return;
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                // If null, we cancel our operation since there is no save.
                MelonLogger.Error("ERROR: Tried resetting when no save is available!");
                return;
            }

            // We previously deleted the category, but it doesn't really help as much, so we leave it be.
            // MelonPreferences.RemoveCategoryFromFile("",currentCampaign.campaignName + currentCampaign.campaignDesktopName + currentCampaign.campaignDays);

            // We have a save file, so we reset the values.
            currentCampaign.currentDay = 1;
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays").Value = 1;

            currentCampaign.currentPermissionTier = 1;
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier").Value = 1;

            currentCampaign.savedCurrentCaller = 0;
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value = 0;

            currentCampaign.savedCallersCorrectAnswer = new List<bool>();

            // Now we reset the saved callers answers.
            for (int i = 0; i < currentCampaign.savedCallerArrayLength; i++)
            {
                currentCampaign.campaignSaveCategory.DeleteEntry($"savedCallerCorrectAnswer{i}");
            }

            // Reset daily score
            currentCampaign.savedDayScores = new List<float>();
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                currentCampaign.campaignSaveCategory.DeleteEntry($"SavedDayScore{i}");
            }

            // We reset our caller array length
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value = 0;
            currentCampaign.savedCallerArrayLength = 0;

            // Special Values
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished").Value = 0;
            currentCampaign.savedGameFinished = 0;

            currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value = 0;
            currentCampaign.savedGameFinishedDisplay = 0;
            
            // Options
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedColorTheme").Value = 0;
            currentCampaign.activeTheme = 0;
            
            // Theme
            currentCampaign.campaignSaveCategory.GetEntry<bool>("themeShownOnce").Value = false;
            currentCampaign.defaultThemeAppliedOnce = false;

            MelonLogger.Msg($"INFO: Finished resetting values for the custom campaign {currentCampaign.campaignName}.");
        }

        /// <summary>
        /// Initializes the custom campaign options once.
        /// </summary>
        private static void initializeCustomCampaignOptionsOnce()
        {
            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }

            if (currentCampaign.campaignSaveCategory == null)
            {
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName +
                    currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }
            
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedColorTheme") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("savedColorTheme", 0);
            }
            
            if (currentCampaign.campaignSaveCategory.GetEntry<bool>("themeShownOnce") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry("themeShownOnce", false);
            }

            MelonPreferences.Save();
        }

        /// <summary>
        /// Saves the campaign options that matter for the custom campaign.
        /// </summary>
        public static void saveCustomCampaignOptions()
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called save custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }
            
            // Custom Campaigns
            if (currentCampaign.campaignSaveCategory == null)
            {
                // We get the category if not set yet.
                // We use this painful category name to avoid any conflicts.
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName +
                    currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }

            // Check if it was ever saved before. If yes, load and if not then we call save once.
            initializeCustomCampaignOptionsOnce();
            
            // Create Theme Saving
            
            #if DEBUG
                MelonLogger.Msg($"DEBUG: Saved color themes ({currentCampaign.activeTheme}).");
            #endif
            
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedColorTheme") == null)
            {
                MelonPreferences_Entry<int> savedColorTheme = currentCampaign.campaignSaveCategory.CreateEntry(
                    "savedColorTheme", 0);
                
                savedColorTheme.Value = currentCampaign.activeTheme;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedColorTheme").Value = currentCampaign.activeTheme;
            }
            
            if (currentCampaign.campaignSaveCategory.GetEntry<bool>("themeShownOnce") == null)
            {
                MelonPreferences_Entry<bool> themeShownOnce = currentCampaign.campaignSaveCategory.CreateEntry(
                    "themeShownOnce", false);
                
                themeShownOnce.Value = currentCampaign.defaultThemeAppliedOnce;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<bool>("themeShownOnce").Value = currentCampaign.defaultThemeAppliedOnce;
            }
            
            MelonPreferences.Save();

            MelonLogger.Msg($"INFO: Finished saving (Options) for the custom campaign {currentCampaign.campaignName}.");
        }

        /// <summary>
        /// Loads the options for the current custom campaign.
        /// </summary>
        public static void loadCustomCampaignOptions()
        {
            MelonLogger.Msg("INFO: Starting to load all custom campaign settings/options.");
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called load custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaignExtraInfo currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of loading.");
                return;
            }

            // Custom Campaigns
            // We use this painful category name to avoid any conflicts.
            if (currentCampaign.campaignSaveCategory == null)
            {
                // We get the category if not set yet.
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName +
                    currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }

            // Check if it was ever saved before. If yes, load and if not then we call save once.
            initializeCustomCampaignOptionsOnce();
            
            #if DEBUG
                MelonLogger.Msg($"DEBUG: Saved color themes ({currentCampaign.campaignSaveCategory.GetEntry<int>("savedColorTheme").Value}).");
            #endif
            // Load all values first into the currentCampaign Object.
            currentCampaign.activeTheme = currentCampaign.campaignSaveCategory.GetEntry<int>("savedColorTheme").Value;
            
            // If we have applied the default theme at least once.
            currentCampaign.defaultThemeAppliedOnce = currentCampaign.campaignSaveCategory.GetEntry<bool>("themeShownOnce").Value;
            
            /*
             * Load the values into actual game values now.
             */

            GlobalVariables.saveManagerScript.savedColorTheme = currentCampaign.activeTheme;
            
            if ((bool)GlobalVariables.colorPaletteController)
            {
                GlobalVariables.colorPaletteController.UpdateColorTheme();
            }

            // Finished loading.
            MelonLogger.Msg("INFO: Finished loading all custom campaign settings/options.");
        }
    }
}