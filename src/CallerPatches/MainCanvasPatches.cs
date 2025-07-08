using System;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;

namespace NewSafetyHelp.CallerPatches
{
    public static class MainCanvasPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "WriteDayString", new Type[] { })]
        public static class ScorecardPatch
        {

            /// <summary>
            /// Patches the main canvas day string function to use custom day strings.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, MainCanvasBehavior __instance, string __result)
            {
                
                List<string> defaultDayNames = new List<string>() {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
                
                if (!GlobalVariables.isXmasDLC)
                {
                    if (GlobalVariables.arcadeMode)
                    {
                        __result = "Arcade Mode";
                    }

                    __result = defaultDayNames[GlobalVariables.currentDay];

                }
                else if (GlobalVariables.isXmasDLC)
                {
                    switch (GlobalVariables.currentDay)
                    {
                        case 1:
                            __result = "3 Days Until Christmas";
                            break;
                        
                        case 2:
                            __result = "2 Days Until Christmas";
                            break;
                        
                        case 3:
                            __result = "1 Day Until Christmas";
                            break;
                        
                        case 4:
                            __result = "Christmas Day";
                            break;
                    }
                }
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign Values
                {
                    
                    CustomCampaignExtraInfo currentCustomCampaign = CustomCampaignGlobal.customCampaignsAvailable.Find(scannedCampaign => scannedCampaign.campaignName == CustomCampaignGlobal.currentCustomCampaignName);

                    if (currentCustomCampaign != null)
                    {
                        
                        if (currentCustomCampaign.campaignDayStrings.Count > 0)
                        {

                            if (GlobalVariables.currentDay > currentCustomCampaign.campaignDayStrings.Count || currentCustomCampaign.campaignDays > currentCustomCampaign.campaignDayStrings.Count)
                            {
                                MelonLogger.Warning("WARNING: Amount of day strings does not correspond with the max amount of days for the custom campaign. Using default values. ");
                                __result = defaultDayNames[GlobalVariables.currentDay];
                            }
                            else
                            {
                                __result = currentCustomCampaign.campaignDayStrings[GlobalVariables.currentDay];
                            }
                        }
                        else
                        {
                            __result = defaultDayNames[GlobalVariables.currentDay];
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Was unable of finding the current campaign. Defaulting to default values.");
                        
                        __result = defaultDayNames[GlobalVariables.currentDay];
                    }
                    
                }
                else
                {
                    __result = "Default";
                }
                
                return false; // Skip function with false.
            }
        }
    }
}