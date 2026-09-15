using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.ArcadeCallerModule;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Arcade
{
    public static class ArcadeHelper
    {
        /// <summary>
        /// Gets the valid arcade caller to render.
        /// </summary>
        /// <param name="customCampaign">Currently running custom campaign.</param>
        /// <param name="arcadeTotalCallers"></param>
        /// <returns>ArcadeCaller; If none available => NULL.</returns>
        public static ArcadeCaller GetValidArcadeCaller(CustomCampaign customCampaign, int arcadeTotalCallers)
        {
            if (customCampaign.TemporaryCopyFixedArcadeCallers.Count <= 0)
            {
                return null;
            }

            List<ArcadeCaller> validArcadeCallers = new List<ArcadeCaller>();

            foreach (ArcadeCaller arcadeCaller in customCampaign.TemporaryCopyFixedArcadeCallers)
            {
                if (arcadeCaller.ArcadeCallersRequired <= arcadeTotalCallers)
                {
                    validArcadeCallers.Add(arcadeCaller);
                }
            }

            if (validArcadeCallers.Count <= 0)
            {
                return null;
            }

            // Now we pick one valid one. Since we might have multiple available. (I don't recommend defining multiple)
            int randomValidArcadeCaller = Random.Range(0, validArcadeCallers.Count);

            ArcadeCaller validArcadeCaller = validArcadeCallers[randomValidArcadeCaller];

            customCampaign.TemporaryCopyFixedArcadeCallers.Remove(validArcadeCaller);

            return validArcadeCaller;
        }

        /// <summary>
        /// Replaces the current custom caller with the custom arcade caller provided.
        /// </summary>
        /// <param name="__instance">Instance of the caller controller.</param>
        /// <param name="customCampaign">Current custom campaign.</param>
        /// <returns>TRUE: Caller was replaced. FALSE: Was not replaced.</returns>
        public static bool ReplaceArcadeCaller(CallerController __instance, CustomCampaign customCampaign)
        {
            if (customCampaign.TemporaryCopyFixedArcadeCallers.Count <= 0)
            {
                return false;
            }

            ArcadeCaller chosenArcadeCaller = GetValidArcadeCaller(customCampaign, __instance.currentArcadeCallTotal);

            if (chosenArcadeCaller != null)
            {
                GlobalVariables.callerControllerScript.currentCustomCaller.callerMonster = null;
                GlobalVariables.callerControllerScript.currentCustomCaller.consequenceCallerProfile = null;
                GlobalVariables.callerControllerScript.currentCustomCaller.increaseTier = false;

                GlobalVariables.callerControllerScript.currentCustomCaller.callerName =
                    chosenArcadeCaller.CallerName;

                GlobalVariables.callerControllerScript.currentCustomCaller.callTranscription =
                    chosenArcadeCaller.CallTranscript;

                if (chosenArcadeCaller.CallerImage.HasChanged)
                {
                    GlobalVariables.callerControllerScript.currentCustomCaller.callerPortrait =
                        chosenArcadeCaller.CallerImage.Data;
                }

                if (chosenArcadeCaller.IsCallerClipLoaded)
                {
                    GlobalVariables.callerControllerScript.currentCustomCaller.callerClip =
                        chosenArcadeCaller.CallerClip;
                }

                LoggingHelper.DebugLog(() =>
                    "Replaced next arcade caller with custom " +
                    $"fixed arcade caller '{chosenArcadeCaller.CallerName}' " +
                    $"(Total Callers Today: '{__instance.currentArcadeCallTotal}'; " +
                    $"Caller Unlock Required: '{chosenArcadeCaller.ArcadeCallersRequired}'; " +
                    $"Temp List Count After: '{customCampaign.TemporaryCopyFixedArcadeCallers.Count}'; " +
                    ").");

                return true;
            }

            return false;
        }
    }
}