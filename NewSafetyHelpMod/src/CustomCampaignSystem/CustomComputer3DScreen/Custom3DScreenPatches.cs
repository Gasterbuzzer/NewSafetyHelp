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
                        /*
                         * Lights
                         */

                        if (computer3DScreen.MainLightColor.HasChanged)
                        {
                            GameObject.Find("Point Light (1)").GetComponent<Light>().color =
                                computer3DScreen.MainLightColor.Data;
                        }

                        if (computer3DScreen.DisableMainLight.HasChanged)
                        {
                            GameObject.Find("Point Light (1)").SetActive(!computer3DScreen.DisableMainLight.Data);
                        }

                        if (computer3DScreen.SecondMainLightColor.HasChanged)
                        {
                            GameObject.Find("Directional Light").GetComponent<Light>().color =
                                computer3DScreen.SecondMainLightColor.Data;
                        }

                        if (computer3DScreen.DisableSecondMainLight.HasChanged)
                        {
                            GameObject.Find("Directional Light")
                                .SetActive(!computer3DScreen.DisableSecondMainLight.Data);
                        }
                        
                        if (computer3DScreen.DeskLightColor.HasChanged)
                        {
                            GameObject.Find("Point Light (3)").GetComponent<Light>().color =
                                computer3DScreen.DeskLightColor.Data;
                        }

                        if (computer3DScreen.DisableDeskLight.HasChanged)
                        {
                            GameObject.Find("Point Light (3)")
                                .SetActive(!computer3DScreen.DisableDeskLight.Data);
                        }
                        
                        if (computer3DScreen.KeyboardLightColor.HasChanged)
                        {
                            GameObject.Find("Point Light").GetComponent<Light>().color =
                                computer3DScreen.KeyboardLightColor.Data;
                        }

                        if (computer3DScreen.DisableKeyboardLight.HasChanged)
                        {
                            GameObject.Find("Point Light")
                                .SetActive(!computer3DScreen.DisableKeyboardLight.Data);
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