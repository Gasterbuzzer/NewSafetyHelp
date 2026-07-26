using System;
using System.Collections;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Callers.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.EntryManager.EntryData;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class EntryLoadingBuxFix
    {
        // Animator Hash
        private static readonly int ScreenLoad = Animator.StringToHash("ScreenLoad");
        private static readonly int Glitch = Animator.StringToHash("glitch");

        private static object easterEggCoroutine;
        private static bool hasActiveEasterEgg;

        [HarmonyLib.HarmonyPatch(typeof(MainCanvasBehavior), "UpdateSelectedEntry", typeof(MonsterProfile))]
        public static class MainCanvasUpdateSelectedEntryPatch
        {
            private static readonly MethodInfo IsNetworkDown = typeof(MainCanvasBehavior).GetMethod("IsNetworkDown",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            private static readonly MethodInfo SelectMonsterPortrait = typeof(MainCanvasBehavior).GetMethod(
                "SelectMonsterPortrait",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            private static readonly MethodInfo UpdateLayoutGroupMethod = typeof(MainCanvasBehavior).GetMethod(
                "UpdateLayoutGroup",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            /// <summary>
            /// UpdateSelectedEntry patch to fix the double loading bug.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="profile">Profile selected to be shown.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MainCanvasBehavior __instance, ref MonsterProfile profile)
            {
                if (SelectMonsterPortrait == null || IsNetworkDown == null || UpdateLayoutGroupMethod == null)
                {
                    LoggingHelper.ReflectionError(nameof(IsNetworkDown),
                        nameof(SelectMonsterPortrait), nameof(UpdateLayoutGroupMethod));
                    return true;
                }

                // If the same profile gets selected, simply ignore it.
                if (__instance.selectedMonsterProfile != null
                    && __instance.selectedMonsterProfile.Equals(profile))
                {
                    return false;
                }

                __instance.selectedMonsterProfile = profile;

                // Original: __instance.IsNetworkDown()
                if ((bool)IsNetworkDown.Invoke(__instance, null))
                {
                    profile = __instance.errorProfile;
                    __instance.cameraAnimator.SetTrigger(Glitch);
                }

                __instance.selectedMonsterPortraitImage.gameObject.SetActive(false);
                __instance.monsterAudioSamplePlayer.SetActive(false);

                if (profile.monsterPortrait != null)
                {
                    __instance.selectedMonsterPortraitImage.gameObject.SetActive(true);

                    // Original: __instance.SelectMonsterPortrait(profile);
                    __instance.selectedMonsterPortraitImage.sprite =
                        (Sprite)SelectMonsterPortrait.Invoke(__instance, new object[] { profile });

                    if (hasActiveEasterEgg)
                    {
                        __instance.selectedMonsterPortraitImage.color = Color.white;
                        MelonCoroutines.Stop(easterEggCoroutine);
                        hasActiveEasterEgg = false;
                    }

                    if (__instance.selectedMonsterProfile.name == "Boggart")
                    {
                        easterEggCoroutine = MelonCoroutines.Start(EasterEgg(() =>
                        {
                            LoggingHelper.DebugLog("Easter Egg!");
                            __instance.selectedMonsterPortraitImage.color = Color.red;
                        }));
                    }
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    EntryMetadata currentlySelectedEntry =
                        CustomCampaignGlobal.GetEntryFromActiveCampaign(profile.monsterName);

                    if (currentlySelectedEntry != null
                        && currentlySelectedEntry.IsVideoPortrait)
                    {
                        __instance.selectedMonsterPortraitImage.gameObject.SetActive(true);

                        MainCanvasEntry.SetVideoUrl(currentlySelectedEntry.VideoUrlPortrait,
                            MainCanvasEntry.PortraitType.ENTRY);

                        if (currentlySelectedEntry.VideoPortraitShouldLoop.HasChanged)
                        {
                            MainCanvasEntry.SetVideoLoop(currentlySelectedEntry.VideoPortraitShouldLoop.Data,
                                MainCanvasEntry.PortraitType.ENTRY);
                        }
                    }
                    else
                    {
                        MainCanvasEntry.RestorePortrait(MainCanvasEntry.PortraitType.ENTRY);

                        MainCanvasEntry.SetVideoLoop(true, MainCanvasEntry.PortraitType.ENTRY);
                    }
                }

                if (profile.monsterAudioClip != null)
                {
                    __instance.monsterAudioSamplePlayer.SetActive(true);
                }

                __instance.selectedMonsterTitle.text = profile.monsterName;
                __instance.selectedMonsterDescription.text = profile.monsterDescription;

                // Fix loading bug that causes the animation to play twice.
                AnimatorStateInfo state = __instance.screenLoader.GetCurrentAnimatorStateInfo(0);

                if (!state.IsName("ScreenLoad")
                    && !__instance.screenLoader.IsInTransition(0))
                {
                    __instance.screenLoader.SetTrigger(ScreenLoad);
                }
                else
                {
                    __instance.screenLoader.ResetTrigger(ScreenLoad);
                    __instance.screenLoader.Play(ScreenLoad, 0, 0f);
                }

                // Original: __instance.UpdateLayoutGroup(__instance.mainEntryLayoutGroup)
                IEnumerator UpdateLayoutGroup =
                    (IEnumerator)UpdateLayoutGroupMethod.Invoke(__instance,
                        new object[] { __instance.mainEntryLayoutGroup });
                __instance.StartCoroutine(UpdateLayoutGroup);

                if (GlobalVariables.UISoundControllerScript != null)
                {
                    GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                }

                __instance.selectedEntryScrollbar.value = 1f;

                return false; // Skip original
            }

            private static IEnumerator EasterEgg(Action updateSpriteAction)
            {
                hasActiveEasterEgg = true;

                yield return new WaitForSeconds(603);

                updateSpriteAction();
            }
        }

        /// <summary>
        /// For EntryCanvas, not for MainCanvasBehavior like above. If you have a change, please update both.
        /// </summary>
        [HarmonyLib.HarmonyPatch(typeof(EntryCanvasStandaloneBehavior), "UpdateSelectedEntry", typeof(MonsterProfile))]
        public static class EntryCanvasUpdateSelectedEntryPatch
        {
            private static readonly MethodInfo SelectMonsterPortrait = typeof(EntryCanvasStandaloneBehavior).GetMethod(
                "SelectMonsterPortrait",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            private static readonly MethodInfo UpdateLayoutGroupMethod =
                typeof(EntryCanvasStandaloneBehavior).GetMethod("UpdateLayoutGroup",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            /// <summary>
            /// UpdateSelectedEntry patch to fix the double loading bug.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="profile">Profile selected to be shown.</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(EntryCanvasStandaloneBehavior __instance, ref MonsterProfile profile)
            {
                if (SelectMonsterPortrait == null || UpdateLayoutGroupMethod == null)
                {
                    LoggingHelper.ReflectionError(nameof(SelectMonsterPortrait),
                        nameof(UpdateLayoutGroupMethod));
                    return true;
                }

                // If the same profile gets selected, simply ignore it.
                if (__instance.selectedMonsterProfile != null
                    && __instance.selectedMonsterProfile.Equals(profile))
                {
                    return false;
                }

                __instance.selectedMonsterProfile = profile;
                __instance.selectedMonsterPortraitImage.gameObject.SetActive(false);
                __instance.monsterAudioSamplePlayer.SetActive(false);

                if (profile.monsterPortrait != null)
                {
                    __instance.selectedMonsterPortraitImage.gameObject.SetActive(true);

                    // Original: __instance.SelectMonsterPortrait(profile);
                    __instance.selectedMonsterPortraitImage.sprite =
                        (Sprite)SelectMonsterPortrait.Invoke(__instance, new object[] { profile });
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    EntryMetadata currentlySelectedEntry =
                        CustomCampaignGlobal.GetEntryFromActiveCampaign(profile.monsterName);

                    if (currentlySelectedEntry != null
                        && currentlySelectedEntry.IsVideoPortrait)
                    {
                        __instance.selectedMonsterPortraitImage.gameObject.SetActive(true);

                        EntryCanvasStandaloneEntry.SetVideoUrlEntryStandaloneCanvas(currentlySelectedEntry
                            .VideoUrlPortrait);
                    }
                    else
                    {
                        EntryCanvasStandaloneEntry.RestoreNormalPortrait();
                    }
                }

                if (profile.monsterAudioClip != null)
                {
                    __instance.monsterAudioSamplePlayer.SetActive(true);
                }

                __instance.selectedMonsterTitle.text = profile.monsterName;
                __instance.selectedMonsterDescription.text = profile.monsterDescription;

                // Fix: Check if our current state isn't playing the animation already.
                AnimatorStateInfo state = __instance.screenLoader.GetCurrentAnimatorStateInfo(0);

                if (!state.IsName("ScreenLoad")
                    && !__instance.screenLoader.IsInTransition(0))
                {
                    __instance.screenLoader.SetTrigger(ScreenLoad);
                }
                else
                {
                    __instance.screenLoader.ResetTrigger(ScreenLoad);
                    __instance.screenLoader.Play(ScreenLoad, 0, 0f);
                }

                // Original: __instance.UpdateLayoutGroup(__instance.mainEntryLayoutGroup)
                IEnumerator UpdateLayoutGroup =
                    (IEnumerator)UpdateLayoutGroupMethod.Invoke(__instance,
                        new object[] { __instance.mainEntryLayoutGroup });
                __instance.StartCoroutine(UpdateLayoutGroup);

                if (GlobalVariables.UISoundControllerScript != null)
                {
                    GlobalVariables.UISoundControllerScript.myMonsterSampleAudioSource.Stop();
                }

                __instance.selectedEntryScrollbar.value = 1f;

                return false; // Skip original
            }
        }
    }
}