using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.ArcadeCallerModule;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.Arcade
{
    public static class ArcadeHelper
    {
        /*
         * Variable that keeps track of the current arcade caller. Is used for checking for animated portraits.
         */
        private static ArcadeCaller currentArcadeCaller;

        /// <summary>
        /// Sets the current arcade caller to the provided arcade caller.
        /// Use this only at initialization!
        /// </summary>
        /// <param name="newArcadeCaller">The newly created arcade caller.</param>
        public static void SetCurrentArcadeCaller(ArcadeCaller newArcadeCaller)
        {
            if (newArcadeCaller != null)
            {
                currentArcadeCaller = newArcadeCaller;
            }
        }

        /// <summary>
        /// Checks if the current fixed arcade caller has an animated portrait to display.
        /// </summary>
        /// <returns>TRUE: Has an animated portrait. FALSE: Has no animated portrait.</returns>
        public static bool DoesCurrentArcadeCallerHaveAnAnimatedPortrait()
        {
            if (currentArcadeCaller != null
                && currentArcadeCaller.CallerHasAnimatedPortrait)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// If the current fixed arcade caller has an animated portrait, this return the URL of that arcade caller.
        /// </summary>
        /// <returns>NULL: Invalid arcade caller; Else: URL of animated portrait to display.</returns>
        public static string GetCurrentArcadeCallerAnimatedURL()
        {
            if (currentArcadeCaller != null
                && currentArcadeCaller.CallerHasAnimatedPortrait)
            {
                return currentArcadeCaller.CallerAnimatedPortraitURL;
            }

            return null;
        }

        /// <summary>
        /// If the current fixed arcade caller has an animated portrait,
        /// this return if the animated portrait should loop or not.
        /// </summary>
        /// <returns>NULL: Invalid arcade caller; Else: (Bool) Should the animated portrait loop?</returns>
        public static bool GetCurrentArcadeCallerShouldLoopAnimatedPortrait()
        {
            if (currentArcadeCaller != null
                && currentArcadeCaller.CallerHasAnimatedPortrait
                && currentArcadeCaller.CallerAnimatedPortraitShouldLoop.HasChanged)
            {
                return currentArcadeCaller.CallerAnimatedPortraitShouldLoop.Data;
            }

            return true;
        }

        /// <summary>
        /// Call this if the current arcade caller is not a custom fixed arcade caller.
        /// </summary>
        public static void DisableCurrentArcadeCaller()
        {
            currentArcadeCaller = null;
        }

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

            if (customCampaign.ArcadeRemoveAllValidFixedCallersWhenChoosing.Data)
            {
                foreach (ArcadeCaller arcadeCaller in validArcadeCallers)
                {
                    customCampaign.TemporaryCopyFixedArcadeCallers.Remove(arcadeCaller);
                }
            }
            else
            {
                customCampaign.TemporaryCopyFixedArcadeCallers.Remove(validArcadeCaller);
            }

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
                    ").", LoggingHelper.LoggingCategory.ARCADE);

                currentArcadeCaller = chosenArcadeCaller;

                return true;
            }

            return false;
        }
    }
}