using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NewSafetyHelp.Callers.UI.AnimatedEntry
{
    public static class MainCanvasEntry
    {
        public enum PortraitType
        {
            ENTRY,
            CALLER,
            LARGE_CALLER,
            CORNER_CALLER
        }
        
        // Public reference to the animated portrait.
        private static GameObject animatedEntryPortrait;
        private static GameObject animatedCallerPortrait;
        private static GameObject animatedCallerLargePortrait;
        private static GameObject animatedCallerCornerPortrait;

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
        /// Updates the visibility of a given portrait to be visible or not.
        /// </summary>
        /// <param name="portraitToUpdate">Portrait to update the visibility of.</param>
        /// <param name="showPortrait">If to show the portrait or not.</param>
        private static void UpdateVisibilityPortrait(PortraitType portraitToUpdate, bool showPortrait = false)
        {
            Image chosenPortrait = null;
            
            switch (portraitToUpdate)
            {
                case PortraitType.ENTRY:
                    chosenPortrait = GetEntryPortrait().GetComponent<Image>();
                    break;
                
                case PortraitType.CALLER:
                    chosenPortrait = GetCallerPortrait().GetComponent<Image>();
                    break;
                
                case PortraitType.LARGE_CALLER:
                    chosenPortrait = GlobalVariables.mainCanvasScript.largeCallerPortrait;
                    break;
                
                case PortraitType.CORNER_CALLER:
                    chosenPortrait = GlobalVariables.mainCanvasScript.callerPortrait;
                    break;
            }

            if (chosenPortrait != null)
            {
                chosenPortrait.enabled = showPortrait;
            }
        }
        
        /// <summary>
        /// Sets the url to the given to url to the given animated portrait.
        /// </summary>
        /// <param name="url"> URL to show as the video. </param>
        /// <param name="chosenPortrait"> Type of portrait this applies to. </param>
        public static void SetVideoUrl(string url, PortraitType chosenPortrait)
        {
            GameObject animatedImageGameObject = null;

            UpdateVisibilityPortrait(chosenPortrait);
            
            switch (chosenPortrait)
            {
                case PortraitType.ENTRY:
                    animatedImageGameObject = animatedEntryPortrait;
                    break;
                
                case PortraitType.CALLER:
                    animatedImageGameObject = animatedCallerPortrait;
                    break;
                
                case PortraitType.LARGE_CALLER:
                    animatedImageGameObject = animatedCallerLargePortrait;
                    break;
                
                case PortraitType.CORNER_CALLER:
                    animatedImageGameObject = animatedCallerCornerPortrait;
                    break;
            }

            if (animatedImageGameObject != null)
            {
                AnimatedImageHelper.SetVideoUrl(url, animatedImageGameObject);
            }
        }
        
        /// <summary>
        /// Restores the normal image portrait.
        /// </summary>
        public static void RestorePortrait(PortraitType chosenPortrait)
        {
            GameObject animatedImageGameObject = null;

            switch (chosenPortrait)
            {
                case PortraitType.ENTRY:
                    animatedImageGameObject = animatedEntryPortrait;
                    break;
                
                case PortraitType.CALLER:
                    animatedImageGameObject = animatedCallerPortrait;
                    break;
                
                case PortraitType.LARGE_CALLER:
                    if (animatedCallerLargePortrait == null)
                    {
                        return;
                    }
                    
                    animatedCallerLargePortrait.SetActive(false);
                    
                    animatedImageGameObject = animatedCallerLargePortrait;
                    
                    break;
                
                case PortraitType.CORNER_CALLER:
                    animatedImageGameObject = animatedCallerCornerPortrait;
                    break;
            }
            
            // Show normal portrait again.
            UpdateVisibilityPortrait(chosenPortrait, true);
            
            if (animatedImageGameObject != null)
            {
                RestoreNormalPortrait(animatedImageGameObject);
            }
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
            /// <param name="__instance"> CALLER of function. </param>
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
                    
                    if (animatedCallerLargePortrait == null)
                    {
                        animatedCallerLargePortrait = AnimatedImageHelper.CreateAnimatedPortrait(
                            GlobalVariables.mainCanvasScript.largeCallerPortrait.gameObject);
                    }
                    
                    if (animatedCallerCornerPortrait == null)
                    {
                        animatedCallerCornerPortrait = AnimatedImageHelper.CreateAnimatedPortrait(
                            GlobalVariables.mainCanvasScript.callerPortrait.gameObject);
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