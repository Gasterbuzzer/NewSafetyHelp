using System.Collections.Generic;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Modifier.Data
{
    public class DesktopModifierSnapshot
    {
        public (bool found, VariableChanged<string> value) UsernameText;

        public (bool found, VariableChanged<bool> value) DisableDesktopLogo;
        public (bool found, VariableChanged<Sprite> value) CustomBackgroundLogo;
        public (bool found, VariableChanged<float> value) BackgroundLogoTransparency;
        
        public (bool found, VariableChanged<Sprite> value) EntryBrowserIcon;
        public (bool found, VariableChanged<string> value) EntryBrowserRename;
        public (bool found, VariableChanged<string> value) ApplicationEntryBrowserTitle;
        
        public (bool found, VariableChanged<Sprite> value) MailBoxIcon;
        public (bool found, VariableChanged<string> value) MailBoxRename;
        public (bool found, VariableChanged<string> value) ApplicationMailBoxTitle;
        
        public (bool found, VariableChanged<Sprite> value) OptionsIcon;
        public (bool found, VariableChanged<string> value) OptionsRename;
        public (bool found, VariableChanged<string> value) ApplicationOptionsTitle;
        
        public (bool found, VariableChanged<Sprite> value) ArtbookIcon;
        public (bool found, VariableChanged<string> value) ArtbookRename;
        public (bool found, VariableChanged<string> value) ApplicationArtbookTitle;
        public (bool found, List<ArtbookPage> value) ArtbookPages;
        
        public (bool found, VariableChanged<Sprite> value) ScorecardIcon;
        public (bool found, VariableChanged<string> value) ScorecardRename;
        public (bool found, VariableChanged<string> value) ApplicationScorecardTitle;
        
        public (bool found, VariableChanged<Sprite> value) ArcadeIcon;
        public (bool found, VariableChanged<string> value) ArcadeRename;
        
        public (bool found, VariableChanged<string> value) DesktopCredits;
        public (bool found, VariableChanged<string> value) CreditsRename;
        public (bool found, VariableChanged<Sprite> value) CreditsIcon;
        public (bool found, VariableChanged<bool> value) HideDesktopCredits;
        
        public (bool found, VariableChanged<bool> value) HideDiscordProgram;
        
        public (bool found, VariableChanged<string> value) RenameMainGameDesktopIcon;
        public (bool found, VariableChanged<Sprite> value) MainGameDesktopIcon;
    }
}