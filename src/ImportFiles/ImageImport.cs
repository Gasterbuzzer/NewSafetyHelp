using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.ImportFiles
{
    public static class ImageImport
    {
        /// <summary>
        /// Function for loading in an image from a provided path and converting it into a Sprite.
        /// </summary>
        /// <param name="imagePath"> Path to the image file. (Includes the image itself in the path) </param>
        [CanBeNull]
        public static Sprite LoadImage(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                LoggingHelper.ErrorLog($"Image file not found at path: '{imagePath}'.");
                return null;
            }

            // Load the image data
            byte[] imageData = File.ReadAllBytes(imagePath);

            // Create a Texture2D from the image data
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(imageData))
            {
                UnityEngine.Object.Destroy(texture);
                LoggingHelper.ErrorLog($"Failed to load image '{imagePath}' data into texture.");
                return null;
            }

            // Create a sprite from the texture
            Sprite newSprite = Sprite.Create(
                texture, // Texture Data
                new Rect(0, 0, texture.width, texture.height), // Size
                new Vector2(0.5f, 0.5f) // Pivot
            );

            return newSprite;
        }

        /// <summary>
        /// Function for loading in an embedded image and converting it into a Sprite.
        /// </summary>
        /// <param name="imageName"> Name of the image inside the embedded files. </param>
        [CanBeNull]
        public static Sprite LoadEmbeddedImage(string imageName)
        {
            imageName = imageName.Trim();

            if (string.IsNullOrEmpty(imageName))
            {
                LoggingHelper.ErrorLog("Empty embedded image provided. Unable of loading embedded image.");
                return null;
            }

            LoggingHelper.InfoLog($"Attempting to load embedded image '{imageName}'.");

            // Get Assembly with the embedded resource.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // We try finding the resource via the file name and use that to get the resource name.
            string fileName = imageName;
            imageName = currentAssembly.GetManifestResourceNames()
                .SingleOrDefault(str => str.EndsWith(fileName, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(imageName))
            {
                LoggingHelper.ErrorLog($"Could not find embedded resource '{fileName}'. " +
                                       "Unable of loading the provided embedded resource.");
                return null;
            }

            using (Stream imageStream = currentAssembly.GetManifestResourceStream(imageName))
            {
                if (imageStream == null)
                {
                    LoggingHelper.ErrorLog($"Could not find embedded resource '{fileName}'. " +
                                           "Unable of loading the provided embedded resource.");
                    return null;
                }

                using (BinaryReader binaryReader = new BinaryReader(imageStream))
                {
                    byte[] imageData = binaryReader.ReadBytes((int)imageStream.Length);

                    // Create a 2D texture from the image stream.
                    Texture2D texture = new Texture2D(2, 2);

                    if (!texture.LoadImage(imageData))
                    {
                        UnityEngine.Object.Destroy(texture);
                        LoggingHelper.ErrorLog($"Failed to load image '{imageName}' data into texture.");
                        return null;
                    }

                    // Create a sprite from the texture
                    return Sprite.Create(
                        texture, // Texture Data
                        new Rect(0, 0, texture.width, texture.height), // Size
                        new Vector2(0.5f, 0.5f) // Pivot
                    );
                }
            }
        }

        /// <summary>
        /// Overload of LoadImage(). Allows providing two paths, if the first one doesn't exist, we attempt to load the second.
        /// </summary>
        /// <param name="imagePath"> Path to the image file. (Includes the image itself in the path) </param>
        /// <param name="fallbackImagePath"> Path to the image file. (Includes the image itself in the path) </param>
        [CanBeNull]
        public static Sprite LoadImage(string imagePath, string fallbackImagePath)
        {
            if (!File.Exists(imagePath) && !File.Exists(fallbackImagePath))
            {
                LoggingHelper.ErrorLog("Image file could not be found in either: " +
                                       $"'{imagePath}' or '{fallbackImagePath}'.");
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
                UnityEngine.Object.Destroy(texture);
                LoggingHelper.ErrorLog($"Failed to load image '{imagePathToUse}' data into texture.");
                return null;
            }

            // Create a sprite from the texture
            Sprite newSprite = Sprite.Create(
                texture, // Texture Data
                new Rect(0, 0, texture.width, texture.height), // Size
                new Vector2(0.5f, 0.5f) // Pivot
            );

            return newSprite;
        }
    }
}