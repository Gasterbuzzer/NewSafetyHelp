using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.ARG
{
    public static class ARGHelper
    {
        private static readonly byte[] ARGTestCampaignName = { 84, 69, 83, 72, 83, 72 }; // TESHSH

        private static readonly byte[] ARGCampaignName =
            { 83, 117, 109, 109, 101, 114, 32, 67, 111, 99, 107, 32, 83, 117, 99, 107, 101, 114 };

        private static readonly FieldInfo MyImage = typeof(AdhereToPalette).GetField("myImage",
            BindingFlags.NonPublic | BindingFlags.Instance);

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

            GameObject programLogo = keypadPopup.transform.GetChild(0).GetChild(2).gameObject;

            programLogo.GetComponent<Image>().sprite = GameObject.Find("MainMenuCanvas/Desktop/Programs/HSH-Executable")
                .GetComponent<Image>().sprite;

            // Resize the Window

            RectTransform keypadRectTransform = keypadPopup.GetComponent<RectTransform>();

            keypadRectTransform.offsetMax = new Vector2(200, 127.645f);
            keypadRectTransform.offsetMin = new Vector2(-200, -159.165f);

            // Remove old content
            Object.Destroy(keypadPopup.transform.GetChild(1).GetChild(1).gameObject);
            Object.Destroy(keypadPopup.transform.GetChild(1).GetChild(0).gameObject);

            // Exit Button
            GameObject closeButton = keypadPopup.transform.GetChild(0).GetChild(0).gameObject;

            Button[] buttonComponents = closeButton.GetComponents<Button>();

            // Destroy first unused button
            Object.Destroy(buttonComponents[0]);

            buttonComponents[1].onClick.RemoveAllListeners();
            buttonComponents[1].onClick.AddListener(ARGKeypadLogic.CloseKeyPadPopup);

            // Update View
            GameObject keypadScrollView = keypadPopup.transform.GetChild(1).gameObject;

            RectTransform keypadScrollViewRectTransform = keypadScrollView.GetComponent<RectTransform>();

            keypadScrollViewRectTransform.offsetMax = new Vector2(194.095f, 104.965f);
            keypadScrollViewRectTransform.offsetMin = new Vector2(-194.515f, -136.125f);

            // Background of Window
            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            keypadScrollView.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

            if (customCampaign.GameFinishedBackground == null)
            {
                // Replace background Image
                keypadScrollView.GetComponent<Image>().sprite =
                    mainMenuCanvas.transform.GetChild(0).GetComponent<Image>().sprite;
            }
            else
            {
                keypadScrollView.GetComponent<Image>().sprite = customCampaign.GameFinishedBackground;
            }

            // Create UI Key Input
            GameObject minimizeButton = keypadPopup.transform.GetChild(0).GetChild(1).gameObject;

            GameObject submitButton = Object.Instantiate(minimizeButton, keypadScrollView.transform);

            submitButton.name = "SubmitButton";

            RectTransform submitButtonRectTransform = submitButton.GetComponent<RectTransform>();

            submitButtonRectTransform.anchoredPosition = new Vector2(0, 0);
            submitButtonRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            submitButtonRectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            submitButtonRectTransform.offsetMin = new Vector2(-50, -20);
            submitButtonRectTransform.offsetMax = new Vector2(50, 20);

            submitButtonRectTransform.pivot = new Vector2(0.5f, 2.8f);

            GameObject submitButtonText = submitButton.transform.GetChild(0).gameObject;

            submitButtonText.GetComponent<TextMeshProUGUI>().text = "Submit";
            submitButtonText.GetComponent<TextMeshProUGUI>().fontSizeMax = 30;

            RectTransform submitButtonTextRectTransform = submitButtonText.GetComponent<RectTransform>();

            submitButtonTextRectTransform.offsetMin = new Vector2(0, 0);
            submitButtonTextRectTransform.offsetMax = new Vector2(0, 0);

            submitButton.SetActive(true);

            // Create UI Title

            GameObject inputPasscodeLabel = Object.Instantiate(submitButtonText, keypadScrollView.transform);

            inputPasscodeLabel.name = "InputPasscodeLabel";

            inputPasscodeLabel.GetComponent<TextMeshProUGUI>().text = "INPUT PASSCODE";

            inputPasscodeLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            inputPasscodeLabel.AddComponent<CanvasGroup>().blocksRaycasts = false;

            // Create Input Fields
            List<GameObject> inputFields = new List<GameObject>();

            for (int i = 0; i < 4; i++)
            {
                // Create base of the input field
                GameObject newInputField = new GameObject("InputField" + i, typeof(RectTransform));
                newInputField.transform.SetParent(keypadScrollView.transform, false);

                RectTransform inputRect = newInputField.GetComponent<RectTransform>();
                inputRect.sizeDelta = new Vector2(50, 80);
                inputRect.anchoredPosition = new Vector2(-130 + (i * 86), 0);

                Image newInputFieldImage = newInputField.AddComponent<Image>();
                newInputFieldImage.color = new Color(0.22745f, 0.27450f, 0.21960f, 1f);

                TMP_InputField inputField = newInputField.AddComponent<TMP_InputField>();
                inputField.characterLimit = 1;

                int currentIndex = i;

                inputField.onValueChanged.AddListener(_ =>
                {
                    if (inputFields.Count >= 4)
                    {
                        EventSystem.current.SetSelectedGameObject(inputFields[(currentIndex + 1) % 4]);
                    }
                });

                // Text Area to contain the text
                GameObject textArea = new GameObject("TextAreaBox", typeof(RectTransform));
                textArea.transform.SetParent(newInputField.transform, false);

                RectTransform textAreaRectTransform = textArea.GetComponent<RectTransform>();
                textAreaRectTransform.anchorMin = Vector2.zero;
                textAreaRectTransform.anchorMax = Vector2.one;
                textAreaRectTransform.offsetMin = new Vector2(10, 6);
                textAreaRectTransform.offsetMax = new Vector2(-10, -6);

                // Actual Text
                GameObject inputFieldText = new GameObject("InputFieldText", typeof(RectTransform));
                inputFieldText.transform.SetParent(textArea.transform, false);

                RectTransform textRectTransform = inputFieldText.GetComponent<RectTransform>();
                textRectTransform.anchorMin = Vector2.zero;
                textRectTransform.anchorMax = Vector2.one;
                textRectTransform.offsetMin = Vector2.zero;
                textRectTransform.offsetMax = Vector2.zero;

                TextMeshProUGUI inputTextComponent = inputFieldText.AddComponent<TextMeshProUGUI>();
                inputTextComponent.fontSize = 24;
                inputTextComponent.color = Color.white;
                inputTextComponent.font = inputPasscodeLabel.GetComponent<TextMeshProUGUI>().font;
                inputTextComponent.alignment = TextAlignmentOptions.Center;

                // Connect GameObjects to input field
                inputField.textViewport = textAreaRectTransform;
                inputField.textComponent = inputTextComponent;
                inputField.text = "0";

                //Adhere to color palette
                AdhereToPalette adhereToPaletteComponent = newInputField.AddComponent<AdhereToPalette>();

                MyImage.SetValue(adhereToPaletteComponent, newInputFieldImage);
                adhereToPaletteComponent.colorSwatchInt = 3;

                adhereToPaletteComponent.ChangeColors();

                // Add to the list
                inputFields.Add(newInputField);
            }

            // Connect Input Fields to Submit button
            submitButton.AddComponent<Button>().onClick.AddListener(() =>
            {
                if (inputFields.Count >= 4)
                {
                    string inputCode =
                        $"{inputFields[0].GetComponent<TMP_InputField>().text}" +
                        $"{inputFields[1].GetComponent<TMP_InputField>().text}" +
                        $"{inputFields[2].GetComponent<TMP_InputField>().text}" +
                        $"{inputFields[3].GetComponent<TMP_InputField>().text}";

                    LoggingHelper.DebugLog(
                        $"Submitted Code: '{inputCode}'.");
                }
            });
        }
    }
}