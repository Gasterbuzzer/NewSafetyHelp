using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyHelpers;
using NewSafetyHelp.CustomDesktop;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;

namespace NewSafetyHelp.CustomVideos
{
    public static class VideoHelper
    {
        private static readonly FieldInfo DayUnlockScript = typeof(VideoExecutableFile).GetField("dayUnlockScript",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        /// <summary>
        /// Create a custom video file program on the desktop.
        /// </summary>
        /// <param name="customVideo">CustomVideo to be created.</param>
        /// <returns>GameObject of the custom video</returns>
        public static GameObject CreateCustomVideoFileProgram(CustomVideo customVideo)
        {
            GameObject trailerFileOriginal =
                CustomDesktopHelper.GetLeftPrograms().transform.Find("TrailerFile").gameObject;

            GameObject newCustomVideo = Object.Instantiate(trailerFileOriginal, trailerFileOriginal.transform.parent);

            if (string.IsNullOrEmpty(customVideo.DesktopName))
            {
                LoggingHelper.ErrorLog("No filename provided for video to be created!");
            }

            newCustomVideo.name = customVideo.DesktopName + customVideo.VideoURL;

            if (customVideo.GameObjectOrder.HasChanged)
            {
                newCustomVideo.transform.SetSiblingIndex(customVideo.GameObjectOrder.Data);
            }

            // Update desktop name
            TextMeshProUGUI textChildGameObjectText = newCustomVideo.transform.Find("TextBackground").transform
                .Find("ExecutableName").gameObject.GetComponent<TextMeshProUGUI>();

            textChildGameObjectText.text = customVideo.DesktopName;

            // Unlock Day
            OnDayUnlock onDayUnlock = newCustomVideo.GetComponent<OnDayUnlock>();
            onDayUnlock.unlockDay = customVideo.UnlockDay;

            // Simple check old check.
            if (customVideo.IgnoreAccuracyChecks)
            {
                if (customVideo.UnlockDay <= GlobalVariables.currentDay)
                {
                    newCustomVideo.SetActive(true);
                }
            }
            else
            {
                if (AccuracyVideoHelper.CheckIfVideoAccuracyType(customVideo))
                {
                    newCustomVideo.SetActive(true);
                }
            }

            // Fix References
            VideoExecutableFile videoExecutableFile = newCustomVideo.GetComponent<VideoExecutableFile>();

            videoExecutableFile.videoClip = null;

            // Update on day unlock script to point at the correct onDayUnlock.
            if (DayUnlockScript == null)
            {
                LoggingHelper.ReflectionError(nameof(DayUnlockScript));
                customVideo.ReferenceToCreatedVideo = null;
                return null;
            }

            DayUnlockScript.SetValue(videoExecutableFile, onDayUnlock);

            customVideo.ReferenceToCreatedVideo = newCustomVideo;

            return newCustomVideo;
        }
    }
}