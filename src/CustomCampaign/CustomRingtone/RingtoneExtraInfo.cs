namespace NewSafetyHelp.CustomCampaign.CustomRingtone
{
    public class RingtoneExtraInfo
    {
        public string customCampaignName = null; // If in a custom campaign, this is the name of custom campaign.
        
        public bool inMainCampaign = false; // If available in main campaign.
        
        // Audio Clip
        public string ringtoneClipPath = "";
        
        public RichAudioClip ringtoneClip = null;
        
        // Unlock Day (When it is allowed to play. 0 => every day)
        public int unlockDay = 0;
    }
}