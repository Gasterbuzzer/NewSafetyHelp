using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.ARG
{
    public static class ARGHelper
    {
        private static readonly string ARGTestCampaignName = "TESHSH";
        private static readonly string ARGCampaignName = "Summer Cock Sucker";

        /// <summary>
        /// MonoBehaviour class for capturing input for the ARG.
        /// </summary>
        public class ARGCaptureInput : MonoBehaviour
        {
            private static List<KeyCode> keyPresses = new List<KeyCode>
            {
                KeyCode.UpArrow,
                KeyCode.DownArrow,
                KeyCode.UpArrow,
                KeyCode.RightArrow,
                KeyCode.Space
            };

            private static float lastPressTime = 0;
            private static int lastKeyPressedIndex = 0;

            private static readonly float timeOutTime = 2f;

            private void Update()
            {
                if (lastKeyPressedIndex > 0
                    && Time.time - lastPressTime > timeOutTime)
                {
                    lastKeyPressedIndex = 0;
                }

                KeyCode expectedKey = keyPresses[lastKeyPressedIndex];

                if (Input.GetKeyDown(expectedKey))
                {
                    lastKeyPressedIndex++;
                    lastPressTime = Time.time;

                    if (lastKeyPressedIndex >= keyPresses.Count)
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

        public static void CreateInputCapture()
        {
            // Prevent this in main campaign or not correct custom campaign.
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                return;
            }
            else if (CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName != ARGCampaignName
                     && CustomCampaignGlobal.GetActiveCustomCampaign().CampaignName != ARGTestCampaignName)
            {
                return;
            }

            GameObject.Find("MainMenuCanvas").gameObject.AddComponent<ARGCaptureInput>();
        }
    }
}