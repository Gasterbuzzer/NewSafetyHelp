using System.Collections;
using MelonLoader;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomDesktop.Utils;
using NewSafetyHelp.EntryManager.EntryUnlocker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewSafetyHelp.JSONParsing
{
    /// <summary>
    /// Contains helper functions to redo the initialization of all the parsed values.
    /// </summary>
    public static class ReloadJSONParsing
    {
        // ReSharper disable once RedundantDefaultMemberInitializer
        /// <summary>
        /// A flag that describes if we are actively hot reloading.
        /// This is used to prevent accidental overwriting.
        /// </summary>
        public static bool IsInHotReload = false;
        
        /// <summary>
        /// Hot reloads all JSON files again
        /// and resets any loaded value to default values from before we overwrote values.
        /// </summary>
        public static void ReloadAllJSONFiles(GameObject resetButton)
        {
            // Prevent clicking again.
            resetButton.SetActive(false);

            string activeCustomCampaignName = null;
            bool wasInCustomCampaign = false;
            
            // First we load back to the main game, if we are in a custom campaign.
            if (CustomCampaignGlobal.InCustomCampaign)
            {
                activeCustomCampaignName = CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName;
                wasInCustomCampaign = true;
                
                MainClassForMonsterEntries.AddedEntriesToCustomCampaign = false;
                
                
                
                CustomCampaignSceneSwitcher.BackToMainGame(false);
            }

            IsInHotReload = true;
            
            // Remove all custom campaigns.
            CustomCampaignGlobal.CustomCampaignsAvailable.Clear();
            
            // We clear any main campaign lists.
            GlobalParsingVariables.EntriesMetadata.Clear();
            GlobalParsingVariables.MainGameThemes.Clear();
            GlobalParsingVariables.MainCampaignEmails.Clear();
            GlobalParsingVariables.CustomCallersMainGame.Clear();
            
            // We clear any pending lists.
            GlobalParsingVariables.PendingCustomCampaignCustomCallers.Clear();
            GlobalParsingVariables.PendingCustomCampaignEntries.Clear();
            GlobalParsingVariables.PendingCustomCampaignReplaceEntries.Clear();
            GlobalParsingVariables.PendingCustomCampaignEmails.Clear();
            GlobalParsingVariables.PendingCustomCampaignMusic.Clear();
            GlobalParsingVariables.PendingCustomCampaignModifiers.Clear();
            GlobalParsingVariables.PendingCustomCampaignThemes.Clear();
            GlobalParsingVariables.PendingCustomCampaignVideos.Clear();
            GlobalParsingVariables.PendingCustomCampaignRingtones.Clear();
            
            // We clear any entry permission list.
            EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierOne.Clear();
            EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierTwo.Clear();
            EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierThree.Clear();
            EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierFour.Clear();
            EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierFive.Clear();
            EntryUnlockerPatcher.FixPermissionOverride.EntriesReaddTierSix.Clear();
            
            // We clear all the permissions back to default
            GlobalVariables.entryUnlockScript.firstTierUnlocks.monsterProfiles =
                MainClassForMonsterEntries.CopyTierUnlocks[0];
            GlobalVariables.entryUnlockScript.secondTierUnlocks.monsterProfiles =
                MainClassForMonsterEntries.CopyTierUnlocks[1];
            GlobalVariables.entryUnlockScript.thirdTierUnlocks.monsterProfiles =
                MainClassForMonsterEntries.CopyTierUnlocks[2];
            GlobalVariables.entryUnlockScript.fourthTierUnlocks.monsterProfiles =
                MainClassForMonsterEntries.CopyTierUnlocks[3];
            GlobalVariables.entryUnlockScript.fifthTierUnlocks.monsterProfiles =
                MainClassForMonsterEntries.CopyTierUnlocks[4];
            GlobalVariables.entryUnlockScript.sixthTierUnlocks.monsterProfiles =
                MainClassForMonsterEntries.CopyTierUnlocks[5];
            
            GlobalVariables.entryUnlockScript.xmastFirstTier.monsterProfiles =
                MainClassForMonsterEntries.CopyXmasTier[0];
            GlobalVariables.entryUnlockScript.xmasSecondTier.monsterProfiles =
                MainClassForMonsterEntries.CopyXmasTier[1];
            GlobalVariables.entryUnlockScript.xmasThirdTier.monsterProfiles =
                MainClassForMonsterEntries.CopyXmasTier[2];
            GlobalVariables.entryUnlockScript.xmasFourthTier.monsterProfiles =
                MainClassForMonsterEntries.CopyXmasTier[3];
            
            // Set offset back to the usual ID:
            GlobalParsingVariables.CustomCampaignEntryIDOffset = 100000;
            
            // We now reset the game values:

            GlobalVariables.entryUnlockScript.allEntries.monsterProfiles =
                MainClassForMonsterEntries.CopyMonsterProfiles;

            // We restart the JSON parsing.
            MainClassForMonsterEntries.StartingJSONParsing(GlobalVariables.entryUnlockScript);
            
            // We reload the scene and all values should be correctly loaded?
            if (wasInCustomCampaign 
                && !string.IsNullOrEmpty(activeCustomCampaignName))
            {
                MelonCoroutines.Start(LoadCustomCampaign(activeCustomCampaignName));
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");

                IsInHotReload = false;
            }
        }

        private static IEnumerator LoadCustomCampaign(string activeCustomCampaignName)
        {
            SceneManager.LoadScene("MainMenuScene");
            
            while (SceneManager.GetActiveScene().name != "MainMenuScene" 
                   || !SceneManager.GetActiveScene().isLoaded)
            {
                yield return null;
            }

            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }
            
            CustomCampaignSceneSwitcher.ChangeToCustomCampaignSettings(activeCustomCampaignName);
            
            while (SceneManager.GetActiveScene().name != "MainMenuScene" 
                   || !SceneManager.GetActiveScene().isLoaded)
            {
                yield return null;
            }
            
            yield return null;

            IsInHotReload = false;
        }
    }
}