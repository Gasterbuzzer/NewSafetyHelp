using MelonLoader;
using NewSafetyHelp.Audio.Music.Intermission;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.CallerPatches.IncomingCallWindow
{
    public static class CallWindowStartPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "OnEnable")]
        public static class OnEnablePatch
        {
            /// <summary>
            /// Patches the OnEnable to consider custom Campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(CallWindowBehavior __instance)
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
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return true;
                    }
                    
                    if (GlobalVariables.callerControllerScript.currentCallerID + 1 <= GlobalVariables.callerControllerScript.callers.Length)
                    {
                        CallerModel.CustomCCaller customCCaller = CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables.callerControllerScript.currentCallerID + 1);
                        
                        if (customCCaller == null)
                        {
                            LoggingHelper.ErrorLog("Custom campaign caller was null. Unable of checking for downed network parameter." +
                                                   " Calling original function.");
                            return true;
                        }
                        
                        // In case the intermission music is playing, we stop it.
                        if (CustomCampaignGlobal.InCustomCampaign)
                        {
                            MelonCoroutines.Start(IntermissionMusicHelper.StopIntermissionMusic());
                        }
                        
                        if (!GlobalVariables.isXmasDLC && customCCaller.DownedNetworkCaller)
                        {

                            LoggingHelper.DebugLog("Custom caller is set to play warped phone call sound" +
                                                   $" (INFO: Downed Network? {customCCaller.DownedNetworkCaller};" +
                                                   $" Caller Name: {customCCaller.CallerName}" +
                                                   ").");
                            
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