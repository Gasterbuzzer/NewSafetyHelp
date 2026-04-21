using System.Collections;
using MelonLoader;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.TimedCaller
{
    public static class TimerCallerHelper
    {
        private static object timerCallerRoutine;
        
        private static GameObject TimerLabel;
        
        /// <summary>
        /// Starts the coroutine and timer for the timed caller.
        /// </summary>
        /// <param name="seconds"></param>
        public static void StartTimedCallerTimer(float seconds)
        {
            LoggingHelper.InfoLog($"Starting timed caller with a timer of '{seconds}'.");

            if (timerCallerRoutine == null)
            {
                timerCallerRoutine = MelonCoroutines.Start(TimedCallerTimer(seconds));
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

        private static IEnumerator TimedCallerTimer(float seconds)
        {
            if (TimerLabel == null)
            {
                LoggingHelper.ErrorLog("TimerLabel is null and can't be updated.");
                yield break;
            }
            
            TextMeshProUGUI textMeshComponent = TimerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
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
            
            timerCallerRoutine = null;
            
            HideCallerTimerUI();
        }

        public static void ShowCallerTimerUI(MainCanvasBehavior __instance, CustomCCaller currentCaller)
        {
            RectTransform replayLabelRectTransform = __instance.callerNameText.gameObject.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();
                        
            TimerLabel = Object.Instantiate(replayLabelRectTransform.gameObject,
                replayLabelRectTransform.gameObject.transform.parent);

            TimerLabel.name = "Timer Label";
                        
            TimerLabel.GetComponent<RectTransform>().localPosition = 
                new Vector3(135, replayLabelRectTransform.localPosition.y, replayLabelRectTransform.localPosition.z);
                        
            TimerLabel.GetComponent<RectTransform>().offsetMax = new Vector2(100.465f, -40.7853f);

            TimerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{currentCaller.TimedCallerDuration}s";
            TimerLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                        
            // Set the replay labels size to allow for the timer label to fit.
            replayLabelRectTransform.offsetMax = new Vector2(60.838f, -40.7853f);
        }

        public static void HideCallerTimerUI()
        {
            if (TimerLabel != null)
            {
                Object.Destroy(TimerLabel);
            }
                        
            RectTransform replayLabelRectTransform = GlobalVariables.mainCanvasScript.callerNameText.transform.parent
                .parent.Find("ReplayLabel").GetComponent<RectTransform>();
                        
            replayLabelRectTransform.offsetMax = new Vector2(100.838f, -40.7853f);
        }
    }
}