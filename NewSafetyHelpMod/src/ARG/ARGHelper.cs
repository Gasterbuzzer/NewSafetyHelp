using System.Linq;
using System.Text;
using NewSafetyHelp.ARG.ARGGUI;
using NewSafetyHelp.ARG.ARGLogic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.JSONParsing.CCParsing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.ARG
{
    public static class ARGHelper
    {
        private static readonly byte[] ARGTestCampaignName = { 84, 69, 83, 72, 83, 72 }; // TESHSH

        private static readonly byte[] ARGCampaignName =
            { 83, 117, 109, 109, 101, 114, 32, 67, 111, 99, 107, 32, 83, 117, 99, 107, 101, 114 };

        /// <summary>
        /// Gets the files for the ARG if the loaded campaign is the correct one.
        /// </summary>
        /// <param name="customCampaignName">Custom campaign being loaded.</param>
        /// <param name="usermodFolder">Folder where the usermod is located at.</param>
        public static void GetARGFilesAtParsing(string customCampaignName, string usermodFolder)
        {
            byte[] campaignAsciiName = Encoding.ASCII.GetBytes(customCampaignName);

            if (!campaignAsciiName.SequenceEqual(ARGCampaignName)
                && !campaignAsciiName.SequenceEqual(ARGTestCampaignName))
            {
                return;
            }

            ARGParsing.GetAllARGFiles(usermodFolder);
        }

        /// <summary>
        /// Creates the input capture for the ARG in the selected custom campaign.
        /// </summary>
        public static void InitializeARGDesktop()
        {
            // Prevent this in main campaign or not correct custom campaign.
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return;
            }

            byte[] campaignAsciiName =
                Encoding.ASCII.GetBytes(CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName);

            if (!campaignAsciiName.SequenceEqual(ARGCampaignName)
                && !campaignAsciiName.SequenceEqual(ARGTestCampaignName))
            {
                return;
            }

            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");

            // Add ARG Input
            mainMenuCanvas.gameObject.AddComponent<ARGSecretInputMono.ARGCaptureInput>();
        }

        /// <summary>
        /// Creates the input capture for the ARG in the selected custom campaign.
        /// </summary>
        public static void SetupARGDesktop()
        {
            // Prevent this in main campaign or not correct custom campaign.
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return;
            }

            byte[] campaignAsciiName =
                Encoding.ASCII.GetBytes(CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName);

            if (!campaignAsciiName.SequenceEqual(ARGCampaignName)
                && !campaignAsciiName.SequenceEqual(ARGTestCampaignName))
            {
                return;
            }

            ARGKeypad.CreateKeypad();

            ARGDesktopVideo.CreateFullScreenVideoPlayer();
            ARGAchievements.CreateAchievementsIcon();
        }

        /// <summary>
        /// Creates an error popup with the provided error message.
        /// </summary>
        /// <param name="errorMessage">Message to display.</param>
        /// <param name="mainMenuCanvas">Canvas to display it at.</param>
        /// <param name="errorTile">Title to show in the error popup.</param>
        public static void CreateErrorMessage(string errorMessage, Transform mainMenuCanvas,
            string errorTile = "ERROR: INVALID PASSWORD")
        {
            if (errorMessage == "")
            {
                return;
            }

            GameObject errorGameObject =
                Object.Instantiate(GlobalVariables.entryCanvasScript.errorPrefab, mainMenuCanvas);

            GameObject errorLogo = errorGameObject.transform.GetChild(0).GetChild(2).gameObject;
            errorLogo.GetComponent<Image>().sprite = EmbedLoader.AdminIcon;
            errorLogo.SetActive(true);

            GameObject errorTitle = errorGameObject.transform.GetChild(0).GetChild(3).gameObject;
            errorTitle.GetComponent<TextMeshProUGUI>().text = errorTile;
            errorTitle.SetActive(true);

            errorGameObject.GetComponent<GenericErrorPopupBehavior>().myErrorText.text = errorMessage;
        }

        /// <summary>
        /// For the ARG, it creates custom settings options.
        /// </summary>
        public static void ARGSettingsSetup()
        {
            // Prevent this in main campaign or not correct custom campaign.
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return;
            }

            byte[] campaignAsciiName =
                Encoding.ASCII.GetBytes(CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName);

            if (!campaignAsciiName.SequenceEqual(ARGCampaignName)
                && !campaignAsciiName.SequenceEqual(ARGTestCampaignName))
            {
                return;
            }

            ARGSettings.CreateCustomInGameSettings();
        }
    }
}