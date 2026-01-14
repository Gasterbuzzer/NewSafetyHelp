using MelonLoader;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.EntryParsing
{
    public static class CallerParsing
    {
        public static void ParseCaller(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string callerName, ref string callerTranscript,
            ref string callerImageLocation, ref float callerReplaceChance, ref bool callerRestartCallAgain,
            ref Sprite callerPortrait)
        {
            ParsingHelper.TryAssign(jsonObjectParsed, "caller_name", ref callerName);
            ParsingHelper.TryAssign(jsonObjectParsed, "caller_transcript", ref callerTranscript);

            // Caller Image
            if (jsonObjectParsed.TryGetValue("caller_image_name", out var callerImageNameValue))
            {
                callerImageLocation = (string) callerImageNameValue;

                if (string.IsNullOrEmpty(callerImageLocation))
                {
                    callerPortrait = null;

                    MelonLogger.Warning($"WARNING: Invalid Caller Portrait for {usermodFolderPath}. No image will be shown.");
                }
                else
                {
                    callerPortrait = ImageImport.LoadImage(jsonFolderPath + "\\" + callerImageLocation,
                        usermodFolderPath + "\\" + callerImageLocation);
                }
            }

            ParsingHelper.TryAssign(jsonObjectParsed, "caller_chance", ref callerReplaceChance);
            
            // If to store the information if it was already called once.
            ParsingHelper.TryAssign(jsonObjectParsed, "allow_calling_again_over_restarts", ref callerRestartCallAgain);
        }

        public static void ParseConsequenceCaller(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string consequenceCallerName, ref string consequenceCallerTranscript,
            ref string consequenceCallerImageLocation, ref Sprite consequenceCallerPortrait)
        {
            /* 
            * Consequence Caller Information
            */
            
            ParsingHelper.TryAssign(jsonObjectParsed, "consequence_caller_name", ref consequenceCallerName);
            ParsingHelper.TryAssign(jsonObjectParsed, "consequence_caller_transcript", ref consequenceCallerTranscript);
            
            // Consequence Caller Image
            if (jsonObjectParsed.TryGetValue("consequence_caller_image_name", out var consequenceCallerImageNameValue))
            {
                consequenceCallerImageLocation = (string) consequenceCallerImageNameValue;

                if (string.IsNullOrEmpty(consequenceCallerImageLocation))
                {
                    consequenceCallerPortrait = null;

                    MelonLogger.Warning($"WARNING: Invalid Consequence Caller Portrait for {usermodFolderPath}. No image will be shown.");
                }
                else
                {
                    consequenceCallerPortrait = ImageImport.LoadImage(jsonFolderPath + "\\" + consequenceCallerImageLocation,
                        usermodFolderPath + "\\" + consequenceCallerImageLocation);
                }
            }
        }
    }
}
