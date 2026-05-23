using System;
using System.Reflection;
using NewSafetyHelp.Callers.UI.AnimatedEntry;
using NewSafetyHelp.Emails;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class EmailHelper
    {
        /// <summary>
        /// Containing the reference to the animated email image GameObject.
        /// </summary>
        private static GameObject animatedEmail;
        /// <summary>
        /// Reference to the Button of the image.
        /// </summary>
        private static Button imageButtonComponent;
        /// <summary>
        /// Reference to the Button of the animated image.
        /// </summary>
        private static Button animatedImageButtonComponent;

        /// <summary>
        /// The SwapCursorHover references for showing if the object can be clicked or not.
        /// </summary>
        private static SwapCursorHoverDisplayer imageSwapCursorHoverDisplayer;
        private static SwapCursorHoverDisplayer animatedImageSwapCursorHoverDisplayer;

        /// <summary>
        /// Sets the private GameObject of the animated email.
        /// </summary>
        /// <param name="email">GameObject that contains the animated email</param>
        public static void SetAnimatedEmail(GameObject email)
        {
            animatedEmail = email;
        }
        
        /// <summary>
        /// Adds a button and any raycasts for the email.
        /// </summary>
        public static void CreateClickableEmail(bool enableClickingImage = false,
            bool enableClickingAnimatedImage = false)
        {
            GameObject emailImageGameObject = GetEmailImageGameObject();
            SwapCursorHoverDisplayer closeButtonSwapCursorHoverDisplayer = GetCloseButtonOnHoverScript();

            if (emailImageGameObject == null 
                || animatedEmail == null
                || closeButtonSwapCursorHoverDisplayer == null)
            {
                return;
            }
            
            // We first make the loading hider not absorb clicks.
            GetEmailLoadingHider().GetComponent<Image>().raycastTarget = false;
            
            // We get the Texture2D of the close button for later usage.
            Texture2D restoredCursor = closeButtonSwapCursorHoverDisplayer.defaultCursor;
            Texture2D hoverCursor = closeButtonSwapCursorHoverDisplayer.hoverCursor;
            
            // Add button and cursor swapper.
            imageButtonComponent = emailImageGameObject.AddComponent<Button>();
            imageSwapCursorHoverDisplayer = emailImageGameObject.AddComponent<SwapCursorHoverDisplayer>();
            imageSwapCursorHoverDisplayer.enabled = false;
            imageSwapCursorHoverDisplayer.defaultCursor = restoredCursor;
            imageSwapCursorHoverDisplayer.hoverCursor = hoverCursor;
            
            animatedImageButtonComponent = animatedEmail.AddComponent<Button>();
            animatedImageSwapCursorHoverDisplayer = animatedEmail.AddComponent<SwapCursorHoverDisplayer>();
            animatedImageSwapCursorHoverDisplayer.enabled = false;
            animatedImageSwapCursorHoverDisplayer.defaultCursor = restoredCursor;
            animatedImageSwapCursorHoverDisplayer.hoverCursor = hoverCursor;
            
            // Allows original image to be pressed.
            if (enableClickingImage)
            {
                emailImageGameObject.GetComponent<Image>().raycastTarget = true;
                animatedImageSwapCursorHoverDisplayer.enabled = true;
            }
            else
            {
                emailImageGameObject.GetComponent<Image>().raycastTarget = false;
                animatedImageSwapCursorHoverDisplayer.enabled = false;
            }

            // Allows animated image to be pressed.
            if (enableClickingAnimatedImage)
            {
                animatedEmail.GetComponent<RawImage>().raycastTarget = true;
                imageSwapCursorHoverDisplayer.enabled = true;
            }
            else
            {
                animatedEmail.GetComponent<RawImage>().raycastTarget = false;
                imageSwapCursorHoverDisplayer.enabled = false;
            }
        }

        /// <summary>
        /// For buttons sets the correct event.
        /// </summary>
        /// <param name="hasNoURL">If when setting the URL, if to simply strip all events.</param>
        /// <param name="urlToOpen">Which URL to open upon pressing the image.</param>
        /// <param name="hasAnimatedVideo">If this is an animated video or if it is the default image.</param>
        public static void SetClickUrlToOpen(bool hasNoURL, Uri urlToOpen, bool hasAnimatedVideo)
        {
            if (imageButtonComponent == null
                || animatedImageButtonComponent == null)
            {
                return;
            }
            
            imageButtonComponent.onClick.RemoveAllListeners();
            animatedImageButtonComponent.onClick.RemoveAllListeners();
            
            GameObject emailImageGameObject = GetEmailImageGameObject();

            if (emailImageGameObject == null 
                || animatedEmail == null)
            {
                DisableEmailImageCursorHover();
                return;
            }
            
            // Allows original image to be pressed.
            if (!hasNoURL)
            {
                emailImageGameObject.GetComponent<Image>().raycastTarget = true;
                animatedEmail.GetComponent<RawImage>().raycastTarget = true;
            }
            else
            {
                emailImageGameObject.GetComponent<Image>().raycastTarget = false;
                animatedEmail.GetComponent<RawImage>().raycastTarget = false;
            }
            
            if (hasNoURL)
            {
                DisableEmailImageCursorHover();
                return;
            }
                
            if (urlToOpen != null 
                && URLVerification.IsURLAndNotUnsafe(urlToOpen))
            {
                imageButtonComponent.onClick.AddListener(() =>
                {
                    URLVerification.OpenEmailURI(urlToOpen);
                });
                
                animatedImageButtonComponent.onClick.AddListener(() =>
                {
                    URLVerification.OpenEmailURI(urlToOpen);
                });
                
                // We also enable the corresponding cursor hover.
                SetWhichImageGetsTheCursorHover(hasAnimatedVideo);
            }
            else
            {
                DisableEmailImageCursorHover();
            }
        }

        /// <summary>
        /// Sets which image object will be able to swap the cursor.
        /// </summary>
        /// <param name="showForAnimatedVideo">If to enable for animated video.</param>
        private static void SetWhichImageGetsTheCursorHover(bool showForAnimatedVideo = false)
        {
            if (showForAnimatedVideo)
            {
                imageSwapCursorHoverDisplayer.enabled = false;
                animatedImageSwapCursorHoverDisplayer.enabled = true;
            }
            else
            {
                imageSwapCursorHoverDisplayer.enabled = true;
                animatedImageSwapCursorHoverDisplayer.enabled = false;
            }
        }
        
        /// <summary>
        /// Disables all email image cursor swapper elements.
        /// </summary>
        public static void DisableEmailImageCursorHover()
        {
            imageSwapCursorHoverDisplayer.enabled = false;
            animatedImageSwapCursorHoverDisplayer.enabled = false;
        }
        
        /// <summary>
        /// Gets the email Image GameObject.
        /// </summary>
        /// <returns>Email Image</returns>
        public static GameObject GetEmailImageGameObject()
        {
            return GameObject.Find("MainMenuCanvas").transform.Find("EmailPopup").transform
                .Find("EmailContentScrollview").transform.Find("Viewport").transform.Find("Content").transform
                .Find("EmailImageBorder").transform.Find("EmailImage").gameObject;
        }
        
        /// <summary>
        /// Gets the SwapCursorHoverDisplayer of the CloseButton.
        /// </summary>
        /// <returns>SwapCursorHoverDisplayer of the CloseButton</returns>
        public static SwapCursorHoverDisplayer GetCloseButtonOnHoverScript()
        {
            return GameObject.Find("MainMenuCanvas").transform.Find("EmailPopup").
                Find("WindowsBar").Find("CloseButton").GetComponent<SwapCursorHoverDisplayer>();
        }
        
        /// <summary>
        /// Gets the Email Loading hider GameObject.
        /// </summary>
        /// <returns>LoadingHider of the emails.</returns>
        private static GameObject GetEmailLoadingHider()
        {
            return GameObject.Find("MainMenuCanvas").transform.Find("EmailPopup").transform
                .Find("EmailContentScrollview").transform.Find("Viewport").transform.Find("LoadingHider").gameObject;
        }
        
        /// <summary>
        /// Sets the URL of the video.
        /// </summary>
        /// <param name="url"></param>
        public static void SetVideoUrlEmail(string url)
        {
            UpdateVisibilityOfNormalEmailPortrait();
            
            AnimatedImageHelper.SetVideoUrl(url, animatedEmail);
        } 
        
        /// <summary>
        /// Restores the default email portrait (image) to the normal style.
        /// </summary>
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
        
        /// <summary>
        /// Shows or hides the default image.
        /// </summary>
        /// <param name="showImage">If to show the default image or hide it.</param>
        private static void UpdateVisibilityOfNormalEmailPortrait(bool showImage = false)
        {
            GetEmailImageGameObject().GetComponent<Image>().enabled = showImage;
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