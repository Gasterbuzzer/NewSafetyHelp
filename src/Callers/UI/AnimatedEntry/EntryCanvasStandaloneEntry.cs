using System.Reflection;
using NewSafetyHelp.CustomCampaignPatches;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NewSafetyHelp.CallerPatches.UI.AnimatedEntry
{
    public static class EntryCanvasStandaloneEntry
    {
        private static GameObject animatedPortrait;
        
        private static GameObject GetEntryPortrait()
        {
            return GameObject.Find("EntryCanvasStandalone").transform.Find("Panel").transform.Find("MainEntryScrollWindow").transform.Find("Viewport").transform.Find("Content").transform.Find("Portrait").gameObject;
        }
        
        private static void UpdateVisibilityNormalEntryPortrait(bool showEntryPortrait = false)
        {
            GetEntryPortrait().GetComponent<Image>().enabled = showEntryPortrait;
        }
        
        public static void SetVideoUrlEntryStandaloneCanvas(string url)
        {
            UpdateVisibilityNormalEntryPortrait();
            
            AnimatedImageHelper.SetVideoUrl(url, animatedPortrait);
        }

        public static void RestoreNormalPortrait()
        {
            // Show normal portrait again.
            UpdateVisibilityNormalEntryPortrait(true);
            
            // Disable video player.
            VideoPlayer videoPlayerComponent = animatedPortrait.GetComponent<VideoPlayer>();
            
            videoPlayerComponent.Stop();
            
            if (videoPlayerComponent.targetTexture != null)
            {
                videoPlayerComponent.targetTexture.Release();
                Object.Destroy(videoPlayerComponent.targetTexture);
            }
            
            animatedPortrait.SetActive(false);
        }
        
        [HarmonyLib.HarmonyPatch(typeof(EntryCanvasStandaloneBehavior), "Start")]
        public static class StartPatch
        {
            private static MethodInfo loadVars = typeof(EntryCanvasStandaloneBehavior).GetMethod("LoadVars", BindingFlags.NonPublic | BindingFlags.Instance);
            private static MethodInfo populateEntriesList = typeof(EntryCanvasStandaloneBehavior).GetMethod("PopulateEntriesList", BindingFlags.NonPublic | BindingFlags.Instance);
            
            /// <summary>
            /// Patches the start function.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(EntryCanvasStandaloneBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign 
                    && animatedPortrait == null)
                {
                    animatedPortrait = AnimatedImageHelper.CreateAnimatedPortrait(GetEntryPortrait());
                }
                
                // OLD: __instance.LoadVars();
                loadVars.Invoke(__instance, null);
                
                // OLD: __instance.PopulateEntriesList();
                populateEntriesList.Invoke(__instance, null);
                
                __instance.CloseWindow();
                
                return false; // Skip function with false.
            }
        }
    }
}