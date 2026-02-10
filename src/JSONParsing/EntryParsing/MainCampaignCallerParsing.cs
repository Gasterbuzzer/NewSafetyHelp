using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.EntryParsing
{
    public static class MainCampaignCallerParsing
    {
        public static void ParseCaller(ref JObject jsonObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string callerName, ref string callerTranscript,
            ref string callerImageLocation, ref float callerReplaceChance, ref bool callerRestartCallAgain,
            ref Sprite callerPortrait)
        {
            ParsingHelper.TryAssign(jsonObjectParsed, "caller_name", ref callerName);
            ParsingHelper.TryAssign(jsonObjectParsed, "caller_transcript", ref callerTranscript);

            // Caller Image
            ParsingHelper.TryAssign(jsonObjectParsed, "caller_image_name", ref callerImageLocation);
            ParsingHelper.TryAssignSprite(jsonObjectParsed, "caller_image_name", ref callerPortrait,
                jsonFolderPath, usermodFolderPath, callerName);

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
            ParsingHelper.TryAssign(jsonObjectParsed, "consequence_caller_image_name", ref consequenceCallerImageLocation);
            ParsingHelper.TryAssignSprite(jsonObjectParsed, "caller_image_name", ref consequenceCallerPortrait,
                jsonFolderPath, usermodFolderPath, consequenceCallerName);
        }
    }
}
