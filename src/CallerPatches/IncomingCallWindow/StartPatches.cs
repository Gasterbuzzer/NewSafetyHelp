using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public static class StartPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "OnEnable")]
        public static class OnEnablePatch
        {
            /// <summary>
            /// Patches the OnEnable to consider custom Campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, CallWindowBehavior __instance)
            {
                __instance.answerButton.SetActive(true);
                __instance.loadingText.SetActive(false);

                if (!CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
                    if (GlobalVariables.callerControllerScript.currentCallerID + 1 <= GlobalVariables.callerControllerScript.callers.Length)
                    {
                        foreach (int downedNetworkCall in GlobalVariables.callerControllerScript.downedNetworkCalls)
                        {
                            if (downedNetworkCall == GlobalVariables.callerControllerScript.currentCallerID + 1)
                            {
                                if (!GlobalVariables.isXmasDLC)
                                {
                                    GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.phoneCallWarped);
                                    return false;
                                }
                            
                                GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.xmasPhoneCallWarped);
                                return false;
                            }
                        }
                    
                        if (!GlobalVariables.isXmasDLC)
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.phoneCall);
                        }
                        else
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.xmasPhoneCall);
                        }
                    }
                    else if (!GlobalVariables.isXmasDLC)
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript
                            .phoneCall);
                    }
                    else
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript
                            .xmasPhoneCall);
                    }
                }
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null. Calling original function.");
                        return true;
                    }
                    
                    if (GlobalVariables.callerControllerScript.currentCallerID + 1 <= GlobalVariables.callerControllerScript.callers.Length)
                    {
                        CustomCallerExtraInfo customCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID + 1);
                        
                        if (customCaller == null)
                        {
                            MelonLogger.Error("ERROR: Custom campaign caller was null. Unable of checking for downed network parameter. Calling original function.");
                            return true;
                        }
                        
                        if (!GlobalVariables.isXmasDLC && customCaller.downedNetworkCaller)
                        {

                            #if DEBUG
                                MelonLogger.Msg($"DEBUG: Custom caller is set to play warped phone call sound" +
                                                $" (INFO: Downed Network? {customCaller.downedNetworkCaller};" +
                                                $" Caller Name: {customCaller.callerName}" +
                                                ").");
                            #endif
                            
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.phoneCallWarped);
                            return false;
                        }
                    
                        if (!GlobalVariables.isXmasDLC)
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.phoneCall);
                        }
                        else
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.xmasPhoneCall);
                        }
                    }
                    else if (!GlobalVariables.isXmasDLC)
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript
                            .phoneCall);
                    }
                    else
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript
                            .xmasPhoneCall);
                    }
                }
                
                return false; // Skip function with false.
            }
        }
    }
}