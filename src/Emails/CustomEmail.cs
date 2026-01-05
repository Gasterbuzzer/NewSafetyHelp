using UnityEngine;

namespace NewSafetyHelp.Emails
{
    public class CustomEmail
    {
        public string emailSubject = "";

        public string senderName = "";

        public string emailBody = "";
        
        public Sprite emailImage = null; // Image shown at the end of the email.
            
        // Custom Campaign Values
        public string customCampaignName = "";
        
        // Unlock
        public int unlockDay = 0;
        public int unlockThreshold = 0;
        
        // Main Campaign Values
        public bool inMainCampaign = false;
    }
}