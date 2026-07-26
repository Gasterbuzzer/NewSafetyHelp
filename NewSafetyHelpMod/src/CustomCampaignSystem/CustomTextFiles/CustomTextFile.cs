using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.CallerRequirementHelper;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomTextFiles
{
    public class CustomTextFile : CustomCampaignElementBase
    {
        public string FileNameOnDesktop = "file.txt";

        public string TextFileContents = "Empty";

        public int UnlockDay = 0;

        /*
         * Priority by which the text file appears in order.
         * Higher Priority => Gets shown first.
         */
        public int OrderPriority = 0;

        public VariableChanged<int> GameObjectOrder = new VariableChanged<int>
        {
            Data = 0
        };

        // New Accuracy Settings
        [CanBeNull] public List<GeneralAccuracyType> UnlockAccuracy = null;

        // For this email to appear, it may require some callers to be correct or false.
        [CanBeNull] public List<CallerRequirement> UnlockRequiredCallers = null;

        // If the player requires to finish the game first.
        // NOTE: It also requires the check to be true.
        public bool UnlockWhenGameFinished = false;

        /// <summary>
        /// Reference to the created custom text file.
        /// </summary>
        public GameObject CustomTextFileReference = null;
    }
}