using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using UnityEngine;

namespace NewSafetyHelp.CustomVideos
{
    public static class VideoPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(VideoExecutableFile), "PlayVideoRoutine", new Type[] { })]
        public static class PlayVideoRoutinePatch
        {
            /// <summary>
            /// This functions plays the video. It was changed to also support URL plays.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Coroutine to be called. </param>
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, VideoExecutableFile __instance, ref IEnumerator __result)
            {

                __result = PlayVideoRoutineCoroutine(__instance);
                
                return false; // Skip the original function
            }

            public static IEnumerator PlayVideoRoutineCoroutine(VideoExecutableFile __instance)
            {
                __instance.notification.SetActive(false);

                if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("CustomCampaignExtraInfo is null. Critical error.");
                        yield break;
                    }

                    CustomVideoExtraInfo correctVideo = null;
                    
                    if (customCampaign.allDesktopVideos.Exists(video => video.desktopName + video.videoURL == __instance.gameObject.name))
                    {
                        correctVideo = customCampaign.allDesktopVideos.Find(video => video.desktopName + video.videoURL == __instance.gameObject.name);
                    }
                    else
                    {
                        MelonLogger.Error("Unable of finding the video show! Critical error.");
                        yield break;
                    }

                    if (correctVideo != null)
                    {
                        __instance.videoPlayer.url = correctVideo.videoURL;
                    }
                }
                else // Main Campaign
                {
                    __instance.videoPlayer.clip = __instance.videoClip;
                }
                
                FieldInfo _myClearScript = typeof(VideoExecutableFile).GetField("myClearScript", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

                if (_myClearScript == null)
                {
                    MelonLogger.Error("ERROR: MyClearScript was not found. Critical error.");
                    yield break;
                }

                var myClearScript = _myClearScript.GetValue(__instance);

                if (myClearScript == null)
                {
                    MelonLogger.Error("ERROR: MyClearScript was able to get value. Critical error.");
                    yield break;
                }
                
                // Getting the method of the myClearScript

                MethodInfo _refreshVideo = myClearScript.GetType().GetMethod("RefreshVideo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                
                if (_refreshVideo == null)
                {
                    MelonLogger.Error("ERROR: RefreshVideo was null unable of calling. Critical error.");
                    yield break;
                }
                
                _refreshVideo.Invoke(myClearScript, null); //__instance.myClearScript.RefreshVideo();
                
                yield return new WaitForSeconds(0.5f);
                
                __instance.videoPopup.SetActive(true);
            }
        }
    }
}