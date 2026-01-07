using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomRingtone;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio.AudioPatches
{
    public static class UISoundPatch
    {
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "Start")]
        public static class StartPatch
        {
            // If we have run the function for the first time.
            // Is used to get the default ringtone.
            private static bool firstRun;
            
            private static RichAudioClip defaultRingtone;
            private static RichAudioClip defaultWarpedRingtone;
            
            /// <summary>
            /// Patches the start function to replace audio clips.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(UISoundController __instance)
            {
                if (!firstRun)
                {
                    defaultRingtone = __instance.phoneCall;
                    defaultWarpedRingtone = __instance.phoneCallWarped;
                    firstRun = true;
                }
                
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign.CustomCampaignModel.CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null! Unable of changing audio clips.");
                        return;
                    }

                    // Ringtone
                    if (customCampaign.customRingtones != null 
                        && customCampaign.customRingtones.Count > 0)
                    {
                        List<CustomRingtone> validRingtonesNormal = new List<CustomRingtone>();
                        List<CustomRingtone> validRingtonesGlitched = new List<CustomRingtone>();

                        // For each ringtone that is valid for this current day, attempt to find all valid.
                        foreach (CustomRingtone customRingtone in customCampaign.customRingtones.Where(c => c.UnlockDay <= GlobalVariables.currentDay))
                        {
                            // If we are only allowed to play on the unlock day.
                            // Then the unlock day must be equal to the current day.
                            
                            if (customRingtone.OnlyOnUnlockDay && customRingtone.UnlockDay != GlobalVariables.currentDay)
                            {
                                continue;
                            }
                            
                            if (customRingtone.IsGlitchedVersion)
                            {
                                validRingtonesGlitched.Add(customRingtone);
                            }
                            else
                            {
                                validRingtonesNormal.Add(customRingtone);
                            }
                        }
                        
                        // Now for each valid ringtone we try to pick one valid.
                        ReplacePhoneCallIfValid(ref __instance.phoneCall, ref validRingtonesNormal,
                            customCampaign.doNotAccountDefaultRingtone, ref defaultRingtone);

                        ReplacePhoneCallIfValid(ref __instance.phoneCallWarped, ref validRingtonesGlitched,
                            customCampaign.doNotAccountDefaultRingtone, ref defaultWarpedRingtone);
                    }
                }
            }

            private static void ReplacePhoneCallIfValid(ref RichAudioClip clipToReplace,
                ref List<CustomRingtone> validRingtones, bool doNotAccountDefaultRingtone,
                ref RichAudioClip gameDefaultRingtone)
            {
                if (validRingtones.Count > 0)
                {
                    if (validRingtones.Any(r => r.AppendRingtone)) // If any ringtones have to be appended. (Random Pick)
                    {
                        int maxExclusive = validRingtones.Count;

                        // If we don't remove the ringtone, we also account for not changing the phone call.
                        if (!doNotAccountDefaultRingtone)
                        {
                            maxExclusive++;
                        }

                        int chosenPhoneCall = Random.Range(0, maxExclusive);
                        
                        MelonLogger.Error($"DEBUG: ChosenPhoneCall: '{chosenPhoneCall}'.\n" +
                                          $"maxExclusive: '{maxExclusive}'.\n" +
                                          $"doNotAccountDefaultRingtone: '{doNotAccountDefaultRingtone}'.\n" +
                                          $"validRingtones.Any(r => r.AppendRingtone): '{validRingtones.Any(r => r.AppendRingtone)}'.\n" +
                                          $"validRingtones.Count '{validRingtones.Count}'.\n");

                        if (doNotAccountDefaultRingtone)
                        {
                            clipToReplace = validRingtones.Where(r => r.AppendRingtone).ElementAt(chosenPhoneCall).RingtoneClip;
                        }
                        else
                        {
                            if (chosenPhoneCall != validRingtones.Count)
                            {
                                clipToReplace = validRingtones.Where(r => r.AppendRingtone).ElementAt(chosenPhoneCall).RingtoneClip;
                            }
                            else
                            {
                                clipToReplace = gameDefaultRingtone;
                            }
                        }
                    }
                    else // No appends, we simply use the first one.
                    {
                        clipToReplace = validRingtones[0].RingtoneClip;
                    }
                }
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayGlitchSound")]
        public static class PlayGlitchSoundPatch
        {
            /// <summary>
            /// Patches the play glitch sound to be more conform with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(UISoundController __instance)
            {
                if (__instance.myGlitchLoopingSource.isPlaying)
                {
                    __instance.myGlitchLoopingSource.volume = 0.0f;
                    __instance.myGlitchLoopingSource.Stop();
                }
                
                if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign) // Don't play scary sound in custom campaign.
                {
                    __instance.myGlitchLoopingSource.clip = __instance.scaryGlitch.clip;
                    __instance.myGlitchLoopingSource.volume = __instance.scaryGlitch.volume;
                }
                else
                {
                    __instance.myGlitchLoopingSource.clip = __instance.normalGlitch.clip;
                    __instance.myGlitchLoopingSource.volume = __instance.normalGlitch.volume;
                }
                __instance.myGlitchLoopingSource.Play();
                
                return false; // Skip function with false.
            }
        }
        
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayGlitchSoundWhileGlitchAnimationIsPlaying")]
        public static class PlayGlitchSoundWhileGlitchAnimationIsPlayingPatch
        {
            /// <summary>
            /// Patches the play glitch sound with glitch animation to work better with custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="__result"> Result of the function. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(UISoundController __instance, ref IEnumerator __result)
            {
                __result = PatchedPlayGlitchSound(__instance);
                
                return false; // Skip function with false.
            }

            private static IEnumerator PatchedPlayGlitchSound(UISoundController __instance)
            {
                UISoundController uiSoundController = __instance;
                
                if (!uiSoundController.myGlitchLoopingSource.isPlaying)
                {
                    if (GlobalVariables.currentDay == 7 && !CustomCampaignGlobal.InCustomCampaign) // Don't play glitch looping source if in custom campaign.
                    {
                        uiSoundController.myGlitchLoopingSource.clip = uiSoundController.scaryGlitch.clip;
                        uiSoundController.myGlitchLoopingSource.volume = uiSoundController.scaryGlitch.volume;
                    }
                    else
                    {
                        uiSoundController.myGlitchLoopingSource.clip = uiSoundController.normalGlitch.clip;
                        uiSoundController.myGlitchLoopingSource.volume = uiSoundController.normalGlitch.volume;
                    }
                    uiSoundController.myGlitchLoopingSource.Play();
                }

                if (Camera.main == null)
                {
                    MelonLogger.Error("UNITY ERROR: Camera is null! Cannot play glitch effect.");
                    yield break;
                }
                
                while (Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips.Length == 0)
                {
                    MelonLogger.Msg( "UNITY: Waiting for animationClips[] to populate.");
                    yield return null;
                }

                while (Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips.Length != 0)
                {
                    yield return null;
                }
                    
                uiSoundController.StartCoroutine(uiSoundController.FadeOutLoopingSound(uiSoundController.myGlitchLoopingSource, 5f));
            }
        }
    }
}