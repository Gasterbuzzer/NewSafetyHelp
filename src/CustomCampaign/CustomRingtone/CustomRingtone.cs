namespace NewSafetyHelp.CustomCampaign.CustomRingtone
{
    public class CustomRingtone
    {
        public string CustomCampaignName = null; // If in a custom campaign, this is the name of custom campaign.
        
        // Audio Clip
        public string RingtoneClipPath = "";
        
        public RichAudioClip RingtoneClip = null;
        
        // If the ringtone is supposed to appended instead of being the only one.
        public bool AppendRingtone = false;
        
        // Unlock Day (When it is allowed to play. 0 => every day)
        public int UnlockDay = 0;
        
        public bool OnlyOnUnlockDay = true; // If the ringtone should only appear on the unlock day.
        
        // Glitched version
        public bool IsGlitchedVersion = false; // When network is down, use this version.
    }
}