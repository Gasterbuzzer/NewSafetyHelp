using System.Collections;
using NewSafetyHelp.CustomCampaignSystem.CustomTextFiles;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomDesktop;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NewSafetyHelp.CustomCampaignSystem.LinkApps
{
    public static class LinkAppHelper
    {
        /// <summary>
        /// Small MonoBehaviour class to make sure that when opening the link app, that it will open the link.
        /// </summary>
        public class LinkAppOpener : MonoBehaviour, IPointerClickHandler
        {
            public void PlayExecutableFunction()
            {
                StartCoroutine(ExecutableRoutine());
            }

            private IEnumerator ExecutableRoutine()
            {
                yield return new WaitForSeconds(0.5f);
                OpenLinkApp();
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.clickCount == 2 || SystemInfo.deviceType == DeviceType.Handheld)
                {
                    PlayExecutableFunction();
                }
            }

            public void OpenLinkApp()
            {
                LinkApp linkApp = CustomCampaignGlobal.GetLinkAppFromActiveCampaign(gameObject);

                if (linkApp != null)
                {
                    URLVerification.OpenURIInBrowser(linkApp.LinkAppClickURL);
                }
                else
                {
                    LoggingHelper.ErrorLog(
                        "Was unable of opening link app. Could not find relevant link app GameObject.");
                }
            }
        }

        /// <summary>
        /// Creates a custom link app program on the desktop.
        /// </summary>
        /// <param name="customLinkApp">Custom Link App to create.</param>
        /// <returns>(GameObject) Link App that was created.</returns>
        public static GameObject CreateCustomLinkApp(LinkApp customLinkApp)
        {
            GameObject desktopCreditsProgram =
                CustomDesktopHelper.GetLeftPrograms().transform.Find("Readme").gameObject;

            GameObject newCustomLinkApp =
                Object.Instantiate(desktopCreditsProgram, desktopCreditsProgram.transform.parent);

            if (!customLinkApp.DesktopName.HasChanged)
            {
                LoggingHelper.WarningLog("No desktop name provided for the custom link app, using default name.");
            }

            newCustomLinkApp.name = customLinkApp.DesktopName.Data + customLinkApp.UnlockDay;

            // Make sure the icon is correct
            if (customLinkApp.DesktopIcon.HasChanged)
            {
                newCustomLinkApp.GetComponent<Image>().sprite = customLinkApp.DesktopIcon.Data;
            }
            else
            {
                newCustomLinkApp.GetComponent<Image>().sprite = CustomTextFileHelper.TextFileIcon;
            }

            if (customLinkApp.GameObjectOrder.HasChanged)
            {
                newCustomLinkApp.transform.SetSiblingIndex(customLinkApp.GameObjectOrder.Data);
            }

            // Update desktop name
            TextMeshProUGUI textChildGameObjectText = newCustomLinkApp.transform.Find("TextBackground").transform
                .Find("ExecutableName").gameObject.GetComponent<TextMeshProUGUI>();

            textChildGameObjectText.text = customLinkApp.DesktopName.Data;

            // Add text content
            TextFileExecutable textFileExecutable = newCustomLinkApp.GetComponent<TextFileExecutable>();
            textFileExecutable.enabled = false;

            // We check if the link app should unlock.

            newCustomLinkApp.SetActive(true);

            if (GlobalVariables.currentDay < customLinkApp.UnlockDay)
            {
                newCustomLinkApp.SetActive(false);
            }

            if (!AccuracyLinkAppHelper.CheckIfLinkAppPassAccuracyChecks(customLinkApp))
            {
                newCustomLinkApp.SetActive(false);
            }

            newCustomLinkApp.AddComponent<LinkAppOpener>();

            customLinkApp.GameObjectReference = newCustomLinkApp;

            return newCustomLinkApp;
        }
    }
}