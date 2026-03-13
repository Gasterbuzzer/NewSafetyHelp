using System.Reflection;
using NewSafetyHelp.CallerPatches.UI.AnimatedEntry;
using NewSafetyHelp.Emails;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class EmailHelper
    {
        private static GameObject animatedEmail;

        public static void SetAnimatedEmail(GameObject email)
        {
            animatedEmail = email;
        }
        
        public static GameObject GetEmailImageGameObject()
        {
            return GameObject.Find("MainMenuCanvas").transform.Find("EmailPopup").transform
                .Find("EmailContentScrollview").transform.Find("Viewport").transform.Find("Content").transform
                .Find("EmailImageBorder").transform.Find("EmailImage").gameObject;
        }
        
        public static void SetVideoUrlEmail(string url)
        {
            UpdateVisibilityOfNormalEmailPortrait();
            
            AnimatedImageHelper.SetVideoUrl(url, animatedEmail);
        } 
        
        public static void RestoreEmailPortrait()
        {
            // Show normal portrait again.
            UpdateVisibilityOfNormalEmailPortrait(true);
            
            // Disable video player.
            VideoPlayer videoPlayerComponent = animatedEmail.GetComponent<VideoPlayer>();
            
            videoPlayerComponent.Stop();
            
            if (videoPlayerComponent.targetTexture != null)
            {
                videoPlayerComponent.targetTexture.Release();
                Object.Destroy(videoPlayerComponent.targetTexture);
            }
            
            animatedEmail.SetActive(false);
        }
        
        private static void UpdateVisibilityOfNormalEmailPortrait(bool showEntryPortrait = false)
        {
            GetEmailImageGameObject().GetComponent<Image>().enabled = showEntryPortrait;
        }
        
        /// <summary>
        /// Creates an email and returns a reference.
        /// </summary>
        /// <returns>New Email reference.</returns>
        public static Email CreateEmail(CustomEmail emailToCreate)
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

                if (emailToCreate.UseOldAccuracyChecks)
                {
                    newEmailOnDayUnlock.scoreThresholdToUnlock = emailToCreate.UnlockThreshold;
                }
                else // Use new system, so we set an impossible value and later handle it separately.
                {
                    newEmailOnDayUnlock.scoreThresholdToUnlock = 2.0f;
                }

                // Mark the email as not read.

                FieldInfo hasClicked = typeof(EmailListingBehavior).GetField("hasClicked",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                if (hasClicked == null)
                {
                    LoggingHelper.WarningLog("HasClicked Field could not be found (null).");
                }
                else
                {
                    // OLD: emailListing.hasClicked = false;
                    hasClicked.SetValue(emailListing, false); 
                }

                return clonedEmail;
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