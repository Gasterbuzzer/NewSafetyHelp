using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CallerPatches;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static List<CustomCampaignExtraInfo> customCampaignsAvailable = new List<CustomCampaignExtraInfo>();
        
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
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available.");
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
                        MelonLogger.Warning($"WARNING: Provided index {i} is not available.");
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
            if (currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                saveCustomCampaignInfo();
            }
            
            // Load all values first into the currentCampaign Object.
            currentCampaign.currentDay = currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays").Value;
            currentCampaign.currentPermissionTier = currentCampaign.campaignSaveCategory.GetEntry<int>("savedEntryTier").Value;
            currentCampaign.savedCallerArrayLength = currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value;
            currentCampaign.savedCurrentCaller = currentCampaign.campaignSaveCategory.GetEntry<int>("savedCurrentCaller").Value;
            
            // Add all correct answers.
            for (int i = 0; i < currentCampaign.savedCallerArrayLength; i++)
            {
                if (currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}") != null)
                {
                    currentCampaign.savedCallersCorrectAnswer.Add(currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{i}").Value);
                }
                else
                {
                    MelonLogger.Warning($"WARNING: While loading all saved caller answers, 'savedCallerCorrectAnswer{i}' does not exist! Setting to false for {i}.");
                    currentCampaign.savedCallersCorrectAnswer.Add(false);
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
                flagArray[index] = currentCampaign.campaignSaveCategory.GetEntry<bool>($"savedCallerCorrectAnswer{index}").Value;
            }

            GlobalVariables.saveManagerScript.savedCallerCorrectAnswers = flagArray;
            
            
            // Finished loading.
            MelonLogger.Msg("INFO: Finished loading in custom campaign values.");
        }

        public static void resetCustomCampaignFile()
        {
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

            if (currentCampaign.campaignSaveCategory != null && currentCampaign.campaignSaveCategory.GetEntry<int>("savedDays") == null)
            {
                // If null, we cancel our operation since there is no save.
                MelonLogger.Error("ERROR: Tried resetting when no save is available!");
                return;
            }

            if (currentCampaign.campaignSaveCategory == null)
            {
                MelonLogger.Error("ERROR: Tried resetting but save is still null!");
                return;
            }
            
            // We have a save file, so we reset the values.
            currentCampaign.currentDay = 1;
            currentCampaign.campaignSaveCategory.GetEntry<int>("currentDay").Value = 1;
            
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
            
            // We reset our caller array length
            currentCampaign.campaignSaveCategory.GetEntry<int>("savedCallerArrayLength").Value = 0;
            currentCampaign.savedCallerArrayLength = 0;
            
            MelonLogger.Msg($"INFO: Finished resetting values for the custom campaign {currentCampaign.campaignName}.");
        }
    }
}