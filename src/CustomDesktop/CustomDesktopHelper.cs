using NewSafetyHelp.LoggingSystem;
using Steamworks;
using UnityEngine;

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
                LoggingHelper.ErrorLog("Failed to find MainMenuCanvas. Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find Desktop from Main Menu." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find Logo from Desktop." +
                                       " Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the desktop GameObject which contains all emails.
        /// </summary>
        /// <returns>Email List GameObject</returns>
        public static GameObject GetEmailList()
        {
            GameObject foundGameObject = GetMainMenuCanvas().transform.Find("EmailPopup").Find("EmailsScrollview")
                .Find("Viewport").Find("Content").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                LoggingHelper.ErrorLog("Failed to find email list from Main Menu Canvas." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                    LoggingHelper.ErrorLog("Failed to find windows bar from Main Menu." +
                                           " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find credits from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find mailbox from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find Options from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find EntryBrowser from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find Artbook from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find Arcade from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find Scorecard from the program list." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find username from window bar." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find next caller button." +
                                       " Possibly called outside of MainCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the Program icons on the left side on the Desktop.
        /// </summary>
        /// <returns> GameObject for the programs on the left. </returns>
        public static GameObject GetLeftPrograms()
        {
            GameObject foundGameObject = GetDesktop().transform.Find("Programs").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                LoggingHelper.ErrorLog("Failed to find left sided Programs from Desktop." +
                                       " Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the Program icons on the right side on the Desktop.
        /// </summary>
        /// <returns> GameObject for the programs on the right. </returns>
        public static GameObject GetRightPrograms()
        {
            GameObject foundGameObject = GetDesktop().transform.Find("RightHandPrograms").gameObject;

            if (foundGameObject != null)
            {
                return foundGameObject;
            }
            else
            {
                LoggingHelper.ErrorLog("Failed to find right sided Programs from Desktop." +
                                       " Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }

        /// <summary>
        /// Gets the GameObject for the Winter DLC program. Used for creating copies to modify.
        /// </summary>
        /// <returns> GameObject for the winter dlc program on the left. </returns>
        public static GameObject GetWinterDlcProgram()
        {
            GameObject foundGameObject = GetLeftPrograms().transform.Find("DLC-Executable").gameObject;

            if (foundGameObject != null)
            {
                LoggingHelper.DebugLog("Found the DLC Icon.");

                return foundGameObject;
            }
            else
            {
                LoggingHelper.ErrorLog("Failed to find the winter DLC program." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.DebugLog("DLC is not installed. Not activating DLC.");

                // To make sure the DLC isn't enabled by accident.
                DisableWinterDlcProgram();

                return;
            }

            LoggingHelper.DebugLog("DLC is installed. Activating DLC!");

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
                LoggingHelper.ErrorLog("Failed to find the main game program." +
                                       " Possibly called outside of MainMenuCanvas?");
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
                LoggingHelper.ErrorLog("Failed to find the discord program." +
                                       " Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
    }
}