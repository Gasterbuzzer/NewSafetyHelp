using System;
using System.Collections;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Saving;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class CustomCampaignSceneSwitcher
    {
        /// <summary>
        /// Changes the games state being in a custom campaign.
        /// </summary>
        /// <param name="customCampaignName">Name of the custom campaign to switch to.</param>
        /// <param name="inHotReload">If the changing comes from a hot reload.</param>
        public static IEnumerator ChangeToCustomCampaignSettings(string customCampaignName, bool inHotReload)
        {
            LoggingHelper.InfoLog($"Changing to custom campaign: {customCampaignName}.",
                consoleColor: ConsoleColor.Green);

            // Activate the Custom Campaign
            CustomCampaignGlobal.ActivateCustomCampaign(customCampaignName);

            // Load Custom Campaign values
            CustomCampaignSaving.LoadFromFileCustomCampaignInfo();

            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign != null
                && customCampaign.FadeInCustomCampaign.HasChanged
                && customCampaign.FadeInCustomCampaign.Data
                && !inHotReload)
            {
                GlobalVariables.fade.FadeIn();
                yield return new WaitForSeconds(1f);
            }

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            if (customCampaign != null
                && customCampaign.Skip3DComputerScreenForCustomCampaign.HasChanged
                && !customCampaign.Skip3DComputerScreenForCustomCampaign.Data)
            {
                SceneManager.LoadScene("Computer3DScene");
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }

            LoggingHelper.DebugLog("Finished changing into custom campaign.");
        }

        /// <summary>
        /// Goes back to the main game (if in custom campaign).
        /// </summary>
        /// <param name="alsoLoadMainMenu">If to also reload / load the main menu desktop scene.</param>
        public static void BackToMainGame(bool alsoLoadMainMenu = true)
        {
            LoggingHelper.InfoLog("Going back to the main game.",
                consoleColor: ConsoleColor.Green);

            // Save values
            CustomCampaignSaving.SaveCustomCampaignInfo();

            // Reset back.
            CustomCampaignGlobal.DeactivateCustomCampaign();

            // Load old values.
            GlobalVariables.saveManagerScript.Load();

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            if (alsoLoadMainMenu)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        /// <summary>
        /// Saves the custom campaign values and then loads the desktop scene if requested.
        /// </summary>
        /// <param name="alsoLoadMainMenu">If to also reload / load the main menu desktop scene.</param>
        public static void SaveAndLoadDesktopScene(bool alsoLoadMainMenu = true)
        {
            // Save values
            CustomCampaignSaving.SaveCustomCampaignInfo();

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            if (alsoLoadMainMenu)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}