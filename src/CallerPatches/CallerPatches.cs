using MelonLoader;
using NewSafetyHelp.src.AudioHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSafetyHelp.src.CallerPatches
{
    public class CallerPatches
    {
        // Patches the class when it opens to also update the monster list, since due to our coroutines problem.
        [HarmonyLib.HarmonyPatch(typeof(CallerController), "CreateCustomCaller", new Type[] { })]
        public static class UpdateListDesktop
        {

            /// <summary>
            /// Update the list when opening.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
            /// <param name="__instance"> Caller of function. </param>
            private static void Postfix(MethodBase __originalMethod, CallerController __instance)
            {
                if (__instance.currentCustomCaller.callerMonster.monsterName != "Frozen Pipes") // We are opening the EntryBrowser, lets update
                {

                    MelonCoroutines.Start(
                    AudioImport.LoadAudio
                    (
                        (myReturnValue) =>
                        {
                            __instance.currentCustomCaller.callerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                        },
                        "C:\\Users\\julia\\Desktop\\Projects\\Mods\\EasyIrritatedMind\\Audio\\Lo-Fi Music Pack Vol. 2\\Lo-Fi Music Pack Vol. 2\\A Serious Matter (RT 3.429)\\Lo-Fi Vol2 A Serious Matter Cut 30.wav", AudioType.WAV)
                    );
                }
            }
        }
    }
}
