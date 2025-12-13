using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Modifier.Data
{
    public class ModifierExtraInfo
    {
        public string customCampaignName = null;
        
        // Days the theme appears in, if set to null, it will apply every day.
        [CanBeNull] public List<int> unlockDays = null; 
        // If a single day was added only, then we only have single day added.
        
        /*
         * Default Desktop Options
         * These are also options in the custom campaign settings.
         */
        public string usernameText = string.Empty; // Name of the player shown on the desktop.
        
        public string renameMainGameDesktopIcon = string.Empty; // Renames the main desktop icon.
        public Sprite mainGameDesktopIcon = null; // Icon of the main game desktop program.
        
        public List<Sprite> desktopBackgrounds = new List<Sprite>(); // Desktop Backgrounds
        
        public Sprite gameFinishedBackground = null; // Desktop Background (Finished the game)
        
        // If to disable the color the background green (or custom) the same as the main game does.
        public bool disableColorBackground = false;
        
        public Color? desktopBackgroundColor = null; // Color for background. If null, it means not set.

        // Disables the desktop logo "Home Safety Hotline" from the background (Also disables custom ones)
        public bool disableDesktopLogo = false;
        
        public Sprite customBackgroundLogo = null; // Logo to show in desktop (if not disabled)
        public float backgroundLogoTransparency = 0.2627f;
        
        /*
         * Enable Scorecard and such.
         */
        
        //public bool entryBrowserAlwaysActive = false;
        //public bool scorecardAlwaysActive = false;
        //public bool artbookAlwaysActive = false;
        //public bool arcadeAlwaysActive = false;
        
        /*
         * Special Desktop Options
         */
        
        public List<string> dayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.
        
        // Removed. The effort to add these are difficult. So for now, we simply ignore it, unless someone needs it.
        //public List<List<string>> loadingTexts = new List<List<string>>(); // Texts shown when entering the desktop.
    }
}