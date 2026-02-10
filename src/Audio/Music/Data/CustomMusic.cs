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

        public List<int> StartEndRange = new List<int>(); // TODO: Implement
    }
}