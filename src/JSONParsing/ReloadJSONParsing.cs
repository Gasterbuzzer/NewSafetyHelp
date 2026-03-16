using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomDesktop.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewSafetyHelp.JSONParsing
{
    /// <summary>
    /// Contains helper functions to redo the initialization of all the parsed values.
    /// </summary>
    public static class ReloadJSONParsing
    {
        /// <summary>
        /// Loads all JSON files again and resets any loaded value to default values before.
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
                
                CustomCampaignSceneSwitcher.BackToMainGame(false);
            }
            
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
                CustomCampaignSceneSwitcher.ChangeToCustomCampaignSettings(activeCustomCampaignName);
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}