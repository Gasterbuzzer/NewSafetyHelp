using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.Saving;
using NewSafetyHelp.CustomDesktop.CustomDoubleClickButton;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomDesktop
{
    public static class CustomDesktopHelper
    {
        /// <summary>
        /// Finds the main menu canvas.
        /// </summary>
        /// <returns>GameObject for the main menu canvas. </returns>
        private static GameObject getMainMenuCanvas()
        {
            GameObject foundMainMenuCanvas = GameObject.Find("MainMenuCanvas");

            if (foundMainMenuCanvas != null)
            {
                return foundMainMenuCanvas;
            }
            else
            {
                MelonLogger.Error("ERROR: Failed to find MainMenuCanvas. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the desktop GameObject from the main menu canvas.
        /// </summary>
        /// <returns>Desktop GameObject</returns>
        private static GameObject getDesktop()
        {
            GameObject foundDesktop = getMainMenuCanvas().transform.Find("Desktop").gameObject;

            if (foundDesktop != null)
            {
                return foundDesktop;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find Desktop from Main Menu. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the desktop logo GameObject.
        /// </summary>
        /// <returns>Logo GameObject</returns>
        public static GameObject getLogo()
        {
            GameObject foundLogo = getDesktop().transform.Find("Logo").gameObject;

            if (foundLogo != null)
            {
                return foundLogo;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find Logo from Desktop. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the desktop GameObject which contains all emails.
        /// </summary>
        /// <returns>Email List GameObject</returns>
        private static GameObject getEmailList()
        {
            GameObject foundEmailList = getMainMenuCanvas().transform.Find("EmailPopup").Find("EmailsScrollview")
                .Find("Viewport").Find("Content").gameObject;

            if (foundEmailList != null)
            {
                return foundEmailList;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find email list from Main Menu Canvas. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Removes all emails from the main campaign.
        /// </summary>
        public static void removeMainGameEmails()
        {
            GameObject foundEmailList = getEmailList();

            if (foundEmailList != null)
            {
                foreach (Transform childEmail in foundEmailList.transform)
                {
                    if (childEmail.gameObject.name.Contains("EmailListing"))
                    {
                        Object.Destroy(childEmail.gameObject);
                    }
                }
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find email list from Main Menu Canvas. Possibly called outside of MainMenuCanvas?");
            }
        }

        /// <summary>
        /// Creates an email and returns a reference.
        /// </summary>
        /// <returns>New Email reference.</returns>
        public static GameObject createEmail(EmailExtraInfo emailToCreate)
        {
            GameObject originalEmail = getEmailList().transform.Find("EmailListing (14)").gameObject;

            if (originalEmail != null)
            {
                GameObject newEmail = Object.Instantiate(originalEmail, originalEmail.transform.parent);

                EmailListingBehavior emailListing = newEmail.GetComponent<EmailListingBehavior>();

                // We create a new email part of the email listing so that the reference doesn't point to the same one.
                Email clonedEmail = ScriptableObject.CreateInstance<Email>();

                emailListing.myEmail = clonedEmail;

                // Get subject and sender text references correct.
                emailListing.mySubjectText = newEmail.transform.Find("SubjectText").GetComponent<TextMeshProUGUI>();
                emailListing.mySenderText = newEmail.transform.Find("FromText").GetComponent<TextMeshProUGUI>();

                if (emailListing == null)
                {
                    MelonLogger.Error("ERROR: Failed to find email listing behavior for EmailListing");
                    Object.Destroy(originalEmail);
                    return null;
                }

                if (emailToCreate.emailSubject != "")
                {
                    newEmail.name = emailToCreate.emailSubject.Replace("EmailListing", "");

                    emailListing.myEmail.name = emailToCreate.emailSubject.Replace("EmailListing", "");

                    emailListing.mySubjectText.text = emailToCreate.emailSubject;

                    // Email Subject
                    emailListing.myEmail.subjectLine = emailToCreate.emailSubject;
                }
                else
                {
                    newEmail.name = "UnnamedEmail";

                    emailListing.myEmail.name = "UnnamedEmail";

                    emailListing.mySubjectText.text = "UnnamedEmail";

                    // Email Subject
                    emailListing.myEmail.subjectLine = "UnnamedEmail";
                }

                if (emailToCreate.senderName != "")
                {
                    emailListing.mySenderText.text = emailToCreate.senderName;

                    // Email Sender
                    emailListing.myEmail.sender = emailToCreate.senderName;
                }
                else
                {
                    emailListing.mySenderText.text = "SenderNameNotProvided";

                    // Email Sender
                    emailListing.myEmail.sender = "SenderNameNotProvided";
                }

                // If empty, it will just not be shown.
                emailListing.myEmail.emailBody = emailToCreate.emailBody;

                emailListing.myEmail.imageAttachment = emailToCreate.emailImage;

                // DayUnlock

                OnDayUnlock newEmailOnDayUnlock = newEmail.GetComponent<OnDayUnlock>();

                newEmailOnDayUnlock.unlockDay = emailToCreate.unlockDay;
                newEmailOnDayUnlock.scoreThresholdToUnlock = emailToCreate.unlockThreshold;

                // Mark the email as not read.

                FieldInfo _hasClicked = typeof(EmailListingBehavior).GetField("hasClicked",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                if (_hasClicked == null)
                {
                    MelonLogger.Warning("WARNING: HasClicked Field could not be found (null).");
                }
                else
                {
                    _hasClicked.SetValue(emailListing, false); // emailListing.hasClicked = false;
                }

                return newEmail;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find email to copy from in the Email List. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the windows bar GameObject from the desktop.
        /// </summary>
        /// <returns>Window Bar GameObject</returns>
        private static GameObject getWindowsBar()
        {
            GameObject foundWindowBar = getDesktop().transform.Find("WindowsBar (1)").gameObject;

            if (foundWindowBar != null)
            {
                return foundWindowBar;
            }
            else // Try again with a different name.
            {
                foundWindowBar = getDesktop().transform.Find("WindowsBar").gameObject;

                if (foundWindowBar != null)
                {
                    MelonLogger.Error(
                        "ERROR: Failed to find windows bar from Main Menu. Possibly called outside of MainMenuCanvas?");
                    return null;
                }

                return foundWindowBar;
            }
        }

        /// <summary>
        /// Gets the credits GameObject from the programs list.
        /// </summary>
        /// <returns>Credits GameObject</returns>
        public static GameObject getCreditsGameObject()
        {
            GameObject foundCredits = getLeftPrograms().transform.Find("Readme").gameObject;

            if (foundCredits != null)
            {
                return foundCredits;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find credits from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the username GameObject from the windows bar.
        /// </summary>
        /// <returns>Username GameObject</returns>
        public static GameObject getUsernameObject()
        {
            GameObject foundUsername = getWindowsBar().transform.Find("Username").gameObject;

            if (foundUsername != null)
            {
                return foundUsername;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find username from window bar. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the skip call wait time button.
        /// </summary>
        /// <returns>Next Call Button GameObject</returns>
        public static GameObject getCallSkipButton()
        {
            GameObject foundNextCallButton = GameObject.Find("MainCanvas").transform.Find("Panel").transform
                .Find("CallWindow").transform.Find("LargeCallerPortrait").transform.Find("CallSkipButton").gameObject;

            if (foundNextCallButton != null)
            {
                return foundNextCallButton;
            }
            else
            {
                MelonLogger.Error("ERROR: Failed to find next caller button. Possibly called outside of MainCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the Program icons on the left side on the Desktop.
        /// </summary>
        /// <returns> GameObject for the programs on the left. </returns>
        private static GameObject getLeftPrograms()
        {
            GameObject foundLeftSidePrograms = getDesktop().transform.Find("Programs").gameObject;

            if (foundLeftSidePrograms != null)
            {
                return foundLeftSidePrograms;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find left sided Programs from Desktop. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the Program icons on the right side on the Desktop.
        /// </summary>
        /// <returns> GameObject for the programs on the right. </returns>
        private static GameObject getRightPrograms()
        {
            GameObject foundRightSidePrograms = getDesktop().transform.Find("RightHandPrograms").gameObject;

            if (foundRightSidePrograms != null)
            {
                return foundRightSidePrograms;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find right sided Programs from Desktop. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the Winter DLC program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the winter dlc program on the left. </returns>
        private static GameObject getWinterDLCProgram()
        {
            GameObject winterDLCProgram = getLeftPrograms().transform.Find("DLC-Executable").gameObject;

            if (winterDLCProgram != null)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found the DLC Icon.");
                #endif

                return winterDLCProgram;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find the winter DLC program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Disables all default video programs on the desktop.
        /// </summary>
        public static void disableDefaultVideos()
        {
            getLeftPrograms().transform.Find("TrailerFile").gameObject.SetActive(false);
            getLeftPrograms().transform.Find("RealEstateVideo").gameObject.SetActive(false);
            getLeftPrograms().transform.Find("ScienceVideo").gameObject.SetActive(false);
            getLeftPrograms().transform.Find("HikingVideo").gameObject.SetActive(false);
        }

        /// <summary>
        /// Disables all default video programs on the desktop.
        /// </summary>
        public static GameObject createCustomVideoFileProgram(CustomVideoExtraInfo customVideo)
        {
            GameObject trailerFileOriginal = getLeftPrograms().transform.Find("TrailerFile").gameObject;

            GameObject newCustomVideo = Object.Instantiate(trailerFileOriginal, trailerFileOriginal.transform.parent);

            if (string.IsNullOrEmpty(customVideo.desktopName))
            {
                MelonLogger.Error(
                    "ERROR: No filename provided for video to be created! Can lead to crashes or unwanted failures.");
            }

            newCustomVideo.name = customVideo.desktopName + customVideo.videoURL;

            // Update desktop name
            TextMeshProUGUI textChildGameObjectText = newCustomVideo.transform.Find("TextBackground").transform
                .Find("ExecutableName").gameObject.GetComponent<TextMeshProUGUI>();

            textChildGameObjectText.text = customVideo.desktopName;

            // Unlock Day
            OnDayUnlock onDayUnlock = newCustomVideo.GetComponent<OnDayUnlock>();
            onDayUnlock.unlockDay = customVideo.unlockDay;

            if (customVideo.unlockDay <= GlobalVariables.currentDay)
            {
                newCustomVideo.SetActive(true);
            }

            // Fix References

            VideoExecutableFile videoExecutableFile = newCustomVideo.GetComponent<VideoExecutableFile>();

            videoExecutableFile.videoClip = null;

            // Update on day unlock script to point at the correct onDayUnlock.
            FieldInfo _onDayUnlock = typeof(VideoExecutableFile).GetField("dayUnlockScript",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (_onDayUnlock == null)
            {
                MelonLogger.Error("ERROR: Could not find OnDayUnlock script for VideoExecutableFile!");
                return null;
            }

            _onDayUnlock.SetValue(videoExecutableFile, onDayUnlock);

            return newCustomVideo;
        }


        /// <summary>
        /// Disables the Winter DLC Program to avoid switching to DLC while custom campaign is active.
        /// </summary>
        public static void disableWinterDLCProgram()
        {
            getWinterDLCProgram().SetActive(false);
        }

        /// <summary>
        /// Reenable the Winter DLC Program to avoid switching to DLC while custom campaign is active.
        /// </summary>
        public static void enableWinterDLCProgram()
        {
            if (!SteamManager.Initialized ||
                !SteamApps.BIsDlcInstalled(new AppId_t(2914730U))) // No DLC, we don't activate.
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: DLC is not installed. Not activating DLC.");
                #endif

                // To make sure the DLC isn't enabled by accident.
                disableWinterDLCProgram();

                return;
            }

            #if DEBUG
                MelonLogger.Msg($"DEBUG: DLC is installed. Activating DLC!");
            #endif

            getWinterDLCProgram().SetActive(true);
        }

        /// <summary>
        /// Gets the GameObject for the main game program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the main game program on the left. </returns>
        public static GameObject getMainGameProgram()
        {
            GameObject mainGameProgram = getLeftPrograms().transform.Find("HSH-Executable").gameObject;

            if (mainGameProgram != null)
            {
                return mainGameProgram;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find the main game program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the NSE Discord program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the NSE Discord program on the right. </returns>
        private static GameObject getNSEDiscordProgram()
        {
            GameObject discordProgram = getRightPrograms().transform.Find("Discord-Executable").gameObject;

            if (discordProgram != null)
            {
                return discordProgram;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find the discord program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }


        public static void createCustomProgramIcon(string customProgramName, string customCampaignName,
            Sprite customIcon = null)
        {
            GameObject customProgramIcon = Object.Instantiate(getWinterDLCProgram(), getLeftPrograms().transform);

            Object.Destroy(customProgramIcon.GetComponent<HSHExecutableBehavior>()); // Remove old Executable Behavior.

            // Change Program Name
            TextMeshProUGUI programName = customProgramIcon.transform.Find("TextBackground").Find("ExecutableName")
                .GetComponent<TextMeshProUGUI>();
            programName.text = customProgramName;

            // Change Program Icon if provided.
            Image customProgramImage = customProgramIcon.GetComponent<Image>();
            if (customIcon != null)
            {
                customProgramImage.sprite = customIcon;
            }

            // Reset Color
            customProgramImage.color = Color.white;

            // Button Changes.
            Button customProgramButton = customProgramIcon.GetComponent<Button>();

            DoubleClickButton doubleClickButton = customProgramIcon.AddComponent<DoubleClickButton>();

            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            customProgramButton.onClick.AddListener(() =>
                doubleClickButton.doubleClickCustomCampaign(customCampaignName));

            // Rename CustomProgramIcon
            customProgramIcon.name = customProgramName;

            // Enable if disabled.
            customProgramIcon.SetActive(true);
        }

        public static void createBackToMainGameButton()
        {
            GameObject backToMainMenuGameButton =
                Object.Instantiate(getNSEDiscordProgram(), getRightPrograms().transform);

            Object.Destroy(backToMainMenuGameButton.GetComponent<LinkExecutable>()); // Remove old executable Behavior.

            // Change Program Name
            backToMainMenuGameButton.transform.Find("TextBackground").Find("ExecutableName")
                .GetComponent<TextMeshProUGUI>().text = "Back to Main Game.";

            // Change Program Icon 
            backToMainMenuGameButton.GetComponent<Image>().sprite = getMainGameProgram().GetComponent<Image>().sprite;
            // Reset Color
            backToMainMenuGameButton.GetComponent<Image>().color = Color.white;

            // Button Changes.
            Button customProgramButton = backToMainMenuGameButton.GetComponent<Button>();

            DoubleClickButton doubleClickButton = backToMainMenuGameButton.AddComponent<DoubleClickButton>();

            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            customProgramButton.onClick.AddListener(() => doubleClickButton.doubleClickBackToMainGameAlsoLoad());

            // Rename Program Object Name
            backToMainMenuGameButton.name = "BackToMainGameButton";
        }

        public static void changeToCustomCampaignSettings(string customCampaignName)
        {
            MelonLogger.Msg(ConsoleColor.Green, $"INFO: Changing to custom campaign: {customCampaignName}.");

            // Activate the Custom Campaign
            CustomCampaignGlobal.ActivateCustomCampaign(customCampaignName);

            // Load Custom Campaign values
            CustomCampaignSaving.loadFromFileCustomCampaignInfo();

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            SceneManager.LoadScene("MainMenuScene");
        }

        public static void backToMainGame(bool alsoLoadMainMenu = true)
        {
            MelonLogger.Msg(ConsoleColor.Green, "INFO: Going back to the main game.");

            // Save values
            CustomCampaignSaving.saveCustomCampaignInfo();

            // Reset back.
            CustomCampaignGlobal.DeactivateCustomCampaign();

            // Load old values.
            GlobalVariables.saveManagerScript.Load();

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            if (alsoLoadMainMenu)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        public static void disableThemeDropdownDesktop()
        {
            GameObject videoOptions = GameObject.Find("MainMenuCanvas").transform.Find("OptionsPopup").transform
                .Find("OptionsScrollRect").transform.Find("Viewport").transform.Find("Content").transform
                .Find("VideoOptions").gameObject;

            if (videoOptions == null)
            {
                return;
            }

            GameObject colorPaletteHeader = videoOptions.transform.Find("ColorPaletteHeader").gameObject;
            GameObject colorDropdown = videoOptions.transform.Find("ColorDropdown").gameObject;

            if (colorPaletteHeader != null)
            {
                colorPaletteHeader.SetActive(false);
            }
            
            if (colorDropdown != null)
            {
                colorDropdown.SetActive(false);
            }
        }
        
        public static void disableThemeDropdownInGame()
        {
            GameObject videoOptions = GameObject.Find("MainCanvas").transform.Find("OptionsPopup").transform
                .Find("OptionsScrollRect").transform.Find("Viewport").transform.Find("Content").transform
                .Find("VideoOptions").gameObject;

            if (videoOptions == null)
            {
                return;
            }

            GameObject colorPaletteHeader = videoOptions.transform.Find("ColorPaletteHeader").gameObject;
            GameObject colorDropdown = videoOptions.transform.Find("ColorDropdown").gameObject;

            if (colorPaletteHeader != null)
            {
                colorPaletteHeader.SetActive(false);
            }
            
            if (colorDropdown != null)
            {
                colorDropdown.SetActive(false);
            }
        }
    }
}