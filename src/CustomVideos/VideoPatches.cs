using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.Video;

namespace NewSafetyHelp.CustomVideos
{
    public static class VideoPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(VideoExecutableFile), "PlayVideoRoutine")]
        public static class PlayVideoRoutinePatch
        {
            private static readonly FieldInfo MyClearScript =
                typeof(VideoExecutableFile).GetField("myClearScript",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

            private static MethodInfo refreshVideo;

            /// <summary>
            /// This functions plays the video. It was changed to also support URL plays.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(VideoExecutableFile __instance, ref IEnumerator __result)
            {
                __result = PlayVideoRoutineCoroutine(__instance);

                return false; // Skip the original function
            }

            /// <summary>
            /// Coroutine for the PlayVideoRoutine function.
            /// </summary>
            /// <param name="__instance">Instance of the VideoExecutableFile.</param>
            /// <returns>Coroutine to be executed.</returns>
            private static IEnumerator PlayVideoRoutineCoroutine(VideoExecutableFile __instance)
            {
                __instance.notification.SetActive(false);

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    if (__instance.videoClip != null)
                    {
                        __instance.videoPlayer.clip = __instance.videoClip;
                    }
                    else
                    {
                        CustomVideo customVideo =
                            CustomCampaignGlobal.GetCustomVideoFromActiveCampaign(__instance.gameObject);

                        if (customVideo == null)
                        {
                            LoggingHelper.CriticalErrorLog("Unable of finding the video show! Critical error.");
                            yield break;
                        }

                        __instance.videoPlayer.url = customVideo.VideoURL;
                    }
                }
                else // Main Campaign
                {
                    __instance.videoPlayer.clip = __instance.videoClip;
                }

                if (MyClearScript == null)
                {
                    LoggingHelper.ReflectionError(nameof(MyClearScript));
                    yield break;
                }

                object myClearScript = MyClearScript.GetValue(__instance);

                if (myClearScript == null)
                {
                    LoggingHelper.CriticalErrorLog("MyClearScript was not able to get value." +
                                                   " Critical error.");
                    yield break;
                }

                // Getting the method of the myClearScript
                refreshVideo = myClearScript.GetType().GetMethod("RefreshVideo",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

                if (refreshVideo == null)
                {
                    LoggingHelper.ReflectionError(nameof(refreshVideo));
                    yield break;
                }

                // OLD: __instance.myClearScript.RefreshVideo();
                refreshVideo.Invoke(myClearScript, null);

                yield return new WaitForSeconds(0.5f);

                __instance.videoPopup.SetActive(true);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(AudioSamplePlayer), "PlayOrPauseVideo")]
        public static class PlayOrPauseVideoPatch
        {
            private static readonly FieldInfo PlayerCurrentPosition =
                typeof(AudioSamplePlayer).GetField("playerCurrentPosition",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

            /// <summary>
            /// This functions plays the video in the video GUI. It is patched to handle URLs.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(AudioSamplePlayer __instance)
            {
                if (!(bool)__instance.myVideoPlayer)
                {
                    return false;
                }

                if (PlayerCurrentPosition == null)
                {
                    LoggingHelper.ReflectionError(nameof(PlayerCurrentPosition));
                    return true;
                }

                if (__instance.myVideoPlayer.isPlaying)
                {
                    __instance.myVideoPlayer.Pause();

                    // OLD: __instance.playerCurrentPosition = __instance.playerTracker.transform.localPosition;
                    PlayerCurrentPosition.SetValue(__instance, __instance.playerTracker.transform.localPosition);

                    __instance.StopAllCoroutines();
                }
                else
                {
                    MelonCoroutines.Start(HandleURLVideoBetter(__instance, PlayerCurrentPosition));
                }

                return false; // Skip the original function
            }

            /// <summary>
            /// Coroutine of the PlayOrPauseVideo function.
            /// This version handles video files better.
            /// </summary>
            /// <param name="__instance">Instance of the AudioSamplePlayer.</param>
            /// <param name="playerCurrentPosition">Video players current cursor position.</param>
            /// <returns>Coroutine to be called.</returns>
            private static IEnumerator HandleURLVideoBetter(AudioSamplePlayer __instance,
                FieldInfo playerCurrentPosition)
            {
                __instance.myVideoPlayer.Play();

                if (__instance.myVideoPlayer.time == 0.0
                    && __instance.playerTracker.transform.localPosition == __instance.playerStartPosition)
                {
                    if (__instance.myVideoPlayer.clip != null)
                    {
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker,
                            __instance.playerStartPosition, __instance.playerEndPosition,
                            (float)__instance.myVideoPlayer.clip.length));
                    }
                    else if (!string.IsNullOrEmpty(__instance.myVideoPlayer.url)) // Url is provided.
                    {
                        yield return WaitForPrepare(__instance.myVideoPlayer);

                        // Compute the duration correctly
                        float duration = __instance.myVideoPlayer.frameCount / __instance.myVideoPlayer.frameRate;

                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker,
                            __instance.playerStartPosition, __instance.playerEndPosition, duration));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Unable of playing video as the URL and the Clip are null.");
                    }
                }
                else
                {
                    if (__instance.myVideoPlayer.clip != null)
                    {
                        // OLD: __instance.playerCurrentPosition
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker,
                            (Vector3)playerCurrentPosition.GetValue(__instance), __instance.playerEndPosition,
                            (float)__instance.myVideoPlayer.clip.length - (float)__instance.myVideoPlayer.time));
                    }
                    else if (!string.IsNullOrEmpty(__instance.myVideoPlayer.url)) // Url is provided.
                    {
                        yield return WaitForPrepare(__instance.myVideoPlayer);

                        // Compute the duration correctly
                        float duration = __instance.myVideoPlayer.frameCount / __instance.myVideoPlayer.frameRate;

                        // OLD: __instance.playerCurrentPosition
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker,
                            (Vector3)playerCurrentPosition.GetValue(__instance), __instance.playerEndPosition,
                            duration - (float)__instance.myVideoPlayer.time));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Unable of playing video as the URL and the Clip are null.");
                    }
                }
            }

            /// <summary>
            /// Waits for the given video player to finish preparing the given URL.
            /// </summary>
            /// <param name="vp">VideoPlayer to be prepared.</param>
            /// <returns>Coroutine to be called.</returns>
            private static IEnumerator WaitForPrepare(VideoPlayer vp)
            {
                vp.Prepare();

                while (!vp.isPrepared)
                {
                    yield return null;
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(AudioSamplePlayer), "Update")]
        public static class UpdatePatch
        {
            private static readonly FieldInfo PlayerCurrentPosition = typeof(AudioSamplePlayer).GetField(
                "playerCurrentPosition",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

            /// <summary>
            /// This functions handles multiple functionality for the player. It is patched to work with URLs.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(AudioSamplePlayer __instance)
            {
                if ((bool)__instance.myAudioSource
                    && __instance.myAudioSource.isPlaying || (bool)__instance.myVideoPlayer
                    && __instance.myVideoPlayer.isPlaying)
                {
                    __instance.myImage.sprite = __instance.stopSprite;
                    __instance.audioLabelText.SetActive(false);
                }
                else
                {
                    __instance.myImage.sprite = __instance.playSprite;
                    __instance.audioLabelText.SetActive(true);
                    if ((bool)__instance.myAudioSource && __instance.playerTracker.transform.localPosition !=
                        __instance.playerStartPosition)
                    {
                        __instance.StopAllCoroutines();
                        __instance.playerTracker.transform.localPosition = __instance.playerStartPosition;
                    }
                }

                if (!__instance.scrubbing)
                {
                    return false;
                }

                if ((bool)__instance.myVideoPlayer
                    && __instance.myVideoPlayer.isPlaying)
                {
                    __instance.StopAllCoroutines();
                    __instance.myVideoPlayer.Pause();
                }

                if (Camera.main == null)
                {
                    LoggingHelper.ErrorLog("Camera missing! " +
                                           "Calling original function!");
                    return true;
                }

                __instance.playerTracker.transform.position = new Vector3(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                    __instance.playerTracker.transform.position.y, __instance.playerTracker.transform.position.z);

                if (__instance.playerTracker.transform.localPosition.x < __instance.playerStartPosition.x)
                {
                    __instance.playerTracker.transform.localPosition = __instance.playerStartPosition;
                }

                if (__instance.playerTracker.transform.localPosition.x > __instance.playerEndPosition.x)
                {
                    __instance.playerTracker.transform.localPosition = __instance.playerEndPosition;
                }

                float computedDistanceFactor =
                    (__instance.playerTracker.transform.localPosition.x - __instance.playerStartPosition.x) /
                    (__instance.playerEndPosition.x - __instance.playerStartPosition.x);

                if ((bool)__instance.myVideoPlayer)
                {
                    MelonCoroutines.Start(HandleURLVideoBetter(__instance, PlayerCurrentPosition,
                        computedDistanceFactor));
                }

                if ((bool)__instance.myAudioSource)
                {
                    __instance.myAudioSource.time = __instance.myAudioSource.clip.length * computedDistanceFactor;

                    // OLD: __instance.playerCurrentPosition = __instance.playerTracker.transform.localPosition;
                    PlayerCurrentPosition.SetValue(__instance, __instance.playerTracker.transform.localPosition);
                }

                return false; // Skip the original function
            }

            /// <summary>
            /// A helper function that helps in handling the URLs of files better.
            /// </summary>
            /// <param name="__instance">Instance of the AudioSamplePlayer.</param>
            /// <param name="playerCurrentPosition">The position of the video player in clip.</param>
            /// <param name="computedDistanceFactor">Computed factor of the UI distance between the start position of
            /// the selected (video) players cursor to the end position of the timeline.
            /// In other words, it is the percentage of how far we are in the timeline, like 50% (0.5) for example.</param>
            /// <returns></returns>
            private static IEnumerator HandleURLVideoBetter(AudioSamplePlayer __instance,
                FieldInfo playerCurrentPosition, float computedDistanceFactor)
            {
                if (__instance.myVideoPlayer.clip != null)
                {
                    __instance.myVideoPlayer.time = __instance.myVideoPlayer.clip.length * computedDistanceFactor;
                }
                // URL Provided
                else if (!string.IsNullOrEmpty(__instance.myVideoPlayer.url))
                {
                    yield return WaitForPrepare(__instance.myVideoPlayer);

                    float duration = __instance.myVideoPlayer.frameCount / __instance.myVideoPlayer.frameRate;

                    __instance.myVideoPlayer.time = duration * computedDistanceFactor;
                }
                else
                {
                    LoggingHelper.ErrorLog("No URL or Clip provided for video player in update function!" +
                                           " Critical error!");
                    yield break;
                }

                // OLD: __instance.playerCurrentPosition = __instance.playerTracker.transform.localPosition;
                playerCurrentPosition.SetValue(__instance, __instance.playerTracker.transform.localPosition);
            }

            /// <summary>
            /// Waits for the given video player to have finished the video parsing.
            /// </summary>
            /// <param name="vp">VideoPlayer to wait for preparing.</param>
            /// <returns>Coroutine to be executed.</returns>
            private static IEnumerator WaitForPrepare(VideoPlayer vp)
            {
                vp.Prepare();

                while (!vp.isPrepared)
                {
                    yield return null;
                }
            }
        }
    }
}