using System.Collections.Generic;
using System.Linq;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.ParsingHelpers
{
    public static class ArtbookParsingHelper
    {
        public static void ParseArtbookPages(JObject jObjectParsed, ref List<ArtbookPage> target,
            string jsonFolderPath, string usermodFolderPath, string firstImageKey = "artbook_page_first_images",
            string secondImageKey = "artbook_page_second_images", string titleKey = "artbook_page_titles",
            string descriptionKey = "artbook_page_descriptions")
        {
            if (target == null)
            {
                target = new List<ArtbookPage>();
            }
            
            List<Sprite> artbookFirstImages = new List<Sprite>();
            bool? singleImageProvided = ImageParsingHelper.TryAssignSpriteListOrSingleSprite(jObjectParsed, firstImageKey,
                ref artbookFirstImages, jsonFolderPath, usermodFolderPath);

            List<Sprite> artbookSecondImages = new List<Sprite>();
            bool? singleImage2Provided = ImageParsingHelper.TryAssignSpriteListOrSingleSprite(jObjectParsed,
                secondImageKey, ref artbookSecondImages, jsonFolderPath, usermodFolderPath, true);
            
            List<string> artbookTitles = new List<string>();
            bool? singleTitleProvided = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, titleKey,
                ref artbookTitles);
            
            List<string> artbookDescriptions = new List<string>();
            bool? singleDescriptionProvided = ParsingHelper.TryAssignListOrSingleElement(jObjectParsed, descriptionKey,
                ref artbookDescriptions);

            if (artbookFirstImages.Count <= 0 
                && artbookSecondImages.Count <= 0
                && artbookTitles.Count <= 0
                && artbookDescriptions.Count <= 0
                && (jObjectParsed.TryGetValue(firstImageKey, out _) || jObjectParsed.TryGetValue(descriptionKey, out _)))
            {
                LoggingHelper.WarningLog("Provided artbook pages are invalid or could not be parsed. " +
                                         "No artbook pages will be updated.");
                return;
            }

            if (artbookFirstImages.Count < artbookSecondImages.Count)
            {
                LoggingHelper.WarningLog("Artbook second Images list is greater than the first artbook images. " +
                                         "You need more or equal the amount of first images. " +
                                         "No artbook pages will be updated.");
                return;
            }

            int maxArtbookPages = new List<int> {artbookFirstImages.Count, artbookSecondImages.Count,
                artbookDescriptions.Count, artbookTitles.Count}.Max();

            for (int i = 0; i < maxArtbookPages; i++)
            {
                ArtbookPage newArtbookPage = ScriptableObject.CreateInstance<ArtbookPage>();
                
                // Default Values
                newArtbookPage.title = "";
                newArtbookPage.description = "";
                newArtbookPage.image = null;
                newArtbookPage.image2 = null;

                if (singleImageProvided != null)
                {
                    if ((bool) singleImageProvided && artbookFirstImages.Count > 0)
                    {
                        newArtbookPage.image = artbookFirstImages[0];
                    }
                    else if (i < artbookFirstImages.Count)
                    {
                        newArtbookPage.image = artbookFirstImages[i];
                    }
                }
                
                if (singleImage2Provided != null)
                {
                    if ((bool) singleImage2Provided && artbookSecondImages.Count > 0)
                    {
                        newArtbookPage.image2 = artbookSecondImages[0];
                    }
                    else if (i < artbookSecondImages.Count)
                    {
                        newArtbookPage.image2 = artbookSecondImages[i];
                    }
                }
                
                if (singleTitleProvided != null)
                {
                    if ((bool) singleTitleProvided && artbookTitles.Count > 0)
                    {
                        newArtbookPage.title = artbookTitles[0];
                    }
                    else if (i < artbookTitles.Count)
                    {
                        newArtbookPage.title = artbookTitles[i];
                    }
                }
                
                if (singleDescriptionProvided != null)
                {
                    if ((bool) singleDescriptionProvided && artbookDescriptions.Count > 0)
                    {
                        newArtbookPage.description = artbookDescriptions[0];
                    }
                    else if (i < artbookDescriptions.Count)
                    {
                        newArtbookPage.description = artbookDescriptions[i];
                    }
                }
                
                target.Add(newArtbookPage);
            }
        }
    }
}