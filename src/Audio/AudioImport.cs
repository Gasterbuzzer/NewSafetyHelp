using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;

namespace NewSafetyHelp.Audio
{
    public static class AudioImport
    {
        
        // List containing all audios currently loading.
        public static List<string> CurrentLoadingAudios = new List<string>();
        

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Coroutine that gets an audio clip from a specified path, please note to also provide an audio type, defaulted to MPEG.
        /// </summary>
        /// <param name="callback"> Callback function used for getting the AudioClip back. </param>
        /// <param name="path"> Path to file. </param>
        /// <param name="_audioType"> Unity AudioType </param>
        public static IEnumerator LoadAudio(Action<AudioClip> callback, string path, AudioType _audioType = AudioType.MPEG)
        {
            MelonLogger.Msg($"INFO: Attempting to add {path} as audio type {_audioType.ToString()}.");
            
            Time.timeScale = 0;
            
            CurrentLoadingAudios.Add($"{path}{_audioType.ToString()}");

            // First we check if the file exists
            if (!File.Exists(path))
            {
                MelonLogger.Error($"ERROR: Given path to file {path} of type {_audioType.ToString()} does not exist.");
                
                // Fix for audio failing to load causing a freeze.
                CurrentLoadingAudios.Remove($"{path}{_audioType.ToString()}");

                // If all audios finished loading we continue letting the game run.
                if (CurrentLoadingAudios.Count <= 0)
                {
                    Time.timeScale = 1.0f;
                }
                
                yield break;
            }

            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, _audioType))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();

                // Wait until the request is done
                while (!operation.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.Success && operation.isDone) // Was able of getting the audio file.
                {
                    MelonLogger.Msg($"INFO: {path} as {_audioType.ToString()} has been successfully loaded.");

                    callback(DownloadHandlerAudioClip.GetContent(www)); // Get actual audio clip
                }
                else // Failed loading the audio file.
                {
                    if (!operation.isDone)
                    {
                        MelonLogger.Error("ERROR: Audio Loading was not finished. This an an unexpected error.");
                    }

                    MelonLogger.Error($"ERROR: Was unable of loading {path} as audio type {_audioType.ToString()}. \n Results in the error: {www.error} and the response code is: {www.responseCode}. Was the process finished?: {www.isDone}");
                }
                
                CurrentLoadingAudios.Remove($"{path}{_audioType.ToString()}");

                // If all audios finished loading we continue letting the game run.
                if (CurrentLoadingAudios.Count <= 0)
                {
                    Time.timeScale = 1.0f;
                }
            }
        }


        /// <summary>
        /// Calls the CallerController Start to reload audio / imports again.
        /// </summary>
        public static void ReCallCallerListStart()
        {
            Type callerController = typeof(CallerController);
            
            MethodInfo startCallerController = callerController.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (startCallerController == null)
            {
                MelonLogger.Error("ERROR: Start of CallerController was not found!");
                return;
            }

            CallerController ccInstance = GameObject.Find("CallerController").GetComponent<CallerController>();
            
            startCallerController.Invoke(ccInstance, null); // Call again.
        }

        /// <summary>
        /// Creates a new rich audio clip from a provided audio clip. Used for creating a monster.
        /// </summary>
        /// <param name="_newAudioClip"> AudioClip to insert into the RichAudioClip. </param>
        /// <param name="_volume"> Volume of the clip. </param>
        public static RichAudioClip CreateRichAudioClip(AudioClip _newAudioClip, float _volume = 0.5f)
        {

            RichAudioClip newRichAudioClip = ScriptableObject.CreateInstance<RichAudioClip>();

            newRichAudioClip.clip = _newAudioClip;
            newRichAudioClip.volume = _volume;

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
            else if (fileName.ToLower().EndsWith(".ogg") || fileName.ToLower().EndsWith(".oga") || fileName.ToLower().EndsWith(".flac") || fileName.ToLower().EndsWith(".opus"))
            {
                return AudioType.OGGVORBIS;
            }
            else if (fileName.ToLower().EndsWith(".acc") || fileName.ToLower().EndsWith(".aac") || fileName.ToLower().EndsWith(".m4a") || fileName.ToLower().EndsWith(".mp4"))
            {
                return AudioType.ACC;
            }
            else if (fileName.ToLower().EndsWith(".aiff") || fileName.ToLower().EndsWith(".aif") || fileName.ToLower().EndsWith(".aifc"))
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
            else if (fileName.ToLower().EndsWith(".mp2") || fileName.ToLower().EndsWith(".mp3") || fileName.ToLower().EndsWith(".wma"))
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
                MelonLogger.Error("ERROR/WARNING: Unknown audio file type, attempting to still parse it. Expect failure.");
                return AudioType.UNKNOWN;
            }
        }
    }
}
