using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NewSafetyHelp.Audio.Music.Intermission;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.CustomDesktop;
using NewSafetyHelp.InGameSettings;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.Callers.UI
{
    public static class MainCanvasPatches
    {
        // Cached animator lookups.
        private static readonly int Glitch = Animator.StringToHash("glitch");

        private static readonly List<string> DefaultDayNames = new List<string>
            { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "WriteDayString")]
        public static class WriteDayStringPatch
        {
            /// <summary>
            /// Patches the main canvas day string function to use custom day strings.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param> 
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref string __result)
            {
                if (!GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign)
                {
                    if (GlobalVariables.arcadeMode)
                    {
                        __result = "Arcade Mode";
                    }

                    __result = DefaultDayNames[GlobalVariables.currentDay - 1];
                }
                else if (GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign)
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
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign Values
                {
                    CustomCampaign currentCustomCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (currentCustomCampaign != null)
                    {
                        string dayString;

                        // Campaign find campaign.
                        if (currentCustomCampaign.CampaignDayStrings.Count > 0)
                        {
                            if (GlobalVariables.currentDay > currentCustomCampaign.CampaignDayStrings.Count
                                || currentCustomCampaign.CampaignDays > currentCustomCampaign.CampaignDayStrings.Count)
                            {
                                LoggingHelper.WarningLog(
                                    "Amount of day strings does not correspond with the max amount of days for the custom campaign." +
                                    " Using default values.");
                                dayString = DefaultDayNames[(GlobalVariables.currentDay - 1) % DefaultDayNames.Count];
                            }
                            else
                            {
                                dayString = currentCustomCampaign.CampaignDayStrings[
                                    (GlobalVariables.currentDay - 1) % currentCustomCampaign.CampaignDayStrings.Count];
                            }
                        }
                        else
                        {
                            dayString = DefaultDayNames[(GlobalVariables.currentDay - 1) % DefaultDayNames.Count];
                        }

                        (bool foundModifier, List<string> value) daysStrings =
                            CustomCampaignGlobal.GetActiveModifierValue(
                                c => c.DayTitleStrings,
                                v => v != null && v.Count > 0);

                        (bool foundModifier, List<int> value) unlockDays = CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.UnlockDays,
                            v => v != null && v.Count > 0);

                        // Modifier
                        if (daysStrings.foundModifier)
                        {
                            // If conditional days, but we don't have enough day strings for amount of unlocked days.
                            // (And only if the campaign didn't provide one)
                            if (unlockDays.value != null
                                && daysStrings.value.Count != unlockDays.value.Count
                                && string.IsNullOrEmpty(dayString))
                            {
                                LoggingHelper.WarningLog(
                                    "Amount of day strings does not correspond with the max amount of days for the custom campaign." +
                                    " Using default values.");
                                dayString = DefaultDayNames[(GlobalVariables.currentDay - 1) % DefaultDayNames.Count];
                            }
                            else
                            {
                                // General Days, we simply display what we can.
                                if (unlockDays.value == null)
                                {
                                    if (currentCustomCampaign.CampaignDays > daysStrings.value.Count)
                                    {
                                        LoggingHelper.WarningLog(
                                            "Amount of day strings does not correspond with the max amount of days for the custom campaign." +
                                            " Using modulated values.");
                                    }

                                    // We simply pick what best fits.
                                    dayString = daysStrings.value[
                                        (GlobalVariables.currentDay - 1) %
                                        daysStrings.value.Count];
                                }
                                else // Not General (Conditional Modifier)
                                {
                                    // If we don't have enough to show.
                                    if (daysStrings.value.Count != unlockDays.value.Count)
                                    {
                                        LoggingHelper.WarningLog(
                                            "Amount of day strings does not correspond with the max amount of days for the custom campaign." +
                                            " Using modulated values.");
                                        dayString = daysStrings.value[
                                            (GlobalVariables.currentDay - 1) % daysStrings.value.Count];
                                    }
                                    else // We do have enough days.
                                    {
                                        int indexUnlockDay = unlockDays.value.IndexOf(GlobalVariables.currentDay);

                                        if (indexUnlockDay != -1)
                                        {
                                            dayString = daysStrings.value[indexUnlockDay];
                                        }
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(dayString)) // If empty, we provide a default one.
                        {
                            dayString = DefaultDayNames[(GlobalVariables.currentDay - 1) % DefaultDayNames.Count];
                        }

                        if (!string.IsNullOrEmpty(
                                dayString)) // Update if not empty. It should if nothing went wrong always work.
                        {
                            __result = dayString;
                        }
                    }
                    else
                    {
                        LoggingHelper.WarningLog("Was unable of finding the current campaign." +
                                                 " Defaulting to default values.");

                        __result = DefaultDayNames[GlobalVariables.currentDay - 1];
                    }
                }
                else
                {
                    __result = "Default";
                }

                return false; // Skip function with false.
            }
        }


        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "StartSoftwareRoutine")]
        public static class SoftwareRoutinePatches
        {
            private static readonly MethodInfo LoadVarsMethod = typeof(MainCanvasBehavior).GetMethod("LoadVars",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static readonly MethodInfo PopulateEntriesListMethod = typeof(MainCanvasBehavior).GetMethod(
                "PopulateEntriesList",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static readonly MethodInfo WriteDayStringMethod = typeof(MainCanvasBehavior).GetMethod(
                "WriteDayString",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static Coroutine clockInAnimationCoroutine;

            /// <summary>
            /// Patches start software routine to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine of function to be called after wards </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                EndDayRoutinePatch.IsDayEnding = false; // Reset it, if not reset yet.

                __result = StartSoftwareRoutine(__instance);

                return false; // Skip function with false.
            }

            private static IEnumerator StartSoftwareRoutine(MainCanvasBehavior __instance)
            {
                if (LoadVarsMethod == null || PopulateEntriesListMethod == null || WriteDayStringMethod == null)
                {
                    LoggingHelper.ReflectionError(nameof(LoadVarsMethod),
                        nameof(PopulateEntriesListMethod), nameof(WriteDayStringMethod));
                    yield break;
                }

                MainCanvasBehavior mainCanvasBehavior = __instance;

                yield return null;

                LoadVarsMethod.Invoke(mainCanvasBehavior, null);
                PopulateEntriesListMethod.Invoke(mainCanvasBehavior, null);

                if (!GlobalVariables.arcadeMode && GlobalVariables.currentDay == 7
                                                && !CustomCampaignGlobal.InCustomCampaign)
                {
                    mainCanvasBehavior.trialScreen.SetActive(true);
                    mainCanvasBehavior.postProcessVolume.profile = mainCanvasBehavior.scaryProcessProfile;
                }
                else if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign Last Day
                {
                    // Currently just skips it.
                }

                if (GlobalVariables.isXmasDLC && (bool)(Object)GlobalVariables.cheerMeterScript)
                {
                    GlobalVariables.cheerMeterScript.UpdateMeterVisuals();
                }

                GlobalVariables.introIsPlaying = true;
                mainCanvasBehavior.clockedIn = false;
                GlobalVariables.callerControllerScript.callersToday = 0;
                GlobalVariables.callerControllerScript.correctCallsToday = 0;

                // So that the accuracy caller knows where to start.
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    AccuracyCallerHelper.StartOfDayCallerID = GlobalVariables.callerControllerScript.currentCallerID;

                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        yield break;
                    }

                    (bool foundModifier, VariableChanged<Sprite> value) inGameProgramIconVC =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.InGameProgramIcon,
                            vCs => vCs.HasChanged);

                    (bool foundModifier, VariableChanged<bool> value) inGameProgramIconCenter =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.InGameProgramIconCenter,
                            vCs => vCs.HasChanged);

                    GameObject inGameProgramIcon = GameObject.Find("MainCanvas/Panel/WindowsBar/ProgramLogo");

                    if (inGameProgramIcon == null)
                    {
                        LoggingHelper.ErrorLog("Could not find Program Logo to change.");
                        yield break;
                    }

                    if (inGameProgramIconVC.foundModifier)
                    {
                        inGameProgramIcon.GetComponent<Image>().sprite = inGameProgramIconVC.value.Data;
                    }

                    if (inGameProgramIconCenter.foundModifier && inGameProgramIconCenter.value.Data)
                    {
                        inGameProgramIcon.GetComponent<RectTransform>().pivot = new Vector2(0.75f, 0.75f);
                    }

                    (bool foundModifier, VariableChanged<Sprite> value) inGamePhoneIcon =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.InGamePhoneIcon,
                            vCs => vCs.HasChanged);

                    (bool foundModifier, VariableChanged<bool> value) inGamePhoneIconCenter =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.InGamePhoneIconCenter,
                            vCs => vCs.HasChanged);

                    GameObject mainCanvas = GameObject.Find("MainCanvas");

                    if (inGamePhoneIconCenter.foundModifier)
                    {
                        // MainCanvas/CallPopup/WindowsBar/ProgramLogo
                        mainCanvas.transform.GetChild(3).GetChild(0).GetChild(2).GetComponent<RectTransform>().pivot =
                            new Vector2(0.75f, 0.75f);
                    }

                    if (inGamePhoneIcon.foundModifier)
                    {
                        // MainCanvas/CallPopup/WindowsBar/ProgramLogo
                        mainCanvas.transform.GetChild(3).GetChild(0).GetChild(2).GetComponent<Image>().sprite =
                            inGamePhoneIcon.value.Data;

                        // MainCanvas/CallPopup/IncomingCall/Image
                        mainCanvas.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().sprite =
                            inGamePhoneIcon.value.Data;
                    }

                    (bool foundModifier, VariableChanged<string> value) incomingCallTitle =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.IncomingCallTitle,
                            vCs => vCs.HasChanged);

                    if (incomingCallTitle.foundModifier)
                    {
                        // MainCanvas/CallPopup/WindowsBar/ProgramTitle
                        GameObject.Find("MainCanvas").transform.GetChild(3).GetChild(0).GetChild(3)
                            .GetComponent<TextMeshProUGUI>().text = incomingCallTitle.value.Data;
                    }

                    (bool foundModifier, VariableChanged<string> value) incomingCallLabel =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.IncomingCallLabel,
                            vCs => vCs.HasChanged);

                    if (incomingCallLabel.foundModifier)
                    {
                        // MainCanvas/CallPopup/IncomingCall/IncomingText
                        GameObject.Find("MainCanvas").transform.GetChild(3).GetChild(1).GetChild(1)
                            .GetComponent<TextMeshProUGUI>().text = incomingCallLabel.value.Data;
                    }

                    (bool foundModifier, VariableChanged<string> value) incomingCallAnswerButtonText =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.IncomingCallAnswerButtonText,
                            vCs => vCs.HasChanged);

                    if (incomingCallAnswerButtonText.foundModifier)
                    {
                        // MainCanvas/CallPopup/IncomingCall/AnswerButton/Text (TMP)
                        GameObject.Find("MainCanvas").transform.GetChild(3).GetChild(1).GetChild(2).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = incomingCallAnswerButtonText.value.Data;
                    }

                    (bool foundModifier, VariableChanged<Sprite> value) incomingCallAnswerButtonImage =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.IncomingCallAnswerButtonImage,
                            vCs => vCs.HasChanged);

                    if (incomingCallAnswerButtonImage.foundModifier)
                    {
                        // MainCanvas/CallPopup/IncomingCall/AnswerButton/Image
                        GameObject.Find("MainCanvas").transform.GetChild(3).GetChild(1).GetChild(2).GetChild(1)
                            .GetComponent<Image>().sprite = incomingCallAnswerButtonImage.value.Data;
                    }

                    (bool foundModifier, VariableChanged<RichAudioClip> value) inGameLogoFadeInAudio =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.InGameLogoFadeInAudio,
                            vCs => vCs.HasChanged);

                    if (inGameLogoFadeInAudio.foundModifier)
                    {
                        // MainCanvas/Panel/SoftwareIntroPanel/LogoAnimation

                        AudioSource logoAudioSource = GameObject.Find("MainCanvas").transform.GetChild(0).GetChild(12)
                            .GetChild(1)
                            .GetComponent<AudioSource>();

                        logoAudioSource.clip = inGameLogoFadeInAudio.value.Data.clip;
                        logoAudioSource.volume = inGameLogoFadeInAudio.value.Data.volume;
                    }

                    (bool foundModifier, VariableChanged<string> value) submitWindowTitle =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.SubmitWindowTitle,
                            vCs => vCs.HasChanged);

                    if (submitWindowTitle.foundModifier)
                    {
                        // MainCanvas/SubmitAnswerPopup/WindowsBar/ProgramTitle
                        GameObject.Find("MainCanvas").transform.GetChild(2).GetChild(0).GetChild(3)
                            .GetComponent<TextMeshProUGUI>().text = submitWindowTitle.value.Data;
                    }

                    (bool foundModifier, VariableChanged<string> value) submitWindowText =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.SubmitWindowText,
                            vCs => vCs.HasChanged);

                    if (submitWindowText.foundModifier)
                    {
                        // MainCanvas/SubmitAnswerPopup/Text (TMP)
                        GameObject.Find("MainCanvas").transform.GetChild(2).GetChild(1)
                            .GetComponent<TextMeshProUGUI>().text = submitWindowText.value.Data;
                    }

                    (bool foundModifier, VariableChanged<Sprite> value) submitWindowIcon =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.SubmitWindowIcon,
                            vCs => vCs.HasChanged);

                    if (submitWindowIcon.foundModifier)
                    {
                        // MainCanvas/SubmitAnswerPopup/WindowsBar/ProgramLogo
                        GameObject.Find("MainCanvas").transform.GetChild(2).GetChild(0).GetChild(2)
                            .GetComponent<Image>().sprite = submitWindowIcon.value.Data;
                    }

                    // Change Animation

                    (bool foundModifier, VariableChanged<List<Sprite>> value) clockInLogoAnimation =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInLogoAnimation,
                            vCs => vCs.HasChanged);

                    if (clockInLogoAnimation.foundModifier)
                    {
                        GameObject logoAnimationGO =
                            GameObject.Find("MainCanvas/Panel/SoftwareIntroPanel/LogoAnimation");

                        logoAnimationGO.GetComponent<Animator>().enabled = false;
                    }

                    (bool foundModifier, VariableChanged<List<Sprite>> value) clockInAnimation =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInAnimation,
                            vCs => vCs.HasChanged);

                    (bool foundModifier, VariableChanged<float> value) clockInAnimationDuration =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInAnimationDuration,
                            vCs => vCs.HasChanged);

                    (bool foundModifier, VariableChanged<float> value) clockInAnimationScale =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInAnimationScale,
                            vCs => vCs.HasChanged);

                    if (clockInAnimation.foundModifier)
                    {
                        GameObject clockInAnimationGO =
                            GameObject.Find("MainCanvas/Panel").transform.GetChild(12).GetChild(0).gameObject;

                        clockInAnimationGO.GetComponent<Animator>().enabled = false;

                        float animationDuration = 2.25f;

                        if (clockInAnimationDuration.foundModifier)
                        {
                            animationDuration = clockInAnimationDuration.value.Data;
                        }

                        Image clockInAnimationImage = clockInAnimationGO.GetComponent<Image>();

                        clockInAnimationImage.preserveAspect = true;

                        clockInAnimationCoroutine = __instance.StartCoroutine(
                            CustomClockInAnimation(clockInAnimationImage, clockInAnimation.value.Data,
                                animationDuration));
                    }

                    if (clockInAnimationScale.foundModifier)
                    {
                        GameObject clockInAnimationGO =
                            GameObject.Find("MainCanvas/Panel").transform.GetChild(12).GetChild(0).gameObject;

                        RectTransform clockInAnimationRectTransform = clockInAnimationGO.GetComponent<RectTransform>();

                        clockInAnimationRectTransform.sizeDelta *= clockInAnimationScale.value.Data;
                    }
                }

                if (!GlobalVariables.arcadeMode)
                {
                    GlobalVariables.fade.FadeIn(1f, (string)WriteDayStringMethod.Invoke(mainCanvasBehavior, null));
                }
                else
                {
                    GlobalVariables.fade.FadeIn(1f);
                    mainCanvasBehavior.arcadeStartPanel.SetActive(true);
                    GlobalVariables.fade.FadeOut(1f);
                }

                if (GlobalPreferences.SkipDayClockIn.Value)
                {
                    GlobalVariables.fade.FadeIn(1f);

                    mainCanvasBehavior.clockInPanel.SetActive(false);
                    mainCanvasBehavior.clockOutElements.SetActive(false);
                    mainCanvasBehavior.clockInElements.SetActive(false);
                    mainCanvasBehavior.clockInButton.SetActive(false);

                    GlobalVariables.fade.FadeOut(1f);
                }

                if (!GlobalVariables.arcadeMode)
                {
                    if (!GlobalPreferences.SkipDayClockIn.Value)
                    {
                        yield return new WaitForSeconds(6f);

                        mainCanvasBehavior.softwareStartupPanel.SetActive(true);
                        mainCanvasBehavior.clockInPanel.SetActive(false);
                        mainCanvasBehavior.logoPanel.SetActive(false);

                        GlobalVariables.fade.FadeOut(1f);

                        yield return new WaitForSeconds(1f);

                        mainCanvasBehavior.logoPanel.SetActive(true);
                        mainCanvasBehavior.StartCoroutine(GlobalVariables.UISoundControllerScript.FadeInLoopingSound(
                            GlobalVariables.UISoundControllerScript.computerFanSpin,
                            GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource));

                        if (CustomCampaignGlobal.InCustomCampaign)
                        {
                            (bool foundModifier, VariableChanged<List<Sprite>> value) clockInLogoAnimation =
                                CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInLogoAnimation,
                                    vCs => vCs.HasChanged);

                            (bool foundModifier, VariableChanged<float> value) clockInLogoAnimationScale =
                                CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInLogoAnimationScale,
                                    vCs => vCs.HasChanged);

                            (bool foundModifier, VariableChanged<float> value) clockInLogoAnimationFadeDuration =
                                CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInLogoAnimationFadeDuration,
                                    vCs => vCs.HasChanged);

                            (bool foundModifier, VariableChanged<float> value) clockInLogoAnimationHoldDuration =
                                CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockInLogoAnimationHoldDuration,
                                    vCs => vCs.HasChanged);


                            float totalFadeInOutDuration = 1.82f;
                            float totalHoldFrameDuration = 1.42f;

                            GameObject logoAnimationGameObject =
                                GameObject.Find("MainCanvas/Panel/SoftwareIntroPanel/LogoAnimation");

                            Image logoAnimationImageComponent = logoAnimationGameObject.GetComponent<Image>();

                            if (clockInLogoAnimationFadeDuration.foundModifier)
                            {
                                totalFadeInOutDuration = clockInLogoAnimationFadeDuration.value.Data;
                            }

                            if (clockInLogoAnimationHoldDuration.foundModifier)
                            {
                                totalHoldFrameDuration = clockInLogoAnimationHoldDuration.value.Data;
                            }

                            if (clockInLogoAnimationScale.foundModifier)
                            {
                                logoAnimationGameObject.GetComponent<RectTransform>().localScale =
                                    new Vector3(clockInLogoAnimationScale.value.Data,
                                        clockInLogoAnimationScale.value.Data, clockInLogoAnimationScale.value.Data);
                            }

                            if (clockInLogoAnimation.foundModifier)
                            {
                                int frameAmount = clockInLogoAnimation.value.Data.Count;

                                if (frameAmount > 0)
                                {
                                    float frameDuration = totalFadeInOutDuration / frameAmount;

                                    logoAnimationImageComponent.sprite = clockInLogoAnimation.value.Data[0];

                                    for (int i = 0; i < frameAmount; i++)
                                    {
                                        logoAnimationImageComponent.sprite = clockInLogoAnimation.value.Data[i];

                                        yield return new WaitForSeconds(frameDuration);
                                    }

                                    yield return new WaitForSeconds(totalHoldFrameDuration);

                                    for (int i = frameAmount - 1; i >= 0; i--)
                                    {
                                        logoAnimationImageComponent.sprite = clockInLogoAnimation.value.Data[i];

                                        yield return new WaitForSeconds(frameDuration);
                                    }
                                }
                            }
                            else
                            {
                                yield return new WaitForSeconds(6f);
                            }
                        }
                        else
                        {
                            yield return new WaitForSeconds(6f);
                        }

                        if (CustomCampaignGlobal.InCustomCampaign)
                        {
                            (bool foundModifier, VariableChanged<RichAudioClip> value) clockDayStartedAudio =
                                CustomCampaignGlobal.GetActiveModifierValue(c => c.ClockDayStartedAudio,
                                    vCs => vCs.HasChanged);

                            if (clockDayStartedAudio.foundModifier)
                            {
                                GlobalVariables.UISoundControllerScript.PlayUISound(clockDayStartedAudio.value.Data);
                            }
                            else
                            {
                                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables
                                    .UISoundControllerScript
                                    .correctSound);
                            }
                        }
                        else
                        {
                            GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript
                                .correctSound);
                        }


                        if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign)
                        {
                            mainCanvasBehavior.cameraAnimator.SetTrigger(Glitch);
                            GlobalVariables.fade.FadeIn();

                            yield return new WaitForSeconds(0.2f);

                            GlobalVariables.fade.FadeOut();
                        }
                        else if (CustomCampaignGlobal.InCustomCampaign) // Just Skip
                        {
                            // Skip
                        }

                        mainCanvasBehavior.logoPanel.SetActive(false);
                        mainCanvasBehavior.clockInPanel.SetActive(true);
                        mainCanvasBehavior.clockOutElements.SetActive(false);
                        mainCanvasBehavior.clockInElements.SetActive(true);
                        mainCanvasBehavior.clockInButton.SetActive(true);

                        while (!mainCanvasBehavior.clockedIn)
                        {
                            yield return null;
                        }

                        if (clockInAnimationCoroutine != null)
                        {
                            __instance.StopCoroutine(clockInAnimationCoroutine);
                        }

                        yield return new WaitForSeconds(5f);
                    }
                }
                else
                {
                    while (!mainCanvasBehavior.startArcadeMode)
                    {
                        yield return null;
                    }
                }

                mainCanvasBehavior.softwareStartupPanel.SetActive(false);

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    (bool foundModifier, VariableChanged<RichAudioClip> value) dayStartedAudio =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.DayStartedAudio,
                            vCs => vCs.HasChanged);

                    if (dayStartedAudio.foundModifier)
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISound(dayStartedAudio.value.Data);
                    }
                    else
                    {
                        GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript
                            .correctSound);
                    }
                }
                else
                {
                    GlobalVariables.UISoundControllerScript.PlayUISound(
                        GlobalVariables.UISoundControllerScript.correctSound);
                }

                if (!GlobalVariables.arcadeMode
                    && GlobalVariables.currentDay == 7
                    && !CustomCampaignGlobal.InCustomCampaign)
                {
                    yield return new WaitForSeconds(0.4f);

                    mainCanvasBehavior.cameraAnimator.SetTrigger(Glitch);
                    GlobalVariables.fade.FadeIn();

                    yield return new WaitForSeconds(0.2f);

                    GlobalVariables.fade.FadeOut();
                    GlobalVariables.musicControllerScript.StartTrialMusic();
                }
                else if (CustomCampaignGlobal.InCustomCampaign)
                {
                    // Skip
                }

                if (GlobalVariables.arcadeMode)
                {
                    mainCanvasBehavior.callTimer.SetActive(true);

                    yield return new WaitForSeconds(1f);

                    GlobalVariables.fade.FadeOut();
                }

                // Custom Enables
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign != null
                        && customCampaign.AlwaysSkipCallButton)
                    {
                        CustomDesktopHelper.GetCallSkipButton().SetActive(true);
                    }
                }

                GlobalVariables.callerControllerScript.StartCallRoutine();
                GlobalVariables.introIsPlaying = false;
            }

            /// <summary>
            /// Custom Coroutine for rendering a custom clock in animation.
            /// </summary>
            /// <param name="clockInAnimationImage">Image that contains the clock animation to show on.</param>
            /// <param name="clockInAnimation">List of sprites that contain the frames for the animation.</param>
            /// <param name="clockInAnimationDuration">Duration of the animation.</param>
            /// <returns>Coroutine Object to run.</returns>
            private static IEnumerator CustomClockInAnimation(Image clockInAnimationImage,
                List<Sprite> clockInAnimation, float clockInAnimationDuration)
            {
                float frameLength = clockInAnimationDuration / clockInAnimation.Count;

                if (clockInAnimation.Count <= 0)
                {
                    LoggingHelper.WarningLog("Provided clock in animation has no images/frames to show.");
                    yield break;
                }

                clockInAnimationImage.sprite = clockInAnimation[0];

                int frameIndex = 0;

                while (true)
                {
                    clockInAnimationImage.sprite = clockInAnimation[frameIndex];

                    yield return new WaitForSeconds(frameLength);

                    if (frameIndex >= clockInAnimation.Count - 1)
                    {
                        frameIndex = 0;
                    }
                    else
                    {
                        frameIndex++;
                    }
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "EndDayRoutine")]
        public static class EndDayRoutinePatch
        {
            // To avoid duplicate day ending.
            public static bool IsDayEnding;

            private static readonly MethodInfo SaveCallerAnswers = typeof(MainCanvasBehavior).GetMethod(
                "SaveCallerAnswers",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static readonly MethodInfo UnlockDailySteamAchievement = typeof(MainCanvasBehavior).GetMethod(
                "UnlockDailySteamAchievement",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static readonly FieldInfo ProgressDay = typeof(MainCanvasBehavior).GetField("progressDay",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            /// <summary>
            /// Patches the EndDayRoutine coroutine to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called after wards. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref IEnumerator __result)
            {
                LoggingHelper.DebugLog("Calling EndDayRoutine.");

                __result = EndDayRoutineChanged(__instance);

                return false; // Skip function with false.
            }

            private static IEnumerator EndDayRoutineChanged(MainCanvasBehavior __instance)
            {
                if (IsDayEnding)
                {
                    LoggingHelper.DebugLog("Skipping EndDayRoutine.");
                    yield break;
                }

                IsDayEnding = true;

                MainCanvasBehavior mainCanvasBehavior = __instance;
                mainCanvasBehavior.clockedOut = false;

                IntermissionMusicHelper.StopIntermissionMusicRoutine();

                yield return new WaitForSeconds(5f);

                mainCanvasBehavior.inputBlocker.SetActive(false);

                GlobalVariables.UISoundControllerScript.PlayUISound(
                    GlobalVariables.UISoundControllerScript.correctSound);

                GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                mainCanvasBehavior.softwareStartupPanel.SetActive(true);
                mainCanvasBehavior.clockInPanel.SetActive(true);
                mainCanvasBehavior.logoPanel.SetActive(false);
                mainCanvasBehavior.clockOutElements.SetActive(true);
                mainCanvasBehavior.clockOutButton.SetActive(true);
                mainCanvasBehavior.clockInElements.SetActive(false);

                IsDayEnding = false;
                while (!mainCanvasBehavior.clockedOut)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(6f);

                if (!GlobalVariables.isXmasDLC)
                {
                    if (UnlockDailySteamAchievement == null)
                    {
                        LoggingHelper.ReflectionError(nameof(UnlockDailySteamAchievement));
                        yield break;
                    }

                    // OLD: mainCanvasBehavior.UnlockDailySteamAchievement();
                    if (!CustomCampaignGlobal.InCustomCampaign) // Only in main campaign
                    {
                        UnlockDailySteamAchievement.Invoke(mainCanvasBehavior, null);
                    }
                }

                GlobalVariables.fade.FadeIn(2f);
                mainCanvasBehavior.StartCoroutine(
                    GlobalVariables.UISoundControllerScript.FadeOutLoopingSound(GlobalVariables.UISoundControllerScript
                        .myFanSpinLoopingSource));

                yield return new WaitForSeconds(2f);

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    PlayerPrefs.SetFloat("SavedDayScore" + GlobalVariables.currentDay,
                        GlobalVariables.callerControllerScript.GetScore());
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    float dayScore = GlobalVariables.callerControllerScript.GetScore();

                    // No callers for that day, so we simply set it to 100%.
                    if (float.IsNaN(dayScore) || float.IsInfinity(dayScore))
                    {
                        dayScore = 100.0f;
                    }

                    customCampaign.SavedDayScores[GlobalVariables.currentDay] = dayScore;


                    LoggingHelper.DebugLog($"Saving day score of day '{GlobalVariables.currentDay}'." +
                                           $"With the score of '{customCampaign.SavedDayScores[GlobalVariables.currentDay]}'.");
                }

                if (ProgressDay == null)
                {
                    LoggingHelper.ReflectionError(nameof(ProgressDay));
                    yield break;
                }

                // OLD: !mainCanvasBehavior.progressDay
                if (!(bool)ProgressDay.GetValue(mainCanvasBehavior))
                {
                    ++GlobalVariables.currentDay;

                    // OLD: mainCanvasBehavior.progressDay = true;
                    ProgressDay.SetValue(mainCanvasBehavior, true);
                }

                if (!CustomCampaignGlobal.InCustomCampaign)
                {
                    GlobalVariables.saveManagerScript.savedDay = GlobalVariables.currentDay;
                    GlobalVariables.saveManagerScript.savedCurrentCaller =
                        GlobalVariables.callerControllerScript.currentCallerID + 1;
                    GlobalVariables.saveManagerScript.savedEntryTier = GlobalVariables.entryUnlockScript.currentTier;

                    if (SaveCallerAnswers == null)
                    {
                        LoggingHelper.ReflectionError(nameof(SaveCallerAnswers));
                        yield break;
                    }

                    // OLD: mainCanvasBehavior.SaveCallerAnswers();
                    SaveCallerAnswers.Invoke(mainCanvasBehavior, null);
                }
                else // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    customCampaign.CurrentDay = GlobalVariables.currentDay;
                    customCampaign.SavedCurrentCaller = GlobalVariables.callerControllerScript.currentCallerID + 1;
                    customCampaign.CurrentPermissionTier = GlobalVariables.entryUnlockScript.currentTier;

                    List<bool> flagArray = new List<bool>();

                    // Create missing values.
                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        flagArray.Add(false);
                    }

                    for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                    {
                        if (GlobalVariables.callerControllerScript.callers[index] !=
                            null) // Sanity check in case there were some unset callers.
                        {
                            flagArray[index] = GlobalVariables.callerControllerScript.callers[index].answeredCorrectly;
                        }
                    }

                    customCampaign.SavedCallersCorrectAnswer = flagArray;
                    customCampaign.SavedCallerArrayLength = GlobalVariables.callerControllerScript.callers.Length;
                }

                GlobalVariables.saveManagerScript.SaveGameProgress();

                yield return null;

                LoggingHelper.DebugLog("Ending the EndDayRoutine.");

                mainCanvasBehavior.ExitToMenu();

                mainCanvasBehavior.StartCoroutine(mainCanvasBehavior.StartSoftwareRoutine());
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "IsNetworkDown")]
        public static class IsNetworkDownPatch
        {
            /// <summary>
            /// Patches the network down patch to also check for custom callers.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> If to down the network. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance, ref bool __result)
            {
                if (GlobalVariables.arcadeMode)
                {
                    if (GlobalVariables.callerControllerScript.downedNetworkCall)
                    {
                        __result = true;
                        return false; // Skip function with false.
                    }
                }
                else
                {
                    if (!CustomCampaignGlobal.InCustomCampaign) // Not in custom campaign, could be main or DLC.
                    {
                        foreach (int downedNetworkCall in GlobalVariables.callerControllerScript.downedNetworkCalls)
                        {
                            if (downedNetworkCall == GlobalVariables.callerControllerScript.currentCallerID)
                            {
                                __result = true;
                                return false; // Skip function with false.
                            }
                        }
                    }
                    else // Custom Campaign
                    {
                        CustomCCaller customCCaller =
                            CustomCampaignGlobal.GetCustomCallerFromActiveCampaign(GlobalVariables
                                .callerControllerScript.currentCallerID);

                        if (customCCaller == null)
                        {
                            LoggingHelper.ErrorLog("Custom campaign caller was null. " +
                                                   "Unable of checking for downed network parameter. " +
                                                   "Calling original function.");
                            return true;
                        }

                        // This is set to true if the caller is allowed to down the network.
                        if (customCCaller.DownedNetworkCaller)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }

                __result = false;
                return false; // Skip function with false.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "LoadCallerAnswers")]
        public static class LoadCallerAnswersPatch
        {
            /// <summary>
            /// Patches the load caller answers to gracefully accept null values.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance)
            {
                if (GlobalVariables.saveManagerScript.savedCallerCorrectAnswers.Length !=
                    GlobalVariables.callerControllerScript.callers.Length)
                {
                    return false;
                }

                for (int index = 0; index < GlobalVariables.callerControllerScript.callers.Length; ++index)
                {
                    if (GlobalVariables.callerControllerScript.callers[index] != null)
                    {
                        GlobalVariables.callerControllerScript.callers[index].answeredCorrectly =
                            GlobalVariables.saveManagerScript.savedCallerCorrectAnswers[index];
                    }
                }

                return false; // Skip function with false.
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "ExitToMenu")]
        public static class ExitToMenuPatch
        {
            /// <summary>
            /// Patches the function to stop intermission music if still playing.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MainCanvasBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    IntermissionMusicHelper.StopIntermissionMusicRoutine();
                }

                GlobalVariables.arcadeMode = false;

                if (!GlobalVariables.isXmasDLC)
                {
                    SceneManager.LoadScene("MainMenuScene");
                }
                else
                {
                    SceneManager.LoadScene("MainMenuSceneXmas");
                }

                return false; // Skip function with false.
            }
        }
    }
}