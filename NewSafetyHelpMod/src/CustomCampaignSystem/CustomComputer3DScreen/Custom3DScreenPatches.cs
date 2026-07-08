namespace NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen
{
    public static class Custom3DScreenPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(StartMenuBehavior), "Update")]
        public static class StartMenuTest
        {
            /// <summary>
            /// Changes the update to ignore any key presses.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(StartMenuBehavior __instance)
            {
                // TODO: REMOVE BEFORE GIVING IT TO SOMEONE; OR ELSE RIP.
                //return false;

                return true;
            }
        }
    }
}