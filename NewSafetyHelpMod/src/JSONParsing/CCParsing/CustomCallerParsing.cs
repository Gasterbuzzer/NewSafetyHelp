using System.Collections.Generic;
using NewSafetyHelp.Audio;
using NewSafetyHelp.Callers.CallerModel;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.Helper.AccuracyModel;
using NewSafetyHelp.JSONParsing.ParsingHelpers;
using NewSafetyHelp.LoggingSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class CustomCallerParsing
    {
        /// <summary>
        /// Creates a custom caller from a provided JSON file.
        /// </summary>
        /// <param name="jObjectParsed"></param>
        /// <param name="usermodFolderPath"></param>
        /// <param name="jsonFolderPath"> Contains the folder path from the JSON file.</param>
        public static void CreateCustomCaller(JObject jObjectParsed, string usermodFolderPath = "",
            string jsonFolderPath = "")
        {
            if (jObjectParsed is null || jObjectParsed.Type != JTokenType.Object ||
                string.IsNullOrEmpty(usermodFolderPath)) // Invalid JSON.
            {
                LoggingHelper.ErrorLog("Provided JSON could not be parsed as a custom caller." +
                                       " Possible syntax mistake?");
                return;
            }

            // Actual logic
            string customCampaignName = "NO_CUSTOM_CAMPAIGN";
            bool inMainCampaign = false;

            // Campaign Values
            int orderInCampaign = -1;

            // Entry / Monster
            string customCallerMonsterName = "NO_CUSTOM_CALLER_MONSTER_NAME";

            // Audio
            string customCallerAudioPath = "";

            // First create a CustomCCaller to assign audio later for it later automatically.
            CustomCCaller customCCaller = ParseCustomCaller(ref jObjectParsed,
                ref usermodFolderPath, ref jsonFolderPath, ref customCampaignName, ref inMainCampaign,
                ref customCallerMonsterName, ref customCallerAudioPath,
                ref orderInCampaign, GlobalParsingVariables.MainCampaignCallAmount,
                GlobalParsingVariables.CustomCallersMainGame);

            if (customCallerMonsterName != "NO_CUSTOM_CALLER_MONSTER_NAME")
            {
                customCCaller.EntryNameAttached = customCallerMonsterName;
            }

            // Custom Caller Audio Path (Later gets added with coroutine) 
            AudioParsingHelper.UpdateAudioAtLocation(jObjectParsed,
                customCCaller.CallerClipPath,
                    clip =>
                    {
                        customCCaller.CallerClip = clip;
                        customCCaller.IsCallerClipLoaded = true;
                        
                        // We finished loading all audios.
                        // We call the start function again.
                        if (AudioImport.CurrentLoadingAudios.Count <= 0)
                        {
                            AudioImport.ReCallCallerListStart();
                        }
                    }, 
                    jsonFolderPath, "custom_caller_audio_clip_name");

            // Now after parsing all values, we add the custom caller to our map
            if (inMainCampaign)
            {
                LoggingHelper.InfoLog("Found entry to add to the main game.");
                GlobalParsingVariables.CustomCallersMainGame.Add(orderInCampaign, customCCaller);
            }
            else
            {
                // Add to correct campaign.
                CustomCampaign customCampaign = CustomCampaignGlobal.GetNamedCustomCampaign(customCampaignName);

                if (customCampaign != null)
                {
                    if (customCCaller.IsGameOverCaller)
                    {
                        customCampaign.CustomGameOverCallersInCampaign.Add(customCCaller);
                    }
                    else if (customCCaller.IsWarningCaller)
                    {
                        customCampaign.CustomWarningCallersInCampaign.Add(customCCaller);
                    }
                    else
                    {
                        customCampaign.CustomCallersInCampaign.Add(customCCaller);
                    }
                }
                else
                {
                    LoggingHelper.DebugLog("Found entry before the custom campaign was found / does not exist.");

                    GlobalParsingVariables.PendingCustomCampaignCustomCallers.Add(customCCaller);
                }
            }

            LoggingHelper.DebugLog("Finished adding this custom caller.");
        }

        private static CustomCCaller ParseCustomCaller(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName, ref bool inMainCampaign,
            ref string customCallerMonsterName, ref string customCallerAudioPath, ref int orderInCampaign,
            int mainCampaignCallAmount, Dictionary<int, CustomCCaller> customCallerMainGame)
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
            
            string callerAnimatedPortraitURL = null;

            // 99% of times should never be used. Scream at the person who uses it in a bad way.
            int customCallerMonsterID = -1; 

            // Warning Call
            bool isWarningCaller = false;
            int warningCallDay = -1; // If set to -1, it will work for every day if not provided.

            // GameOver Call
            bool isGameOverCaller = false;
            int gameOverCallDay = -1; // If set to -1, it will work for every day if not provided.

            // Accuracy Caller
            bool isAccuracyCaller = false; // If this caller is an accuracy caller.
            List<CallerAccuracyType> accuracyChecks = new List<CallerAccuracyType>(); // How it should be checked for.
            bool countEveryCallerForLocalAccuracy = false;
            // If the accuracy check should consider every caller for the day.
            
            // Timed Caller
            bool isTimedCaller = false;
            float timedCallerDuration = 0;
            
            if (jObjectParsed.TryGetValue("custom_campaign_attached", out var customCampaignAttachedValue))
            {
                customCampaignName = (string)customCampaignAttachedValue;
            }
            else if (jObjectParsed.TryGetValue("include_in_main_campaign", out var includeInMainCampaignValue))
            {
                inMainCampaign = (bool)includeInMainCampaignValue;
            }
            else
            {
                LoggingHelper.ErrorLog("Provided custom caller is not attached to either custom campaign or main campaign?");
            }

            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_name", ref customCallerName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_transcript", ref customCallerTranscript);

            ImageParsingHelper.TryAssignSprite(jObjectParsed, "custom_caller_image_name", ref customCallerImage,
                jsonFolderPath, usermodFolderPath, customCampaignName);

            ParsingHelper.TryAssign(jObjectParsed, "order_in_campaign", ref orderInCampaign);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_monster_name", ref customCallerMonsterName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_monster_id", ref customCallerMonsterID);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_increases_tier", ref increasesTier);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_last_caller_day", ref isLastCallerOfDay);

            AudioParsingHelper.TryAssignAudioPath(jObjectParsed, "custom_caller_audio_clip_name",
                ref customCallerAudioPath,  jsonFolderPath, usermodFolderPath, customCallerName);

            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_consequence_caller_id",
                ref customCallerConsequenceCallerID);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_downed_network", ref downedCall);

            // Warning Caller Section

            ParsingHelper.TryAssign(jObjectParsed, "is_warning_caller", ref isWarningCaller);
            ParsingHelper.TryAssign(jObjectParsed, "warning_caller_day", ref warningCallDay);

            // GameOver Caller Section

            ParsingHelper.TryAssign(jObjectParsed, "is_gameover_caller", ref isGameOverCaller);
            ParsingHelper.TryAssign(jObjectParsed, "gameover_caller_day", ref gameOverCallDay);
            
            // Accuracy Caller Section

            bool isAccuracyCallerChanged = false;
            ParsingHelper.TryAssignWithBool(jObjectParsed, "is_accuracy_caller", ref isAccuracyCaller,
                ref isAccuracyCallerChanged);
            
            bool doWeHaveAccuracyCall = AccuracyParsingHelper.TryAssignListAccuracyType(jObjectParsed, ref accuracyChecks);
            if (!isAccuracyCallerChanged 
                && doWeHaveAccuracyCall)
            {
                isAccuracyCaller = true;
            }
            
            ParsingHelper.TryAssign(jObjectParsed, "accuracy_caller_count_total_day_instead",
                ref countEveryCallerForLocalAccuracy);

            // Animated Portrait
            bool callerHasAnimatedPortrait = VideoParsingHelper.TryAssignVideoPath(jObjectParsed,
                "custom_caller_animated_portrait_name",
                ref callerAnimatedPortraitURL, jsonFolderPath, usermodFolderPath);
            
            // Timed Caller
            ParsingHelper.TryAssign(jObjectParsed, "is_timed_caller", ref isTimedCaller);
            ParsingHelper.TryAssign(jObjectParsed, "timed_caller_duration", ref timedCallerDuration);

            // Check if order is valid and if not, we warn the user.
            if (orderInCampaign < 0 && !isWarningCaller && !isGameOverCaller)
            {
                LoggingHelper.WarningLog($"No order was provided for custom caller at '{jsonFolderPath}'. " +
                                         "This could accidentally replace a caller! Set to replace last caller! " +
                                         $"{((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"(Caller Name: {customCallerName})" : "")}");
                orderInCampaign = mainCampaignCallAmount + customCallerMainGame.Count;
            }

            return new CustomCCaller(orderInCampaign)
            {
                CallerName = customCallerName,
                CallerImage = customCallerImage,
                CallTranscript = customCallerTranscript,
                EntryIDAttached = customCallerMonsterID, // Note, this should 99% of times not be set by user!!!
                InCustomCampaign = !inMainCampaign,
                CallerIncreasesTier = increasesTier,
                CallerClipPath = customCallerAudioPath,
                ConsequenceCallerID = customCallerConsequenceCallerID,
                CustomCampaignName = customCampaignName,
                LastDayCaller = isLastCallerOfDay,
                DownedNetworkCaller = downedCall,
                
                CallerAnimatedPortraitURL =  callerAnimatedPortraitURL,
                CallerHasAnimatedPortrait = callerHasAnimatedPortrait,

                IsWarningCaller = isWarningCaller,
                WarningCallDay = warningCallDay,

                IsGameOverCaller = isGameOverCaller,
                GameOverCallDay = gameOverCallDay,
                
                IsAccuracyCaller = isAccuracyCaller,
                AccuracyChecks = accuracyChecks,
                CountEveryCallerForLocalAccuracy = countEveryCallerForLocalAccuracy,
                
                IsTimedCaller = isTimedCaller,
                TimedCallerDuration = timedCallerDuration
            };
        }
    }
}