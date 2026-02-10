using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.Saving;
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
        public static GameObject GetMainMenuCanvas()
        {
            GameObject foundGameObject = GameObject.Find("MainMenuCanvas");

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        private static GameObject GetDesktop()
        {
            GameObject foundGameObject = GetMainMenuCanvas().transform.Find("Desktop").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        public static GameObject GetLogo()
        {
            GameObject foundGameObject = GetDesktop().transform.Find("Logo").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        private static GameObject GetEmailList()
        {
            GameObject foundGameObject = GetMainMenuCanvas().transform.Find("EmailPopup").Find("EmailsScrollview")
                .Find("Viewport").Find("Content").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        public static void RemoveMainGameEmails()
        {
            GameObject foundGameObject = GetEmailList();

            if (foundGameObject != null)
            {
                foreach (Transform childEmail in foundGameObject.transform)
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
        public static GameObject CreateEmail(CustomEmail emailToCreate)
        {
            GameObject originalEmail = GetEmailList().transform.Find("EmailListing (14)").gameObject;

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

                if (emailToCreate.EmailSubject != "")
                {
                    newEmail.name = emailToCreate.EmailSubject.Replace("EmailListing", "");

                    emailListing.myEmail.name = emailToCreate.EmailSubject.Replace("EmailListing", "");

                    emailListing.mySubjectText.text = emailToCreate.EmailSubject;

                    // Email Subject
                    emailListing.myEmail.subjectLine = emailToCreate.EmailSubject;
                }
                else
                {
                    newEmail.name = "UnnamedEmail";

                    emailListing.myEmail.name = "UnnamedEmail";

                    emailListing.mySubjectText.text = "UnnamedEmail";

                    // Email Subject
                    emailListing.myEmail.subjectLine = "UnnamedEmail";
                }

                if (emailToCreate.SenderName != "")
                {
                    emailListing.mySenderText.text = emailToCreate.SenderName;

                    // Email Sender
                    emailListing.myEmail.sender = emailToCreate.SenderName;
                }
                else
                {
                    emailListing.mySenderText.text = "SenderNameNotProvided";

                    // Email Sender
                    emailListing.myEmail.sender = "SenderNameNotProvided";
                }

                // If empty, it will just not be shown.
                emailListing.myEmail.emailBody = emailToCreate.EmailBody;

                emailListing.myEmail.imageAttachment = emailToCreate.EmailImage;

                // DayUnlock

                OnDayUnlock newEmailOnDayUnlock = newEmail.GetComponent<OnDayUnlock>();

                newEmailOnDayUnlock.unlockDay = emailToCreate.UnlockDay;
                newEmailOnDayUnlock.scoreThresholdToUnlock = emailToCreate.UnlockThreshold;

                // Mark the email as not read.

                FieldInfo hasClicked = typeof(EmailListingBehavior).GetField("hasClicked",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                if (hasClicked == null)
                {
                    MelonLogger.Warning("WARNING: HasClicked Field could not be found (null).");
                }
                else
                {
                    hasClicked.SetValue(emailListing, false); // emailListing.hasClicked = false;
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
        private static GameObject GetWindowsBar()
        {
            GameObject foundGameObject = GetDesktop().transform.Find("WindowsBar (1)").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else // Try again with a different name.
            {
                foundGameObject = GetDesktop().transform.Find("WindowsBar").gameObject;

                if (foundGameObject != null)
                {
                    MelonLogger.Error(
                        "ERROR: Failed to find windows bar from Main Menu. Possibly called outside of MainMenuCanvas?");
                    return null;
                }

                return foundGameObject;
            }
        }

        /// <summary>
        /// Gets the credits GameObject from the programs list.
        /// </summary>
        /// <returns>Credits GameObject</returns>
        public static GameObject GetCreditsGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("Readme").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find credits from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Mailbox GameObject from the programs list.
        /// </summary>
        /// <returns>Mailbox GameObject</returns>
        public static GameObject GetMailboxGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("Email-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find mailbox from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Options GameObject from the programs list.
        /// </summary>
        /// <returns>Options GameObject</returns>
        public static GameObject GetOptionsGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("Options-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find Options from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the EntryBrowser GameObject from the programs list.
        /// </summary>
        /// <returns>EntryBrowser GameObject</returns>
        public static GameObject GetEntryBrowserGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("EntryBrowser-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find EntryBrowser from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Artbook GameObject from the programs list.
        /// </summary>
        /// <returns>Artbook GameObject</returns>
        public static GameObject GetArtbookGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("Artbook-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find Artbook from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Arcade GameObject from the programs list.
        /// </summary>
        /// <returns>Arcade GameObject</returns>
        public static GameObject GetArcadeGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("Arcade-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find Arcade from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Scorecard GameObject from the programs list.
        /// </summary>
        /// <returns>Scorecard GameObject</returns>
        public static GameObject GetScorecardGameObject()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("Scorecard").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find Scorecard from the program list. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the username GameObject from the windows bar.
        /// </summary>
        /// <returns>Username GameObject</returns>
        public static GameObject GetUsernameObject()
        {
            GameObject foundUsername = GetWindowsBar().transform.Find("Username").gameObject;

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
        public static GameObject GetCallSkipButton()
        {
            GameObject foundGameObject = GameObject.Find("MainCanvas").transform.Find("Panel").transform
                .Find("CallWindow").transform.Find("LargeCallerPortrait").transform.Find("CallSkipButton").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        private static GameObject GetLeftPrograms()
        {
            GameObject foundGameObject = GetDesktop().transform.Find("Programs").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        private static GameObject GetRightPrograms()
        {
            GameObject foundGameObject = GetDesktop().transform.Find("RightHandPrograms").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        private static GameObject GetWinterDlcProgram()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("DLC-Executable").gameObject;

            if (foundGameObject != null)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found the DLC Icon.");
                #endif

                return foundGameObject;
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
        public static void DisableDefaultVideos()
        {
            GetLeftPrograms().transform.Find("TrailerFile").gameObject.SetActive(false);
            GetLeftPrograms().transform.Find("RealEstateVideo").gameObject.SetActive(false);
            GetLeftPrograms().transform.Find("ScienceVideo").gameObject.SetActive(false);
            GetLeftPrograms().transform.Find("HikingVideo").gameObject.SetActive(false);
        }

        /// <summary>
        /// Disables all default video programs on the desktop.
        /// </summary>
        public static GameObject CreateCustomVideoFileProgram(CustomVideo customVideo)
        {
            GameObject trailerFileOriginal = GetLeftPrograms().transform.Find("TrailerFile").gameObject;

            GameObject newCustomVideo = Object.Instantiate(trailerFileOriginal, trailerFileOriginal.transform.parent);

            if (string.IsNullOrEmpty(customVideo.DesktopName))
            {
                MelonLogger.Error(
                    "ERROR: No filename provided for video to be created! Can lead to crashes or unwanted failures.");
            }

            newCustomVideo.name = customVideo.DesktopName + customVideo.VideoURL;

            // Update desktop name
            TextMeshProUGUI textChildGameObjectText = newCustomVideo.transform.Find("TextBackground").transform
                .Find("ExecutableName").gameObject.GetComponent<TextMeshProUGUI>();

            textChildGameObjectText.text = customVideo.DesktopName;

            // Unlock Day
            OnDayUnlock onDayUnlock = newCustomVideo.GetComponent<OnDayUnlock>();
            onDayUnlock.unlockDay = customVideo.UnlockDay;

            if (customVideo.UnlockDay <= GlobalVariables.currentDay)
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
        public static void DisableWinterDlcProgram()
        {
            GetWinterDlcProgram().SetActive(false);
        }

        /// <summary>
        /// Reenable the Winter DLC Program to avoid switching to DLC while custom campaign is active.
        /// </summary>
        public static void EnableWinterDlcProgram()
        {
            if (!SteamManager.Initialized ||
                !SteamApps.BIsDlcInstalled(new AppId_t(2914730U))) // No DLC, we don't activate.
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: DLC is not installed. Not activating DLC.");
                #endif

                // To make sure the DLC isn't enabled by accident.
                DisableWinterDlcProgram();

                return;
            }

            #if DEBUG
                MelonLogger.Msg($"DEBUG: DLC is installed. Activating DLC!");
            #endif

            GetWinterDlcProgram().SetActive(true);
        }

        /// <summary>
        /// Gets the GameObject for the main game program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the main game program on the left. </returns>
        public static GameObject GetMainGameProgram()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("HSH-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
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
        public static GameObject GetNSEDiscordProgram()
        {
            GameObject foundGameObject = GetRightPrograms().transform.Find("Discord-Executable").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Failed to find the discord program. Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }


        public static void CreateCustomProgramIcon(string customProgramName, string customCampaignName,
            Sprite customIcon = null)
        {
            GameObject customProgramIcon = Object.Instantiate(GetWinterDlcProgram(), GetLeftPrograms().transform);

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
                doubleClickButton.DoubleClickCustomCampaign(customCampaignName));

            // Rename CustomProgramIcon
            customProgramIcon.name = customProgramName;

            // Enable if disabled.
            customProgramIcon.SetActive(true);
        }

        public static void CreateBackToMainGameButton()
        {
            GameObject backToMainMenuGameButton =
                Object.Instantiate(GetNSEDiscordProgram(), GetRightPrograms().transform);

            Object.Destroy(backToMainMenuGameButton.GetComponent<LinkExecutable>()); // Remove old executable Behavior.

            // Change Program Name
            backToMainMenuGameButton.transform.Find("TextBackground").Find("ExecutableName")
                .GetComponent<TextMeshProUGUI>().text = "Back to Main Game.";

            // Change Program Icon 
            backToMainMenuGameButton.GetComponent<Image>().sprite = GetMainGameProgram().GetComponent<Image>().sprite;
            // Reset Color
            backToMainMenuGameButton.GetComponent<Image>().color = Color.white;

            // Button Changes.
            Button customProgramButton = backToMainMenuGameButton.GetComponent<Button>();

            DoubleClickButton doubleClickButton = backToMainMenuGameButton.AddComponent<DoubleClickButton>();

            customProgramButton.onClick.RemoveAllListeners(); // Remove all previous on click events.

            customProgramButton.onClick.AddListener(() => doubleClickButton.DoubleClickBackToMainGameAlsoLoad());

            // Rename Program Object Name
            backToMainMenuGameButton.name = "BackToMainGameButton";
        }

        public static void ChangeToCustomCampaignSettings(string customCampaignName)
        {
            MelonLogger.Msg(ConsoleColor.Green, $"INFO: Changing to custom campaign: {customCampaignName}.");

            // Activate the Custom Campaign
            CustomCampaignGlobal.ActivateCustomCampaign(customCampaignName);

            // Load Custom Campaign values
            CustomCampaignSaving.LoadFromFileCustomCampaignInfo();

            // Reload Scene (Mainly to hide the fact that it is actually seamless.)
            SceneManager.LoadScene("MainMenuScene");
        }

        public static void BackToMainGame(bool alsoLoadMainMenu = true)
        {
            MelonLogger.Msg(ConsoleColor.Green, "INFO: Going back to the main game.");

            // Save values
            CustomCampaignSaving.SaveCustomCampaignInfo();

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

        public static void DisableThemeDropdownDesktop()
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
        
        public static void DisableThemeDropdownInGame()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas == null)
            {
                return;
            }

            GameObject optionsPopup = mainCanvas.transform.Find("OptionsPopup").gameObject;
            if (optionsPopup == null)
            {
                return;
            }

            GameObject optionsScrollRect = optionsPopup.transform.Find("OptionsScrollRect").gameObject;
            if (optionsScrollRect == null)
            {
                return;
            }
            
            GameObject viewport = optionsScrollRect.transform.Find("Viewport").gameObject;
            if (viewport == null)
            {
                return;
            }
            
            GameObject content = viewport.transform.Find("Content").gameObject;
            if (content == null)
            {
                return;
            }
            
            GameObject videoOptions = content.transform.Find("VideoOptions").gameObject;
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