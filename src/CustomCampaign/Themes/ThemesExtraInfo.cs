using System.Collections.Generic;

namespace NewSafetyHelp.CustomCampaign.Themes
{
    public class ThemesExtraInfo
    {
        public string themeName = "NO THEME NAME SET"; // Name of which this theme is displayed.
        
        public string customCampaignName = null; // If in a custom campaign, this is the name of custom campaign.
        
        public bool inMainCampaign = false; // If available in main campaign.
        
        /*
         * Special Values
         */
        
        public string attachedToTheme = null; // If a conditional theme, what theme is it attached to?
        public List<int> unlockDays = null; // Days this theme is allowed to be shown.
                                            // Please note, this is only possible for attached themes.
        /*
         * Theme Options (Colors)
         */
        
        // TODO: Update comments.
        
        public ColorPalette defaultPalette = null; // Color for the desktop taskbar. If null, it means not set.
        public ColorPalette windowsPalette = null; // Color for the desktop taskbar. If null, it means not set.
        public ColorPalette tirePalette = null; // Color for the desktop taskbar. If null, it means not set.
        public ColorPalette nightPalette = null; // Color for the desktop taskbar. If null, it means not set.
    }
}