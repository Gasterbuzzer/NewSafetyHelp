using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen
{
    public static class Custom3DScreenPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(StartMenuBehavior), "Start")]
        public static class StartMenuPatches
        {
            private static readonly FieldInfo CanStartField =
                typeof(StartMenuBehavior).GetField("canStart", BindingFlags.NonPublic | BindingFlags.Instance);

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
                         * Properties
                         */
                        if (computer3DScreen.SkipClickTime.HasChanged
                            && computer3DScreen.SkipClickTime.Data)
                        {
                            CanStartField.SetValue(__instance, true);
                        }

                        /*
                         * Music Settings
                         */

                        if (computer3DScreen.Music != null
                            && computer3DScreen.Music.clip != null)
                        {
                            AudioSource[] audioSources =
                                GameObject.Find("MusicController").GetComponents<AudioSource>();

                            foreach (AudioSource audioSource in audioSources)
                            {
                                audioSource.clip = computer3DScreen.Music.clip;

                                if (audioSource.playOnAwake
                                    && !audioSource.isPlaying)
                                {
                                    audioSource.Play();
                                }
                            }
                        }

                        if (computer3DScreen.BringMusicCloser.HasChanged
                            && computer3DScreen.BringMusicCloser.Data)
                        {
                            AudioSource[] audioSources =
                                GameObject.Find("MusicController").GetComponents<AudioSource>();

                            foreach (AudioSource audioSource in audioSources)
                            {
                                audioSource.minDistance = 5;
                            }
                        }

                        if (computer3DScreen.CenterMusic.HasChanged
                            && computer3DScreen.CenterMusic.Data)
                        {
                            AudioSource[] audioSources =
                                GameObject.Find("MusicController").GetComponents<AudioSource>();

                            foreach (AudioSource audioSource in audioSources)
                            {
                                audioSource.spatialBlend = 0.5f;
                            }
                        }

                        if (computer3DScreen.MusicVolume.HasChanged)
                        {
                            AudioSource[] audioSources =
                                GameObject.Find("MusicController").GetComponents<AudioSource>();

                            foreach (AudioSource audioSource in audioSources)
                            {
                                audioSource.volume = computer3DScreen.MusicVolume.Data;
                            }
                        }

                        if (computer3DScreen.DisableMusic.HasChanged
                            && computer3DScreen.DisableMusic.Data)
                        {
                            AudioSource[] audioSources =
                                GameObject.Find("MusicController").GetComponents<AudioSource>();

                            foreach (AudioSource audioSource in audioSources)
                            {
                                audioSource.enabled = false;
                            }
                        }

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

                        if (computer3DScreen.RightLightColor.HasChanged)
                        {
                            GameObject.Find("model").transform.Find("Point Light (2)").GetComponent<Light>().color =
                                computer3DScreen.RightLightColor.Data;
                        }

                        if (computer3DScreen.DisableRightLight.HasChanged)
                        {
                            GameObject.Find("model").transform.Find("Point Light (2)").gameObject
                                .SetActive(!computer3DScreen.DisableRightLight.Data);
                        }

                        /*
                         * 3D Objects Settings
                         */

                        if (computer3DScreen.DisableComputerScreen.HasChanged)
                        {
                            GameObject.Find("model").transform.Find("diannao").gameObject
                                .SetActive(!computer3DScreen.DisableComputerScreen.Data);
                        }

                        if (computer3DScreen.DisableKeyboard.HasChanged)
                        {
                            GameObject.Find("model").transform.Find("jianpan").gameObject
                                .SetActive(!computer3DScreen.DisableKeyboard.Data);
                        }

                        if (computer3DScreen.DisableTable.HasChanged)
                        {
                            GameObject.Find("Cube")
                                .SetActive(!computer3DScreen.DisableTable.Data);
                        }

                        /*
                         * Camera Settings
                         */

                        if (computer3DScreen.BackgroundColor.HasChanged)
                        {
                            GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor =
                                computer3DScreen.BackgroundColor.Data;
                        }

                        if (computer3DScreen.DisablePostProcessing.HasChanged)
                        {
                            GameObject.Find("Main Camera/Post-process Volume")
                                .SetActive(!computer3DScreen.DisablePostProcessing.Data);
                        }

                        /*
                         * Particle Settings
                         */

                        if (computer3DScreen.DisableParticles.HasChanged)
                        {
                            GameObject.Find("model").transform.Find("Particle System").gameObject
                                .SetActive(!computer3DScreen.DisableParticles.Data);
                        }

                        if (computer3DScreen.ParticleEmissionRate.HasChanged)
                        {
                            ParticleSystem.EmissionModule particleEmission = GameObject.Find("model").transform
                                .Find("Particle System")
                                .GetComponent<ParticleSystem>().emission;

                            particleEmission.rateOverTime = computer3DScreen.ParticleEmissionRate.Data;
                        }

                        if (computer3DScreen.ParticleStartSize.HasChanged)
                        {
                            ParticleSystem.MainModule particleMain = GameObject.Find("model").transform
                                .Find("Particle System")
                                .GetComponent<ParticleSystem>().main;

                            particleMain.startSize = computer3DScreen.ParticleStartSize.Data;
                        }

                        if (computer3DScreen.ParticleColor.HasChanged)
                        {
                            ParticleSystem.MainModule particleMain = GameObject.Find("model").transform
                                .Find("Particle System")
                                .GetComponent<ParticleSystem>().main;

                            particleMain.startColor = computer3DScreen.ParticleColor.Data;
                        }

                        if (computer3DScreen.ParticleTexture.HasChanged)
                        {
                            ParticleSystemRenderer particleSystemRenderer = GameObject.Find("model").transform
                                .Find("Particle System")
                                .GetComponent<ParticleSystemRenderer>();

                            particleSystemRenderer.material.mainTexture = computer3DScreen.ParticleTexture.Data.texture;
                        }

                        /*
                         * Title Settings
                         */

                        if (computer3DScreen.TitleText.HasChanged)
                        {
                            AnimatedText animatedText =
                                GameObject.Find("TitleCanvas/Text (TMP)").GetComponent<AnimatedText>();

                            animatedText.textFrames = new string[computer3DScreen.TitleText.Data.Count];

                            for (int i = 0; i < computer3DScreen.TitleText.Data.Count; i++)
                            {
                                animatedText.textFrames[i] = computer3DScreen.TitleText.Data[i];
                            }

                            if (computer3DScreen.TitleText.Data.Count >= 1)
                            {
                                GameObject.Find("TitleCanvas/Text (TMP)").GetComponent<TextMeshProUGUI>().text =
                                    computer3DScreen.TitleText.Data[0];
                            }
                        }

                        if (computer3DScreen.TitleLogo.HasChanged)
                        {
                            GameObject.Find("TitleCanvas/Image").GetComponent<Image>().sprite =
                                computer3DScreen.TitleLogo.Data;
                        }
                    }
                }
            }
        }
    }
}