using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.InGameSettings;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Saving
{
    public static class SpecialOptionUpdatePatches
    {
        [HarmonyLib.HarmonyPatch(typeof(ScreenResolutions), "OnEnable")]
        public static class OnEnablePatch
        {
            /// <summary>
            /// OnEnable start patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ScreenResolutions __instance)
            {
                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
                {
                    if (GlobalVariables.screenWidthSetting == 0)
                    {
                        return false;
                    }
                    __instance.SetMenuValue(GlobalVariables.screenWidthSetting, GlobalVariables.screenHeightSetting, GlobalVariables.refreshRateSetting);
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }
                    
                    if (customCampaign.SavedScreenWidth == 0)
                    {
                        return false;
                    }
                    
                    __instance.SetMenuValue(customCampaign.SavedScreenWidth, customCampaign.SavedScreenHeight, customCampaign.SavedRefreshRate);
                }
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ScreenResolutions), "Start")]
        public static class ScreenResolutionsStartPatch
        {
            private static readonly FieldInfo ResolutionsField = typeof(ScreenResolutions).GetField("resolutions",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            private static readonly MethodInfo ResToString = typeof(ScreenResolutions).GetMethod("ResToString",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            private static readonly MethodInfo SetSteamDeckResolution = typeof(ScreenResolutions).
                GetMethod("SetSteamDeckResolution", BindingFlags.NonPublic | BindingFlags.Instance);
            private static readonly FieldInfo JustStarted = typeof(ScreenResolutions).GetField("justStarted", BindingFlags.NonPublic | BindingFlags.Static);
            
            /// <summary>
            /// ScreenResolutions start patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ScreenResolutions __instance)
            {
                if (ResolutionsField == null 
                    || ResToString == null 
                    || JustStarted == null 
                    || SetSteamDeckResolution == null)
                {
                    LoggingHelper.ReflectionError(nameof(ResolutionsField), nameof(ResToString),
                        nameof(SetSteamDeckResolution), nameof(JustStarted));
                    return true;
                }
                
                Resolution[] resolutions = (Resolution[]) ResolutionsField.GetValue(__instance);
                
                __instance.dropdownMenu.options.Clear();

                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
                {
                    for (int index = 0; index < resolutions.Length; ++index)
                    {
                        // OLD:  __instance.ResToString()
                        string label = (string) ResToString.Invoke(__instance, new object[] { resolutions[index] });
                    
                        __instance.dropdownMenu.options.Add(new TMP_Dropdown.OptionData(label)); 
                    
                        if (resolutions[index].width == GlobalVariables.screenWidthSetting 
                            && resolutions[index].height == GlobalVariables.screenHeightSetting 
                            && resolutions[index].refreshRate == GlobalVariables.refreshRateSetting)
                        {
                            __instance.dropdownMenu.value = index;
                        }
                    }
                    
                    __instance.fullScreenToggle.isOn = GlobalVariables.isFullScreen;
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }
                    
                    for (int index = 0; index < resolutions.Length; ++index)
                    {
                        string label = (string) ResToString.Invoke(__instance, new object[] { resolutions[index] });
                    
                        __instance.dropdownMenu.options.Add(new TMP_Dropdown.OptionData(label)); 
                    
                        if (resolutions[index].width == customCampaign.SavedScreenWidth 
                            && resolutions[index].height == customCampaign.SavedScreenHeight 
                            && resolutions[index].refreshRate == customCampaign.SavedRefreshRate)
                        {
                            __instance.dropdownMenu.value = index;
                        }
                    }

                    __instance.fullScreenToggle.isOn = customCampaign.SavedFullScreenToggle;
                }
                
                // We add the listener afterward, to avoid is listening us add values and selecting one.
                __instance.dropdownMenu.onValueChanged.AddListener(_ => 
                    __instance.SetDropdownResolution(resolutions[__instance.dropdownMenu.value].width,
                        resolutions[__instance.dropdownMenu.value].height,
                        resolutions[__instance.dropdownMenu.value].refreshRate));
                
                // OLD: ScreenResolutions.justStarted = false;
                JustStarted.SetValue(__instance, false); 
                
                // OLD: __instance.SetSteamDeckResolution();
                SetSteamDeckResolution.Invoke(__instance, null);
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ScreenResolutions), "SaveResolutionInfo")]
        public static class SaveResolutionInfoPatch
        {
            /// <summary>
            /// SaveResolutionInfo start patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix()
            {
                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
                {
                    if (GlobalVariables.screenWidthSetting >= 480 && GlobalVariables.screenHeightSetting >= 480)
                    {
                        Screen.SetResolution(GlobalVariables.screenWidthSetting, GlobalVariables.screenHeightSetting,
                            GlobalVariables.isFullScreen, GlobalVariables.refreshRateSetting);

                        if (GlobalPreferences.Vsync.Value)
                        {
                            QualitySettings.vSyncCount = 1;
                        }
                        else
                        {
                            QualitySettings.vSyncCount = 0;
                        }
                        
                        Application.targetFrameRate = GlobalVariables.refreshRateSetting;
                        
                        LoggingHelper.DebugLog("Saved Resolution:" +
                                               $" {GlobalVariables.screenWidthSetting.ToString()}x{GlobalVariables.screenHeightSetting.ToString()} @{GlobalVariables.refreshRateSetting.ToString()}");
                        LoggingHelper.DebugLog(
                            $"Target Frame Rate: {Application.targetFrameRate}"
                        );
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }
                    
                    if (customCampaign.SavedScreenWidth >= 480 && customCampaign.SavedScreenHeight >= 480)
                    {
                        Screen.SetResolution(customCampaign.SavedScreenWidth, customCampaign.SavedScreenHeight, 
                            customCampaign.SavedFullScreenToggle, customCampaign.SavedRefreshRate);

                        if (GlobalPreferences.Vsync.Value)
                        {
                            QualitySettings.vSyncCount = 1;
                        }
                        else
                        {
                            QualitySettings.vSyncCount = 0;
                        }
                        
                        Application.targetFrameRate = customCampaign.SavedRefreshRate;
                        
                        LoggingHelper.DebugLog("Saved Resolution:" +
                                               $" {GlobalVariables.screenWidthSetting.ToString()}x{GlobalVariables.screenHeightSetting.ToString()} @{GlobalVariables.refreshRateSetting.ToString()}");
                        LoggingHelper.DebugLog(
                            $"Target Frame Rate: {Application.targetFrameRate}");
                    }
                }
                
                GlobalVariables.saveManagerScript.SaveOptions();
                
                return false; // Skip original function.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(ScreenResolutions), "SetDropdownResolution", typeof(int),
            typeof(int), typeof(int))]
        public static class SetDropdownResolutionPatch
        {
            private static readonly FieldInfo ResolutionsField = typeof(ScreenResolutions).GetField("resolutions",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            private static readonly FieldInfo JustStartedField = typeof(ScreenResolutions).GetField("justStarted",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            
            /// <summary>
            /// SetDropdownResolution start patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="width">Width of the screen.</param>
            /// <param name="height">Height of the screen.</param>
            /// <param name="refresh">Refresh rate of the screen.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ScreenResolutions __instance, ref int width, ref int height, ref int refresh)
            {
                if (ResolutionsField == null || JustStartedField == null)
                {
                    LoggingHelper.ReflectionError(nameof(ResolutionsField),
                        nameof(JustStartedField));
                    return true;
                }
                
                Resolution[] resolutions = (Resolution[]) ResolutionsField.GetValue(__instance);
                
                // OLD: ScreenResolutions.justStarted
                if ((bool) JustStartedField.GetValue(__instance)) 
                {
                    return false;
                }
                
                // Main game
                if (!CustomCampaignGlobal.InCustomCampaign) 
                {
                    for (int index = 0; index < resolutions.Length; ++index)
                    {
                        Resolution resolution = resolutions[index];
                        
                        if (resolution.width == width 
                            && resolution.height == height
                            && resolution.refreshRate == refresh)
                        {
                            __instance.dropdownMenu.value = index;
                            
                            GlobalVariables.refreshRateSetting = refresh;
                            GlobalVariables.screenHeightSetting = height;
                            GlobalVariables.screenWidthSetting = width;
                            
                            break;
                        }
                    }
                }
                // Custom Campaign
                else 
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }
                    
                    for (int index = 0; index < resolutions.Length; ++index)
                    {
                        Resolution resolution = resolutions[index];
                        
                        if (resolution.width == width 
                            && resolution.height == height
                            && resolution.refreshRate == refresh)
                        {
                            __instance.dropdownMenu.value = index;
                            
                            customCampaign.SavedRefreshRate = refresh;
                            customCampaign.SavedScreenHeight = height;
                            customCampaign.SavedScreenWidth = width;
                            
                            break;
                        }
                    }
                }
                
                __instance.SaveResolutionInfo();
                
                return false; // Skip original function.
            }
        }
    }
}