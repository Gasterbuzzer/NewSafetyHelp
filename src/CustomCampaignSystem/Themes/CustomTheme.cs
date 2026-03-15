using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignPatches.Abstract;

namespace NewSafetyHelp.CustomCampaignPatches.Themes
{
    public class CustomTheme : CustomCampaignElementBase
    {
        public string ThemeName = "NO THEME NAME SET"; // Name of which this theme is displayed.
        
        public bool InMainCampaign = false; // If available in main campaign.
        
        /*
         * Conditional Theme Values
         */
        
        public string AttachedToTheme = null; // If a conditional theme, what theme is it attached to?
        public List<int> UnlockDays = null; // Days this theme is allowed to be shown.
                                            // Please note, this is only possible for attached themes.
                                            
        /*
         * Theme Options (Colors) (Each palette is made of 4 colors)
         */
        
        // First Color (Index 0) is the title bar color.
        // Second Color (Index 1) is the menus colors. Meaning buttons, scrollbars, checkboxes, etc.
        // Third Color (Index 2) is unknown. No idea.
        // Fourth Color (Index 3) is the main window color. Meaning what is behind the buttons, the window.
        
        public ColorPalette CustomThemePalette = null; // Theme of color palette. If null, it means not set.
    }
}