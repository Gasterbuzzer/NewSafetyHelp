using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.InGameSettings
{
    public static class InGameSettingHelper
    {
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

        private static GameObject GetContentSettings()
        {
            GameObject contentSettings = GameObject.Find("MainMenuCanvas").transform.Find("OptionsPopup").transform.Find("OptionsScrollRect").transform.Find("Viewport").transform.Find("Content").gameObject;

            if (contentSettings != null)
            {
                return contentSettings;
            }
            
            return null;
        }
        
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