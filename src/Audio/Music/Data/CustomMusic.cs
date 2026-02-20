using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignPatches.Abstract;

namespace NewSafetyHelp.Audio.Music.Data
{
    public class CustomMusic : CustomCampaignElementBase
    {
        public string MusicClipPath = "";
        
        public RichAudioClip MusicClip = null;

        public int UnlockDay = 0;
        
        public bool OnlyPlayOnUnlockDay = false;

        // Intermission Music Option
        public bool IsIntermissionMusic = false;

        // Start or End ranges for the Intermission music.
        public List<float> StartRange = new List<float>(); 
        public List<float> EndRange = new List<float>();
    }
}