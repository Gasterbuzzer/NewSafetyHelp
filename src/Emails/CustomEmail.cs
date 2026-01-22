using NewSafetyHelp.CustomCampaign.Abstract;
using UnityEngine;

namespace NewSafetyHelp.Emails
{
    public class CustomEmail : CustomCampaignElementBase
    {
        public string EmailSubject = "";

        public string SenderName = "";

        public string EmailBody = "";
        
        public Sprite EmailImage = null; // Image shown at the end of the email.
        
        // Unlock
        public int UnlockDay = 0;
        public int UnlockThreshold = 0;
        
        // Main Campaign Values
        public bool InMainCampaign = false;
    }
}