using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignPatches.Abstract;
using NewSafetyHelp.CustomCampaignPatches.Helper;
using UnityEngine;

namespace NewSafetyHelp.Emails
{
    public class CustomEmail : CustomCampaignElementBase
    {
        public string EmailSubject = "";

        public string SenderName = "";

        public string EmailBody = "";
        
        // Image shown at the end of the email.
        [CanBeNull] public Sprite EmailImage = null; 
        
        public string EmailAnimatedVideo = null;
        public bool HasAnimatedVideo = false;
        
        // Day from which day onward this email is allowed to appear.
        public int UnlockDay = 0;

        // When showing the list of emails, which priority should this email have?
        // Higher int => Higher priority
        public int EmailPriority = 0;
        
        public float UnlockThreshold = 0;
        public List<EmailAccuracyType> UnlockAccuracy = null;
        public bool UseOldAccuracyChecks = true;
        
        // If the player requires to finish the game first.
        // NOTE: It also requires the check to be true.
        public bool UnlockWhenGameFinished = false;
        
        
        
        // Main Campaign Values
        public bool InMainCampaign = false;
        
        // For finding the custom email:
        [CanBeNull] public Email ReferenceToEmailObject = null;
    }
}