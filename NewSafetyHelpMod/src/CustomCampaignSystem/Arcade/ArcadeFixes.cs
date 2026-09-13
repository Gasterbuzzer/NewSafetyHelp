using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Arcade
{
    public static class ArcadeFixes
    {
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "GenerateRandomCallText")]
        public static class GenerateRandomCallTextPatch
        {
            // ReSharper disable once UnusedMember.Global
            /// <summary>
            /// Prefix that makes sure that when we are in a custom campaign that the custom arcade logic is applied.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="__result">Result of the function.</param>
            /// <param name="monster">Monster that is calling.</param>
            /// <returns></returns>
            public static bool Prefix(CallerController __instance, ref string __result, ref MonsterProfile monster)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    int index1 = Random.Range(0, monster.arcadeCalls.Length);
                    int index2 = Random.Range(0, __instance.randomCallerIntros.Length);
                    int index3 = Random.Range(0, __instance.randomCallerOutros.Length);

                    string arcadeCall = $"No arcade call for '{monster.monsterName}'";

                    if (monster.arcadeCalls.Length >= index1 + 1)
                    {
                        arcadeCall = monster.arcadeCalls[index1];
                    }

                    __result =
                        $"{__instance.randomCallerIntros[index2]} {arcadeCall} {__instance.randomCallerOutros[index3]}";
                }
                // Main Campaign
                else
                {
                    int index1 = Random.Range(0, monster.arcadeCalls.Length);
                    int index2 = Random.Range(0, __instance.randomCallerIntros.Length);
                    int index3 = Random.Range(0, __instance.randomCallerOutros.Length);

                    string arcadeCall = monster.arcadeCalls[index1];

                    __result =
                        $"{__instance.randomCallerIntros[index2]} {arcadeCall} {__instance.randomCallerOutros[index3]}";
                }

                return false; // Skip original function.
            }
        }
    }
}