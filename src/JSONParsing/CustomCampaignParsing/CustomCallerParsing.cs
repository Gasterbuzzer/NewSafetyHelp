using System.Collections.Generic;
using System.IO;
using MelonLoader;
using NewSafetyHelp.CallerPatches;
using NewSafetyHelp.EntryManager;
using NewSafetyHelp.ImportFiles;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CustomCampaignParsing
{
    public static class CustomCallerParsing
    {
        public static CustomCallerExtraInfo ParseCustomCaller(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName, ref bool inMainCampaign, 
            ref string customCallerMonsterName, ref string customCallerAudioPath, ref int orderInCampaign,
            int mainCampaignCallAmount, ref Dictionary<int, CustomCallerExtraInfo> customCallerMainGame)
        {
            
            // Caller Information
            string customCallerName = "NO_CUSTOM_CALLER_NAME";
            string customCallerTranscript = "NO_CUSTOM_CALLER_TRANSCRIPT";

            bool increasesTier = false;
            bool isLastCallerOfDay = false;

            bool downedCall = false; // If the entries cannot be accessed while the caller is calling.

            int customCallerConsequenceCallerID =
                -1; // If this call is due to a consequence caller. You can provide it here.

            Sprite customCallerImage = null;
            
            int customCallerMonsterID = -1; // 99% of times should never be used. Scream at the person who uses it in a bad way.

            // Warning Call

            bool isWarningCaller = false;
            int warningCallDay = -1; // If set to -1, it will work for every day if not provided.

            // GameOver Call
            bool isGameOverCaller = false;
            int gameOverCallDay = -1; // If set to -1, it will work for every day if not provided.


            if (jObjectParsed.TryGetValue("custom_campaign_attached", out var customCampaignAttachedValue))
            {
                customCampaignName = (string) customCampaignAttachedValue;
            }
            else if (jObjectParsed.TryGetValue("include_in_main_campaign", out var includeInMainCampaignValue))
            {
                inMainCampaign = (bool) includeInMainCampaignValue;
            }
            else
            {
                MelonLogger.Error(
                    "ERROR: Provided custom caller is not attached to either custom campaign or main campaign?");
            }

            if (jObjectParsed.TryGetValue("custom_caller_name", out var customCallerNameValue))
            {
                customCallerName = (string) customCallerNameValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_transcript", out var customCallerTranscriptValue))
            {
                customCallerTranscript = (string) customCallerTranscriptValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_image_name", out var customCallerImageNameValue))
            {
                string customCallerImageLocation = (string) customCallerImageNameValue;

                if (string.IsNullOrEmpty(customCallerImageLocation))
                {
                    MelonLogger.Error($"ERROR: Invalid file name given for '{usermodFolderPath}'. No image will be shown {((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"for {customCallerName}" : "")}.");
                }
                else
                {
                    customCallerImage = ImageImport.LoadImage(jsonFolderPath + "\\" + customCallerImageLocation,
                        usermodFolderPath + "\\" + customCallerImageLocation);
                }
            }
            else
            {
                MelonLogger.Warning(
                    $"WARNING: No custom caller portrait given for file in {usermodFolderPath}. No image will be shown {((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"for {customCallerName}" : "")}.");
            }

            if (jObjectParsed.TryGetValue("order_in_campaign", out var orderInCampaignValue))
            {
                orderInCampaign = (int) orderInCampaignValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_monster_name", out var customCallerMonsterNameValue))
            {
                customCallerMonsterName = (string) customCallerMonsterNameValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_monster_id", out var customCallerMonsterIDValue))
            {
                customCallerMonsterID = (int) customCallerMonsterIDValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_increases_tier", out var customCallerIncreaseTierValue))
            {
                increasesTier = (bool) customCallerIncreaseTierValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_last_caller_day", out var customCallerLastCallerDayValue))
            {
                isLastCallerOfDay = (bool) customCallerLastCallerDayValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_audio_clip_name", out var customCallerAudioClipNameValue))
            {
                if (!File.Exists(jsonFolderPath + "\\" + customCallerAudioClipNameValue))
                {
                    if (!File.Exists(usermodFolderPath + "\\" + customCallerAudioClipNameValue))
                    {
                        MelonLogger.Warning($"WARNING: Could not find provided audio file for custom caller at '{jsonFolderPath}' {((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"for {customCallerName}" : "")}.");
                    }
                    else
                    {
                        customCallerAudioPath = usermodFolderPath + "\\" + customCallerAudioClipNameValue;
                    }
                }
                else
                {
                    customCallerAudioPath = jsonFolderPath + "\\" + customCallerAudioClipNameValue;
                }
                
                
            }

            if (jObjectParsed.TryGetValue("custom_caller_consequence_caller_id", out var customCallerConsequenceCallerIDValue))
            {
                customCallerConsequenceCallerID = (int) customCallerConsequenceCallerIDValue;
            }

            if (jObjectParsed.TryGetValue("custom_caller_downed_network", out var customCallerDownedNetworkValue))
            {
                downedCall = (bool) customCallerDownedNetworkValue;
            }

            // Warning Caller Section

            if (jObjectParsed.TryGetValue("is_warning_caller", out var isWarningCallerValue))
            {
                isWarningCaller = (bool) isWarningCallerValue;
            }

            if (isWarningCaller && jObjectParsed.TryGetValue("warning_caller_day", out var warningCallerDayValue))
            {
                warningCallDay = (int) warningCallerDayValue;
            }

            // GameOver Caller Section

            if (jObjectParsed.TryGetValue("is_gameover_caller", out var isGameOverCallerValue))
            {
                isGameOverCaller = (bool) isGameOverCallerValue;
            }

            if (isGameOverCaller && jObjectParsed.TryGetValue("gameover_caller_day", out var gameOverCallerDayValue))
            {
                gameOverCallDay = (int) gameOverCallerDayValue;
            }

            // Check if order is valid and if not, we warn the user.
            if (orderInCampaign < 0 && !isWarningCaller && !isGameOverCaller)
            {
                MelonLogger.Warning(
                    $"WARNING: No order was provided for custom caller at '{jsonFolderPath}'. " +
                    "This could accidentally replace a caller! Set to replace last caller! " +
                    $"{((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"(Caller Name: {customCallerName})" : "")}");
                orderInCampaign = mainCampaignCallAmount + customCallerMainGame.Count;
            }
            
            return new CustomCallerExtraInfo(orderInCampaign)
            {
                callerName = customCallerName,
                callerImage = customCallerImage,
                callTranscript = customCallerTranscript,
                monsterIDAttached = customCallerMonsterID, // Note, this should 99% of times not be set by user!!!
                inCustomCampaign = !inMainCampaign,
                callerIncreasesTier = increasesTier,
                callerClipPath = customCallerAudioPath,
                consequenceCallerID = customCallerConsequenceCallerID,
                belongsToCustomCampaign = customCampaignName,
                lastDayCaller = isLastCallerOfDay,
                downedNetworkCaller = downedCall,

                isWarningCaller = isWarningCaller,
                warningCallDay = warningCallDay,

                isGameOverCaller = isGameOverCaller,
                gameOverCallDay = gameOverCallDay
            };
        }
    }
}