using System.Collections.Generic;
using JetBrains.Annotations;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.Audio
{
    public static class AudioCache
    {
        private static readonly Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

        /// <summary>
        /// Try's getting a given RichAudioClip from a given key.
        /// </summary>
        /// <param name="audioFullPath">Path to the audio.</param>
        /// <param name="getRecentlyAddedAudio">This boolean should be set to true if you just added it to the cache.</param>
        /// <returns>Null => Not in cache; Else: RichAudioClip</returns>
        [CanBeNull]
        public static AudioClip TryGet(string audioFullPath, bool getRecentlyAddedAudio)
        {
            if (audioCache.TryGetValue(audioFullPath, out AudioClip audio))
            {
                if (!getRecentlyAddedAudio)
                {
                    LoggingHelper.DebugLog($"Using audio cache for audio '{audioFullPath}'.", LoggingHelper.LoggingCategory.MEMORY);
                }

                return audio;
            }

            return null;
        }

        /// <summary>
        /// Adds a given audio in the cache.
        /// </summary>
        /// <param name="audioFullPath">Path to the audio file. Used for finding the cache.</param>
        /// <param name="audio">Audio to store.</param>
        public static void AddCache(string audioFullPath, AudioClip audio)
        {
            audioCache[audioFullPath] = audio;
        }

        /// <summary>
        /// Removes all references from audio cache.
        /// Use this on hot reload.
        /// </summary>
        public static void RemoveEntireCache()
        {
            foreach (AudioClip audio in audioCache.Values)
            {
                Object.Destroy(audio);
            }

            audioCache.Clear();
        }
    }
}