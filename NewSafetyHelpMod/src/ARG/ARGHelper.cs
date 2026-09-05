using System.Linq;
using System.Text;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
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

            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");

            /*
             * Add Keypad, for inputting the code.
             */

            GameObject rightHandSide = mainMenuCanvas.transform.Find("Desktop/RightHandPrograms").gameObject;

            GameObject argKeypad = Object.Instantiate(rightHandSide.transform.GetChild(0), rightHandSide.transform)
                .gameObject;

            argKeypad.name = "ARGKeyPad";
            argKeypad.transform.SetAsFirstSibling();

            Object.Destroy(argKeypad.GetComponent<LinkExecutable>());

            // Change Executable name.
            argKeypad.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "ADMIN";

            // Change Icon.
            argKeypad.transform.GetComponent<Image>().sprite = GameObject
                .Find("MainMenuCanvas/Desktop/Programs/HSH-Executable").GetComponent<Image>().sprite;

            ARGClickEvent argClickEventComponent = argKeypad.AddComponent<ARGClickEvent>();

            Button doubleClickButton = argKeypad.GetComponent<Button>();

            doubleClickButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            doubleClickButton.onClick.AddListener(argClickEventComponent.OpenKeyPadPopup);

            /*
             * Create Keypad Window
             */

            GameObject keypadPopup = Object
                .Instantiate(mainMenuCanvas.transform.GetChild(4).gameObject, mainMenuCanvas.transform)
                .gameObject;

            ARGKeypadLogic.SetKeypadPopup(keypadPopup);

            // Rename Program
            keypadPopup.name = "KeypadPopup";

            GameObject programTitle = keypadPopup.transform.GetChild(0).GetChild(3).gameObject;

            programTitle.GetComponent<TextMeshProUGUI>().text = "ADMIN PANEL";

            // Resize the Window

            RectTransform keypadRectTransform = keypadPopup.GetComponent<RectTransform>();

            keypadRectTransform.offsetMax = new Vector2(200, 127.645f);
            keypadRectTransform.offsetMin = new Vector2(-200, -159.165f);

            //keypadScrollViewRectTransform.offsetMax = new Vector2(196.095f, 104.965f);
            //keypadScrollViewRectTransform.offsetMin = new Vector2(-198.515f, -133.125f);

            // Remove old content
            Object.Destroy(keypadPopup.transform.GetChild(1).gameObject);

            // Exit Button
            GameObject closeButton = keypadPopup.transform.GetChild(0).GetChild(0).gameObject;

            Button[] buttonComponents = closeButton.GetComponents<Button>();
            
            // Destroy first unused button
            Object.Destroy(buttonComponents[0]);

            buttonComponents[1].onClick.RemoveAllListeners();
            buttonComponents[1].onClick.AddListener(ARGKeypadLogic.CloseKeyPadPopup);

            // Background of Window
            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign.GameFinishedBackground == null)
            {
                // Replace background Image
                keypadPopup.GetComponent<Image>().sprite =
                    mainMenuCanvas.transform.GetChild(0).GetComponent<Image>().sprite;
            }
            else
            {
                keypadPopup.GetComponent<Image>().sprite = customCampaign.GameFinishedBackground;
            }
        }
    }
}