using MelonLoader;
using NewSafetyHelp.src.AudioHandler;
using NewSafetyHelp.src.EntryManager;
using NewSafetyHelp.src.JSONParsing;
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

                foreach (EntryExtraInfo item in ParseMonster.entriesExtraInfo)
                {
                    if (__instance.currentCustomCaller.callerMonster.monsterName == item.Name || __instance.currentCustomCaller.callerMonster.monsterID == item.ID) // We found an entry to replace the audio for.
                    {
                        __instance.currentCustomCaller.callerClip = item.callerClip;
                    }
                }
            }
        }
    }
}
