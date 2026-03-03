using UnityEngine;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class ThemeProgramHelper
    {
        public static void DisableThemeDropdownDesktop()
        {
            GameObject videoOptions = GameObject.Find("MainMenuCanvas").transform.Find("OptionsPopup").transform
                .Find("OptionsScrollRect").transform.Find("Viewport").transform.Find("Content").transform
                .Find("VideoOptions").gameObject;

            if (videoOptions == null)
            {
                return;
            }

            GameObject colorPaletteHeader = videoOptions.transform.Find("ColorPaletteHeader").gameObject;
            GameObject colorDropdown = videoOptions.transform.Find("ColorDropdown").gameObject;

            if (colorPaletteHeader != null)
            {
                colorPaletteHeader.SetActive(false);
            }
            
            if (colorDropdown != null)
            {
                colorDropdown.SetActive(false);
            }
        }
        
        public static void DisableThemeDropdownInGame()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas == null)
            {
                return;
            }

            GameObject optionsPopup = mainCanvas.transform.Find("OptionsPopup").gameObject;
            if (optionsPopup == null)
            {
                return;
            }

            GameObject optionsScrollRect = optionsPopup.transform.Find("OptionsScrollRect").gameObject;
            if (optionsScrollRect == null)
            {
                return;
            }
            
            GameObject viewport = optionsScrollRect.transform.Find("Viewport").gameObject;
            if (viewport == null)
            {
                return;
            }
            
            GameObject content = viewport.transform.Find("Content").gameObject;
            if (content == null)
            {
                return;
            }
            
            GameObject videoOptions = content.transform.Find("VideoOptions").gameObject;
            if (videoOptions == null)
            {
                return;
            }

            GameObject colorPaletteHeader = videoOptions.transform.Find("ColorPaletteHeader").gameObject;
            GameObject colorDropdown = videoOptions.transform.Find("ColorDropdown").gameObject;

            if (colorPaletteHeader != null)
            {
                colorPaletteHeader.SetActive(false);
            }
            
            if (colorDropdown != null)
            {
                colorDropdown.SetActive(false);
            }
        }
    }
}