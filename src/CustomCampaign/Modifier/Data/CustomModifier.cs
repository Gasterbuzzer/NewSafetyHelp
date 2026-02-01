using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaign.Abstract;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Modifier.Data
{
    public class CustomModifier : CustomCampaignElementBase
    {
        // Days the theme appears in, if set to null, it will apply every day.
        [CanBeNull] public List<int> UnlockDays = null; 
        // If a single day was added only, then we only have single day added.
        
        /*
         * Default Desktop Options
         * These are also options in the custom campaign settings.
         */
        public string UsernameText = string.Empty; // Name of the player shown on the desktop.
        
        public string RenameMainGameDesktopIcon = string.Empty; // Renames the main desktop icon.
        public Sprite MainGameDesktopIcon = null; // Icon of the main game desktop program.
        
        public List<Sprite> DesktopBackgrounds = new List<Sprite>(); // Desktop Backgrounds
        
        public Sprite GameFinishedBackground = null; // Desktop Background (Finished the game)
        
        // If to disable the color the background green (or custom) the same as the main game does.
        public bool DisableColorBackground = false;
        
        public Color? DesktopBackgroundColor = null; // Color for background. If null, it means not set.

        // Disables the desktop logo "Home Safety Hotline" from the background (Also disables custom ones)
        public bool DisableDesktopLogo = false;
        
        public Sprite CustomBackgroundLogo = null; // Logo to show in desktop (if not disabled)
        public float BackgroundLogoTransparency = 0.2627f;
        
        public bool HideDiscordProgramChanged = false; // (If it was changed) (It means if to take into consideration)
        public bool HideDiscordProgram = false; // For those who want more immersion. Should not be recommended.

        // Program Icons
        
        public Sprite MailBoxIcon = null; // Mail Box Icon on Desktop
        public Sprite EntryBrowserIcon = null; // Entry Browser Icon on Desktop
        public Sprite OptionsIcon = null; // Options Icon on Desktop
        public Sprite ArtbookIcon = null; // Artbook Icon on Desktop
        public Sprite ArcadeIcon = null; // Arcade Icon on Desktop
        public Sprite ScorecardIcon = null; // Weekly Report Icon on Desktop
        
        // Credits
        [CanBeNull] public string DesktopCredits = null;
        public Sprite CreditsIcon = null; // Credits Icon on Desktop
        
        /*
         * Enable Scorecard and such.
         */
        
        public bool EntryBrowserActive = false;
        // If this setting was changed at all. Is used when checking.
        // If this is true and the "active" is false, it will disable the entry browser for example.
        public bool EntryBrowserChanged = false; 
        
        public bool ScorecardActive = false;
        public bool ScorecardChanged = false; // See entryBrowserChanged for explanation.
        
        public bool ArtbookActive = false;
        public bool ArtbookChanged = false; // See entryBrowserChanged for explanation.
        
        public bool ArcadeActive = false;
        public bool ArcadeChanged = false; // See entryBrowserChanged for explanation.
        
        /*
         * Special Desktop Options
         */
        
        public List<string> DayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.
        
        // Removed. The effort to add these are difficult. So for now, we simply ignore it, unless someone needs it.
        //public List<List<string>> loadingTexts = new List<List<string>>(); // Texts shown when entering the desktop.
    }
}