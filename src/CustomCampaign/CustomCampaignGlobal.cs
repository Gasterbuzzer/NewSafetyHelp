using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CallerPatches;
using NewSafetyHelp.EntryManager;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static List<CustomCampaignExtraInfo> customCampaignsAvailable = new List<CustomCampaignExtraInfo>();
        
        // ReSharper disable once RedundantDefaultMemberInitializer
        public static bool inCustomCampaign = false;

        public static string currentCustomCampaignName = "";
        
        /// <summary>
        /// Activates the custom campaign values.
        /// </summary>
        /// <param name="customCampaignName">Name of the custom campaign to set as the current one.</param>
        public static void activateCustomCampaign(string customCampaignName)
        {
            inCustomCampaign = true;
            currentCustomCampaignName = customCampaignName;
        }

        /// <summary>
        /// Deactivates the custom campaign and sets values as if it were the main campaign.
        /// </summary>
        public static void deactivateCustomCampaign()
        {
            inCustomCampaign = false;
            currentCustomCampaignName = "";
        }

        /// <summary>
        /// Returns the current campaign as CustomCampaignExtraInfo.
        /// </summary>
        /// <returns>CustomCampaignExtraInfo Object of the current activate custom campaign.</returns>
        public static CustomCampaignExtraInfo getCustomCampaignExtraInfo()
        {
            return customCampaignsAvailable.Find(scannedCampaign => scannedCampaign.campaignName == currentCustomCampaignName);
        }

        /// <summary>
        /// Gets the custom caller by its order id provided. 
        /// </summary>
        /// <param name="orderID">Order number in the current custom campaign.</param>
        /// <returns>CustomCallerExtraInfo Object with the returned object. If not found, default. </returns>
        public static CustomCallerExtraInfo getCustomCampaignCustomCallerByOrderID(int orderID)
        {
            return getCustomCampaignExtraInfo().customCallersInCampaign.Find(customCaller => customCaller.orderInCampaign == orderID);
        }

        /// <summary>
        /// Adds all entries of a custom campaign to the array of entries.
        /// </summary>
        /// <param name="_monsterProfileList">MonsterProfileList to add the entries to.</param>
        public static void addAllCustomCampaignEntriesToArray(ref MonsterProfileList _monsterProfileList)
        {
            
            CustomCampaignExtraInfo customCampaignExtraInfo = getCustomCampaignExtraInfo();

            if (customCampaignExtraInfo == null)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of adding entries to custom campaign!");
                return;
            }
            
            #if DEBUG
                MelonLogger.Msg($"DEBUG: Now adding all {customCampaignExtraInfo.entriesOnlyInCampaign.Count} entries to the custom campaign.");
            #endif
            
            // Add all entries.
            foreach (EntryExtraInfo entryInCustomCampaign in customCampaignExtraInfo.entriesOnlyInCampaign)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Adding entry {entryInCustomCampaign.Name} to custom campaign.");
                #endif
                
                EntryManager.EntryManager.AddMonsterToTheProfile(entryInCustomCampaign.referenceCopyEntry, ref _monsterProfileList.monsterProfiles, "allEntries");
            }
            
            // Sort afterward
            EntryManager.EntryManager.SortMonsterProfiles(ref _monsterProfileList.monsterProfiles);
            
        }

        public static void replaceAllProvidedCampaignEntries(ref MonsterProfileList _monsterProfileList)
        {
            CustomCampaignExtraInfo customCampaignExtraInfo = getCustomCampaignExtraInfo();

            if (customCampaignExtraInfo == null || !inCustomCampaign)
            {
                MelonLogger.Error("ERROR: customCampaignExtraInfo is null! Unable of adding entries to custom campaign!");
                return;
            }

            #if DEBUG
                MelonLogger.Msg($"DEBUG: Now replacing all {customCampaignExtraInfo.entryReplaceOnlyInCampaign.Count} entries to the custom campaign.");
            #endif

            if (_monsterProfileList.monsterProfiles.Length <= 0)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Monster Profile ");
                #endif
                return;
            }
            
            for (int i = 0; i < _monsterProfileList.monsterProfiles.Length; i++)
            {
                MonsterProfile realEntry = _monsterProfileList.monsterProfiles[i];

                if (realEntry == null)
                {
                    MelonLogger.Warning("WARNING: realEntry is null! Unable of replacing entry for this entry!");
                    return;
                }

                // Find matching entry to replace
                EntryExtraInfo entryFound = customCampaignExtraInfo.entryReplaceOnlyInCampaign.Find(
                    replaceEntry => replaceEntry.Name.Equals(realEntry.monsterName) || replaceEntry.ID.Equals(realEntry.monsterID)
                );
                
                // MelonLogger.Msg($"DEBUG: Comparing Names: '{realEntry.monsterName}' with {customCampaignExtraInfo.entryReplaceOnlyInCampaign[0].Name}");


                if (entryFound != null) // It exists, so replace it.
                {
                    if (entryFound.referenceCopyEntry == null)
                    {
                        // I am too lazy to implement this. But if ever returns errors or problems, I will implement it this way.
                        MelonLogger.Warning("WARNING: referenceCopyEntry of EntryFound is null. Was the entry initialized?");
                        continue;
                    }
                    
                    _monsterProfileList.monsterProfiles[i] = entryFound.referenceCopyEntry;
                    
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Replacing entry {entryFound.Name} with custom entry in custom campaign.");
                    #endif
                }
            }
            
        }

        /// <summary>
        /// Calls this if your values aren't loaded in yet.
        /// </summary>
        public static void initializeCustomCampaignOnce()
        {
            CustomCampaignExtraInfo currentCampaign = getCustomCampaignExtraInfo();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }
            
            if (currentCampaign.campaignSaveCategory == null)
            {
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName + currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry<int>("savedDays", 1);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry<int>("savedCurrentCaller", 0);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry<int>("savedEntryTier", 1);
            }
            
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry<int>("savedCallerArrayLength", 0);
            }

            #if DEBUG
            MelonLogger.Msg($"DEBUG: Amount of saved caller array length: { currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value}");
            #endif
            for (int i = 0; i < currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") == null)
                {
                    currentCampaign.campaignSaveCategory.CreateEntry<bool>($"savedCallerCorrectAnswer{i}", false);
                }
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry<int>("savedGameFinished", 0);
            }

            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay") == null)
            {
                currentCampaign.campaignSaveCategory.CreateEntry<int>("savedGameFinishedDisplay", 0);
            }

            
            #if DEBUG
                MelonLogger.Msg($"DEBUG: Amount of campaign days: {currentCampaign.campaignDays}");
            #endif
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") == null)
                {
                    currentCampaign.campaignSaveCategory.CreateEntry<float>($"SavedDayScore{i}", 0.0f);
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
            if (!inCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called save custom campaign but there is no campaign active.");
                return;
            }
            
            CustomCampaignExtraInfo currentCampaign = getCustomCampaignExtraInfo();

            if (currentCampaign == null)
            {
                MelonLogger.Error("ERROR: Custom Campaign active but no campaign found. Unable of saving.");
                return;
            }
            
            // Custom Campaigns
            // We use this painful category name to avoid any conflicts.
            if (currentCampaign.campaignSaveCategory == null)
            {
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName + currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }
            
            // Create Campaign Days Save Value
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                MelonPreferences_Entry<int> currentSavedDays = currentCampaign.campaignSaveCategory.CreateEntry<int>("savedDays", 1);
                currentSavedDays.Value = currentCampaign.currentDay;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays").Value = currentCampaign.currentDay;
            }
            
            
            // Current Caller Index
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller") == null)
            {
                MelonPreferences_Entry<int> savedCurrentCaller = currentCampaign.campaignSaveCategory.CreateEntry<int>("savedCurrentCaller", 0);
                savedCurrentCaller.Value = currentCampaign.savedCurrentCaller;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value = currentCampaign.savedCurrentCaller;
            }
            
            // Current permission tier
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier") == null)
            {
                MelonPreferences_Entry<int> currentSavedTier = currentCampaign.campaignSaveCategory.CreateEntry<int>("savedEntryTier", 1);
                currentSavedTier.Value = currentCampaign.currentPermissionTier;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier").Value = currentCampaign.currentPermissionTier;
            }
            
            // Correct answers.
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength") == null)
            {
                MelonPreferences_Entry<int> savedCallerArrayLength = currentCampaign.campaignSaveCategory.CreateEntry<int>("savedCallerArrayLength", 0);
                savedCallerArrayLength.Value = currentCampaign.savedCallerArrayLength;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value = currentCampaign.savedCallerArrayLength;
            }
            
            // For each correct answer create the correct entry.
            for (int i = 0; i < currentCampaign.savedCallerArrayLength; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") == null)
                {
                    MelonPreferences_Entry<bool> savedCallerCorrectAnswers = currentCampaign.campaignSaveCategory.CreateEntry<bool>($"savedCallerCorrectAnswer{i}", false);

                    if (currentCampaign.savedCallersCorrectAnswer.Count > i) // If we have enough values for "i". It should be but who knows.
                    {
                        savedCallerCorrectAnswers.Value = currentCampaign.savedCallersCorrectAnswer[i]; // What ever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available. (savedCallerCorrectAnswer doesn't exist)");
                    }
                }
                else
                {
                    if (currentCampaign.savedCallersCorrectAnswer.Count > i) // If we have enough values for "i". It should be but who knows.
                    {
                        currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value = currentCampaign.savedCallersCorrectAnswer[i]; // What ever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available. (savedCallerCorrectAnswer exists)");
                    }
                }
                
            }
            
            // Special Values
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished") == null)
            {
                MelonPreferences_Entry<int> savedGameFinished = currentCampaign.campaignSaveCategory.CreateEntry<int>("savedGameFinished", 0);
                savedGameFinished.Value = currentCampaign.savedGameFinished;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished").Value = currentCampaign.savedGameFinished;
            }
            
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay") == null)
            {
                MelonPreferences_Entry<int> savedGameFinishedDisplay = currentCampaign.campaignSaveCategory.CreateEntry<int>("savedGameFinishedDisplay", 0);
                savedGameFinishedDisplay.Value = currentCampaign.savedGameFinishedDisplay;
            }
            else
            {
                currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value = currentCampaign.savedGameFinishedDisplay;
            }
            
            // Day score
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") == null)
                {
                    MelonPreferences_Entry<float> savedCallerCorrectAnswers = currentCampaign.campaignSaveCategory.CreateEntry<float>($"SavedDayScore{i}", 0.0f);

                    if (currentCampaign.savedDayScores.Count > i) // If we have enough values for "i". It should be but who knows.
                    {
                        savedCallerCorrectAnswers.Value = currentCampaign.savedDayScores[i]; // What ever value where have at that index.
                    }
                    else
                    {
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available. (SavedDayScore doesn't exist)");
                    }
                }
                else
                {
                    if (currentCampaign.savedDayScores.Count > i) // If we have enough values for "i". It should be but who knows.
                    {
                        currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}").Value = currentCampaign.savedDayScores[i]; // What ever value where have at that index.
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
            if (!inCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called load custom campaign but there is no campaign active.");
                return;
            }
            
            CustomCampaignExtraInfo currentCampaign = getCustomCampaignExtraInfo();

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
                currentCampaign.campaignSaveCategory = MelonPreferences.CreateCategory(currentCampaign.campaignName + currentCampaign.campaignDesktopName + currentCampaign.campaignDays);
            }
            
            // Check if it was ever saved before. If yes, load and if not then we call save once.
            initializeCustomCampaignOnce();
            
            // Load all values first into the currentCampaign Object.
            currentCampaign.currentDay = currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays").Value;
            currentCampaign.currentPermissionTier = currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier").Value;
            currentCampaign.savedCallerArrayLength = currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value;
            currentCampaign.savedCurrentCaller = currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value;
            
            // Special Values
            currentCampaign.savedGameFinished = currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinished").Value;
            currentCampaign.savedGameFinishedDisplay = currentCampaign.campaignSaveCategory.GetEntry<int>("savedGameFinishedDisplay").Value;
            
            // Add all correct answers.
            for (int i = 0; i < currentCampaign.savedCallerArrayLength; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") != null)
                {
                    currentCampaign.savedCallersCorrectAnswer.Add(currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value);
                }
                else
                {
                    MelonLogger.Warning($"WARNING: While loading all saved caller answers, 'savedCallerCorrectAnswer{i}' does not exist! Setting to 0.0 for {i}.");
                    currentCampaign.savedCallersCorrectAnswer.Add(false);
                }
            }
            
            // Saved Day Scores
            for (int i = 0; i < currentCampaign.campaignDays; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}") != null)
                {
                    currentCampaign.savedDayScores.Add(currentCampaign.campaignSaveCategory.GetEntry<float>($"SavedDayScore{i}").Value);
                }
                else
                {
                    MelonLogger.Warning($"WARNING: While loading all saved caller answers, 'SavedDayScore{i}' does not exist! Setting to false for {i}.");
                    currentCampaign.savedDayScores.Add(0.0f);
                }
            }
            
            /*
             * Load the values into actual game values now.
             */
            
            GlobalVariables.saveManagerScript.savedDay =  currentCampaign.currentDay;
            GlobalVariables.currentDay = currentCampaign.currentDay;
            
            GlobalVariables.saveManagerScript.savedCurrentCaller = currentCampaign.savedCurrentCaller;
            
            GlobalVariables.saveManagerScript.savedEntryTier = currentCampaign.currentPermissionTier;
            
            GlobalVariables.saveManagerScript.savedCallerArrayLength = currentCampaign.savedCallerArrayLength;
            
            // Add all saved answers.
            bool[] flagArray = new bool[currentCampaign.savedCallerArrayLength];

            for (int index = 0; index < currentCampaign.savedCallerArrayLength; ++index)
            {
                MelonPreferences_Entry<bool> entry = currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{index}");

                if (entry == null)
                {
                    // If entry does not exist, create it with default value false.
                    entry = currentCampaign.campaignSaveCategory.CreateEntry<bool>($"savedCallerCorrectAnswer{index}", false);
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

        public static void resetCustomCampaignFile()
        {

            MelonLogger.Msg("INFO: Resetting custom campaign file.");
            
            if (!inCustomCampaign)
            {
                MelonLogger.Warning("WARNING: Called reset custom campaign but there is no campaign active.");
                return;
            }
            
            CustomCampaignExtraInfo currentCampaign = getCustomCampaignExtraInfo();

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
            
            MelonLogger.Msg($"INFO: Finished resetting values for the custom campaign {currentCampaign.campaignName}.");
        }
    }
}