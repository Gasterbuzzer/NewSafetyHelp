using System;
using System.Collections.Generic;
using System.Reflection;
using NewSafetyHelp.ARG.ARGLogic;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.ARG.ARGGUI
{
    public static class ARGKeypad
    {
        private static readonly FieldInfo MyImage = typeof(AdhereToPalette).GetField("myImage",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly Vector4 Key = new Vector4(12, -9, 49, 63);

        private static List<GameObject> inputFields = new List<GameObject>();

        private static AudioSource keypadAudioSource;

        /// <summary>
        /// Creates the ARG Keypad.
        /// </summary>
        public static void CreateKeypad()
        {
            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");

            /*
             * Add Keypad, for inputting the code.
             */

            GameObject rightHandSide = mainMenuCanvas.transform.Find("Desktop/RightHandPrograms").gameObject;

            GameObject argKeypad = Object.Instantiate(rightHandSide.transform.Find("Discord-Executable"),
                rightHandSide.transform).gameObject;

            argKeypad.name = "ARGKeyPad";
            argKeypad.transform.SetAsFirstSibling();

            Object.Destroy(argKeypad.GetComponent<LinkExecutable>());

            // Change Executable name.
            argKeypad.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "ADMIN";

            // Change Icon.
            argKeypad.transform.GetComponent<Image>().sprite = EmbedLoader.AdminIcon;

            ARGClickEvent argClickEventComponent = argKeypad.AddComponent<ARGClickEvent>();

            Button doubleClickButton = argKeypad.GetComponent<Button>();

            doubleClickButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            doubleClickButton.onClick.AddListener(argClickEventComponent.OpenKeyPadPopup);

            argKeypad.SetActive(true);

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

            programLogo.GetComponent<Image>().sprite = EmbedLoader.AdminIcon;

            // Add Sound Source
            keypadAudioSource = keypadPopup.AddComponent<AudioSource>();
            keypadAudioSource.playOnAwake = false;
            keypadAudioSource.volume = 0.5f;

            GameObject optionsPopup =
                GameObject.Find("MainMenuCanvas").transform.Find("OptionsPopup").gameObject;

            AudioMixer audioMixer = optionsPopup.GetComponent<OptionsMenuBehavior>().masterMixer;

            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups("SFX");

            if (audioMixerGroups.Length > 0)
            {
                keypadAudioSource.outputAudioMixerGroup = audioMixerGroups[0];
            }
            else
            {
                LoggingHelper.ErrorLog("Could not add keypad sounds to SFX group.");
            }

            // Resize the Window

            RectTransform keypadRectTransform = keypadPopup.GetComponent<RectTransform>();

            keypadRectTransform.offsetMax = new Vector2(200, 127.645f);
            keypadRectTransform.offsetMin = new Vector2(-200, -159.165f);

            // Add Keypad Input Logic
            keypadPopup.AddComponent<ARGKeypadInput.ARGCaptureKeypadInput>();

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
            inputFields = new List<GameObject>();

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
                inputField.transition = Selectable.Transition.None;
                inputField.interactable = false;

                newInputField.AddComponent<Shadow>();

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
                inputField.text = "-";

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
                    List<string> inputKey = new List<string>
                    {
                        inputFields[0].GetComponent<TMP_InputField>().text,
                        inputFields[1].GetComponent<TMP_InputField>().text,
                        inputFields[2].GetComponent<TMP_InputField>().text,
                        inputFields[3].GetComponent<TMP_InputField>().text
                    };

                    List<int> inputKeyInt = new List<int>();

                    if (Int32.TryParse(inputKey[0], out int firstDigit)
                        && Int32.TryParse(inputKey[1], out int secondDigit)
                        && Int32.TryParse(inputKey[2], out int thirdDigit)
                        && Int32.TryParse(inputKey[3], out int fourthDigit))
                    {
                        inputKeyInt.Add(firstDigit);
                        inputKeyInt.Add(secondDigit);
                        inputKeyInt.Add(thirdDigit);
                        inputKeyInt.Add(fourthDigit);
                    }

                    if (inputKeyInt.Count < 4)
                    {
                        inputKeyInt.Clear();

                        for (int i = 0; i < 4; i++)
                        {
                            inputKeyInt.Add(0);
                        }
                    }

                    Vector4 inputKeyVector =
                        new Vector4(inputKeyInt[0], inputKeyInt[1], inputKeyInt[2], inputKeyInt[3]);

                    string inputCode =
                        $"{inputKey[0]}" +
                        $"{inputKey[1]}" +
                        $"{inputKey[2]}" +
                        $"{inputKey[3]}";

                    LoggingHelper.DebugLog($"Submitted Code: '{inputCode}'.");

                    // Key
                    if ((inputKeyVector + new Vector4(8, -12, 42, 54)).Equals(Key))
                    {
                        LoggingHelper.DebugLog($"Detected correct digit code: '{inputCode}'.");

                        // TO DO: Implement logic to avoid this triggering twice.

                        ARGDesktopVideo.PlayFullScreenVideo();
                    }
                    else if (inputKeyVector.Equals(new Vector4(1, 9, 8, 7)) ||
                             inputKeyVector.Equals(new Vector4(1, 9, 8, 3)))
                    {
                        ARGHelper.CreateErrorMessage("Bite victim information package collected.",
                            mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(1, 1, 1, 1)))
                    {
                        ARGHelper.CreateErrorMessage("Briefcase protection team dispatched!", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(3, 3, 1, 3)))
                    {
                        ARGHelper.CreateErrorMessage("If you're the adventurous sort, " +
                                                     "pay a visit to Toni’s and get some pizza!",
                            mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(1, 2, 3, 4)))
                    {
                        ARGHelper.CreateErrorMessage("Poor password detected, please reset.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(2, 0, 3, 6)))
                    {
                        ARGHelper.CreateErrorMessage("The year the universe will end.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(8, 8, 8, 8)))
                    {
                        ARGHelper.CreateErrorMessage("Skull throne replenished.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(1, 2, 1, 1)))
                    {
                        ARGHelper.CreateErrorMessage("Running visitor detection software.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(4, 2, 5, 5)))
                    {
                        ARGHelper.CreateErrorMessage("Inscribing data.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(6, 9, 6, 9)))
                    {
                        ARGHelper.CreateErrorMessage("Nice.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(0, 6, 2, 8)))
                    {
                        ARGHelper.CreateErrorMessage("HAPPY BIRTHDAY!!!!!", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(0, 6, 0, 5)))
                    {
                        ARGHelper.CreateErrorMessage("Initiating rebirth protocol.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(2, 3, 1, 9)))
                    {
                        ARGHelper.CreateErrorMessage("Quarantine squad requested.", mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(8, 0, 0, 8)))
                    {
                        ARGHelper.CreateErrorMessage("You aren't as funny as you think you are.",
                            mainMenuCanvas.transform);
                    }
                    else if (inputKeyVector.Equals(new Vector4(1, 9, 9, 9)))
                    {
                        ARGHelper.CreateErrorMessage("Beginning ritual.", mainMenuCanvas.transform);
                    }
                }
            });
        }

        /// <summary>
        /// Removes the last digit of the fields.
        /// </summary>
        public static void RemoveLastDigit()
        {
            if (inputFields.Count >= 4)
            {
                List<TMP_InputField> inputKey = new List<TMP_InputField>
                {
                    inputFields[0].GetComponent<TMP_InputField>(),
                    inputFields[1].GetComponent<TMP_InputField>(),
                    inputFields[2].GetComponent<TMP_InputField>(),
                    inputFields[3].GetComponent<TMP_InputField>()
                };

                int wantedIndex = -1;
                for (int i = 0; i < inputKey.Count; i++)
                {
                    if (!string.IsNullOrEmpty(inputKey[i].text)
                        && !inputKey[i].text.Equals("-"))
                    {
                        wantedIndex = i;
                    }
                }

                if (wantedIndex <= -1)
                {
                    return;
                }

                if (wantedIndex >= 3)
                {
                    wantedIndex = 3;
                }

                inputKey[wantedIndex].text = "-";

                PlayKeyboardSound();

                LoggingHelper.DebugLog($"Removed digit at position '{wantedIndex}'.");
            }
        }

        /// <summary>
        /// Adds a digit to the input.
        /// </summary>
        public static void AddDigit(int digit)
        {
            if (inputFields.Count >= 4)
            {
                List<TMP_InputField> inputKey = new List<TMP_InputField>
                {
                    inputFields[0].GetComponent<TMP_InputField>(),
                    inputFields[1].GetComponent<TMP_InputField>(),
                    inputFields[2].GetComponent<TMP_InputField>(),
                    inputFields[3].GetComponent<TMP_InputField>()
                };

                int wantedIndex = -1;
                for (int i = 0; i < inputKey.Count; i++)
                {
                    if (string.IsNullOrEmpty(inputKey[i].text)
                        || inputKey[i].text.Equals("-"))
                    {
                        wantedIndex = i;
                        break;
                    }
                }

                if (wantedIndex <= -1)
                {
                    return;
                }

                inputKey[wantedIndex].text = $"{digit}";

                PlayKeyboardSound();

                LoggingHelper.DebugLog($"Added digit '{digit}' at position '{wantedIndex}'.");
            }
        }

        /// <summary>
        /// Plays a random keyboard sound.
        /// </summary>
        private static void PlayKeyboardSound()
        {
            keypadAudioSource.PlayOneShot(EmbedLoader.KeyboardSounds[Random.Range(0, EmbedLoader.KeyboardSounds.Count)]
                .clip);
        }
    }
}