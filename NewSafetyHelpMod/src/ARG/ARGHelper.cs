using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.ARG
{
    public static class ARGHelper
    {
        private static readonly byte[] ARGTestCampaignName = { 84, 69, 83, 72, 83, 72 }; // TESHSH

        private static readonly byte[] ARGCampaignName =
            { 83, 117, 109, 109, 101, 114, 32, 67, 111, 99, 107, 32, 83, 117, 99, 107, 101, 114 };

        /// <summary>
        /// MonoBehaviour class for capturing input for the ARG.
        /// </summary>
        public class ARGCaptureInput : MonoBehaviour
        {
            private static readonly List<KeyCode> KeyPresses = new List<KeyCode>
            {
                KeyCode.DownArrow,
                KeyCode.DownArrow,
                KeyCode.UpArrow,
                KeyCode.LeftArrow,
                KeyCode.RightArrow
            };

            private static float lastPressTime;
            private static int lastKeyPressedIndex;

            private const float TimeOutTime = 2f;

            private void Update()
            {
                if (lastKeyPressedIndex > 0
                    && Time.time - lastPressTime > TimeOutTime)
                {
                    lastKeyPressedIndex = 0;
                }

                KeyCode expectedKey = KeyPresses[lastKeyPressedIndex];

                if (Input.GetKeyDown(expectedKey))
                {
                    lastKeyPressedIndex++;
                    lastPressTime = Time.time;

                    if (lastKeyPressedIndex >= KeyPresses.Count)
                    {
                        OpenARGHTML();
                        lastKeyPressedIndex = 0;
                    }
                }
                else if (Input.anyKeyDown)
                {
                    lastKeyPressedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Open up the HTML page in browser for the ARG:
        /// </summary>
        public static void OpenARGHTML()
        {
            LoggingHelper.InfoLog("Opening HTML file in browser.");

            string htmlContents =
                "<H1>In ■he blackest wake of an endless sea, An un■ealthy dosing of abno■mality, Only th■n will you truly see, Just what it is you can b■.</H1>";

            // Path to temp file.
            string tempFilePath = Path.Combine(
                Path.GetTempPath(),
                $"{EmbedHelpers.NewSafetyHelpPrefix}secrets_{Guid.NewGuid()}.html");

            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    streamWriter.WriteLine(htmlContents);
                }
            }

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = tempFilePath,
                    UseShellExecute = true
                });
        }

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

            // Add ARG Input
            GameObject.Find("MainMenuCanvas").gameObject.AddComponent<ARGCaptureInput>();
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

            // Add Keypad, for inputting the code.
            GameObject rightHandSide = GameObject.Find("MainMenuCanvas/Desktop/RightHandPrograms");

            GameObject argKeypad = Object.Instantiate(rightHandSide.transform.GetChild(0), rightHandSide.transform)
                .gameObject;

            argKeypad.name = "ARGKeyPad";
            argKeypad.transform.SetAsLastSibling();
            Object.Destroy(argKeypad.GetComponent<LinkExecutable>());

            // Change Executable name.
            argKeypad.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "ADMIN";

            // Change Icon.
            argKeypad.transform.GetComponent<Image>().sprite = GameObject
                .Find("MainMenuCanvas/Desktop/Programs/HSH-Executable").GetComponent<Image>().sprite;
        }
    }
}