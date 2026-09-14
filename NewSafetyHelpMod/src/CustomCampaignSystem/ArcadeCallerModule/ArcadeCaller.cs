using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.ArcadeCallerModule
{
    public class ArcadeCaller : CustomCampaignElementBase
    {
        // Caller Basics
        public string CallerName = "NO_CALLER_NAME";
        public string CallTranscript = "NO_TRANSCRIPT";
        public VariableChanged<Sprite> CallerImage = new VariableChanged<Sprite>
        {
            Data = null
        };

        public int ArcadeCallersRequired = 0;

        // Caller Audio
        public RichAudioClip CallerClip = null;
        public string CallerClipPath = "";
        public bool IsCallerClipLoaded = false;
        public bool CompressAudio = true;

        // Animated Portrait
        public string CallerAnimatedPortraitURL = null;
        public bool CallerHasAnimatedPortrait = false;
        public VariableChanged<bool> CallerAnimatedPortraitShouldLoop = new VariableChanged<bool>
        {
            Data = true
        };
    }
}