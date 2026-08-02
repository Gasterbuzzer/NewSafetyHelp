using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.HelperFunctions;
using NewSafetyHelp.JSONParsing;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace NewSafetyHelp.Audio
{
    public static class AudioImport
    {
        // List containing all audios currently loading.
        public static readonly List<string> CurrentLoadingAudios = new List<string>();

        private static readonly MethodInfo StartCallerController = typeof(CallerController).GetMethod("Start",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Loads embedded audio to location.
        /// </summary>
        /// <param name="callback">CallBack function to write the value to.</param>
        /// <param name="fileName">Filename of the embedded file in the assembly.</param>
        /// <param name="compressAudio">If to compress the audio.</param>
        /// <param name="audioType">Audio Type (Will be automatically assigned)</param>
        public static void LoadEmbeddedAudio(Action<AudioClip> callback, string fileName, bool compressAudio,
            AudioType audioType = AudioType.WAV)
        {
            string temporaryEmbeddedAudioPath = EmbedHelpers.ExtractEmbeddedResourceToTempFile(fileName);

            MelonCoroutines.Start(UpdateAudioClip(callback, temporaryEmbeddedAudioPath, compressAudio, audioType));
        }

        /// <summary>
        /// Helper coroutine for updating the audio correctly for an audio clip.
        /// </summary>
        /// <param name="callback">Callback function for returning values
        /// and doing stuff with it that require the coroutine to finish first. </param>
        /// <param name="audioPath">Path to the audio file. </param>
        /// <param name="compressAudio">If to compress the audio.</param>
        /// <param name="audioType">Audio type to parse. </param>
        public static IEnumerator UpdateAudioClip(Action<AudioClip> callback, string audioPath, bool compressAudio,
            AudioType audioType = AudioType.WAV)
        {
            AudioClip soundClip = null;

            // Attempt to get the type
            if (audioType != AudioType.UNKNOWN)
            {
                audioType = GetAudioType(audioPath);

                yield return MelonCoroutines.Start(
                    LoadAudio
                    (
                        myReturnValue => { soundClip = myReturnValue; },
                        audioPath, audioType, compressAudio)
                );
            }

            callback(soundClip);
        }

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Coroutine that gets an audio clip from a specified path, please note to also provide an audio type, defaulted to MPEG.
        /// </summary>
        /// <param name="callback"> Callback function used for getting the AudioClip back. </param>
        /// <param name="path"> Path to audio file. </param>
        /// <param name="audioType"> Unity AudioType; Used for proper loading in. </param>
        /// <param name="compressAudio">If to compress the audio.</param>
        private static IEnumerator LoadAudio(Action<AudioClip> callback, string path,
            AudioType audioType = AudioType.MPEG, bool compressAudio = true)
        {
            long audioFileSize;

            if (File.Exists(path))
            {
                audioFileSize = new FileInfo(path).Length;
            }
            else
            {
                LoggingHelper.ErrorLog($"Given path to file '{path}' of type '{audioType.ToString()}' does not exist.");

                // Fix for audio failing to load causing a freeze.
                CurrentLoadingAudios.Remove($"{path}{audioType.ToString()}");

                // If all audios finished loading we continue letting the game run.
                if (CurrentLoadingAudios.Count <= 0)
                {
                    Time.timeScale = 1.0f;
                }

                yield break;
            }

            // We check if the audio is already being loaded in. If yes, we wait for the audio.
            if (CurrentLoadingAudios.Contains($"{path}{audioType.ToString()}"))
            {
                while (CurrentLoadingAudios.Contains($"{path}{audioType.ToString()}"))
                {
                    yield return new WaitForSecondsRealtime(Random.Range(0.1f, 0.7f));
                }
            }
            else
            {
                CurrentLoadingAudios.Add($"{path}{audioType.ToString()}");
            }

            bool fromHotReload = ReloadJSONParsing.IsInHotReload;

            LoggingHelper.DebugLog(() =>
                    $"Current allocated memory (audio is waiting for slot): Allocated: '{Profiler.GetTotalAllocatedMemoryLong()}'; " +
                    $"Reserved: '{Profiler.GetTotalReservedMemoryLong()}' " +
                    $"(File size '{audioFileSize}').",
                LoggingHelper.LoggingCategory.MEMORY);

            // We try searching our cache first and if we find it, we use that.
            AudioClip cachedAudio = AudioCache.TryGet(path, false);

            if (cachedAudio != null)
            {
                callback(cachedAudio);

                FinishAudioImport(path, audioType, audioFileSize, fromHotReload);

                yield break;
            }

            yield return AudioLoadThrottler.WaitForSlot(fromHotReload, audioFileSize);

            // (Bool: We pass if we skip the waiting for slot.)
            LoggingHelper.InfoLog($"Attempting to add {path} as audio type {audioType.ToString()}.");

            Time.timeScale = 0;

            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                if (compressAudio
                    && www.downloadHandler is DownloadHandlerAudioClip handlerAudio)
                {
                    handlerAudio.compressed = true;
                }

                UnityWebRequestAsyncOperation operation = www.SendWebRequest();

                // Wait until the request is done
                while (!operation.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.Success
                    && operation.isDone)
                {
                    LoggingHelper.InfoLog($"Audio: '{path}' as {audioType.ToString()} has been successfully loaded.");

                    AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(www);

                    AudioCache.AddCache(path, loadedClip);

                    callback(loadedClip);
                }
                else // Failed loading the audio file.
                {
                    if (!operation.isDone)
                    {
                        LoggingHelper.ErrorLog("Audio Loading was not finished. This an an unexpected error.");
                    }

                    LoggingHelper.ErrorLog($"Was unable of loading '{path}' as audio type {audioType.ToString()}. " +
                                           $"\n Results in the error: '{www.error}' and the response code is: {www.responseCode}. " +
                                           $"Was the process finished?: '{www.isDone}'.");
                }

                FinishAudioImport(path, audioType, audioFileSize, fromHotReload);
            }
        }

        private static void FinishAudioImport(string path, AudioType audioType, long audioFileSize, bool fromHotReload)
        {
            CurrentLoadingAudios.Remove($"{path}{audioType.ToString()}");

            LoggingHelper.DebugLog(() =>
                    "CACHE: Current allocated memory (audio finished loading in): " +
                    $"Allocated: '{Profiler.GetTotalAllocatedMemoryLong()}'; Reserved: {Profiler.GetTotalReservedMemoryLong()}' " +
                    $"(File size '{audioFileSize}').",
                LoggingHelper.LoggingCategory.MEMORY);

            AudioLoadThrottler.ReleaseSlot(fromHotReload);

            // If all audios finished loading we continue letting the game run.
            if (CurrentLoadingAudios.Count <= 0)
            {
                Time.timeScale = 1.0f;
            }
        }


        /// <summary>
        /// Calls the CallerController "Start" function to reload audio / imports again.
        /// </summary>
        public static void ReCallCallerListStart()
        {
            if (StartCallerController == null)
            {
                LoggingHelper.ReflectionError(nameof(StartCallerController));
                return;
            }

            GameObject callerController = GameObject.Find("CallerController");

            if (callerController != null)
            {
                CallerController ccInstance = callerController.GetComponent<CallerController>();

                // Call again.
                StartCallerController.Invoke(ccInstance, null);
            }
        }

        /// <summary>
        /// Creates a new rich audio clip from a provided audio clip. Used for creating a monster.
        /// </summary>
        /// <param name="newAudioClip"> AudioClip to insert into the RichAudioClip. </param>
        /// <param name="volume"> Volume of the clip. </param>
        public static RichAudioClip CreateRichAudioClip(AudioClip newAudioClip, float volume = 0.5f)
        {
            RichAudioClip newRichAudioClip = ScriptableObject.CreateInstance<RichAudioClip>();

            newRichAudioClip.clip = newAudioClip;
            newRichAudioClip.volume = volume;

            return newRichAudioClip;
        }

        /// <summary>
        /// Tries to get the Unity's AudioType from a given fileName (path).
        /// </summary>
        /// <param name="fileName"> Path / Filename to be given the AudioType for. </param>
        private static AudioType GetAudioType(string fileName)
        {
            if (fileName.ToLower().EndsWith(".wav"))
            {
                return AudioType.WAV;
            }
            else if (fileName.ToLower().EndsWith(".ogg") || fileName.ToLower().EndsWith(".oga") ||
                     fileName.ToLower().EndsWith(".flac") || fileName.ToLower().EndsWith(".opus"))
            {
                return AudioType.OGGVORBIS;
            }
            else if (fileName.ToLower().EndsWith(".acc") || fileName.ToLower().EndsWith(".aac") ||
                     fileName.ToLower().EndsWith(".m4a") || fileName.ToLower().EndsWith(".mp4"))
            {
                return AudioType.ACC;
            }
            else if (fileName.ToLower().EndsWith(".aiff") || fileName.ToLower().EndsWith(".aif") ||
                     fileName.ToLower().EndsWith(".aifc"))
            {
                return AudioType.AIFF;
            }
            else if (fileName.ToLower().EndsWith(".it"))
            {
                return AudioType.IT;
            }
            else if (fileName.ToLower().EndsWith(".mod"))
            {
                return AudioType.MOD;
            }
            else if (fileName.ToLower().EndsWith(".mp2") || fileName.ToLower().EndsWith(".mp3") ||
                     fileName.ToLower().EndsWith(".wma"))
            {
                return AudioType.MPEG;
            }
            else if (fileName.ToLower().EndsWith(".s3m"))
            {
                return AudioType.S3M;
            }
            else if (fileName.ToLower().EndsWith(".xm"))
            {
                return AudioType.XM;
            }
            else if (fileName.ToLower().EndsWith(".vag"))
            {
                return AudioType.VAG;
            }
            else if (fileName.ToLower().EndsWith(".alac"))
            {
                return AudioType.AUDIOQUEUE;
            }
            else if (fileName.ToLower().EndsWith(".xma"))
            {
                return AudioType.XMA;
            }
            else
            {
                // Unknown File type, we return with Unknown
                LoggingHelper.ErrorLog("Unknown audio file type, attempting to still parse it. Expect failure.");
                return AudioType.UNKNOWN;
            }
        }
    }
}