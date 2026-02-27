using System.Reflection;
using NewSafetyHelp.CustomCampaignPatches;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NewSafetyHelp.CallerPatches.UI.AnimatedEntry
{
    public static class MainCanvasEntry
    {
        public static GameObject GetEntryContent()
        {
            return GameObject.Find("MainCanvas").transform.Find("Panel").transform.Find("MainEntryScrollWindow").transform.Find("Viewport").transform.Find("Content").gameObject;
        }

        public static GameObject GetEntryPortrait()
        {
            return GetEntryContent().transform.Find("Portrait").gameObject;
        }
        
        public static GameObject CreateAnimatedPortrait()
        {
            GameObject portrait = GetEntryPortrait();
            GameObject portraitAnimated = Object.Instantiate(portrait, portrait.transform);

            portraitAnimated.transform.localPosition = Vector3.zero;

            // Add updated texture
            Object.DestroyImmediate(portraitAnimated.GetComponent<Image>());
            
            RawImage rawImageComponent = portraitAnimated.AddComponent<RawImage>();
            rawImageComponent.raycastTarget = false;

            // Add video player.
            VideoPlayer videoPlayerComponent = portraitAnimated.AddComponent<VideoPlayer>();

            videoPlayerComponent.playOnAwake = true;
            videoPlayerComponent.waitForFirstFrame = true;
            videoPlayerComponent.isLooping = true;
            videoPlayerComponent.renderMode = VideoRenderMode.RenderTexture;
            videoPlayerComponent.aspectRatio = VideoAspectRatio.FitInside;
            
            var aspectFitter = portraitAnimated.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            
            // Make render texture be the RawImage texture.

            videoPlayerComponent.prepareCompleted += (videoPlayer) =>
            {   
                RenderTexture renderTexture = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 0);
                renderTexture.Create();
                
                videoPlayer.targetTexture = renderTexture;
                rawImageComponent.texture = renderTexture;
                
                float ratio = (float)videoPlayer.width / videoPlayer.height;
                aspectFitter.aspectRatio = ratio;
            };

            return portraitAnimated;
        }

        public static void UpdateVisibilityNormalEntryPortrait(bool showEntryPortrait = false)
        {
            GetEntryPortrait().GetComponent<Image>().enabled = showEntryPortrait;
        }

        public static void SetVideoUrl(GameObject animatedPortrait, string url)
        {
            VideoPlayer videoPlayerComponent = animatedPortrait.GetComponent<VideoPlayer>();

            UpdateVisibilityNormalEntryPortrait();
            
            videoPlayerComponent.Stop();
            
            if(videoPlayerComponent.targetTexture != null)
            {
                videoPlayerComponent.targetTexture.Release();
                Object.Destroy(videoPlayerComponent.targetTexture);
            }
            
            videoPlayerComponent.url = url;
            
            // Activate the portrait
            animatedPortrait.SetActive(true);
        }
        
        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "Start")]
        public static class StartPatch
        {
            /// <summary>
            /// Patches the start function 
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MainCanvasBehavior __instance)
            {
                FieldInfo shakeAnimationString = typeof(MainCanvasBehavior).GetField("shakeAnimationString", BindingFlags.NonPublic | BindingFlags.Instance);

                if (shakeAnimationString == null)
                {
                    LoggingHelper.ErrorLog("'shakeAnimationString' not found. Calling original function.");
                    return true;
                }
                
                __instance.StartCoroutine(__instance.StartSoftwareRoutine());
                
                if (GlobalVariables.arcadeMode && (bool) (Object) __instance.callTimer)
                {
                    __instance.callTimer.SetActive(true);
                    __instance.livesPanel.SetActive(true);
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    GameObject animatedPortrait = CreateAnimatedPortrait();
                    SetVideoUrl(animatedPortrait,
                        "C:/Program Files (x86)/Steam/steamapps/common/Home Safety Hotline/UserData/NewSafetyHelp/TESHSH/burger_cheese_butter.mp4");
                }

                if (!GlobalVariables.isXmasDLC)
                {
                    return false;
                }
                
                // OLD: __instance.shakeAnimationString = "xmasShake";
                shakeAnimationString.SetValue(__instance, "xmasShake");
                
                return false; // Skip function with false.
            }
        }
    }
}