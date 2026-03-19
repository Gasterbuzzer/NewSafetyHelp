using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using UnityEngine;

namespace NewSafetyHelp.Emails
{
    public class CustomEmail : CustomCampaignElementBase
    {
        public string EmailSubject = "";

        public string SenderName = "";

        public string EmailBody = "";

        // Image shown at the end of the email.
        [CanBeNull] public Sprite EmailImage = null;

        [CanBeNull] public string EmailAnimatedVideo = null;
        public bool HasAnimatedVideo = false;

        // On click URL.
        // This opens up this website on the browser when the attachment is clicked on.
        // Please make sure to double-check that this is set correctly!
        [CanBeNull] public Uri EmailClickURL;

        // Day from which day onward this email is allowed to appear.
        public int UnlockDay = 0;

        // When showing the list of emails, which priority should this email have?
        // Higher int => Higher priority
        public int EmailPriority = 0;

        public float UnlockThreshold = 0;
        [CanBeNull] public List<GeneralAccuracyType> UnlockAccuracy = null;
        public bool UseOldAccuracyChecks = true;

        // For this email to appear, it may require some callers to be correct or false.
        [CanBeNull] public List<CallerRequirement> UnlockRequiredCallers = null;

        // If the player requires to finish the game first.
        // NOTE: It also requires the check to be true.
        public bool UnlockWhenGameFinished = false;

        // Main Campaign Values
        public bool InMainCampaign = false;

        // For finding the custom email:
        [CanBeNull] public Email ReferenceToEmailObject = null;
    }
}