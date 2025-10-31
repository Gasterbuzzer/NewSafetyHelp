using System.Reflection;

namespace NewSafetyHelp.Audio.Music
{
    public static class MusicControllerPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartRandomMusic")]
        public static class StartRandomMusicPatch
        {

            /// <summary>
            /// Patches the start random music to consider custom music.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(MethodBase __originalMethod, MusicController __instance)
            {

                return true;
                
                
                return false; // Skip function with false.
            }
        }
    }
}