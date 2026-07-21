using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;

namespace NewSafetyHelp.CustomCampaignSystem.LinkApps
{
    public class LinkApp : CustomCampaignElementBase
    {
        /*
         * General Properties
         */

        // Day from which day onward this link app is allowed to appear.
        public int UnlockDay = 0;

        // When showing the list of link apps, which priority should this link app have?
        // Higher int => Higher priority
        public int LinkAppPriority = 0;

        // On click URL.
        // This opens up this website on the browser when the app is opened.
        // Please make sure to double-check that this is set correctly!
        [CanBeNull] public Uri LinkAppClickURL;

        /*
         * Requirements
         */

        [CanBeNull] public List<GeneralAccuracyType> UnlockAccuracy = null;

        // For this email to appear, it may require some callers to be correct or false.
        [CanBeNull] public List<CallerRequirement> UnlockRequiredCallers = null;

        // If the player requires to finish the game first.
        // NOTE: It also requires the checks to be true.
        public bool UnlockWhenGameFinished = false;
    }
}