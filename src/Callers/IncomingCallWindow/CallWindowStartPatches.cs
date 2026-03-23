using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.Audio.AudioPatches;
using NewSafetyHelp.Audio.Music.Intermission;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CustomRingtone;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.Callers.IncomingCallWindow
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
                        return true;
                    }

                    // Ringtone
                    if (customCampaign.CustomRingtones != null 
                        && customCampaign.CustomRingtones.Count > 0)
                    {
                        List<CustomRingtone> validRingtonesNormal = new List<CustomRingtone>();
                        List<CustomRingtone> validRingtonesGlitched = new List<CustomRingtone>();

                        // For each ringtone that is valid for this current day, attempt to find all valid.
                        foreach (CustomRingtone customRingtone in customCampaign.CustomRingtones.Where(c => c.UnlockDay <= GlobalVariables.currentDay))
                        {
                            // If we are only allowed to play on the unlock day.
                            // Then the unlock day must be equal to the current day.
                            
                            if (customRingtone.OnlyOnUnlockDay 
                                && customRingtone.UnlockDay != GlobalVariables.currentDay)
                            {
                                continue;
                            }
                            
                            if (customRingtone.IsGlitchedVersion)
                            {
                                validRingtonesGlitched.Add(customRingtone);
                            }
                            else
                            {
                                validRingtonesNormal.Add(customRingtone);
                            }
                        }
                        
                        // Now for each valid ringtone we try to pick one valid.
                        GlobalVariables.UISoundControllerScript.phoneCall = RingtoneHelper.ReplacePhoneRingtoneIfValid(ref validRingtonesNormal,
                            customCampaign.DoNotAccountDefaultRingtone, ref UISoundPatch.StartPatch.DefaultRingtone);

                        GlobalVariables.UISoundControllerScript.phoneCallWarped = RingtoneHelper.ReplacePhoneRingtoneIfValid(ref validRingtonesGlitched,
                            customCampaign.DoNotAccountDefaultRingtone, ref UISoundPatch.StartPatch.DefaultWarpedRingtone);
                    }

                    if (GlobalVariables.callerControllerScript.currentCallerID + 1 <=
                        GlobalVariables.callerControllerScript.callers.Length)
                    {
                        CustomCCaller customCCaller =
                            CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(
                                GlobalVariables.callerControllerScript.currentCallerID + 1);

                        if (customCCaller == null)
                        {
                            LoggingHelper.ErrorLog(
                                "Custom campaign caller was null. Unable of checking for downed network parameter." +
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