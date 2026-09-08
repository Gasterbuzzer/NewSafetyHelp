using System.Linq;
using System.Text;
using NewSafetyHelp.CustomCampaignSystem;
using UnityEngine;

namespace NewSafetyHelp.ARG
{
    public static class ARGHelper
    {
        private static readonly byte[] ARGTestCampaignName = { 84, 69, 83, 72, 83, 72 }; // TESHSH

        private static readonly byte[] ARGCampaignName =
            { 83, 117, 109, 109, 101, 114, 32, 67, 111, 99, 107, 32, 83, 117, 99, 107, 101, 114 };

        /// <summary>
        /// Creates the input capture for the ARG in the selected custom campaign.
        /// </summary>
        public static void InitializeARGDesktop()
        {
            byte[] campaignAsciiName =
                Encoding.ASCII.GetBytes(CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName);

            // Prevent this in main campaign or not correct custom campaign.
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return;
            }

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
            byte[] campaignAsciiName =
                Encoding.ASCII.GetBytes(CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName);

            // Prevent this in main campaign or not correct custom campaign.
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return;
            }

            if (!campaignAsciiName.SequenceEqual(ARGCampaignName)
                && !campaignAsciiName.SequenceEqual(ARGTestCampaignName))
            {
                return;
            }

            ARGKeypad.CreateKeypad();
        }
    }
}