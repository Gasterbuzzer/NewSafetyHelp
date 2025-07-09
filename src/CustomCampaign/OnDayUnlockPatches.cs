using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using Object = System.Object;

namespace NewSafetyHelp.CustomCampaign
{
    public static class OnDayUnlockPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(OnDayUnlock), "OnEnable", new Type[] { })]
        public static class OnEnablePatch
        {

            /// <summary>
            /// Changes the function to work better with custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, OnDayUnlock __instance)
            {
                if (GlobalVariables.arcadeMode)
                {
                    return false;
                }
                
                if (!__instance.enableForArcadeMode)
                {
                    if (GlobalVariables.currentDay < __instance.unlockDay)
                    {
                        __instance.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (PlayerPrefs.HasKey("SavedDayScore" + (__instance.unlockDay - 1).ToString()))
                        {
                            if (PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1).ToString()) < (double) __instance.scoreThresholdToUnlock)
                            {
                                __instance.gameObject.SetActive(false);
                            }
                            else
                            {
                                MelonLogger.Msg($"UNITY LOG: Email unlocked: {__instance.gameObject.name}| Day Checked: {(__instance.unlockDay - 1).ToString()}| Day Score: " +
                                                $"{PlayerPrefs.GetFloat("SavedDayScore" + (__instance.unlockDay - 1).ToString()).ToString()}");
                            }
                        }
                        if (!__instance.beatGameUnlock || !(bool) GlobalVariables.saveManagerScript || GlobalVariables.saveManagerScript.savedGameFinished >= 1 || __instance.xmasUnlock && GlobalVariables.isXmasDLC)
                        {
                            return false;
                        }
                        __instance.gameObject.SetActive(false);
                    }
                }
                else
                {
                    __instance.gameObject.SetActive(false);
                }
                
                return false; // Skip the original function
            }
        }
    }
}