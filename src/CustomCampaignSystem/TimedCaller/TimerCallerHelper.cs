using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomCampaignSystem.TimedCaller
{
    public static class TimerCallerHelper
    {
        private static object timerCallerRoutine;

        private static GameObject timerLabel;

        /// <summary>
        /// Starts the coroutine and timer for the timed caller.
        /// </summary>
        /// <param name="seconds"></param>
        public static void StartTimedCallerTimer(float seconds)
        {
            if (timerCallerRoutine == null)
            {
                LoggingHelper.InfoLog($"Starting timed caller with a timer of '{seconds}'.");
                timerCallerRoutine = MelonCoroutines.Start(TimedCallerTimer(seconds));
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

        private static readonly FieldInfo OnCallConcluded = typeof(CallerController).GetField("OnCallConcluded",
            BindingFlags.Static | BindingFlags.NonPublic);

        private static IEnumerator TimedCallerTimer(float seconds)
        {
            if (timerLabel == null)
            {
                LoggingHelper.ErrorLog("TimerLabel is null and can't be updated.");
                yield break;
            }

            TextMeshProUGUI textMeshComponent = timerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            textMeshComponent.text = $"{seconds}s";

            while (seconds > 1)
            {
                yield return new WaitForSeconds(1);
                seconds--;
                textMeshComponent.text = $"{seconds}s";
            }

            yield return new WaitForSeconds(seconds);

            textMeshComponent.text = "0s";

            LoggingHelper.InfoLog($"Finished timer caller with a time of '{seconds}'.");

            yield return new WaitForSeconds(0.1f);

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

            /*
            SubmitWindowBehavior submitWindowComponent = GameObject.Find("MainCanvas").transform.GetChild(2).GetComponent<SubmitWindowBehavior>();
            submitWindowComponent.submitButton.SetActive(true);
            submitWindowComponent.loadingText.SetActive(false);
            submitWindowComponent.gameObject.SetActive(false);
            */

            timerCallerRoutine = null;

            HideCallerTimerUI();
        }

        public static void ShowCallerTimerUI(MainCanvasBehavior __instance, CustomCCaller currentCaller)
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
                $"{currentCaller.TimedCallerDuration}s";
            timerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Set the replay labels size to allow for the timer label to fit.
            replayLabelRectTransform.offsetMax = new Vector2(60.838f, -40.7853f);
        }

        public static void HideCallerTimerUI()
        {
            if (timerLabel != null)
            {
                Object.Destroy(timerLabel);
            }

            RectTransform replayLabelRectTransform = GlobalVariables.mainCanvasScript.callerNameText.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();

            replayLabelRectTransform.offsetMax = new Vector2(100.838f, -40.7853f);
        }
    }
}