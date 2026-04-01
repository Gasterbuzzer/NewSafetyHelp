using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Modifier.Data
{
    public class CustomModifier : CustomCampaignElementBase
    {
        // Days the theme appears in, if set to null, it will apply every day.
        [CanBeNull] public List<int> UnlockDays = null; 
        // If a single day was added only, then we only have single day added.

        // If the modifier should only work if the game was finished.
        public bool OnlyIfGameFinished = false;
        
        /*
         * Default Desktop Options
         * These are also options in the custom campaign settings.
         */
        public string UsernameText = string.Empty; // Name of the player shown on the desktop.
        
        public string RenameMainGameDesktopIcon = string.Empty; // Renames the main desktop icon.
        
        // Icon of the main game desktop program.
        public VariableChanged<Sprite> MainGameDesktopIcon = new VariableChanged<Sprite>(); 
        
        // Backgrounds
        public List<Sprite> DesktopBackgrounds = new List<Sprite>(); // Desktop Backgrounds
        
        // Desktop Background (Finished the game)
        public VariableChanged<Sprite> GameFinishedBackground = new VariableChanged<Sprite>(); 
        
        // If the final cutscene should fade to black.
        public VariableChanged<bool> FinalCutsceneFadeToBlack = new VariableChanged<bool>
        {
            Data = true
        };
        
        public VariableChanged<bool> FinalCutsceneShake = new VariableChanged<bool>
        {
            Data = true
        };
        
        public VariableChanged<bool> FinalCutsceneGlitchSounds = new VariableChanged<bool>
        {
            Data = true
        };
        
        public VariableChanged<bool> FinalCutscenePreventClicks = new VariableChanged<bool>
        {
            Data = false
        };
        
        public VariableChanged<float> FinalCutsceneFadeDuration = new VariableChanged<float>
        {
            Data = 3f
        };
        
        public VariableChanged<float> FinalCutsceneFadePaddingDuration = new VariableChanged<float>
        {
            Data = 1f
        };
        
        public VariableChanged<bool> FinalCutsceneStopAudioAfterFade = new VariableChanged<bool>
        {
            Data = true
        };
        
        // Final Cutscene Audio
        public VariableChanged<RichAudioClip> FinalCutsceneAudio = new VariableChanged<RichAudioClip>
        {
            Data = null
        };

        public string FinalCutsceneAudioPath = null;
        
        // URLs to each animated background.
        public List<string> AnimatedDesktopBackgrounds = new List<string>();
        public bool BlackBackgroundOnAnimatedBackground = false;
        
        // If to disable the color the background green (or custom) the same as the main game does.
        public bool DisableColorBackground = false;
        
        public Color? DesktopBackgroundColor = null; // Color for background. If null, it means not set.

        // Disables the desktop logo "Home Safety Hotline" from the background (Also disables custom ones)
        public bool DisableDesktopLogo = false;
        
        // Logo to show in desktop (if not disabled)
        public VariableChanged<Sprite> CustomBackgroundLogo = new VariableChanged<Sprite>(); 
        
        public float BackgroundLogoTransparency = 0.2627f;
        
        // For those who want more immersion. Should not be recommended.
        public VariableChanged<bool> HideDiscordProgram = new VariableChanged<bool>(); 

        // Program Icons
        
        // OLD: public Sprite MailBoxIcon = null; 
        public VariableChanged<Sprite> MailBoxIcon = new VariableChanged<Sprite>(); // Mailbox Icon on Desktop
        
        public VariableChanged<Sprite> EntryBrowserIcon = new VariableChanged<Sprite>(); // Entry Browser Icon on Desktop
        
        public VariableChanged<Sprite> OptionsIcon = new VariableChanged<Sprite>(); // Options Icon on Desktop
        
        public VariableChanged<Sprite> ArtbookIcon = new VariableChanged<Sprite>(); // Artbook Icon on Desktop
        public VariableChanged<string> ArtbookRename = new VariableChanged<string>();
        
        public VariableChanged<Sprite> ArcadeIcon = new VariableChanged<Sprite>(); // Arcade Icon on Desktop
        
        public VariableChanged<Sprite> ScorecardIcon = new VariableChanged<Sprite>(); // Weekly Report Icon on Desktop
        
        // Credits
        [CanBeNull] public string DesktopCredits = null;
        public VariableChanged<Sprite> CreditsIcon = new VariableChanged<Sprite>(); // Credits Icon on Desktop
        
        public VariableChanged<bool> HideDesktopCredits = new VariableChanged<bool>
        {
            Data = false
        };
        
        /*
         * Enable Scorecard and such.
         */
        
        public VariableChanged<bool> EntryBrowserActive = new VariableChanged<bool>
        {
            Data = false
        };
        
        public VariableChanged<bool> ScorecardActive = new VariableChanged<bool>
        {
            Data = false
        };
        
        public VariableChanged<bool> ArtbookActive = new VariableChanged<bool>
        {
            Data = false
        };
        
        public VariableChanged<bool> ArcadeActive = new VariableChanged<bool>()
        {
            Data = false
        };
        
        /*
         * Special Desktop Options
         */
        
        public List<string> DayTitleStrings = new List<string>(); // Strings shown at the beginning of each day.
        
        /*
         * Cheats
         */
        // If to show the accuracy UI text string from the base game.
        public VariableChanged<bool> ShowDefaultUIAccuracyText = new VariableChanged<bool>
        {
            Data = false
        };
        
        // If to disable the desktop loading.
        public bool DisableDesktopLoading = false;
        
        // Removed. The effort to add these are difficult. So for now, we simply ignore it, unless someone needs it.
        //public List<List<string>> loadingTexts = new List<List<string>>(); // Texts shown when entering the desktop.
    }
}