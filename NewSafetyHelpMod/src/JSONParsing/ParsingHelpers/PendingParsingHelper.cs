using System.Collections.Generic;
using NewSafetyHelp.CustomCampaignSystem.Abstract;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class PendingParsingHelper
    {
        /// <summary>
        /// Adds any pending elements (elements that were parsed before the campaign was parsed)
        /// to the provided campaign list.
        /// </summary>
        /// <param name="pendingList">List of pending to be added.</param>
        /// <param name="listToBeAddedTo">List where to add the pending elements.</param>
        /// <param name="customCampaignName">Custom Campaign to be which the elements get added to.</param>
        /// <param name="elementName">For debug printing. It shows what type of element was added.</param>
        /// <typeparam name="T">Type of the target in the lists.</typeparam>
        public static void AddPendingElementsToCampaign<T>(ref List<T> pendingList, ref List<T> listToBeAddedTo,
            string customCampaignName, string elementName = "NO_NAME_GIVEN") where T : CustomCampaignElementBase
        {
            if (pendingList.Count > 0)
            {
                // Create a copy of the list to iterate over.
                List<T> tempList = new List<T>(pendingList);

                foreach (T missingElement in tempList)
                {
                    if (missingElement.CustomCampaignName == customCampaignName)
                    {
                        LoggingHelper.DebugLog(
                            $"Adding missing {elementName} to the custom campaign: {customCampaignName}.");

                        listToBeAddedTo.Add(missingElement);

                        pendingList.Remove(missingElement);
                    }
                }
            }
        }
    }
}