using MelonLoader;
using NewSafetyHelp.EntryManager;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing
{
    public static class CallerParsing
    {
        public static void parseCaller(ref JObject jsonObjectParsed, ref string filePath, ref string _callerAudioClipLocation, ref string _callerName, ref string _callerTranscript, ref string _callerImageLocation,  ref float _callerReplaceChance, ref bool _callerRestartCallAgain,
            ref Sprite _callerPortrait, ref bool replaceEntry, ref EntryExtraInfo newExtra)
        {
            /* 
             * Caller Information
            */

            // Caller Name
            if (jsonObjectParsed.ContainsKey("caller_name"))
            {
                _callerName = (string) jsonObjectParsed["caller_name"];
            }

            // Caller Transcript
            if (jsonObjectParsed.ContainsKey("caller_transcript"))
            {
                _callerTranscript = (string) jsonObjectParsed["caller_transcript"];
            }

            // Caller Image
            if (jsonObjectParsed.ContainsKey("caller_image_name"))
            {
                _callerImageLocation = (string) jsonObjectParsed["caller_image_name"];

                if (string.IsNullOrEmpty(_callerImageLocation))
                {
                    _callerPortrait = null;

                    MelonLogger.Warning($"WARNING: Invalid Caller Portrait for {filePath}. No image will be shown.");
                }
                else
                {
                    _callerPortrait = ImageImport.LoadImage(filePath + "\\" + _callerImageLocation);
                }
            }

            // Caller Replace Chance
            if (jsonObjectParsed.ContainsKey("caller_chance"))
            {
                _callerReplaceChance = (float) jsonObjectParsed["caller_chance"];
            }


            // If to store the information if it was already called once.
            if (jsonObjectParsed.ContainsKey("allow_calling_again_over_restarts"))
            {
                _callerRestartCallAgain = (bool) jsonObjectParsed["allow_calling_again_over_restarts"];
            }
            
        }

        public static void parseConsequenceCaller(ref JObject jsonObjectParsed, ref string filePath, ref string _consequenceCallerName, ref string _consequenceCallerTranscript, ref string _consequenceCallerImageLocation, ref Sprite _consequenceCallerPortrait)
        {
            /* 
            * Consequence Caller Information
            */

            // Consequence Caller Name
            if (jsonObjectParsed.ContainsKey("consequence_caller_name"))
            {
                _consequenceCallerName = (string) jsonObjectParsed["consequence_caller_name"];
            }

            // Consequence Caller Transcript
            if (jsonObjectParsed.ContainsKey("consequence_caller_transcript"))
            {
                _consequenceCallerTranscript = (string) jsonObjectParsed["consequence_caller_transcript"];
            }

            // Consequence Caller Image
            if (jsonObjectParsed.ContainsKey("consequence_caller_image_name"))
            {
                _consequenceCallerImageLocation = (string) jsonObjectParsed["consequence_caller_image_name"];

                if (string.IsNullOrEmpty(_consequenceCallerImageLocation))
                {
                    _consequenceCallerPortrait = null;

                    MelonLogger.Warning($"WARNING: Invalid Consequence Caller Portrait for {filePath}. No image will be shown.");
                }
                else
                {
                    _consequenceCallerPortrait = ImageImport.LoadImage(filePath + "\\" + _consequenceCallerImageLocation);
                }
            }
        }
    }
}
