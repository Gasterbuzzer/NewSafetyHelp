using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio.Music
{
    public static class MusicControllerPatches
    {
        [HarmonyLib.HarmonyPatch(typeof(MusicController), "StartRandomMusic", new Type[] { })]
        public static class StartRandomMusicPatch
        {

            /// <summary>
            /// Patches the play random music to not play day 7 music in custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MethodBase __originalMethod, MusicController __instance)
            {

                // If in the main game and the current day is the 7th day.
                if (!CustomCampaignGlobal.inCustomCampaign)
                {
                    if (GlobalVariables.currentDay == 7 && !GlobalVariables.arcadeMode)
                    {
                        return false;
                    }
                }
                
                int chosenMusicIndex = 0;
                bool playCustomMusic = false;
                
                FieldInfo _previousHoldMusicIndex = typeof(MusicController).GetField("previousHoldMusicIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (_previousHoldMusicIndex == null)
                {
                    MelonLogger.Error("ERROR: PreviousHoldMusicIndex is null. Unable of replacing MusicController StartNewRandomMusic. Calling original function.");
                    return true;
                }

                if (!CustomCampaignGlobal.inCustomCampaign) // Main game
                {
                    if (GlobalVariables.currentDay > 4)
                    {
                        for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }
                        
                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                        
                    }
                    else
                    {
                        for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                        {
                            chosenMusicIndex = Random.Range(0, GlobalVariables.currentDay);
                        }
                    
                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                    }
                }
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign Music. Ignores the day
                {

                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign is null! Cannot play random music. Using default logic.");
                        return true;
                    }

                    if (customCampaign.alwaysRandomMusic)
                    {
                        int amountOfClips = 0;
                        
                        int customMusicAmount = customCampaign.customMusic.Where(clip => clip.unlockDay <= GlobalVariables.currentDay).ToList().Count;
                        
                        if (!customCampaign.removeDefaultMusic)
                        {
                            amountOfClips +=  __instance.onHoldMusicClips.Length;
                        }
                        
                        if (customCampaign.customMusic.Count > 0) // We have custom music
                        {
                            amountOfClips += customMusicAmount;
                        }
                        
                        chosenMusicIndex = Random.Range(0, amountOfClips); // Set it once.
                        
                        for (int musicChoosingAttempt = 0;
                             chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3;
                             ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, amountOfClips);
                        }

                        if (!customCampaign.removeDefaultMusic)
                        {
                            if (chosenMusicIndex >= __instance.onHoldMusicClips.Length)
                            {
                                playCustomMusic = true;
                                chosenMusicIndex -= __instance.onHoldMusicClips.Length;
                            }
                        }
                        else if (customCampaign.removeDefaultMusic)
                        {
                            playCustomMusic = true;
                        }
                        
                        #if DEBUG
                            MelonLogger.Msg("DEBUG: " +
                                            $"Chose to play the music track: {chosenMusicIndex} with the previous being {(int) _previousHoldMusicIndex.GetValue(__instance)}." +
                                            $" (From custom music? {playCustomMusic})" +
                                            $" (Amount of clips: {amountOfClips})");

                            if (playCustomMusic)
                            {
                                MelonLogger.Msg($"DEBUG: Amount of custom music: {customMusicAmount}.");
                            }
                        #endif
                    
                        _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex);
                    }
                    else
                    {
                        if (GlobalVariables.currentDay > 4)
                        {
                            for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                            {
                                chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                            }
                        
                            _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                        
                        }
                        else
                        {
                            for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt) // __instance.previousHoldMusicIndex
                            {
                                chosenMusicIndex = Random.Range(0, Mathf.Min(GlobalVariables.currentDay, 7));
                            }
                    
                            _previousHoldMusicIndex.SetValue(__instance, chosenMusicIndex); // __instance.previousHoldMusicIndex = index1;
                        }
                    }
                }

                if (!CustomCampaignGlobal.inCustomCampaign) // Main Campaign
                {
                    __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                }
                else if (CustomCampaignGlobal.inCustomCampaign) // Custom Campaign
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: CustomCampaign is null! Cannot play random music. Using default logic.");
                        return true;
                    }

                    if (playCustomMusic)
                    {
                        List<MusicExtraInfo> customMusicList = customCampaign.customMusic.Where(clip => clip.unlockDay <= GlobalVariables.currentDay).ToList();
                        
                        if (customMusicList.Count > 0 && customMusicList[chosenMusicIndex].musicClip != null)
                        {
                            __instance.StartMusic(customMusicList[chosenMusicIndex].musicClip);
                        }
                        else
                        {
                            MelonLogger.Warning("WARNING: Music clip is empty! Possibly failed loading?");
                        }
                    }
                    else if (!customCampaign.removeDefaultMusic)
                    {
                        __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                    }
                }
                
                return false; // Skip function with false.
            }
        }
    }
}