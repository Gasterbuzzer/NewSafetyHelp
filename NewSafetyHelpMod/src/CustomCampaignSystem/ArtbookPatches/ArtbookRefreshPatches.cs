using System.Reflection;
using Steamworks;

namespace NewSafetyHelp.CustomCampaignSystem.ArtbookPatches
{
    public static class ArtbookRefreshPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(ArtbookPopupBehavior), "RefreshPageContent")]
        public static class ArtbookRefreshPageContentPatch
        {
            private static readonly FieldInfo CurrentPage = typeof(ArtbookPopupBehavior).GetField("currentPage",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            /// <summary>
            /// Patches the 'RefreshPageContent' function to not fail if an image wasn't provided.
            /// </summary>
            /// <param name="__instance"> Instance calling the function. </param>
            // ReSharper disable once UnusedMember.Global
            public static bool Prefix(ArtbookPopupBehavior __instance)
            {
                int currentPage = (int) CurrentPage.GetValue(__instance);
                
                // Hide images first.
                __instance.image1.gameObject.SetActive(false);
                __instance.image2.gameObject.SetActive(false);
                
                if (__instance.artbookPages[currentPage].image != null)
                {
                    __instance.image1.gameObject.SetActive(true);
                    __instance.image1.sprite = __instance.artbookPages[currentPage].image;
                }
                
                __instance.descriptionText.text = __instance.artbookPages[currentPage].description;
                __instance.titleText.text = __instance.artbookPages[currentPage].title;
                
                if (__instance.artbookPages[currentPage].image2 != null)
                {
                    __instance.image2.gameObject.SetActive(true);
                    __instance.image2.sprite = __instance.artbookPages[currentPage].image2;
                }
                
                // We only allow the achievement in the base game.
                if (currentPage == __instance.artbookPages.Length - 1
                    && !CustomCampaignGlobal.InCustomCampaign)
                {
                    SteamUserStats.SetAchievement("Artbook");
                    SteamUserStats.StoreStats();
                }
                
                __instance.pageNumText.text = $"{(currentPage + 1).ToString()}/{__instance.artbookPages.Length.ToString()}";
                
                return false; // Skip original function.
            }
        }
    }
}