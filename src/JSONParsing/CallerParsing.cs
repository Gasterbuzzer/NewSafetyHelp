using MelonLoader;
using MelonLoader.TinyJSON;
using NewSafetyHelp.src.ImportFiles;
using NewSafetyHelp.src.EntryManager;
using System.IO;
using NewSafetyHelp.src.AudioHandler;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

namespace NewSafetyHelp.src.JSONParsing
{
    public class CallerParsing
    {
        public static void parseCaller(ref ProxyObject jsonObject, ref string filePath, ref string _callerAudioClipLocation, ref string _callerName, ref string _callerTranscript, ref string _callerImageLocation,  ref float _callerReplaceChance, ref bool _callerRestartCallAgain,
            ref Sprite _callerPortrait, ref bool replaceEntry, ref EntryExtraInfo newExtra)
        {
            /* 
             * Caller Information
            */

            // Caller Name
            if (jsonObject.Keys.Contains("caller_name"))
            {
                _callerName = jsonObject["caller_name"];
            }

            // Caller Transcript
            if (jsonObject.Keys.Contains("caller_transcript"))
            {
                _callerTranscript = jsonObject["caller_transcript"];
            }

            // Caller Image
            if (jsonObject.Keys.Contains("caller_image_name"))
            {
                _callerImageLocation = jsonObject["caller_image_name"];

                if (_callerImageLocation == "" || _callerImageLocation == null)
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
            if (jsonObject.Keys.Contains("caller_chance"))
            {
                _callerReplaceChance = jsonObject["caller_chance"];
            }


            // If to store the information if it was already called once.
            if (jsonObject.Keys.Contains("allow_calling_again_over_restarts"))
            {
                _callerRestartCallAgain = jsonObject["allow_calling_again_over_restarts"];
            }
            
        }

        public static void parseConsequenceCaller(ref ProxyObject jsonObject, ref string filePath, ref string _consequenceCallerName, ref string _consequenceCallerTranscript, ref string _consequenceCallerImageLocation, ref Sprite _consequenceCallerPortrait)
        {
            /* 
            * Consequence Caller Information
            */

            // Consequence Caller Name
            if (jsonObject.Keys.Contains("consequence_caller_name"))
            {
                _consequenceCallerName = jsonObject["consequence_caller_name"];
            }

            // Consequence Caller Transcript
            if (jsonObject.Keys.Contains("consequence_caller_transcript"))
            {
                _consequenceCallerTranscript = jsonObject["consequence_caller_transcript"];
            }

            // Consequence Caller Image
            if (jsonObject.Keys.Contains("consequence_caller_image_name"))
            {
                _consequenceCallerImageLocation = jsonObject["consequence_caller_image_name"];

                if (_consequenceCallerImageLocation == "" || _consequenceCallerImageLocation == null)
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
