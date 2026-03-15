using System;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.Saving;
using NewSafetyHelp.LoggingSystem;
using UnityEngine.SceneManagement;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class CustomCampaignSceneSwitcher
    {
        public static void ChangeToCustomCampaignSettings(string customCampaignName)
        {
            LoggingHelper.InfoLog($"Changing to custom campaign: {customCampaignName}.",
                consoleColor: ConsoleColor.Green);

            // Activate the Custom Campaign
            CustomCampaignGlobal.ActivateCustomCampaign(customCampaignName);

            // Load Custom Campaign values
            CustomCampaignSaving.LoadFromFileCustomCampaignInfo();

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            SceneManager.LoadScene("MainMenuScene");
        }

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
    }
}