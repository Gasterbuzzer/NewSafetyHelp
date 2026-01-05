using MelonLoader;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.EntryParsing
{
    public static class CallerParsing
    {
        public static void ParseCaller(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string _callerName, ref string _callerTranscript,
            ref string _callerImageLocation, ref float _callerReplaceChance, ref bool _callerRestartCallAgain,
            ref Sprite _callerPortrait)
        {
            // Caller Name
            if (jsonObjectParsed.TryGetValue("caller_name", out var callerNameValue))
            {
                _callerName = (string) callerNameValue;
            }

            // Caller Transcript
            if (jsonObjectParsed.TryGetValue("caller_transcript", out var callerTranscriptValue))
            {
                _callerTranscript = (string) callerTranscriptValue;
            }

            // Caller Image
            if (jsonObjectParsed.TryGetValue("caller_image_name", out var callerImageNameValue))
            {
                _callerImageLocation = (string) callerImageNameValue;

                if (string.IsNullOrEmpty(_callerImageLocation))
                {
                    _callerPortrait = null;

                    MelonLogger.Warning($"WARNING: Invalid Caller Portrait for {usermodFolderPath}. No image will be shown.");
                }
                else
                {
                    _callerPortrait = ImageImport.LoadImage(jsonFolderPath + "\\" + _callerImageLocation,
                        usermodFolderPath + "\\" + _callerImageLocation);
                }
            }

            // Caller Replace Chance
            if (jsonObjectParsed.TryGetValue("caller_chance", out var callerChanceValue))
            {
                _callerReplaceChance = (float) callerChanceValue;
            }


            // If to store the information if it was already called once.
            if (jsonObjectParsed.TryGetValue("allow_calling_again_over_restarts", out var allowCallingAgainOverRestartsValue))
            {
                _callerRestartCallAgain = (bool) allowCallingAgainOverRestartsValue;
            }
            
        }

        public static void ParseConsequenceCaller(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string _consequenceCallerName, ref string _consequenceCallerTranscript,
            ref string _consequenceCallerImageLocation, ref Sprite _consequenceCallerPortrait)
        {
            /* 
            * Consequence Caller Information
            */

            // Consequence Caller Name
            if (jsonObjectParsed.TryGetValue("consequence_caller_name", out var consequenceCallerNameValue))
            {
                _consequenceCallerName = (string) consequenceCallerNameValue;
            }

            // Consequence Caller Transcript
            if (jsonObjectParsed.TryGetValue("consequence_caller_transcript", out var consequenceCallerTranscriptValue))
            {
                _consequenceCallerTranscript = (string) consequenceCallerTranscriptValue;
            }

            // Consequence Caller Image
            if (jsonObjectParsed.TryGetValue("consequence_caller_image_name", out var consequenceCallerImageNameValue))
            {
                _consequenceCallerImageLocation = (string) consequenceCallerImageNameValue;

                if (string.IsNullOrEmpty(_consequenceCallerImageLocation))
                {
                    _consequenceCallerPortrait = null;

                    MelonLogger.Warning($"WARNING: Invalid Consequence Caller Portrait for {usermodFolderPath}. No image will be shown.");
                }
                else
                {
                    _consequenceCallerPortrait = ImageImport.LoadImage(jsonFolderPath + "\\" + _consequenceCallerImageLocation,
                        usermodFolderPath + "\\" + _consequenceCallerImageLocation);
                }
            }
        }
    }
}
