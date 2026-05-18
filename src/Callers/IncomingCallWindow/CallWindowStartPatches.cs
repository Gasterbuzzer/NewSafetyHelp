using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.AudioPatches;
using NewSafetyHelp.Audio.Music.Intermission;
using NewSafetyHelp.Callers.CallerHelpers;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CustomRingtone;
using NewSafetyHelp.CustomCampaignSystem.TimedCaller;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.Callers.IncomingCallWindow
{
    public static class CallWindowStartPatches
    {
        private static readonly FieldInfo FirstCaller =
            typeof(CallerController).GetField("firstCaller", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyLib.HarmonyPatch(typeof(CallWindowBehavior), "OnEnable")]
        public static class OnEnablePatch
        {
            /// <summary>
            /// Patches the OnEnable to consider custom Campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(CallWindowBehavior __instance)
            {
                __instance.answerButton.SetActive(true);
                __instance.loadingText.SetActive(false);

                // Main Campaign
                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.callerControllerScript.currentCallerID + 1 <=
                        GlobalVariables.callerControllerScript.callers.Length)
                    {
                        foreach (int downedNetworkCall in GlobalVariables.callerControllerScript.downedNetworkCalls)
                        {
                            if (downedNetworkCall == GlobalVariables.callerControllerScript.currentCallerID + 1)
                            {
                                if (!GlobalVariables.isXmasDLC)
                                {
                                    GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                        .UISoundControllerScript.phoneCallWarped);
                                    return false;
                                }

                                GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                    .UISoundControllerScript.xmasPhoneCallWarped);
                                return false;
                            }
                        }

                        if (!GlobalVariables.isXmasDLC)
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                .UISoundControllerScript.phoneCall);
                        }
                        else
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                .UISoundControllerScript.xmasPhoneCall);
                        }
                    }
                    else if (!GlobalVariables.isXmasDLC)
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                            .UISoundControllerScript
                            .phoneCall);
                    }
                    else
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                            .UISoundControllerScript
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
                        foreach (CustomRingtone customRingtone in customCampaign.CustomRingtones.Where(c =>
                                     c.UnlockDay <= GlobalVariables.currentDay))
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
                        GlobalVariables.UISoundControllerScript.phoneCall = RingtoneHelper.ReplacePhoneRingtoneIfValid(
                            ref validRingtonesNormal,
                            customCampaign.DoNotAccountDefaultRingtone, ref UISoundPatch.StartPatch.DefaultRingtone);

                        GlobalVariables.UISoundControllerScript.phoneCallWarped =
                            RingtoneHelper.ReplacePhoneRingtoneIfValid(ref validRingtonesGlitched,
                                customCampaign.DoNotAccountDefaultRingtone,
                                ref UISoundPatch.StartPatch.DefaultWarpedRingtone);
                    }

                    if (GlobalVariables.callerControllerScript.currentCallerID + 1 <=
                        GlobalVariables.callerControllerScript.callers.Length)
                    {
                        int currentCallerID = GlobalVariables.callerControllerScript.currentCallerID;
                        int checkResult =
                            CallerSkipping.GetCallersSkippedAmount(GlobalVariables.callerControllerScript);

                        int callersLookedAhead = 1;

                        if (checkResult > 0)
                        {
                            callersLookedAhead = checkResult + 1;
                        }

                        if ((bool)FirstCaller.GetValue(GlobalVariables.callerControllerScript))
                        {
                            LoggingHelper.DebugLog("First caller of the day. Callers ahead will be set to 0.",
                                LoggingHelper.LoggingCategory.RINGTONE);
                            callersLookedAhead = 0;
                        }

                        int callerToBeCalledID = currentCallerID + callersLookedAhead;

                        LoggingHelper.DebugLog(() =>
                                $"Checking the ringtone for the caller with ID: '{callerToBeCalledID}' " +
                                $"Check Result: '{checkResult}'. " +
                                $"(Look ahead '{callersLookedAhead}').",
                            LoggingHelper.LoggingCategory.RINGTONE);

                        CustomCCaller customCCaller =
                            CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(callerToBeCalledID);

                        if (customCCaller == null)
                        {
                            LoggingHelper.ErrorLog(
                                "Custom campaign caller was null. Unable of checking for downed network parameter. " +
                                "Calling original function.");
                            return true;
                        }

                        // In case the intermission music is playing, we stop it.
                        if (CustomCampaignGlobal.InCustomCampaign)
                        {
                            MelonCoroutines.Start(IntermissionMusicHelper.StopIntermissionMusic());
                            TimerCallerHelper.StopTimedCallerTimer();
                        }

                        if (!GlobalVariables.isXmasDLC
                            && customCCaller.DownedNetworkCaller)
                        {
                            LoggingHelper.DebugLog("Custom caller is set to play warped phone call sound " +
                                                   $"(INFO: Downed Network? {customCCaller.DownedNetworkCaller}; " +
                                                   $"Caller Name: {customCCaller.CallerName}).");

                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                .UISoundControllerScript.phoneCallWarped);
                            return false;
                        }

                        if (!GlobalVariables.isXmasDLC)
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                .UISoundControllerScript.phoneCall);
                        }
                        else
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                                .UISoundControllerScript.xmasPhoneCall);
                        }
                    }
                    else if (!GlobalVariables.isXmasDLC)
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                            .UISoundControllerScript
                            .phoneCall);
                    }
                    else
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables
                            .UISoundControllerScript
                            .xmasPhoneCall);
                    }
                }

                return false; // Skip function with false.
            }
        }
    }
}