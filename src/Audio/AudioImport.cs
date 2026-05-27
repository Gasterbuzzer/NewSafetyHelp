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
        /// <param name="audioType">Audio Type (Will be automatically assigned)</param>
        public static void LoadEmbeddedAudio(Action<AudioClip> callback, string fileName,
            AudioType audioType = AudioType.WAV)
        {
            string temporaryEmbeddedAudioPath = EmbedHelpers.ExtractEmbeddedResourceToTempFile(fileName);

            MelonCoroutines.Start(UpdateAudioClip(callback, temporaryEmbeddedAudioPath, audioType));
        }

        /// <summary>
        /// Helper coroutine for updating the audio correctly for an audio clip.
        /// </summary>
        /// <param name="callback">Callback function for returning values
        /// and doing stuff with it that require the coroutine to finish first. </param>
        /// <param name="audioPath">Path to the audio file. </param>
        /// <param name="audioType">Audio type to parse. </param>
        public static IEnumerator UpdateAudioClip(Action<AudioClip> callback, string audioPath,
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
                        audioPath, audioType)
                );
            }

            callback(soundClip);
        }

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Coroutine that gets an audio clip from a specified path, please note to also provide an audio type, defaulted to MPEG.
        /// </summary>
        /// <param name="callback"> Callback function used for getting the AudioClip back. </param>
        /// <param name="path"> Path to file. </param>
        /// <param name="audioType"> Unity AudioType </param>
        private static IEnumerator LoadAudio(Action<AudioClip> callback, string path,
            AudioType audioType = AudioType.MPEG)
        {
            bool fromHotReload = ReloadJSONParsing.IsInHotReload;
            
            yield return AudioLoadThrottler.WaitForSlot(fromHotReload); 
            
            LoggingHelper.InfoLog($"Attempting to add {path} as audio type {audioType.ToString()}.");

            Time.timeScale = 0;

            CurrentLoadingAudios.Add($"{path}{audioType.ToString()}");

            // First we check if the file exists
            if (!File.Exists(path))
            {
                LoggingHelper.ErrorLog($"Given path to file {path} of type {audioType.ToString()} does not exist.");

                // Fix for audio failing to load causing a freeze.
                CurrentLoadingAudios.Remove($"{path}{audioType.ToString()}");

                // If all audios finished loading we continue letting the game run.
                if (CurrentLoadingAudios.Count <= 0)
                {
                    Time.timeScale = 1.0f;
                }

                yield break;
            }

            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();

                // Wait until the request is done
                while (!operation.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.Success &&
                    operation.isDone) // Was able of getting the audio file.
                {
                    LoggingHelper.InfoLog($"{path} as {audioType.ToString()} has been successfully loaded.");

                    callback(DownloadHandlerAudioClip.GetContent(www)); // Get actual audio clip into a variable.
                }
                else // Failed loading the audio file.
                {
                    if (!operation.isDone)
                    {
                        LoggingHelper.ErrorLog("Audio Loading was not finished. This an an unexpected error.");
                    }

                    LoggingHelper.ErrorLog($"Was unable of loading {path} as audio type {audioType.ToString()}." +
                                           $" \n Results in the error: {www.error} and the response code is: {www.responseCode}." +
                                           $" Was the process finished?: {www.isDone}");
                }

                CurrentLoadingAudios.Remove($"{path}{audioType.ToString()}");
                
                AudioLoadThrottler.ReleaseSlot(fromHotReload);

                // If all audios finished loading we continue letting the game run.
                if (CurrentLoadingAudios.Count <= 0)
                {
                    Time.timeScale = 1.0f;
                }
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

            CallerController ccInstance = GameObject.Find("CallerController").GetComponent<CallerController>();

            // Call again.
            StartCallerController.Invoke(ccInstance, null);
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
        public static AudioType GetAudioType(string fileName)
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