using System.Collections.Generic;

namespace NewSafetyHelp.CustomCampaign.Themes
{
    public class CustomTheme
    {
        public string themeName = "NO THEME NAME SET"; // Name of which this theme is displayed.
        
        public string customCampaignName = null; // If in a custom campaign, this is the name of custom campaign.
        
        public bool inMainCampaign = false; // If available in main campaign.
        
        /*
         * Conditional Theme Values
         */
        
        public string attachedToTheme = null; // If a conditional theme, what theme is it attached to?
        public List<int> unlockDays = null; // Days this theme is allowed to be shown.
                                            // Please note, this is only possible for attached themes.
                                            
        /*
         * Theme Options (Colors) (Each palette is made of 4 colors)
         */
        
        // First Color (Index 0) is the title bar color.
        // Second Color (Index 1) is the menus colors. Meaning buttons, scrollbars, checkboxes, etc.
        // Third Color (Index 2) is unknown. No idea.
        // Fourth Color (Index 3) is the main window color. Meaning what is behind the buttons, the window.
        
        public ColorPalette customThemePalette = null; // Theme of color palette. If null, it means not set.
    }
}