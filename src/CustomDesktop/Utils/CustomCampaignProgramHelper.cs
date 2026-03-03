using NewSafetyHelp.CustomDesktop.CustomDoubleClickButton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class CustomCampaignProgramHelper
    {
        public static void CreateCustomProgramIcon(string customProgramName, string customCampaignName,
            Sprite customIcon = null)
        {
            GameObject customProgramIcon = Object.Instantiate(CustomDesktopHelper.GetWinterDlcProgram(),
                CustomDesktopHelper.GetLeftPrograms().transform);

            Object.Destroy(customProgramIcon.GetComponent<HSHExecutableBehavior>()); // Remove old Executable Behavior.

            // Change Program Name
            TextMeshProUGUI programName = customProgramIcon.transform.Find("TextBackground").Find("ExecutableName")
                .GetComponent<TextMeshProUGUI>();
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

            DoubleClickButton doubleClickButton = customProgramIcon.AddComponent<DoubleClickButton>();

            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            customProgramButton.onClick.AddListener(() =>
                doubleClickButton.DoubleClickCustomCampaign(customCampaignName));

            // Rename CustomProgramIcon
            customProgramIcon.name = customProgramName;

            // Enable if disabled.
            customProgramIcon.SetActive(true);
        }

        public static void CreateBackToMainGameButton()
        {
            GameObject backToMainMenuGameButton =
                Object.Instantiate(CustomDesktopHelper.GetNSEDiscordProgram(),
                    CustomDesktopHelper.GetRightPrograms().transform);

            Object.Destroy(backToMainMenuGameButton.GetComponent<LinkExecutable>()); // Remove old executable Behavior.

            // Change Program Name
            backToMainMenuGameButton.transform.Find("TextBackground").Find("ExecutableName")
                .GetComponent<TextMeshProUGUI>().text = "Back to Main Game.";

            // Change Program Icon 
            backToMainMenuGameButton.GetComponent<Image>().sprite = CustomDesktopHelper.GetMainGameProgram().GetComponent<Image>().sprite;
            // Reset Color
            backToMainMenuGameButton.GetComponent<Image>().color = Color.white;

            // Button Changes.
            Button customProgramButton = backToMainMenuGameButton.GetComponent<Button>();

            DoubleClickButton doubleClickButton = backToMainMenuGameButton.AddComponent<DoubleClickButton>();

            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            customProgramButton.onClick.AddListener(() => doubleClickButton.DoubleClickBackToMainGameAlsoLoad());

            // Rename Program Object Name
            backToMainMenuGameButton.name = "BackToMainGameButton";
        }
    }
}