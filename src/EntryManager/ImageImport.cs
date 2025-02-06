using MelonLoader;
using System.IO;
using UnityEngine;

namespace NewSafetyHelp.src.EntryManager
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
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            return newSprite;
        }
    }
}
