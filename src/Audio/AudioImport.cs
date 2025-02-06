using MelonLoader;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NewSafetyHelp.src.AudioHandler
{
    public static class AudioImport
    {

        /// <summary>
        /// Coroutine that gets an audio clip from a specified path, please note to also provide an audiotype, defaulted to MPEG.
        /// </summary>
        /// <param name="callback"> Callback function used for getting the AudioClip back. </param>
        /// <param name="path"> Path to file. </param>
        /// <param name="_audioType"> Unity AudioType </param>
        public static IEnumerator LoadAudio(System.Action<AudioClip> callback, string path, AudioType _audioType = AudioType.MPEG)
        {
            MelonLogger.Msg($"INFO: Attempting to add {path} as audio type {_audioType.ToString()}.");

            // First we check if the file exists
            if (!File.Exists(path))
            {
                MelonLogger.Error($"ERROR: Given path to file {path} of type {_audioType.ToString()} does not exist.");
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
                    MelonLogger.Msg($"Info: {path} as {_audioType.ToString()} has been successfully loaded.");

                    callback(DownloadHandlerAudioClip.GetContent(www)); // Get actual audio clip
                    yield break;
                }
                else // Failed loading the audio file.
                {
                    if (!operation.isDone)
                    {
                        MelonLogger.Error("Not finished, please wait.");
                    }

                    MelonLogger.Error($"ERROR: Was unable of loading {path} as audio type {_audioType.ToString()}. \n Results in the error: {www.error} and the response code is: {www.responseCode}. Was the proccess finished?: {www.isDone}");
                    yield break; // Rip rip
                }
            }
        }

        /// <summary>
        /// Creates a new rich audio clip from a provided audioclip. Used for creating a monster.
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
    }
}
