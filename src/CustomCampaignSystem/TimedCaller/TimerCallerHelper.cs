using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomCampaignSystem.TimedCaller
{
    public static class TimerCallerHelper
    {
        private static object timerCallerRoutine;

        private static GameObject timerLabel;

        private static GameObject analogClock;
        private static GameObject clockHand;
        private static Image clockFill;

        private static RichAudioClip clockStart = EmbeddedTimerData.ClockStart;
        private static RichAudioClip clockHalfTime = EmbeddedTimerData.ClockHalfTime;
        private static RichAudioClip clockFivePercent = EmbeddedTimerData.ClockFivePercent;

        private static readonly FieldInfo OnCallConcluded = typeof(CallerController).GetField("OnCallConcluded",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Starts the coroutine and timer for the timed caller.
        /// </summary>
        /// <param name="seconds"></param>
        public static void StartTimedCallerTimer(float seconds)
        {
            if (timerCallerRoutine == null)
            {
                // Setting up all values:

                (bool foundModifier, VariableChanged<RichAudioClip> value) timedCallerStartSound =
                    CustomCampaignGlobal.GetActiveModifierValue(c => c.TimedCallerStartSound,
                        vCb => vCb.HasChanged);

                if (timedCallerStartSound.foundModifier)
                {
                    clockStart = timedCallerStartSound.value.Data;
                }
                else
                {
                    clockStart = EmbeddedTimerData.ClockStart;
                }

                (bool foundModifier, VariableChanged<RichAudioClip> value) timedCallerHalfSound =
                    CustomCampaignGlobal.GetActiveModifierValue(c => c.TimedCallerHalfSound,
                        vCb => vCb.HasChanged);

                if (timedCallerHalfSound.foundModifier)
                {
                    clockHalfTime = timedCallerHalfSound.value.Data;
                }
                else
                {
                    clockHalfTime = EmbeddedTimerData.ClockHalfTime;
                }

                (bool foundModifier, VariableChanged<RichAudioClip> value) timedCallerCriticalSound =
                    CustomCampaignGlobal.GetActiveModifierValue(c => c.TimedCallerCriticalSound,
                        vCb => vCb.HasChanged);

                if (timedCallerCriticalSound.foundModifier)
                {
                    clockFivePercent = timedCallerCriticalSound.value.Data;
                }
                else
                {
                    clockFivePercent = EmbeddedTimerData.ClockHalfTime;
                }

                (bool foundModifier, VariableChanged<bool> value) useClockInsteadOfTimer =
                    CustomCampaignGlobal.GetActiveModifierValue(c => c.UseClockInsteadOfTimer,
                        vCb => vCb.HasChanged);

                if (useClockInsteadOfTimer.foundModifier
                    && useClockInsteadOfTimer.value.Data)
                {
                    timerCallerRoutine = MelonCoroutines.Start(AnalogTimedCallerTimer(seconds));
                }
                else
                {
                    timerCallerRoutine = MelonCoroutines.Start(DigitalTimedCallerTimer(seconds));
                }

                LoggingHelper.InfoLog($"Starting timed caller with a timer of '{seconds}'.");
            }
            else
            {
                LoggingHelper.DebugLog("Timer is already running. Not running again.");
            }
        }

        /// <summary>
        /// Stops the coroutine of the timed caller.
        /// </summary>
        public static void StopTimedCallerTimer()
        {
            if (timerCallerRoutine != null)
            {
                LoggingHelper.DebugLog("Stopped timer caller coroutine.");
                MelonCoroutines.Stop(timerCallerRoutine);
                timerCallerRoutine = null;
            }
        }

        /// <summary>
        /// Displays the seconds into a more readable format.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private static string GetTimerDisplay(float seconds)
        {
            int displayMinutes = (int)(seconds / 60);
            int displaySeconds = (int)(seconds % 60);

            return $"{displayMinutes:D2}:{displaySeconds:D2}";
        }

        /// <summary>
        /// Coroutine for the timed caller. (For digital clock display)
        /// </summary>
        /// <param name="seconds">Seconds to run for.</param>
        /// <returns>(IEnumerator) Routine to run.</returns>
        private static IEnumerator DigitalTimedCallerTimer(float seconds)
        {
            if (timerLabel == null)
            {
                LoggingHelper.ErrorLog("TimerLabel is null and can't be updated.");
                yield break;
            }

            float tenPercentTimerCheckmark = seconds * 0.1f;
            bool playedTenPercent = false;

            float halfPercentTimerCheckmark = seconds * 0.5f;
            bool playedHalfPercent = false;

            // Digital Label
            TextMeshProUGUI textMeshComponent = timerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            textMeshComponent.text = GetTimerDisplay(seconds);

            GlobalVariables.UISoundControllerScript.PlayUISound(clockStart);

            float tickRate = 1f;

            (bool modifierFound, VariableChanged<float> value) tickRateModifier =
                CustomCampaignGlobal.GetActiveModifierValue(c => c.DigitalClockTickRate,
                    vCf => vCf.HasChanged);

            if (tickRateModifier.modifierFound)
            {
                tickRate = tickRateModifier.value.Data;
            }

            while (seconds > tickRate)
            {
                yield return new WaitForSeconds(tickRate);
                seconds -= tickRate;

                textMeshComponent.text = GetTimerDisplay(seconds);

                if (!playedHalfPercent
                    && seconds <= halfPercentTimerCheckmark)
                {
                    playedHalfPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(clockHalfTime);
                }

                if (!playedTenPercent
                    && seconds <= tenPercentTimerCheckmark)
                {
                    playedTenPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(clockFivePercent);
                }
            }

            // Wait out remainder (less than 1 second)
            yield return new WaitForSeconds(seconds);

            // Finished Digital Call Timer
            textMeshComponent.text = "00:00";

            LoggingHelper.InfoLog($"Finished timer caller with a time of '{seconds}'.");

            yield return new WaitForSeconds(0.1f);

            // Clean Up Call
            GlobalVariables.callerControllerScript.SubmitAnswer();

            GlobalVariables.musicControllerScript.StopMusic();
            GlobalVariables.UISoundControllerScript.StopUISoundLooping();

            FinishUpTimedCaller();
        }

        /// <summary>
        /// Coroutine for the timed caller. (For digital clock display)
        /// </summary>
        /// <param name="seconds">Seconds to run for.</param>
        /// <returns>(IEnumerator) Routine to run.</returns>
        private static IEnumerator AnalogTimedCallerTimer(float seconds)
        {
            // If the clock isn't set up correctly.
            if (clockHand == null)
            {
                LoggingHelper.ErrorLog("ClockHand of the Clock UI is null. (Possibly not set up correctly)?");
                yield break;
            }

            float startSeconds = seconds;

            float tenPercentTimerCheckmark = seconds * 0.1f;
            bool playedTenPercent = false;

            float halfPercentTimerCheckmark = seconds * 0.5f;
            bool playedHalfPercent = false;

            GlobalVariables.UISoundControllerScript.PlayUISound(clockStart);

            float tickRate = 0.05f;

            (bool modifierFound, VariableChanged<float> value) tickRateModifier =
                CustomCampaignGlobal.GetActiveModifierValue(c => c.AnalogClockTickRate,
                    vCf => vCf.HasChanged);

            if (tickRateModifier.modifierFound)
            {
                tickRate = tickRateModifier.value.Data;
                LoggingHelper.DebugLog($"Setting tick rate of analog timer to '{tickRate}'.");
            }

            while (seconds > tickRate)
            {
                yield return new WaitForSeconds(tickRate);
                seconds -= tickRate;

                float rotationPercentage = (startSeconds - seconds) / startSeconds; // 0.0 -> 1.0

                float rotationValue = rotationPercentage * 360; // This gives us the slice of the rotation.
                clockHand.transform.localRotation = Quaternion.Euler(0f, 0f, -rotationValue);

                clockFill.fillAmount = rotationPercentage;

                if (!playedHalfPercent
                    && seconds <= halfPercentTimerCheckmark)
                {
                    playedHalfPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(clockHalfTime);
                }

                if (!playedTenPercent
                    && seconds <= tenPercentTimerCheckmark)
                {
                    playedTenPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(clockFivePercent);
                }
            }

            // Wait out remainder (less than 1 second)
            yield return new WaitForSeconds(seconds);

            LoggingHelper.InfoLog($"Finished timer caller with a time of '{seconds}'.");

            clockHand.transform.localRotation = Quaternion.Euler(0f, 0f, -360f);
            clockFill.fillAmount = 1.0f;

            yield return new WaitForSeconds(0.1f);

            FinishUpTimedCaller();
        }

        /// <summary>
        /// Finishes up the timed call by setting all the correct values and disconnecting the call.
        /// </summary>
        private static void FinishUpTimedCaller()
        {
            // Clean Up Call
            GlobalVariables.callerControllerScript.SubmitAnswer();

            GlobalVariables.musicControllerScript.StopMusic();
            GlobalVariables.UISoundControllerScript.StopUISoundLooping();

            MulticastDelegate eventDelegate = OnCallConcluded.GetValue(null) as MulticastDelegate;

            if (eventDelegate != null)
            {
                // OLD: CallerController.OnCallConcluded();
                eventDelegate.DynamicInvoke();
            }

            GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.disconnect);

            timerCallerRoutine = null;

            HideCallerTimerUI();

            // Show Call Ended Badly error:

            string callEndedMessage = "TIMES UP!\nCALL DISCONNECTED";

            (bool foundModifier, VariableChanged<string> value) timedCallerDisconnectedMessage =
                CustomCampaignGlobal.GetActiveModifierValue(c => c.TimedCallerDisconnectedMessage,
                    vCs => vCs.HasChanged);

            if (timedCallerDisconnectedMessage.foundModifier)
            {
                callEndedMessage = timedCallerDisconnectedMessage.value.Data;
            }

            GlobalVariables.mainCanvasScript.CreateError(callEndedMessage);
            GlobalVariables.mainCanvasScript.inputBlocker.SetActive(false);
        }

        /// <summary>
        /// Shows the caller timer UI.
        /// Use this when a timer caller appears.
        /// </summary>
        /// <param name="__instance">Instance of the MainCanvas.</param>
        /// <param name="currentCaller">Current caller calling.</param>
        public static void ShowCallerTimerUI(MainCanvasBehavior __instance, CustomCCaller currentCaller)
        {
            (bool foundModifier, VariableChanged<bool> value) useClockInsteadOfTimer =
                CustomCampaignGlobal.GetActiveModifierValue(c => c.UseClockInsteadOfTimer,
                    vCb => vCb.HasChanged);

            if (useClockInsteadOfTimer.foundModifier
                && useClockInsteadOfTimer.value.Data)
            {
                ShowAnalogClockTimerUI(__instance);
            }
            else
            {
                ShowDigitTimerUI(__instance, currentCaller);
            }
        }

        /// <summary>
        /// Shows the digit clock (00:00 -> 99:99) in the UI.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="currentCaller"></param>
        private static void ShowDigitTimerUI(MainCanvasBehavior __instance, CustomCCaller currentCaller)
        {
            RectTransform replayLabelRectTransform = __instance.callerNameText.gameObject.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();

            timerLabel = Object.Instantiate(replayLabelRectTransform.gameObject,
                replayLabelRectTransform.gameObject.transform.parent);

            timerLabel.name = "Timer Label";

            timerLabel.GetComponent<RectTransform>().localPosition =
                new Vector3(135, replayLabelRectTransform.localPosition.y, replayLabelRectTransform.localPosition.z);

            timerLabel.GetComponent<RectTransform>().offsetMax = new Vector2(100.465f, -40.7853f);

            timerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                GetTimerDisplay(currentCaller.TimedCallerDuration);
            timerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Set the replay labels size to allow for the timer label to fit.
            replayLabelRectTransform.offsetMax = new Vector2(60.838f, -40.7853f);
        }

        /// <summary>
        /// Shows the digit clock (00:00 -> 99:99) in the UI.
        /// </summary>
        /// <param name="__instance"></param>
        private static void ShowAnalogClockTimerUI(MainCanvasBehavior __instance)
        {
            RectTransform replayLabelRectTransform = __instance.callerNameText.gameObject.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();

            analogClock = Object.Instantiate(replayLabelRectTransform.gameObject,
                replayLabelRectTransform.gameObject.transform.parent);

            analogClock.name = "Timer Clock";

            analogClock.GetComponent<RectTransform>().localPosition =
                new Vector3(141,
                    replayLabelRectTransform.localPosition.y - 5f,
                    replayLabelRectTransform.localPosition.z);

            analogClock.GetComponent<RectTransform>().offsetMax = new Vector2(105.465f, -30.7853f);

            analogClock.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            Object.Destroy(analogClock.transform.GetComponent<Image>());

            // Add clock UI:
            GameObject baseClock = new GameObject("ClockBase");
            baseClock.transform.SetParent(analogClock.transform, false);

            baseClock.AddComponent<CanvasRenderer>();

            Image clockBaseImage = baseClock.AddComponent<Image>();
            clockBaseImage.sprite = EmbeddedTimerData.ClockBase;

            // Fill parent
            RectTransform baseRectTransform = baseClock.GetComponent<RectTransform>();

            baseRectTransform.anchorMin = Vector2.zero;
            baseRectTransform.anchorMax = Vector2.one;
            baseRectTransform.offsetMin = Vector2.zero;
            baseRectTransform.offsetMax = Vector2.zero;

            // Add clock fill (Grey zone):

            GameObject clockFillGameObject = new GameObject("ClockFill");
            clockFillGameObject.transform.SetParent(baseClock.transform, false);

            clockFillGameObject.AddComponent<CanvasRenderer>();

            clockFill = clockFillGameObject.AddComponent<Image>();

            clockFill.sprite = EmbeddedTimerData.ClockBase;

            clockFill.type = Image.Type.Filled;
            clockFill.fillMethod = Image.FillMethod.Radial360;
            clockFill.fillOrigin = (int)Image.Origin360.Top;
            clockFill.fillCenter = true;
            clockFill.fillClockwise = true;
            clockFill.color = Color.gray;
            clockFill.fillAmount = 0.0f;

            RectTransform fillRectTransform = clockFillGameObject.GetComponent<RectTransform>();
            fillRectTransform.anchorMin = Vector2.zero;
            fillRectTransform.anchorMax = Vector2.one;
            fillRectTransform.offsetMin = Vector2.zero;
            fillRectTransform.offsetMax = Vector2.zero;

            // Add Clock Hand:
            clockHand = new GameObject("ClockHand");
            clockHand.transform.SetParent(baseClock.transform, false);

            clockHand.AddComponent<CanvasRenderer>();

            Image clockHandImage = clockHand.AddComponent<Image>();
            clockHandImage.sprite = EmbeddedTimerData.ClockHand;

            // Fill parent
            RectTransform handRectTransform = clockHand.GetComponent<RectTransform>();

            handRectTransform.anchorMin = Vector2.zero;
            handRectTransform.anchorMax = Vector2.one;
            handRectTransform.offsetMin = Vector2.zero;
            handRectTransform.offsetMax = Vector2.zero;

            // Set to start (Pointing up)
            clockHand.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            // Set the replay labels size to allow for the timer label to fit.
            replayLabelRectTransform.offsetMax = new Vector2(60.838f, -40.7853f);
        }

        /// <summary>
        /// Hides the caller UI.
        /// Do so, when you are switching back to the normal caller.
        /// </summary>
        public static void HideCallerTimerUI()
        {
            if (timerLabel != null)
            {
                Object.Destroy(timerLabel);
            }

            if (analogClock != null)
            {
                Object.Destroy(analogClock);
            }

            if (clockHand != null)
            {
                Object.Destroy(clockHand);
            }

            RectTransform replayLabelRectTransform = GlobalVariables.mainCanvasScript.callerNameText.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();

            replayLabelRectTransform.offsetMax = new Vector2(100.838f, -40.7853f);
        }
    }
}