using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.CustomCampaignPatches.CustomCampaignModel;
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
            /// <summary>
            /// This functions plays the video. It was changed to also support URL plays.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(VideoExecutableFile __instance, ref IEnumerator __result)
            {

                __result = PlayVideoRoutineCoroutine(__instance);
                
                return false; // Skip the original function
            }

            private static IEnumerator PlayVideoRoutineCoroutine(VideoExecutableFile __instance)
            {
                __instance.notification.SetActive(false);

                if (CustomCampaignGlobal.InCustomCampaign) // Custom Campaign
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        yield break;
                    }

                    CustomVideo correctVideo;
                    
                    if (customCampaign.AllDesktopVideos.Exists(video => video.DesktopName + video.VideoURL == __instance.gameObject.name))
                    {
                        correctVideo = customCampaign.AllDesktopVideos.Find(video => video.DesktopName + video.VideoURL == __instance.gameObject.name);
                    }
                    else
                    {
                        LoggingHelper.CriticalErrorLog("Unable of finding the video show! Critical error.");
                        yield break;
                    }

                    if (correctVideo != null)
                    {
                        __instance.videoPlayer.url = correctVideo.VideoURL;
                    }
                }
                else // Main Campaign
                {
                    __instance.videoPlayer.clip = __instance.videoClip;
                }
                
                FieldInfo _myClearScript = typeof(VideoExecutableFile).GetField("myClearScript", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

                if (_myClearScript == null)
                {
                    LoggingHelper.CriticalErrorLog("MyClearScript was not found. Critical error.");
                    yield break;
                }

                var myClearScript = _myClearScript.GetValue(__instance);

                if (myClearScript == null)
                {
                    LoggingHelper.CriticalErrorLog("MyClearScript was able to get value. Critical error.");
                    yield break;
                }
                
                // Getting the method of the myClearScript

                MethodInfo _refreshVideo = myClearScript.GetType().GetMethod("RefreshVideo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                
                if (_refreshVideo == null)
                {
                    LoggingHelper.CriticalErrorLog("RefreshVideo was null unable of calling. Critical error.");
                    yield break;
                }
                
                _refreshVideo.Invoke(myClearScript, null); //__instance.myClearScript.RefreshVideo();
                
                yield return new WaitForSeconds(0.5f);
                
                __instance.videoPopup.SetActive(true);
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(AudioSamplePlayer), "PlayOrPauseVideo")]
        public static class PlayOrPauseVideoPatch
        {
            /// <summary>
            /// This functions plays the video in the video GUI. It is patched to handle URLs.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(MethodBase __originalMethod, AudioSamplePlayer __instance)
            {
                if (!(bool) __instance.myVideoPlayer)
                {
                    return false;
                }
                
                FieldInfo _playerCurrentPosition = typeof(AudioSamplePlayer).GetField("playerCurrentPosition", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

                if (_playerCurrentPosition == null)
                {
                    LoggingHelper.ErrorLog("CurrentPosition was null. Calling original function.");
                    return true;
                }
                
                if (__instance.myVideoPlayer.isPlaying)
                {
                    __instance.myVideoPlayer.Pause();
                    
                    // OLD: __instance.playerCurrentPosition = __instance.playerTracker.transform.localPosition;
                    _playerCurrentPosition.SetValue(__instance, __instance.playerTracker.transform.localPosition); 
                    
                    __instance.StopAllCoroutines();
                }
                else
                {
                    MelonCoroutines.Start(HandleURLVideoBetter(__instance, _playerCurrentPosition));
                }
                
                return false; // Skip the original function
            }

            private static IEnumerator HandleURLVideoBetter(AudioSamplePlayer __instance, FieldInfo _playerCurrentPosition)
            {
                __instance.myVideoPlayer.Play();
                
                if (__instance.myVideoPlayer.time == 0.0 && __instance.playerTracker.transform.localPosition == __instance.playerStartPosition)
                {
                    if (__instance.myVideoPlayer.clip != null)
                    {
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker, __instance.playerStartPosition, __instance.playerEndPosition, (float)__instance.myVideoPlayer.clip.length));
                    }
                    else if (!string.IsNullOrEmpty(__instance.myVideoPlayer.url)) // Url is provided.
                    {
                        yield return WaitForPrepare(__instance.myVideoPlayer);
                        
                        // Compute the duration correctly
                        float duration = __instance.myVideoPlayer.frameCount / __instance.myVideoPlayer.frameRate;
                        
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker, __instance.playerStartPosition, __instance.playerEndPosition, duration));
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
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker,(Vector3) _playerCurrentPosition.GetValue(__instance) , __instance.playerEndPosition,   
                            (float) __instance.myVideoPlayer.clip.length - (float) __instance.myVideoPlayer.time));
                    }
                    else if (!string.IsNullOrEmpty(__instance.myVideoPlayer.url)) // Url is provided.
                    {
                        yield return WaitForPrepare(__instance.myVideoPlayer);
                        
                        // Compute the duration correctly
                        float duration = __instance.myVideoPlayer.frameCount / __instance.myVideoPlayer.frameRate;
                        
                        // OLD: __instance.playerCurrentPosition
                        yield return __instance.StartCoroutine(__instance.MoveOverSeconds(__instance.playerTracker,(Vector3) _playerCurrentPosition.GetValue(__instance) , __instance.playerEndPosition,   
                            duration - (float) __instance.myVideoPlayer.time));
                    }
                    else
                    {
                        LoggingHelper.ErrorLog("Unable of playing video as the URL and the Clip are null.");
                    }
                }
            }
            
            private static IEnumerator WaitForPrepare(VideoPlayer vp)
            {
                vp.Prepare();

                while (!vp.isPrepared)
                    yield return null;
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(AudioSamplePlayer), "Update")]
        public static class UpdatePatch
        {
            /// <summary>
            /// This functions handles multiple functionality for the player. It is patched to work with URLs.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(AudioSamplePlayer __instance)
            {
                if ((bool) __instance.myAudioSource 
                    && __instance.myAudioSource.isPlaying || (bool) __instance.myVideoPlayer 
                    && __instance.myVideoPlayer.isPlaying)
                {
                    __instance.myImage.sprite = __instance.stopSprite;
                    __instance.audioLabelText.SetActive(false);
                }
                else
                {
                    __instance.myImage.sprite = __instance.playSprite;
                    __instance.audioLabelText.SetActive(true);
                    if ((bool) __instance.myAudioSource && __instance.playerTracker.transform.localPosition != __instance.playerStartPosition)
                    {
                        __instance.StopAllCoroutines();
                        __instance.playerTracker.transform.localPosition = __instance.playerStartPosition;
                    }
                }
                
                if (!__instance.scrubbing)
                {
                    return false;
                }
                
                if ((bool) __instance.myVideoPlayer && __instance.myVideoPlayer.isPlaying)
                {
                    __instance.StopAllCoroutines();
                    __instance.myVideoPlayer.Pause();
                }

                if (Camera.main == null)
                {
                    LoggingHelper.ErrorLog("Camera missing! Calling original function!");
                    return true;
                }
                
                __instance.playerTracker.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, __instance.playerTracker.transform.position.y, __instance.playerTracker.transform.position.z);
                
                if (__instance.playerTracker.transform.localPosition.x < __instance.playerStartPosition.x)
                {
                    __instance.playerTracker.transform.localPosition = __instance.playerStartPosition;
                }
                
                if (__instance.playerTracker.transform.localPosition.x > __instance.playerEndPosition.x)
                {
                    __instance.playerTracker.transform.localPosition = __instance.playerEndPosition;
                }
                
                float num = (__instance.playerTracker.transform.localPosition.x - __instance.playerStartPosition.x) / (__instance.playerEndPosition.x - __instance.playerStartPosition.x);
                
                FieldInfo playerCurrentPosition = typeof(AudioSamplePlayer).GetField("playerCurrentPosition", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

                if (playerCurrentPosition == null)
                {
                    LoggingHelper.ErrorLog("CurrentPosition was null. Calling original function.");
                    return true;
                }
                
                if ((bool) __instance.myVideoPlayer)
                {
                    MelonCoroutines.Start(HandleURLVideoBetter(__instance, playerCurrentPosition, num));
                }
                
                if ((bool) __instance.myAudioSource)
                {
                    __instance.myAudioSource.time = __instance.myAudioSource.clip.length * num;
                    
                    // OLD: __instance.playerCurrentPosition = __instance.playerTracker.transform.localPosition;
                    playerCurrentPosition.SetValue(__instance, __instance.playerTracker.transform.localPosition); 
                }
                
                return false; // Skip the original function
            }

            private static IEnumerator HandleURLVideoBetter(AudioSamplePlayer __instance,
                FieldInfo _playerCurrentPosition, float num)
            {
                if (__instance.myVideoPlayer.clip != null)
                {
                    __instance.myVideoPlayer.time = __instance.myVideoPlayer.clip.length * num;
                }
                else if (!string.IsNullOrEmpty(__instance.myVideoPlayer.url)) // URL Provided
                {
                    yield return WaitForPrepare(__instance.myVideoPlayer);

                    float duration = __instance.myVideoPlayer.frameCount / __instance.myVideoPlayer.frameRate;
                    
                    __instance.myVideoPlayer.time = duration * num;
                }
                else
                {
                    LoggingHelper.ErrorLog("No URL or Clip provided for video player in update function! Critical error!");
                    yield break;
                }
                
                // OLD: __instance.playerCurrentPosition = __instance.playerTracker.transform.localPosition;
                _playerCurrentPosition.SetValue(__instance, __instance.playerTracker.transform.localPosition);
            }
            
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