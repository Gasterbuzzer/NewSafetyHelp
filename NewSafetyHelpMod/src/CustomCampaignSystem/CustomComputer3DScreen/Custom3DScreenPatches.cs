using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen
{
    public static class Custom3DScreenPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(StartMenuBehavior), "Start")]
        public static class StartMenuPatches
        {
            /// <summary>
            /// Changes the update to ignore any key presses.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static void Prefix(StartMenuBehavior __instance)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    Computer3DScreen computer3DScreen = Computer3DScreenHelper.Pick3DComputerScreen();

                    if (computer3DScreen != null)
                    {
                        LoggingHelper.TestLog("Test");
                        
                        if (computer3DScreen.MainLightColor.HasChanged)
                        {
                            GameObject.Find("Directional Light").GetComponent<Light>().color =
                                computer3DScreen.MainLightColor.Data;
                        }
                    }
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(StartMenuBehavior), "Update")]
        public static class UpdateMenuTest
        {
            /// <summary>
            /// Changes the update to ignore any key presses.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(StartMenuBehavior __instance)
            {
                // TODO: REMOVE BEFORE GIVING IT TO SOMEONE; OR ELSE RIP.
                return false;
            }
        }
    }
}