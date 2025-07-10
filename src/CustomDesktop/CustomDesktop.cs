using System;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using UnityEngine;

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
            private static void Postfix(MethodBase __originalMethod, MainMenuCanvasBehavior __instance)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Start of Main Menu Canvas Behavior.");
                #endif

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
            }
        }
    }
}