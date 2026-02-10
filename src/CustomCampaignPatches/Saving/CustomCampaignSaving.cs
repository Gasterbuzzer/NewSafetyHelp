using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignPatches.Saving
{
    public static class CustomCampaignSaving
    {
        /// <summary>
        /// Calls this if your values aren't loaded in yet.
        /// </summary>
        private static void initializeCustomCampaignOnce()
        {
            CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

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

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedDays", 1);
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCurrentCaller") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedCurrentCaller", 0);
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedEntryTier") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedEntryTier", 1);
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCallerArrayLength") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedCallerArrayLength", 0);
            }
            
            for (int i = 0; i < currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value; i++)
            {
                if (currentCampaign.CampaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") == null)
                {
                    currentCampaign.CampaignSaveCategory.CreateEntry($"savedCallerCorrectAnswer{i}", false);
                }
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinished") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedGameFinished", 0);
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay") == null)
            {
                currentCampaign.CampaignSaveCategory.CreateEntry("savedGameFinishedDisplay", 0);
            }
            
            for (int i = 0; i < currentCampaign.CampaignDays; i++)
            {
                if (currentCampaign.CampaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") == null)
                {
                    currentCampaign.CampaignSaveCategory.CreateEntry($"SavedDayScore{i}", 0.0f);
                }
            }

            MelonPreferences.Save();
        }

        /// <summary>
        /// Saves the information on the current custom campaign to MelonLoader preferences.
        /// Note: Saving is destructive if not called at the correct time. It's best to call it only after it has been loaded at least once.
        /// </summary>
        public static void SaveCustomCampaignInfo()
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called save custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }

            // Custom Campaigns
            // We use this painful category name to avoid any conflicts.
            if (currentCampaign.CampaignSaveCategory == null)
            {
                currentCampaign.CampaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.CampaignName +
                    currentCampaign.CampaignDesktopName + currentCampaign.CampaignDays);
            }

            // Create Campaign Days Save Value
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                MelonPreferences_Entry<int> currentSavedDays =
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedDays", 1);
                currentSavedDays.Value = currentCampaign.CurrentDay;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedDays").Value = currentCampaign.CurrentDay;
            }
            
            // Current Caller Index
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCurrentCaller") == null)
            {
                MelonPreferences_Entry<int> savedCurrentCaller =
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedCurrentCaller", 0);
                savedCurrentCaller.Value = currentCampaign.SavedCurrentCaller;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value =
                    currentCampaign.SavedCurrentCaller;
            }

            // Current permission tier
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedEntryTier") == null)
            {
                MelonPreferences_Entry<int> currentSavedTier =
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedEntryTier", 1);
                currentSavedTier.Value = currentCampaign.CurrentPermissionTier;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedEntryTier").Value =
                    currentCampaign.CurrentPermissionTier;
            }

            // Correct answers.
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCallerArrayLength") == null)
            {
                MelonPreferences_Entry<int> savedCallerArrayLength =
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedCallerArrayLength", 0);
                savedCallerArrayLength.Value = currentCampaign.SavedCallerArrayLength;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value =
                    currentCampaign.SavedCallerArrayLength;
            }

            // For each correct answer create the correct entry.
            for (int i = 0; i < currentCampaign.SavedCallerArrayLength; i++)
            {
                if (currentCampaign.CampaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") == null)
                {
                    MelonPreferences_Entry<bool> savedCallerCorrectAnswers =
                        currentCampaign.CampaignSaveCategory.CreateEntry($"savedCallerCorrectAnswer{i}", false);

                    if (currentCampaign.SavedCallersCorrectAnswer.Count >
                        i) // If we have enough values for "i". It should be correct but who knows.
                    {
                        savedCallerCorrectAnswers.Value =
                            currentCampaign.SavedCallersCorrectAnswer[i]; // Whatever value we have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Provided index {i} is not available. (savedCallerCorrectAnswer doesn't exist)");
                    }
                }
                else
                {
                    if (currentCampaign.SavedCallersCorrectAnswer.Count >
                        i) // If we have enough values for "i". It should be but who knows.
                    {
                        currentCampaign.CampaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value =
                            currentCampaign.SavedCallersCorrectAnswer[i]; // Whatever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Provided index {i} is not available. (savedCallerCorrectAnswer exists)");
                    }
                }
            }

            // Special Values
            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinished") == null)
            {
                MelonPreferences_Entry<int> savedGameFinished =
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedGameFinished", 0);
                savedGameFinished.Value = currentCampaign.SavedGameFinished;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinished").Value =
                    currentCampaign.SavedGameFinished;
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay") == null)
            {
                MelonPreferences_Entry<int> savedGameFinishedDisplay =
                    currentCampaign.CampaignSaveCategory.CreateEntry("savedGameFinishedDisplay", 0);
                savedGameFinishedDisplay.Value = currentCampaign.SavedGameFinishedDisplay;
            }
            else
            {
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value =
                    currentCampaign.SavedGameFinishedDisplay;
            }

            // Day score
            for (int i = 0; i < currentCampaign.CampaignDays; i++)
            {
                if (currentCampaign.CampaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") == null)
                {
                    MelonPreferences_Entry<float> savedCallerCorrectAnswers =
                        currentCampaign.CampaignSaveCategory.CreateEntry($"SavedDayScore{i}", 0.0f);

                    if (currentCampaign.SavedDayScores.Count >
                        i) // If we have enough values for "i". It should be but who knows.
                    {
                        savedCallerCorrectAnswers.Value =
                            currentCampaign.SavedDayScores[i]; // Whatever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning(
                            $"WARNING: Provided index {i} is not available. (SavedDayScore doesn't exist)");
                    }
                }
                else
                {
                    if (currentCampaign.SavedDayScores.Count >
                        i) // If we have enough values for "i". It should be but who knows.
                    {
                        currentCampaign.CampaignSaveCategory.GetEntry<float>($"SavedDayScore{i}").Value =
                            currentCampaign.SavedDayScores[i]; // Whatever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available. (SavedDayScore exists)");
                    }
                }
            }

            // We finished storing all important values. Now we save.
            MelonPreferences.Save();

            MelonLogger.Msg($"INFO: Finished saving custom campaign {currentCampaign.CampaignName}.");
        }

        /// <summary>
        /// Loads the information on the current custom campaign to all possible values. Should realistically only be called once on custom campaign switch.
        /// </summary>
        public static void LoadFromFileCustomCampaignInfo()
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called load custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

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
            initializeCustomCampaignOnce();

            // Load all values first into the currentCampaign Object.
            currentCampaign.CurrentDay = currentCampaign.CampaignSaveCategory.GetEntry<int>("savedDays").Value;
            currentCampaign.CurrentPermissionTier =
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedEntryTier").Value;
            currentCampaign.SavedCallerArrayLength =
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value;
            currentCampaign.SavedCurrentCaller =
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value;

            // Special Values
            currentCampaign.SavedGameFinished =
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinished").Value;
            currentCampaign.SavedGameFinishedDisplay =
                currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value;

            // Add all correct answers.
            for (int i = 0; i < currentCampaign.SavedCallerArrayLength; i++)
            {
                if (currentCampaign.CampaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") != null)
                {
                    currentCampaign.SavedCallersCorrectAnswer.Add(currentCampaign.CampaignSaveCategory
                        .GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value);
                }
                else
                {
                    MelonLogger.Warning(
                        $"WARNING: While loading all saved caller answers, 'savedCallerCorrectAnswer{i}' does not exist! Setting to 0.0 for {i}.");
                    currentCampaign.SavedCallersCorrectAnswer.Add(false);
                }
            }

            // Saved Day Scores
            for (int i = 0; i < currentCampaign.CampaignDays; i++)
            {
                if (currentCampaign.CampaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") != null)
                {
                    currentCampaign.SavedDayScores.Add(currentCampaign.CampaignSaveCategory
                        .GetEntry<float>($"SavedDayScore{i}").Value);
                }
                else
                {
                    MelonLogger.Warning(
                        $"WARNING: While loading all saved caller answers, 'SavedDayScore{i}' does not exist! Setting to false for {i}.");
                    currentCampaign.SavedDayScores.Add(0.0f);
                }
            }

            /*
             * Load the values into actual game values now.
             */

            GlobalVariables.saveManagerScript.savedDay = currentCampaign.CurrentDay;
            GlobalVariables.currentDay = currentCampaign.CurrentDay;

            GlobalVariables.saveManagerScript.savedCurrentCaller = currentCampaign.SavedCurrentCaller;

            GlobalVariables.saveManagerScript.savedEntryTier = currentCampaign.CurrentPermissionTier;

            GlobalVariables.saveManagerScript.savedCallerArrayLength = currentCampaign.SavedCallerArrayLength;

            // Add all saved answers.
            bool[] flagArray = new bool[currentCampaign.SavedCallerArrayLength];

            for (int index = 0; index < currentCampaign.SavedCallerArrayLength; ++index)
            {
                MelonPreferences_Entry<bool> entry =
                    currentCampaign.CampaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{index}");

                if (entry == null)
                {
                    // If entry does not exist, create it with default value false.
                    entry = currentCampaign.CampaignSaveCategory.CreateEntry($"savedCallerCorrectAnswer{index}", false);
                }

                flagArray[index] = entry.Value;
            }

            GlobalVariables.saveManagerScript.savedCallerCorrectAnswers = flagArray;

            // Special Values
            GlobalVariables.saveManagerScript.savedGameFinished = currentCampaign.SavedGameFinished;
            GlobalVariables.saveManagerScript.savedGameFinishedDisplay = currentCampaign.SavedGameFinishedDisplay;

            for (int i = 0; i < Mathf.Min(7, currentCampaign.CampaignDays); ++i)
            {
                switch (i)
                {
                    case 0:
                        GlobalVariables.saveManagerScript.savedDayScore1 = currentCampaign.SavedDayScores[i];
                        break;

                    case 1:
                        GlobalVariables.saveManagerScript.savedDayScore2 = currentCampaign.SavedDayScores[i];
                        break;

                    case 2:
                        GlobalVariables.saveManagerScript.savedDayScore3 = currentCampaign.SavedDayScores[i];
                        break;

                    case 3:
                        GlobalVariables.saveManagerScript.savedDayScore4 = currentCampaign.SavedDayScores[i];
                        break;

                    case 4:
                        GlobalVariables.saveManagerScript.savedDayScore5 = currentCampaign.SavedDayScores[i];
                        break;

                    case 5:
                        GlobalVariables.saveManagerScript.savedDayScore6 = currentCampaign.SavedDayScores[i];
                        break;

                    case 6:
                        GlobalVariables.saveManagerScript.savedDayScore7 = currentCampaign.SavedDayScores[i];
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
        public static void ResetCustomCampaignFile()
        {
            MelonLogger.Msg("INFO: Resetting custom campaign file.");

            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called reset custom campaign but there is no campaign active.");
                return;
            }

            CustomCampaign currentCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of resetting.");
                return;
            }

            // Custom Campaigns

            if (currentCampaign.CampaignSaveCategory == null) // We haven't loaded it in?
            {
                LoadFromFileCustomCampaignInfo();
            }

            if (currentCampaign.CampaignSaveCategory == null)
            {
                MelonLogger.Error("ERROR: Tried resetting but save is still null!");
                return;
            }

            if (currentCampaign.CampaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                // If null, we cancel our operation since there is no save.
                MelonLogger.Error("ERROR: Tried resetting when no save is available!");
                return;
            }

            // We previously deleted the category, but it doesn't really help as much, so we leave it be.
            // MelonPreferences.RemoveCategoryFromFile("",currentCampaign.campaignName + currentCampaign.campaignDesktopName + currentCampaign.campaignDays);

            // We have a save file, so we reset the values.
            currentCampaign.CurrentDay = 1;
            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedDays").Value = 1;

            currentCampaign.CurrentPermissionTier = 1;
            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedEntryTier").Value = 1;

            currentCampaign.SavedCurrentCaller = 0;
            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value = 0;

            currentCampaign.SavedCallersCorrectAnswer = new List<bool>();

            // Now we reset the saved callers answers.
            for (int i = 0; i < currentCampaign.SavedCallerArrayLength; i++)
            {
                currentCampaign.CampaignSaveCategory.DeleteEntry($"savedCallerCorrectAnswer{i}");
            }

            // Reset daily score
            currentCampaign.SavedDayScores = new List<float>();
            for (int i = 0; i < currentCampaign.CampaignDays; i++)
            {
                currentCampaign.CampaignSaveCategory.DeleteEntry($"SavedDayScore{i}");
            }

            // We reset our caller array length
            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value = 0;
            currentCampaign.SavedCallerArrayLength = 0;

            // Special Values
            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinished").Value = 0;
            currentCampaign.SavedGameFinished = 0;

            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value = 0;
            currentCampaign.SavedGameFinishedDisplay = 0;
            
            // Options
            currentCampaign.CampaignSaveCategory.GetEntry<int>("savedColorTheme").Value = 0;
            currentCampaign.ActiveTheme = 0;
            
            // Theme
            currentCampaign.CampaignSaveCategory.GetEntry<bool>("themeShownOnce").Value = false;
            currentCampaign.DefaultThemeAppliedOnce = false;

            MelonLogger.Msg($"INFO: Finished resetting values for the custom campaign {currentCampaign.CampaignName}.");
        }
    }
}