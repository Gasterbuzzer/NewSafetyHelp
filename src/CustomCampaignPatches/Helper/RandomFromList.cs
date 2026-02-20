using System.Collections.Generic;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaignPatches.Helper
{
    public static class RandomFromList
    {
        /// <summary>
        /// From a provided list picks out the correct element.
        /// If one element provided => Returns that element.
        /// If two elements provided => Returns between the two values
        /// If three elements are provided => Chooses a random index and uses that index position to return that.
        /// If zero elements => Returns null;
        /// </summary>
        /// <param name="providedList"> List with one or multiple elements </param>
        public static float? GetRandomFromList(List<float> providedList)
        {
            if (providedList == null || providedList.Count == 0)
            {
                return null;
            }
            else if (providedList.Count == 1)
            {
                return providedList[0];
            }
            else if (providedList.Count == 2)
            {
                return Random.Range(providedList[0], providedList[1]);
            }
            else
            {
                int randomIndex = Random.Range(0, providedList.Count);
                return providedList[randomIndex];
            }
        }
    }
}