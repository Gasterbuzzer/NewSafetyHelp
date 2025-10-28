using System.IO;
using MelonLoader;
using UnityEngine;

namespace NewSafetyHelp.EntryManager
{
    public static class ImageImport
    {
        /// <summary>
        /// Function for loading in an image from a provided path and converting it into a Sprite.
        /// </summary>
        /// <param name="imagePath"> Path to the image file. (Includes the image itself in the path) </param>
        public static Sprite LoadImage(string imagePath)
        {

            if (!File.Exists(imagePath))
            {
                MelonLogger.Error($"ERROR: Image file not found at path: {imagePath}");
                return null;
            }

            // Load the image data
            byte[] imageData = File.ReadAllBytes(imagePath);

            // Create a Texture2D from the image data
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(imageData))
            {
                MelonLogger.Error($"ERROR: Failed to load image {imagePath} data into texture.");
                return null;
            }

            // Create a sprite from the texture
            Sprite newSprite = Sprite.Create(
                texture,                                        // Texture Data
                new Rect(0, 0, texture.width, texture.height),  // Size
                new Vector2(0.5f, 0.5f)                         // Pivot
            );

            return newSprite;
        }
        
        /// <summary>
        /// Overload of LoadImage(). Allows providing two paths, if the first one doesn't exist, we attempt to load the second.
        /// </summary>
        /// <param name="imagePath"> Path to the image file. (Includes the image itself in the path) </param>
        /// <param name="fallbackImagePath"> Path to the image file. (Includes the image itself in the path) </param>
        public static Sprite LoadImage(string imagePath, string fallbackImagePath)
        {
            if (!File.Exists(imagePath) && !File.Exists(fallbackImagePath))
            {
                MelonLogger.Error($"ERROR: Image file could not be found in either: {imagePath} or {fallbackImagePath}");
                return null;
            }

            // We attempt to read the first path first and then the fallback.
            string imagePathToUse = imagePath;

            if (!File.Exists(imagePath))
            {
                imagePathToUse = fallbackImagePath;
            }

            // Load the image data
            byte[] imageData = File.ReadAllBytes(imagePathToUse);

            // Create a Texture2D from the image data
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(imageData))
            {
                MelonLogger.Error($"ERROR: Failed to load image {imagePathToUse} data into texture.");
                return null;
            }

            // Create a sprite from the texture
            Sprite newSprite = Sprite.Create(
                texture,                                        // Texture Data
                new Rect(0, 0, texture.width, texture.height),  // Size
                new Vector2(0.5f, 0.5f)                         // Pivot
            );

            return newSprite;
        }
    }
}
