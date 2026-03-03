using System.Reflection;
using NewSafetyHelp.Emails;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class EmailHelper
    {
        /// <summary>
        /// Creates an email and returns a reference.
        /// </summary>
        /// <returns>New Email reference.</returns>
        public static GameObject CreateEmail(CustomEmail emailToCreate)
        {
            GameObject originalEmail = CustomDesktopHelper.GetEmailList().transform.Find("EmailListing (14)").gameObject;

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
                    LoggingHelper.ErrorLog("Failed to find email listing behavior for EmailListing.");
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
                    LoggingHelper.WarningLog("HasClicked Field could not be found (null).");
                }
                else
                {
                    hasClicked.SetValue(emailListing, false); // emailListing.hasClicked = false;
                }

                return newEmail;
            }
            else
            {
                LoggingHelper.ErrorLog("Failed to find email to copy from in the Email List." +
                                       " Possibly called outside of MainMenuCanvas?");
                return null;
            }
        }
        
        /// <summary>
        /// Removes all emails from the main campaign.
        /// </summary>
        public static void RemoveMainGameEmails()
        {
            GameObject foundGameObject = CustomDesktopHelper.GetEmailList();

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
                LoggingHelper.ErrorLog("Failed to find email list from Main Menu Canvas." +
                                       " Possibly called outside of MainMenuCanvas?");
            }
        }
    }
}