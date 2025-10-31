using System;
using System.Reflection;
using MelonLoader;
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
                        for (int musicChoosingAttempt = 0; chosenMusicIndex == (int) _previousHoldMusicIndex.GetValue(__instance) && musicChoosingAttempt < 3; ++musicChoosingAttempt)
                        {
                            chosenMusicIndex = Random.Range(0, __instance.onHoldMusicClips.Length);
                        }
                        
                        #if DEBUG
                            MelonLogger.Msg($"DEBUG: Chose to play the music track: {chosenMusicIndex} with the previous being {(int) _previousHoldMusicIndex.GetValue(__instance)}");
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
                
                __instance.StartMusic(GlobalVariables.musicControllerScript.onHoldMusicClips[chosenMusicIndex]);
                
                return false; // Skip function with false.
            }
        }
    }
}