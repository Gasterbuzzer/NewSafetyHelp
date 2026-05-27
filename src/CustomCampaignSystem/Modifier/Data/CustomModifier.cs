using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Modifier.Data
{
    public class CustomModifier : CustomCampaignElementBase
    {
        /*
         * Conditions of Modifier Section
         */

        // Days the theme appears in, if set to null, it will apply every day.
        // If a single day was added only, then we only have single day added.
        [CanBeNull] public List<int> UnlockDays = null;

        // If the modifier should only work if the game was finished.
        public bool OnlyIfGameFinished = false;

        /*
         * Username Section (Desktop username)
         */
        public VariableChanged<string> UsernameText = new VariableChanged<string>
        {
            Data = string.Empty
        };

        /*
         * Desktop Background Section
         */
        public List<Sprite> DesktopBackgrounds = new List<Sprite>();

        // URLs to each animated background.
        public List<string> AnimatedDesktopBackgrounds = new List<string>();
        public bool BlackBackgroundOnAnimatedBackground = false;

        // If to disable the color the background green (or custom) the same as the main game does.
        public bool DisableColorBackground = false;

        // Color for background. If null, it means not set.
        public Color? DesktopBackgroundColor = null;

        public VariableChanged<Sprite> GameFinishedBackground = new VariableChanged<Sprite>();

        /*
         * Desktop Logo Section
         */

        // Disables the desktop logo "Home Safety Hotline" from the background (Also disables custom ones)
        public VariableChanged<bool> DisableDesktopLogo = new VariableChanged<bool>
        {
            Data = false
        };

        // Logo to show in desktop (if not disabled)
        public VariableChanged<Sprite> CustomBackgroundLogo = new VariableChanged<Sprite>();

        public VariableChanged<float> BackgroundLogoTransparency = new VariableChanged<float>
        {
            Data = 0.2627f
        };

        /*
         * Programs on Desktop Section
         */

        // Video Player
        public VariableChanged<bool> VideoPlayerDesktopIsWideMode = new VariableChanged<bool>
        {
            Data = false
        };

        // Main Game Desktop Program
        public VariableChanged<string> RenameMainGameDesktopIcon = new VariableChanged<string>
        {
            Data = string.Empty
        };

        public VariableChanged<Sprite> MainGameDesktopIcon = new VariableChanged<Sprite>();

        // Mailbox Icon on Desktop
        public VariableChanged<Sprite> MailboxIcon = new VariableChanged<Sprite>();
        public VariableChanged<Sprite> ApplicationMailboxIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> MailboxRename = new VariableChanged<string>();
        public VariableChanged<string> ApplicationMailboxTitle = new VariableChanged<string>();

        public VariableChanged<bool> DisplayMailboxOnDesktop = new VariableChanged<bool>
        {
            Data = false
        };

        // Entry Browser Icon on Desktop
        public VariableChanged<Sprite> EntryBrowserIcon = new VariableChanged<Sprite>();
        public VariableChanged<Sprite> ApplicationEntryBrowserIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> EntryBrowserRename = new VariableChanged<string>();
        public VariableChanged<string> ApplicationEntryBrowserTitle = new VariableChanged<string>();

        public VariableChanged<bool> DisplayEntryBrowserOnDesktop = new VariableChanged<bool>
        {
            Data = false
        };

        // Options Icon on Desktop
        public VariableChanged<Sprite> OptionsIcon = new VariableChanged<Sprite>();
        public VariableChanged<Sprite> ApplicationOptionsIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> OptionsRename = new VariableChanged<string>();
        public VariableChanged<string> ApplicationOptionsTitle = new VariableChanged<string>();

        // Artbook Icon on Desktop
        public VariableChanged<Sprite> ArtbookIcon = new VariableChanged<Sprite>();
        public VariableChanged<Sprite> ApplicationArtbookIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> ArtbookRename = new VariableChanged<string>();
        public VariableChanged<string> ApplicationArtbookTitle = new VariableChanged<string>();
        public List<ArtbookPage> ArtbookPages = new List<ArtbookPage>();

        public VariableChanged<bool> DisplayArtbookOnDesktop = new VariableChanged<bool>
        {
            Data = false
        };

        // Arcade Icon on Desktop
        public VariableChanged<Sprite> ArcadeIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> ArcadeRename = new VariableChanged<string>();

        public VariableChanged<bool> DisplayArcadeOnDesktop = new VariableChanged<bool>
        {
            Data = false
        };

        // Scorecard: Weekly Report Icon on Desktop
        public VariableChanged<Sprite> ScorecardIcon = new VariableChanged<Sprite>();
        public VariableChanged<Sprite> ApplicationScorecardIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> ScorecardRename = new VariableChanged<string>();
        public VariableChanged<string> ApplicationScorecardTitle = new VariableChanged<string>();

        public VariableChanged<bool> DisplayScorecardOnDesktop = new VariableChanged<bool>
        {
            Data = false
        };

        // Credits
        public VariableChanged<string> DesktopCredits = new VariableChanged<string>
        {
            Data = null
        };

        public VariableChanged<Sprite> CreditsIcon = new VariableChanged<Sprite>();
        public VariableChanged<string> CreditsRename = new VariableChanged<string>();

        public VariableChanged<bool> HideDesktopCredits = new VariableChanged<bool>
        {
            Data = false
        };

        // For those who want more immersion. Should not be recommended.
        public VariableChanged<bool> HideDiscordProgram = new VariableChanged<bool>();

        /*
         * Special Desktop Options Section
         */

        // Strings shown at the beginning of each day.
        public List<string> DayTitleStrings = new List<string>();

        /*
         * Final Cutscene
         */

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

        public VariableChanged<RichAudioClip> FinalCutsceneAudio = new VariableChanged<RichAudioClip>
        {
            Data = null
        };

        public string FinalCutsceneAudioPath = null;

        /*
         * Caller Section
         */

        public VariableChanged<bool> DisablePhoneStatic = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<bool> UseClockInsteadOfTimer = new VariableChanged<bool>
        {
            Data = false
        };

        public VariableChanged<float> DigitalClockTickRate = new VariableChanged<float>
        {
            Data = 1f
        };

        public VariableChanged<float> AnalogClockTickRate = new VariableChanged<float>
        {
            Data = 0.05f
        };

        public VariableChanged<string> TimedCallerDisconnectedMessage = new VariableChanged<string>
        {
            Data = "TIMES UP!\nCALL DISCONNECTED"
        };

        public VariableChanged<RichAudioClip> TimedCallerStartSound = new VariableChanged<RichAudioClip>
        {
            Data = null
        };

        public VariableChanged<string> TimedCallerStartSoundPath = new VariableChanged<string>
        {
            Data = null
        };

        public VariableChanged<RichAudioClip> TimedCallerHalfSound = new VariableChanged<RichAudioClip>
        {
            Data = null
        };

        public VariableChanged<string> TimedCallerHalfSoundPath = new VariableChanged<string>
        {
            Data = null
        };

        public VariableChanged<RichAudioClip> TimedCallerCriticalSound = new VariableChanged<RichAudioClip>
        {
            Data = null
        };

        public VariableChanged<string> TimedCallerCriticalSoundPath = new VariableChanged<string>
        {
            Data = null
        };

        public VariableChanged<Sprite> TimedCallerBaseClock = new VariableChanged<Sprite>
        {
            Data = null
        };

        public VariableChanged<Sprite> TimedCallerClockHand = new VariableChanged<Sprite>
        {
            Data = null
        };
        
        public VariableChanged<float> TimedCallerProfileClockSize = new VariableChanged<float>
        {
            Data = 57.5f
        };
        
        public VariableChanged<float> TimedCallerProfileClockSizeMultiplier = new VariableChanged<float>
        {
            Data = 1f
        };
        
        public VariableChanged<float> TimedCallerProfileClockPadX = new VariableChanged<float>
        {
            Data = 5f
        };
        
        public VariableChanged<float> TimedCallerProfileClockPadY = new VariableChanged<float>
        {
            Data = 5f
        };
        
        /*
         * Cheats / Settings Section
         */

        // If to show the accuracy UI text string from the base game.
        public VariableChanged<bool> ShowDefaultUIAccuracyText = new VariableChanged<bool>
        {
            Data = false
        };

        // If to disable the desktop loading.
        public bool DisableDesktopLoading = false;

        /*
         * Removed/Unfinished Section
         */

        // The effort to add these are difficult.
        // So for now, we simply ignore it, unless someone needs it.
        //public List<List<string>> loadingTexts = new List<List<string>>();
    }
}