using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using TMPro;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignPatches.Saving
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
                        MelonLogger.Error("ERROR: Called custom campaign part while no active custom campaign is available! Calling original function.");
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
            /// <summary>
            /// ScreenResolutions start patch to allow the options to also affect the custom campaign stored values.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(ScreenResolutions __instance)
            {
                FieldInfo resolutionsField = typeof(ScreenResolutions).GetField("resolutions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                MethodInfo resToString = typeof(ScreenResolutions).GetMethod("ResToString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                MethodInfo setSteamDeckResolution = typeof(ScreenResolutions).GetMethod("SetSteamDeckResolution", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo justStarted = typeof(ScreenResolutions).GetField("justStarted", BindingFlags.NonPublic | BindingFlags.Static);

                if (resolutionsField == null || resToString == null || justStarted == null || setSteamDeckResolution == null)
                {
                    MelonLogger.Error("ERROR: " +
                                      "Field 'resolutions' or Method 'ResToString' or Field 'justStarted' or Method 'setSteamDeckResolution' was null. Calling normal function.");
                    return true;
                }
                
                Resolution[] resolutions = (Resolution[]) resolutionsField.GetValue(__instance);
                
                __instance.dropdownMenu.options.Clear();

                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
                {
                    for (int index = 0; index < resolutions.Length; ++index)
                    {
                        // OLD:  __instance.ResToString()
                        string label = (string) resToString.Invoke(__instance, new object[] { resolutions[index] });
                    
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
                        MelonLogger.Error("ERROR: Called custom campaign part while no active custom campaign is available! Calling original function.");
                        return true;
                    }
                    
                    for (int index = 0; index < resolutions.Length; ++index)
                    {
                        string label = (string) resToString.Invoke(__instance, new object[] { resolutions[index] });
                    
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
                
                justStarted.SetValue(__instance, false); // ScreenResolutions.justStarted = false;
                
                setSteamDeckResolution.Invoke(__instance, null); // __instance.SetSteamDeckResolution();
                
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
                        
                        QualitySettings.vSyncCount = 0;
                        Application.targetFrameRate = GlobalVariables.refreshRateSetting;
                        
                        #if DEBUG

                        MelonLogger.Msg(
                            $"DEBUG: Saved Resolution: {GlobalVariables.screenWidthSetting.ToString()}x{GlobalVariables.screenHeightSetting.ToString()} @{GlobalVariables.refreshRateSetting.ToString()}");
                        
                        var cur = Screen.currentResolution;
                        
                        MelonLogger.Msg(
                            "DEBUG: Unity currentResolution: " +
                            $"{cur.width}x{cur.height}@{cur.refreshRate}"
                        );

                        MelonLogger.Msg(
                            $"DEBUG: TargetFrameRate: {Application.targetFrameRate}"
                        );

                        #endif
                    }
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Called custom campaign part while no active custom campaign is available! Calling original function.");
                        return true;
                    }
                    
                    if (customCampaign.SavedScreenWidth >= 480 && customCampaign.SavedScreenHeight >= 480)
                    {
                        Screen.SetResolution(customCampaign.SavedScreenWidth, customCampaign.SavedScreenHeight, 
                            customCampaign.SavedFullScreenToggle, customCampaign.SavedRefreshRate);

                        QualitySettings.vSyncCount = 0;
                        Application.targetFrameRate = customCampaign.SavedRefreshRate;
                        
                        #if DEBUG
                        
                        MelonLogger.Msg(
                            $"DEBUG: Saved Resolution: {customCampaign.SavedScreenWidth}x{customCampaign.SavedScreenHeight} @{customCampaign.SavedRefreshRate}");
                        
                        var cur = Screen.currentResolution;
                        
                        MelonLogger.Msg(
                            "DEBUG: Unity currentResolution: " +
                            $"{cur.width}x{cur.height}@{cur.refreshRate}"
                        );

                        MelonLogger.Msg(
                            $"DEBUG: TargetFrameRate: {Application.targetFrameRate}"
                        );
                
                        #endif
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
                FieldInfo resolutionsField = typeof(ScreenResolutions).GetField("resolutions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                FieldInfo justStartedField = typeof(ScreenResolutions).GetField("justStarted", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                
                if (resolutionsField == null || justStartedField == null)
                {
                    MelonLogger.Error("ERROR: " +
                                      "Field 'resolutions' or Field 'justStarted' was null. Calling normal function.");
                    return true;
                }
                
                Resolution[] resolutions = (Resolution[]) resolutionsField.GetValue(__instance);
                
                if ((bool) justStartedField.GetValue(__instance)) //ScreenResolutions.justStarted
                {
                    return false;
                }
                
                if (!CustomCampaignGlobal.InCustomCampaign) // Main game
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
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Called custom campaign part while no active custom campaign is available! Calling original function.");
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