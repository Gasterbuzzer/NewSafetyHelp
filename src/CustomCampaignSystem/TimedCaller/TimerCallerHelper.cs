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

            GlobalVariables.UISoundControllerScript.PlayUISound(EmbeddedTimerData.ClockStart);

            while (seconds > 1)
            {
                yield return new WaitForSeconds(1);
                seconds--;

                textMeshComponent.text = GetTimerDisplay(seconds);

                if (!playedHalfPercent
                    && seconds <= halfPercentTimerCheckmark)
                {
                    playedHalfPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(EmbeddedTimerData.ClockHalfTime);
                }

                if (!playedTenPercent
                    && seconds <= tenPercentTimerCheckmark)
                {
                    playedTenPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(EmbeddedTimerData.ClockFivePercent);
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
            float tenPercentTimerCheckmark = seconds * 0.1f;
            bool playedTenPercent = false;

            float halfPercentTimerCheckmark = seconds * 0.5f;
            bool playedHalfPercent = false;

            GlobalVariables.UISoundControllerScript.PlayUISound(EmbeddedTimerData.ClockStart);

            while (seconds > 1)
            {
                yield return new WaitForSeconds(1);
                seconds--;

                if (!playedHalfPercent
                    && seconds <= halfPercentTimerCheckmark)
                {
                    playedHalfPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(EmbeddedTimerData.ClockHalfTime);
                }

                if (!playedTenPercent
                    && seconds <= tenPercentTimerCheckmark)
                {
                    playedTenPercent = true;
                    GlobalVariables.UISoundControllerScript.PlayUISound(EmbeddedTimerData.ClockFivePercent);
                }
            }

            // Wait out remainder (less than 1 second)
            yield return new WaitForSeconds(seconds);

            LoggingHelper.InfoLog($"Finished timer caller with a time of '{seconds}'.");

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
            GlobalVariables.mainCanvasScript.CreateError("CALL RELEASED BY REMOTE");
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
                ShowAnalogClockTimerUI(__instance, currentCaller);
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
        /// <param name="currentCaller"></param>
        private static void ShowAnalogClockTimerUI(MainCanvasBehavior __instance, CustomCCaller currentCaller)
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
            // 1. Create a blank GameObject and parent it immediately
            GameObject baseClockGO = new GameObject("Clock Base");
            baseClockGO.transform.SetParent(analogClock.transform, false);
            // false = don't preserve world position — use local space of the parent instead

            // 2. Add the required UI components
            baseClockGO.AddComponent<CanvasRenderer>(); // Required by Unity's UI system to render anything
            Image clockImage = baseClockGO.AddComponent<Image>();

            // 3. Assign your sprite
            clockImage.sprite = EmbeddedTimerData.ClockBase;

            // 4. Stretch it to fill the parent exactly
            RectTransform rt = baseClockGO.GetComponent<RectTransform>();
            // AddComponent<Image>() auto-adds a RectTransform, so GetComponent is safe here

            rt.anchorMin = Vector2.zero; // Bottom-left corner anchored to 0,0 of parent
            rt.anchorMax = Vector2.one; // Top-right corner anchored to 1,1 of parent (full stretch)
            rt.offsetMin = Vector2.zero; // No offset from the anchor on the bottom-left
            rt.offsetMax = Vector2.zero; // No offset from the anchor on the top-right

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

            RectTransform replayLabelRectTransform = GlobalVariables.mainCanvasScript.callerNameText.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();

            replayLabelRectTransform.offsetMax = new Vector2(100.838f, -40.7853f);
        }
    }
}