using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.ImportFiles
{
    public static class ImageCache
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Try's getting a given sprite from a given key.
        /// </summary>
        /// <param name="audioFullPath">Path to the image.</param>
        /// <returns>Null => Not in cache; Else: Sprite</returns>
        [CanBeNull]
        public static Sprite TryGet(string audioFullPath)
        {
            if (string.IsNullOrEmpty(audioFullPath))
            {
                return null;
            }

            if (SpriteCache.TryGetValue(audioFullPath, out Sprite image))
            {
                LoggingHelper.DebugLog($"Using image cache: '{audioFullPath}'.",
                    LoggingHelper.LoggingCategory.MEMORY);

                return image;
            }

            return null;
        }

        /// <summary>
        /// Adds a given sprite to the cache.
        /// </summary>
        /// <param name="fullImagePath">Path to the image file. Used for finding the cache.</param>
        /// <param name="imageSprite">Sprite to store.</param>
        public static void AddCache(string fullImagePath, Sprite imageSprite)
        {
            if (imageSprite == null)
            {
                return;
            }

            if (!File.Exists(fullImagePath))
            {
                LoggingHelper.ErrorLog("Sprite file not found. " +
                                       "This should not happen, as the image import should have failed before.");
                return;
            }

            SpriteCache[fullImagePath] = imageSprite;
        }

        /// <summary>
        /// Removes all references from the image cache.
        /// Use this on hot reload.
        /// </summary>
        public static void RemoveEntireCache()
        {
            foreach (Sprite imageSprite in SpriteCache.Values)
            {
                if (imageSprite == null)
                {
                    continue;
                }

                if (imageSprite.texture != null)
                {
                    Object.Destroy(imageSprite.texture);
                }

                Object.Destroy(imageSprite);
            }

            SpriteCache.Clear();
        }
    }
}