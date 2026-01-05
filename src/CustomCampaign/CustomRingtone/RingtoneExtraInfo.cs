namespace NewSafetyHelp.CustomCampaign.CustomRingtone
{
    public class RingtoneExtraInfo
    {
        public string customCampaignName = null; // If in a custom campaign, this is the name of custom campaign.
        
        // Audio Clip
        public string ringtoneClipPath = "";
        
        public RichAudioClip ringtoneClip = null;
        
        // Unlock Day (When it is allowed to play. 0 => every day)
        public int unlockDay = 0;
        
        public bool onlyOnUnlockDay = true; // If the ringtone should only appear on the unlock day.
        
        // Glitched version
        public bool isGlitchedVersion = false; // When network is down, use this version.
    }
}