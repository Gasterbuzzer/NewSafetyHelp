using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using TMPro;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CustomDesktop
{
    public static class CustomDesktop
    {
        
        [HarmonyLib.HarmonyPatch(typeof(MainMenuCanvasBehavior), "Start", new Type[] { })]
        public static class StartPatch
        {

            /// <summary>
            /// Hooks into the Main Menu Canvas Start function to add our own logic after wards.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, MainMenuCanvasBehavior __instance)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Start of Main Menu Canvas Behavior.");
                #endif

                // If in custom campaign, we replace it with custom text.
                if (CustomCampaignGlobal.inCustomCampaign)
                {

                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();
                    
                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null! Unable of replacing loading screen texts. Calling original function.");
                        return true;
                    }

                    if (customCampaign.loadingTexts[0].Count > 0 && !string.IsNullOrEmpty(customCampaign.loadingTexts[0][0]))
                    {
                        __instance.loginText.GetComponent<TextMeshProUGUI>().text = customCampaign.loadingTexts[0][0];

                        // Set animated texts to provided texts. (Even if just 1)
                        AnimatedText loginText01 = __instance.loginText.GetComponent<AnimatedText>();
                            
                        loginText01.textFrames = new string[customCampaign.loadingTexts[0].Count];

                        for (int i = 0; i < customCampaign.loadingTexts[0].Count; i++)
                        {
                            loginText01.textFrames[i] = customCampaign.loadingTexts[0][i];
                        }
                    }
                    
                    if (customCampaign.loadingTexts[1].Count > 0 && !string.IsNullOrEmpty(customCampaign.loadingTexts[1][0]))
                    {
                        __instance.loginText2.GetComponent<TextMeshProUGUI>().text = customCampaign.loadingTexts[1][0];
                        
                        // Set animated texts to provided texts. (Even if just 1)
                        AnimatedText loginText02 = __instance.loginText2.GetComponent<AnimatedText>();
                        
                        loginText02.textFrames = new string[customCampaign.loadingTexts[1].Count];

                        for (int i = 0; i < customCampaign.loadingTexts[1].Count; i++)
                        {
                            loginText02.textFrames[i] = customCampaign.loadingTexts[1][i];
                        }
                    }
                }

                // Plays beginning segment to desktop.
                MelonCoroutines.Start(StartupRoutine(__instance));

                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    foreach (CustomCampaignExtraInfo customCampaign in CustomCampaignGlobal.customCampaignsAvailable)
                    {
                        CustomDesktopHelper.createCustomProgramIcon(customCampaign.campaignDesktopName, customCampaign.campaignName, customCampaign.campaignIcon);
                    }
                    
                    // Enable DLC Button if DLC is installed.
                    // Hide DLC Button
                    CustomDesktopHelper.enableWinterDLCProgram();
                }
                else
                {
                    CustomDesktopHelper.createBackToMainGameButton();
                    
                    // Hide DLC Button
                    CustomDesktopHelper.disableWinterDLCProgram();
                }

                return false; // Skip original function.
            }


            public static IEnumerator StartupRoutine(MainMenuCanvasBehavior __instance)
            {
                while (GlobalVariables.UISoundControllerScript ==null)
                {
                    yield return null;
                }
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.computerStartup);
                
                yield return new WaitForSeconds(1.3f);
                
                __instance.loginText.SetActive(true);
                
                yield return new WaitForSeconds(2f);
                
                GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.computerFanSpin, GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource);
                
                __instance.loginText2.SetActive(true);
                
                yield return new WaitForSeconds(3f);
                
                __instance.loginText.SetActive(false);
                __instance.loginText2.SetActive(false);
                
                GlobalVariables.fade.FadeOut(0.0001f);
                
                yield return new WaitForSeconds(0.1f);
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.connectionSuccess);
            }
        }
    }
}