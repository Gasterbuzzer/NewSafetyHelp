using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignSystem.CustomRingtone
{
    public static class RingtoneHelper
    {
        /// <summary>
        /// Replaces a given ringtone.
        /// </summary>
        /// <param name="validRingtones"></param>
        /// <param name="doNotAccountDefaultRingtone"></param>
        /// <param name="gameDefaultRingtone"></param>
        /// <returns></returns>
        [CanBeNull]
        public static RichAudioClip ReplacePhoneRingtoneIfValid(ref List<CustomRingtone> validRingtones,
            bool doNotAccountDefaultRingtone, ref RichAudioClip gameDefaultRingtone)
            {
                if (validRingtones.Count > 0)
                {
                    List<CustomRingtone> appendRingtones = validRingtones.Where(r => r.AppendRingtone).ToList();
                    
                    // If any ringtones have to be appended. (Random Pick)
                    if (appendRingtones.Count > 0) 
                    {
                        int maxExclusive = appendRingtones.Count;

                        // If we don't remove the ringtone, we also account for not changing the phone call.
                        if (!doNotAccountDefaultRingtone)
                        {
                            maxExclusive++;
                        }

                        int chosenPhoneCall = Random.Range(0, maxExclusive);

                        LoggingHelper.DebugLog(() =>
                                $"ChosenPhoneCall: '{chosenPhoneCall}'.\n" +
                                $"maxExclusive: '{maxExclusive}'.\n" +
                                $"doNotAccountDefaultRingtone: '{doNotAccountDefaultRingtone}'.\n" +
                                $"validRingtones.Any(r => r.AppendRingtone): '{appendRingtones.Count > 0}'.\n" +
                                $"appendRingtones.Count '{appendRingtones.Count}'.\n",
                            LoggingHelper.LoggingCategory.RINGTONE);

                        if (doNotAccountDefaultRingtone)
                        {
                            return appendRingtones.ElementAt(chosenPhoneCall).RingtoneClip;
                        }
                        else
                        {
                            if (chosenPhoneCall != validRingtones.Count)
                            {
                                return appendRingtones.ElementAt(chosenPhoneCall).RingtoneClip;
                            }
                            else
                            {
                                return gameDefaultRingtone;
                            }
                        }
                    }

                    // No appends, we simply use the first one.
                    return validRingtones[0].RingtoneClip;
                }

                return null;
            }
    }
}