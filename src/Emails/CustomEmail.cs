using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignPatches.Abstract;
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
        
        // Unlock
        public int UnlockDay = 0;
        public int UnlockThreshold = 0;
        
        // Main Campaign Values
        public bool InMainCampaign = false;
        
        // For finding the custom email:
        [CanBeNull] public Email ReferenceToEmailObject = null;
    }
}