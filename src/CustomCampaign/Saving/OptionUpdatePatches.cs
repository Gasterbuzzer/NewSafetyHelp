using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign.Saving
{
    public static class OptionUpdatePatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnMusicSliderChange")]
        public static class OnMusicSliderChangePatch
        {
            /// <summary>
            /// OnMusicSliderChange patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(OptionsMenuBehavior __instance)
            {
                // First we store the slider value in the global SaveManagerScript.
                GlobalVariables.saveManagerScript.savedMusicVolume = __instance.musicSlider.value;
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving volume. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedMusicVolume = __instance.musicSlider.value;
                }
                
                // Applying the value.
                if (Mathf.Approximately(__instance.musicSlider.value, __instance.musicSlider.minValue))
                {
                    __instance.masterMixer.SetFloat("musicVol", -80f);
                }
                else
                {
                    __instance.masterMixer.SetFloat("musicVol", Mathf.Log10(__instance.musicSlider.value) * 20f);
                }
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnSFXSliderChange")]
        public static class OnSFXSliderChangePatch
        {
            /// <summary>
            /// OnSFXSliderChange patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(OptionsMenuBehavior __instance)
            {
                // First we store the slider value in the global SaveManagerScript.
                GlobalVariables.saveManagerScript.savedSFXVolume = __instance.sfxSlider.value;
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving volume. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedSFXVolume = __instance.sfxSlider.value;
                }
                
                // Applying the value.
                if (Mathf.Approximately(__instance.sfxSlider.value, __instance.sfxSlider.minValue))
                {
                    __instance.masterMixer.SetFloat("sfxVol", -80f);
                }
                else
                {
                    __instance.masterMixer.SetFloat("sfxVol", Mathf.Log10(__instance.sfxSlider.value) * 20f);
                }
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnMasterSliderChange")]
        public static class OnMasterSliderChangePatch
        {
            /// <summary>
            /// OnMasterSliderChange patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(OptionsMenuBehavior __instance)
            {
                // First we store the slider value in the global SaveManagerScript.
                GlobalVariables.saveManagerScript.savedAmbienceVolume = __instance.masterSlider.value;
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving volume. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedAmbienceVolume = __instance.masterSlider.value;
                }
                
                // Applying the value.
                if (Mathf.Approximately(__instance.masterSlider.value, __instance.masterSlider.minValue))
                {
                    __instance.masterMixer.SetFloat("masterVol", -80f);
                }
                else
                {
                    __instance.masterMixer.SetFloat("masterVol", Mathf.Log10(__instance.masterSlider.value) * 20f);
                }
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "CRTToggle", typeof(bool))]
        public static class CRTTogglePatch
        {
            /// <summary>
            /// CRTToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Value of the checkbox</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedCRTToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving CRT toggle. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedCRTToggle = value;
                }
                
                // Applying the value.
                if (GlobalVariables.crtShader == null)
                {
                    return false;
                }

                GlobalVariables.crtShader.scanlineIntensity = value ? 8f : 0.0f;
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ScreenResolutions), "FullScreenToggle")]
        public static class FullScreenTogglePatch
        {
            /// <summary>
            /// CRTToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ScreenResolutions __instance)
            {
                GlobalVariables.isFullScreen = __instance.fullScreenToggle.isOn;
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving fullscreen. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedFullScreenToggle = __instance.fullScreenToggle.isOn;
                }
                
                Screen.fullScreen = GlobalVariables.isFullScreen;
                __instance.SaveResolutionInfo();
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "DyslexiaToggle", typeof(bool))]
        public static class DyslexiaTogglePatch
        {
            /// <summary>
            /// DyslexiaToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Value of the checkbox</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedDyslexiaToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving dyslexia toggle. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedDyslexiaToggle = value;
                }
                
                GlobalVariables.dyslexiaMode = value;
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "OnTextSizeSliderChange")]
        public static class OnTextSizeSliderChangePatch
        {
            /// <summary>
            /// OnTextSizeSliderChange patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(OptionsMenuBehavior __instance)
            {
                GlobalVariables.textSizeMultiplier = __instance.textSizeSlider.value;
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving text size. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedTextSizeMultiplier = __instance.textSizeSlider.value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "SpiderToggle", typeof(bool))]
        public static class SpiderTogglePatch
        {
            /// <summary>
            /// SpiderToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedSpiderToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedSpiderToggle = value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "InsectToggle", typeof(bool))]
        public static class InsectTogglePatch
        {
            /// <summary>
            /// InsectToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedInsectToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedInsectToggle = value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "DarkToggle", typeof(bool))]
        public static class DarkTogglePatch
        {
            /// <summary>
            /// DarkToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedDarkToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedDarkToggle = value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "HoleToggle", typeof(bool))]
        public static class HoleTogglePatch
        {
            /// <summary>
            /// HoleToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedHoleToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedHoleToggle = value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "WatchToggle", typeof(bool))]
        public static class WatchTogglePatch
        {
            /// <summary>
            /// WatchToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedWatchToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedWatchToggle = value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "TightToggle", typeof(bool))]
        public static class TightTogglePatch
        {
            /// <summary>
            /// TightToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedTightToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedTightToggle = value;
                }

                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(OptionsMenuBehavior), "DogToggle", typeof(bool))]
        public static class DogTogglePatch
        {
            /// <summary>
            /// DogToggle patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="value">Boolean value of the toggle.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ref bool value)
            {
                GlobalVariables.saveManagerScript.savedDogToggle = GlobalVariables.saveManagerScript.BoolToInt(value);
                
                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign saving
                {
                    CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign null! Unable of saving phobia setting. " +
                                          "Calling original function.");
                        return true;
                    }
                    
                    customCampaign.SavedDogToggle = value;
                }

                return false; // Skip original function.
            }
        }
    }
}