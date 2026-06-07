using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using UnityEngine;

namespace NewSafetyHelp.Audio.AudioPatches
{
    public static class PlaySoundPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(UISoundController), "PlayUISoundLooping", typeof(RichAudioClip),
            typeof(AudioSource))]
        public static class PlayGlitchSoundPatch
        {
            /// <summary>
            /// Patches the PlayUISoundLooping function to skip the phone static when requested.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            /// <param name="clip">Clip to play.</param>
            /// <param name="source">Audio Source to play the clip at (Can be null).</param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(UISoundController __instance, ref RichAudioClip clip, ref AudioSource source)
            {
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    (bool foundModifier, VariableChanged<bool> value) disablePhoneStatic =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.DisablePhoneStatic,
                            vCb => vCb.HasChanged);

                    if (disablePhoneStatic.foundModifier && disablePhoneStatic.value.Data)
                    {
                        if (clip == __instance.phoneStatic)
                        {
                            return false; // This skips the original function.
                        }
                    }
                }

                if (source != null)
                {
                    source.clip = clip.clip;
                    source.volume = clip.volume;
                    source.Play();
                }
                else
                {
                    __instance.myLoopingSource.clip = clip.clip;
                    __instance.myLoopingSource.volume = clip.volume;
                    __instance.myLoopingSource.Play();
                }

                return false; // This skips the original function.
            }
        }
    }
}