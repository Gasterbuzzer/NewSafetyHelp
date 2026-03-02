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
        // Public reference to the animated portrait.
        private static GameObject animatedEntryPortrait;
        private static GameObject animatedCallerPortrait;

        private static GameObject GetEntryPortrait()
        {
            return GameObject.Find("MainCanvas").transform.Find("Panel").transform.Find("MainEntryScrollWindow").transform.Find("Viewport").transform.Find("Content").transform.Find("Portrait").gameObject;
        }
        
        /// <summary>
        /// Finds the animated caller portrait.
        /// </summary>
        /// <returns></returns>
        private static GameObject GetCallerPortrait()
        {
            return GameObject.Find("MainCanvas").transform.Find("CallPopup").transform.Find("CurrentCall").transform.Find("CallerPortrait").gameObject;
        }
        
        /// <summary>
        /// Updates the visibility of the normal entry portrait to be visible or not.
        /// </summary>
        /// <param name="showEntryPortrait"></param>
        private static void UpdateVisibilityNormalEntryPortrait(bool showEntryPortrait = false)
        {
            GetEntryPortrait().GetComponent<Image>().enabled = showEntryPortrait;
        }
        
        /// <summary>
        /// Updates the visibility of the normal caller portrait.
        /// </summary>
        /// <param name="showEntryPortrait"></param>
        private static void UpdateVisibilityCallerPortrait(bool showEntryPortrait = false)
        {
            GetCallerPortrait().GetComponent<Image>().enabled = showEntryPortrait;
        }
        
        /// <summary>
        /// Sets the video URL of the entry animated portrait.
        /// </summary>
        /// <param name="url"></param>
        public static void SetVideoUrlMainCanvas(string url)
        {
            UpdateVisibilityNormalEntryPortrait();
            SetVideoUrl(url, animatedEntryPortrait);
        }
        
        /// <summary>
        /// Sets the video url for the caller portrait.
        /// </summary>
        /// <param name="url"></param>
        public static void SetVideoUrlCaller(string url)
        {
            UpdateVisibilityCallerPortrait();
            SetVideoUrl(url, animatedCallerPortrait);
        }
        
        /// <summary>
        /// Sets the url to the given to url to the given animaed portrait.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="animatedImageGameObject"></param>
        private static void SetVideoUrl(string url, GameObject animatedImageGameObject)
        {
            AnimatedImageHelper.SetVideoUrl(url, animatedImageGameObject);
        }

        /// <summary>
        /// Restores the entry portrait.
        /// </summary>
        public static void RestoreEntryPortrait()
        {
            // Show normal portrait again.
            UpdateVisibilityNormalEntryPortrait(true);
            
            RestoreNormalPortrait(animatedEntryPortrait);
        }

        /// <summary>
        /// Restores the caller portrait.
        /// </summary>
        public static void RestoreCallerPortrait()
        {
            // Show normal portrait again.
            UpdateVisibilityCallerPortrait(true);
            
            RestoreNormalPortrait(animatedCallerPortrait);
        }

        /// <summary>
        /// Restores a given animated portrait.
        /// </summary>
        private static void RestoreNormalPortrait(GameObject animatedImageGameObject)
        {
            // Disable video player.
            VideoPlayer videoPlayerComponent = animatedImageGameObject.GetComponent<VideoPlayer>();
            
            videoPlayerComponent.Stop();
            
            if(videoPlayerComponent.targetTexture != null)
            {
                videoPlayerComponent.targetTexture.Release();
                Object.Destroy(videoPlayerComponent.targetTexture);
            }
            
            animatedImageGameObject.SetActive(false);
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
                    if (animatedEntryPortrait == null)
                    {
                        animatedEntryPortrait = AnimatedImageHelper.CreateAnimatedPortrait(GetEntryPortrait());
                    }
                    
                    if (animatedCallerPortrait == null)
                    {
                        animatedCallerPortrait = AnimatedImageHelper.CreateAnimatedPortrait(GetCallerPortrait());
                    }
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