using NewSafetyHelp.CustomCampaign.Abstract;

namespace NewSafetyHelp.Audio.Music.Data
{
    public class CustomMusic : CustomCampaignElementBase
    {
        public string MusicClipPath = "";
        
        public RichAudioClip MusicClip = null;

        public int UnlockDay = 0;
        
        public bool OnlyPlayOnUnlockDay = false;

        public bool IsIntermissionMusic = false;
    }
}