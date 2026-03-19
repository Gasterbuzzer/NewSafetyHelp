using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using UnityEngine;

namespace NewSafetyHelp.CustomVideos
{
    public class CustomVideo : CustomCampaignElementBase
    {
        public string DesktopName = "NO_DESKTOP_NAME";

        public string VideoURL = null;

        public int UnlockDay = 1;

        // Uses a reference to check if a Game Object belongs to the custom video.
        public GameObject ReferenceToCreatedVideo = null;
        
        // Per default, videos always unlock on the correct unlock day.
        // But we may wish to use the new accuracy system, so we provide the option to override.
        public bool IgnoreAccuracyChecks = true;
        
        // New Accuracy Settings
        [CanBeNull] public List<GeneralAccuracyType> UnlockAccuracy = null;

        // For this email to appear, it may require some callers to be correct or false.
        [CanBeNull] public List<CallerRequirement> UnlockRequiredCallers = null;

        // If the player requires to finish the game first.
        // NOTE: It also requires the check to be true.
        public bool UnlockWhenGameFinished = false;
    }
}