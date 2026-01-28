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
    }
}