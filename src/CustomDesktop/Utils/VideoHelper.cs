using System.Reflection;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.LoggingSystem;
using TMPro;
using UnityEngine;

namespace NewSafetyHelp.CustomDesktop.Utils
{
    public static class VideoHelper
    {
        /// <summary>
        /// Disables all default video programs on the desktop.
        /// </summary>
        public static GameObject CreateCustomVideoFileProgram(CustomVideo customVideo)
        {
            GameObject trailerFileOriginal = CustomDesktopHelper.GetLeftPrograms().transform.Find("TrailerFile").gameObject;

            GameObject newCustomVideo = Object.Instantiate(trailerFileOriginal, trailerFileOriginal.transform.parent);

            if (string.IsNullOrEmpty(customVideo.DesktopName))
            {
                LoggingHelper.ErrorLog("No filename provided for video to be created!" +
                                       " Can lead to crashes or unwanted failures.");
            }

            newCustomVideo.name = customVideo.DesktopName + customVideo.VideoURL;

            // Update desktop name
            TextMeshProUGUI textChildGameObjectText = newCustomVideo.transform.Find("TextBackground").transform
                .Find("ExecutableName").gameObject.GetComponent<TextMeshProUGUI>();

            textChildGameObjectText.text = customVideo.DesktopName;

            // Unlock Day
            OnDayUnlock onDayUnlock = newCustomVideo.GetComponent<OnDayUnlock>();
            onDayUnlock.unlockDay = customVideo.UnlockDay;

            if (customVideo.UnlockDay <= GlobalVariables.currentDay)
            {
                newCustomVideo.SetActive(true);
            }

            // Fix References

            VideoExecutableFile videoExecutableFile = newCustomVideo.GetComponent<VideoExecutableFile>();

            videoExecutableFile.videoClip = null;

            // Update on day unlock script to point at the correct onDayUnlock.
            FieldInfo _onDayUnlock = typeof(VideoExecutableFile).GetField("dayUnlockScript",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (_onDayUnlock == null)
            {
                LoggingHelper.ErrorLog("Could not find OnDayUnlock script for VideoExecutableFile!");
                return null;
            }

            _onDayUnlock.SetValue(videoExecutableFile, onDayUnlock);

            return newCustomVideo;
        }
    }
}