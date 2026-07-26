using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomDesktop;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.CustomCampaignSystem.CustomTextFiles
{
    public static class CustomTextFileHelper
    {
        public static Sprite TextFileIcon;

        /// <summary>
        /// Creates a custom text file on the desktop.
        /// </summary>
        /// <param name="customTextFile">Custom Text file to create.</param>
        /// <returns>GameObject Text file that was created.</returns>
        public static GameObject CreateCustomTextFile(CustomTextFile customTextFile)
        {
            GameObject desktopCreditsProgram =
                CustomDesktopHelper.GetLeftPrograms().transform.Find("Readme").gameObject;

            GameObject newCustomTextFile =
                Object.Instantiate(desktopCreditsProgram, desktopCreditsProgram.transform.parent);

            if (string.IsNullOrEmpty(customTextFile.FileNameOnDesktop))
            {
                LoggingHelper.ErrorLog("No desktop name provided for the custom text file to be created!");
            }

            newCustomTextFile.name = customTextFile.FileNameOnDesktop + customTextFile.UnlockDay;

            // Make sure the icon is correct
            if (TextFileIcon != null)
            {
                newCustomTextFile.GetComponent<Image>().sprite = TextFileIcon;
            }

            // Update desktop name
            TextMeshProUGUI textChildGameObjectText = newCustomTextFile.transform.Find("TextBackground").transform
                .Find("ExecutableName").gameObject.GetComponent<TextMeshProUGUI>();

            textChildGameObjectText.text = customTextFile.FileNameOnDesktop;

            // Add text content
            TextFileExecutable textFileExecutable = newCustomTextFile.GetComponent<TextFileExecutable>();

            textFileExecutable.myContent = customTextFile.TextFileContents;

            if (customTextFile.GameObjectOrder.HasChanged)
            {
                textFileExecutable.transform.SetSiblingIndex(customTextFile.GameObjectOrder.Data);
            }

            // We enable the text file and then check if we disable it.
            newCustomTextFile.SetActive(true);

            if (GlobalVariables.currentDay < customTextFile.UnlockDay)
            {
                newCustomTextFile.SetActive(false);
            }

            if (!AccuracyTextFileHelper.CheckIfTextFilePassAccuracyChecks(customTextFile))
            {
                newCustomTextFile.SetActive(false);
            }

            customTextFile.CustomTextFileReference = newCustomTextFile;

            return newCustomTextFile;
        }
    }
}