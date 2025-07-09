using System;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomDesktop
{
    public static class CustomDesktopHelper
    {
        /// <summary>
        /// Finds the main menu canvas.
        /// </summary>
        /// <returns>GameObject for the main menu canvas. </returns>
        public static GameObject getMainMenuCanvas()
        {
            GameObject foundMainMenuCanvas = GameObject.Find("MainMenuCanvas");

            if (foundMainMenuCanvas != null)
            {
                return foundMainMenuCanvas;
            }
            else
            {
                MelonLogger.Msg("Failed to find MainMenuCanvas. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the desktop GameObject from the main menu canvas.
        /// </summary>
        /// <returns>Desktop GameObject</returns>
        public static GameObject getDesktop()
        {
            GameObject foundDesktop = getMainMenuCanvas().transform.Find("Desktop").gameObject;

            if (foundDesktop != null)
            {
                return foundDesktop;
            }
            else
            {
                MelonLogger.Msg("Failed to find Desktop from Main Menu. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the GameObject for the Program icons on the left side on the Desktop.
        /// </summary>
        /// <returns> GameObject for the programs on the left. </returns>
        public static GameObject getLeftPrograms()
        {
            GameObject foundLeftSidePrograms = getDesktop().transform.Find("Programs").gameObject;

            if (foundLeftSidePrograms != null)
            {
                return foundLeftSidePrograms;
            }
            else
            {
                MelonLogger.Msg("Failed to find left sided Programs from Desktop. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the GameObject for the Winter DLC program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the programs on the left. </returns>
        public static GameObject getWinterDLCProgram()
        {
            GameObject winterDLCProgram = getLeftPrograms().transform.Find("DLC-Executable").gameObject;

            if (winterDLCProgram != null)
            {
                return winterDLCProgram;
            }
            else
            {
                MelonLogger.Msg("Failed to find the winter DLC program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }


        public static void createCustomProgramIcon(string customProgramName, string customCampaignName, Sprite customIcon = null)
        {
            GameObject customProgramIcon = (GameObject) Object.Instantiate(getWinterDLCProgram(), getLeftPrograms().transform);

            Object.Destroy(customProgramIcon.GetComponent<HSHExecutableBehavior>()); // Remove old Executable Behavior.

            // Change Program Name
            TextMeshProUGUI programName = customProgramIcon.transform.Find("TextBackground").Find("ExecutableName").GetComponent<TextMeshProUGUI>();
            programName.text = customProgramName;
            
            // Change Program Icon if provided.
            Image customProgramImage = customProgramIcon.GetComponent<Image>();
            if (customIcon != null)
            {
                customProgramImage.sprite = customIcon;
            }
            
            // Reset Color
            customProgramImage.color = Color.white;
            
            // Button Changes.
            Button customProgramButton = customProgramIcon.GetComponent<Button>();
            
            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.
            
            customProgramButton.onClick.AddListener(() => changeToCustomCampaignSettings(customCampaignName));

        }


        public static void changeToCustomCampaignSettings(string customCampaignName)
        {
            MelonLogger.Msg(ConsoleColor.Green, $"INFO: Changing to custom campaign: {customCampaignName}.");
            
            // Activate the Custom Campaign
            CustomCampaignGlobal.activateCustomCampaign(customCampaignName);
            
            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}