using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.InGameSettings
{
    public static class InGameSettingHelper
    {
        /// <summary>
        /// Gets the video options section in the settings.
        /// </summary>
        /// <returns>Null if we failed to find. The video options section if found.</returns>
        public static GameObject GetVideoOptionsSection()
        {
            GameObject contentSection = GetContentSettings();

            if (contentSection != null)
            {
                GameObject videoOptionsSection = contentSection.transform.Find("VideoOptions").gameObject;

                return videoOptionsSection;
            }

            return null;
        }

        /// <summary>
        /// Gets the phobia toggles section. It contains multiple toggles.
        /// </summary>
        /// <returns>GameObject reference to the phobias section. NULL if not found.</returns>
        private static GameObject GetPhobiasTogglesSection()
        {
            GameObject contentSection = GetContentSettings();

            if (contentSection != null)
            {
                GameObject phobiaTogglesSection = contentSection.transform.Find("PhobiaToggles").gameObject;

                if (phobiaTogglesSection != null)
                {
                    return phobiaTogglesSection;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the options popup window.
        /// </summary>
        /// <returns>GameObject of the options popup window.</returns>
        private static GameObject GetOptionsPopup()
        {
            GameObject contentSettings = GameObject.Find("MainMenuCanvas").transform.Find("OptionsPopup").gameObject;

            return contentSettings;
        }

        /// <summary>
        /// Gets the close button of the options window.
        /// </summary>
        /// <returns>GameObject of the close button window (Options Popup Window).</returns>
        private static GameObject GetCloseButton()
        {
            GameObject closeButton = GetOptionsPopup().transform.Find("WindowsBar").Find("CloseButton").gameObject;

            return closeButton;
        }

        /// <summary>
        /// Gets the content section of the options window.
        /// </summary>
        /// <returns>GameObject of the content section of the window (Options Popup Window).</returns>
        private static GameObject GetContentSettings()
        {
            GameObject contentSettings = GetOptionsPopup().transform.Find("OptionsScrollRect").Find("Viewport")
                .Find("Content").gameObject;

            return contentSettings;
        }

        /// <summary>
        /// Creates a new toggle in a given options section.
        /// </summary>
        /// <param name="parentGameObject">Section to add the toggle to.</param>
        /// <param name="eventOnLabelChange">Event to be called when the toggle value changes.
        /// (Accepts a boolean (toggle value) and returns one (new value))</param>
        /// <param name="newToggleName">Label (Description) of the toggle UI (Also in hierarchy view).</param>
        /// <param name="startingState">Starting state of the toggle.</param>
        /// <returns>GameObject representing the new toggle button.</returns>
        public static GameObject CreateNewToggle(GameObject parentGameObject,
            Func<bool, bool> eventOnLabelChange,
            string newToggleName = "New Toggle Name", bool startingState = false)
        {
            GameObject phobiaSection = GetPhobiasTogglesSection();

            if (phobiaSection != null)
            {
                GameObject insectToggle = phobiaSection.transform.Find("SpiderToggle").gameObject;

                GameObject newToggle = Object.Instantiate(insectToggle, parentGameObject.transform);

                newToggle.name = newToggleName;

                newToggle.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = newToggleName;

                Toggle toggle = newToggle.GetComponent<Toggle>();

                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged = new Toggle.ToggleEvent(); // completely new empty event

                toggle.isOn = startingState;

                toggle.onValueChanged.AddListener(inputBool => eventOnLabelChange(inputBool));

                return newToggle;
            }

            return null;
        }

        /// <summary>
        /// Creates a new button in a given options section.
        /// </summary>
        /// <param name="parentGameObject">Section to add the button to.</param>
        /// <param name="eventOnClick">Event that is triggered when the button is pressed.
        /// (Accepts the GameObject button that was clicked. Returns the same GameObject button).</param>
        /// <param name="newButtonName">Name of the button. (In Hierarchy view)</param>
        /// <param name="newButtonDescription">Description on the button.</param>
        /// <returns>GameObject representing the newly created button.</returns>
        public static GameObject CreateButton(GameObject parentGameObject, Func<GameObject, GameObject> eventOnClick,
            string newButtonName = "New Toggle Name", string newButtonDescription = "New Toggle Description")
        {
            GameObject closeButton = GetCloseButton();

            if (closeButton != null)
            {
                GameObject newButton = Object.Instantiate(closeButton, parentGameObject.transform);

                newButton.name = newButtonName;

                newButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = newButtonDescription;

                Button newButtonComponent = newButton.GetComponent<Button>();

                newButtonComponent.onClick.RemoveAllListeners();
                newButtonComponent.onClick = new Button.ButtonClickedEvent(); // Clear all previous events.

                newButtonComponent.onClick.AddListener(() => eventOnClick(newButton));

                return newButton;
            }

            return null;
        }

        /// <summary>
        /// Creates a new setting section in the options popup window.
        /// </summary>
        /// <param name="newSectionName">Name/Label of the new settings section.</param>
        /// <param name="newSectionDescription">Description of the new settings section.</param>
        /// <returns>GameObject that represents the new settings section.</returns>
        public static GameObject CreateNewSettingsSection(string newSectionName = "New Settings Section",
            string newSectionDescription = "New Settings Section Description")
        {
            GameObject phobiaSection = GetPhobiasTogglesSection();

            if (phobiaSection != null)
            {
                GameObject newSettingsSection = Object.Instantiate(phobiaSection, phobiaSection.transform.parent);

                newSettingsSection.name = newSectionName;

                newSettingsSection.transform.Find("Banner (2)").gameObject.SetActive(false);

                // New Header
                GameObject header = newSettingsSection.transform.Find("Header").gameObject;
                header.SetActive(true);
                header.GetComponent<TextMeshProUGUI>().text = newSectionName;

                // Changing Description
                GameObject description = newSettingsSection.transform.Find("Description").gameObject;
                description.GetComponent<TextMeshProUGUI>().text = newSectionDescription;

                // Removing old toggles.
                newSettingsSection.transform.Find("SpiderToggle").gameObject.SetActive(false);
                newSettingsSection.transform.Find("InsectToggle").gameObject.SetActive(false);
                newSettingsSection.transform.Find("DarkToggle").gameObject.SetActive(false);
                newSettingsSection.transform.Find("HoleToggle").gameObject.SetActive(false);
                newSettingsSection.transform.Find("WatchedToggle").gameObject.SetActive(false);
                newSettingsSection.transform.Find("TightSpaceToggle").gameObject.SetActive(false);
                newSettingsSection.transform.Find("DogToggle").gameObject.SetActive(false);

                return newSettingsSection;
            }

            return null;
        }
    }
}