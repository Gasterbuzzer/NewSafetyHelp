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
                MelonLogger.Error("ERROR: Failed to find MainMenuCanvas. Possibly called outside of MainMenuCanvas?");
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
                MelonLogger.Error("ERROR: Failed to find Desktop from Main Menu. Possibly called outside of MainMenuCanvas?");
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
                MelonLogger.Error("ERROR: Failed to find left sided Programs from Desktop. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the GameObject for the Program icons on the right side on the Desktop.
        /// </summary>
        /// <returns> GameObject for the programs on the right. </returns>
        public static GameObject getRightPrograms()
        {
            GameObject foundRightSidePrograms = getDesktop().transform.Find("RightHandPrograms").gameObject;

            if (foundRightSidePrograms != null)
            {
                return foundRightSidePrograms;
            }
            else
            {
                MelonLogger.Error("ERROR: Failed to find right sided Programs from Desktop. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the GameObject for the Winter DLC program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the winter dlc program on the left. </returns>
        public static GameObject getWinterDLCProgram()
        {
            GameObject winterDLCProgram = getLeftPrograms().transform.Find("DLC-Executable").gameObject;

            if (winterDLCProgram != null)
            {
                return winterDLCProgram;
            }
            else
            {
                MelonLogger.Error("ERROR: Failed to find the winter DLC program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the GameObject for the main game program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the main game program on the left. </returns>
        public static GameObject getMainGameProgram()
        {
            GameObject mainGameProgram = getLeftPrograms().transform.Find("HSH-Executable").gameObject;

            if (mainGameProgram != null)
            {
                return mainGameProgram;
            }
            else
            {
                MelonLogger.Error("ERROR: Failed to find the main game program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the GameObject for the NSE Discord program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the NSE Discord program on the right. </returns>
        public static GameObject getNSEDiscordProgram()
        {
            GameObject discordProgram = getRightPrograms().transform.Find("Discord-Executable").gameObject;

            if (discordProgram != null)
            {
                return discordProgram;
            }
            else
            {
                MelonLogger.Error("ERROR: Failed to find the discord program. Possibly called outside of MainMenuCanvas?");
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
        
        public static void createBackToMainGameButton()
        {
            GameObject backToMainMenuGameButton = (GameObject) Object.Instantiate(getNSEDiscordProgram(), getRightPrograms().transform);

            Object.Destroy(backToMainMenuGameButton.GetComponent<LinkExecutable>()); // Remove old executable Behavior.

            // Change Program Name
            backToMainMenuGameButton.transform.Find("TextBackground").Find("ExecutableName").GetComponent<TextMeshProUGUI>().text = "Back to Main Game.";
            
            // Change Program Icon 
            backToMainMenuGameButton.GetComponent<Image>().sprite = getMainGameProgram().GetComponent<Image>().sprite;
            // Reset Color
            backToMainMenuGameButton.GetComponent<Image>().color = Color.white;
            
            // Button Changes.
            Button customProgramButton = backToMainMenuGameButton.GetComponent<Button>();
            
            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.
            
            customProgramButton.onClick.AddListener(backToMainGame);
        }


        public static void changeToCustomCampaignSettings(string customCampaignName)
        {
            MelonLogger.Msg(ConsoleColor.Green, $"INFO: Changing to custom campaign: {customCampaignName}.");
            
            // Activate the Custom Campaign
            CustomCampaignGlobal.activateCustomCampaign(customCampaignName);
            
            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            SceneManager.LoadScene("MainMenuScene");
        }

        public static void backToMainGame()
        {
            MelonLogger.Msg(ConsoleColor.Green, $"INFO: Going back to the main game.");
            
            // Reset back.
            CustomCampaignGlobal.deactivateCustomCampaign();
            
            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}