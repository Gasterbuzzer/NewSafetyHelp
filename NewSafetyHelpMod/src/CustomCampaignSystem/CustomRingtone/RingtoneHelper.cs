using System.Collections.Generic;
using System.Linq;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomRingtone
{
    public static class RingtoneHelper
    {
        /// <summary>
        /// Replaces a given ringtone.
        /// </summary>
        /// <param name="validRingtones">A list of all ringtones to consider.</param>
        /// <param name="doNotAccountDefaultRingtone">If the function should not account the default ringtone.</param>
        /// <param name="gameDefaultRingtone">The default ringtone in case it gets picked.</param>
        /// <returns>Ringtone to play.</returns>
        public static RichAudioClip ReplacePhoneRingtoneIfValid(ref List<CustomRingtone> validRingtones,
            bool doNotAccountDefaultRingtone, ref RichAudioClip gameDefaultRingtone)
        {
            LoggingHelper.DebugLog($"Finding valid ringtone with '{validRingtones.Count > 0}' ringtones available. " +
                                   $"Do we include the default ringtone? '{doNotAccountDefaultRingtone}'.",
                LoggingHelper.LoggingCategory.RINGTONE);

            if (validRingtones.Count <= 0)
            {
                return gameDefaultRingtone;
            }

            List<CustomRingtone> appendRingtones = validRingtones.Where(r => r.AppendRingtone).ToList();
            List<CustomRingtone> workingList = new List<CustomRingtone>(appendRingtones);

            // No append ringtones, so we can return the first element.
            if (workingList.Count <= 0)
            {
                // No appends (or chances), we simply use the first one.
                return validRingtones[0].RingtoneClip;
            }

            // If we account for the default ringtone to be included, we add it to the list.
            if (!doNotAccountDefaultRingtone)
            {
                workingList.Add(
                    new CustomRingtone
                    {
                        AppendRingtone = true,
                        RingtoneClip = gameDefaultRingtone,
                        PlayChance = 1.0f
                    });
            }

            // We sum all percentages and then pick a percentage and then find the position the element belongs to.
            float percentageSum = 0;
            foreach (CustomRingtone customRingtone in workingList)
            {
                percentageSum += customRingtone.PlayChance;
            }

            // No valid ringtone.
            // (All percentages are zero, or we have negative percentages).
            if (percentageSum <= 0)
            {
                return gameDefaultRingtone;
            }

            // The randomly chosen chance for a ringtone.
            float chosenPhoneCallPercentage = Random.Range(0, percentageSum);

            LoggingHelper.DebugLog(() =>
                    $"(Random Number that was chosen) ChosenPhoneCall Percentage: '{chosenPhoneCallPercentage}'.\n" +
                    $"percentageSum: '{percentageSum}'.\n" +
                    $"doNotAccountDefaultRingtone: '{doNotAccountDefaultRingtone}'.\n" +
                    $"Do we have any append ringtones?: '{workingList.Count > 0}'.\n" +
                    $"How many append ringtones?: '{workingList.Count}'.\n",
                LoggingHelper.LoggingCategory.RINGTONE);

            // Cumulative represents at what position the percentages are.
            // So if we are at lets say 5.1 and the first two elements had a chance of 2.5,
            // that means we are at the third element with chance of 0.1.
            float cumulative = 0;
            foreach (CustomRingtone customRingtone in workingList)
            {
                cumulative += customRingtone.PlayChance;

                if (chosenPhoneCallPercentage < cumulative)
                {
                    return customRingtone.RingtoneClip;
                }
            }

            return gameDefaultRingtone;
        }
    }
}