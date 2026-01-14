using System.Collections.Generic;
using System.IO;
using MelonLoader;
using NewSafetyHelp.Audio;
using NewSafetyHelp.CallerPatches.CallerModel;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.ImportFiles;
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
                MelonLogger.Error(
                    "ERROR: Provided JSON could not be parsed as a custom caller. Possible syntax mistake?");
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
                ref GlobalParsingVariables.CustomCallersMainGame);

            if (customCallerMonsterName != "NO_CUSTOM_CALLER_MONSTER_NAME")
            {
                customCCaller.MonsterNameAttached = customCallerMonsterName;
            }

            // Custom Caller Audio Path (Later gets added with coroutine)
            if (jObjectParsed.ContainsKey("custom_caller_audio_clip_name"))
            {
                if (string.IsNullOrEmpty(customCallerAudioPath))
                {
                    MelonLogger.Warning(
                        $"WARNING: No caller audio given for file in {jsonFolderPath}. No audio will be heard.");
                }
                // Check if location is valid now, since we are storing it now.
                else if (!File.Exists(customCallerAudioPath))
                {
                    MelonLogger.Error(
                        $"ERROR: Location {jsonFolderPath} does not contain '{customCallerAudioPath}'. Unable to add audio.");
                }
                else // Valid location, so we load in the value.
                {
                    MelonCoroutines.Start(ParsingHelper.UpdateAudioClip
                        (
                            (myReturnValue) =>
                            {
                                if (myReturnValue != null)
                                {
                                    // Add the audio
                                    customCCaller.CallerClip = AudioImport.CreateRichAudioClip(myReturnValue);
                                    customCCaller.IsCallerClipLoaded = true;

                                    if (AudioImport.CurrentLoadingAudios.Count <= 0)
                                    {
                                        // We finished loading all audios. We call the start function again.
                                        AudioImport.ReCallCallerListStart();
                                    }
                                }
                                else
                                {
                                    MelonLogger.Error(
                                        $"ERROR: Failed to load audio clip {customCallerAudioPath} for custom caller.");
                                }
                            },
                            customCallerAudioPath)
                    );
                }
            }

            // Now after parsing all values, we add the custom caller to our map

            if (inMainCampaign)
            {
                MelonLogger.Msg("INFO: Found entry to add to the main game.");
                GlobalParsingVariables.CustomCallersMainGame.Add(orderInCampaign, customCCaller);
            }
            else
            {
                // Add to correct campaign.
                CustomCampaign.CustomCampaignModel.CustomCampaign foundCustomCampaign =
                    CustomCampaignGlobal.CustomCampaignsAvailable.Find(customCampaignSearch =>
                        customCampaignSearch.CampaignName == customCampaignName);

                if (foundCustomCampaign != null)
                {
                    if (customCCaller.IsGameOverCaller)
                    {
                        foundCustomCampaign.CustomGameOverCallersInCampaign.Add(customCCaller);
                    }
                    else if (customCCaller.IsWarningCaller)
                    {
                        foundCustomCampaign.CustomWarningCallersInCampaign.Add(customCCaller);
                    }
                    else
                    {
                        foundCustomCampaign.CustomCallersInCampaign.Add(customCCaller);
                    }
                }
                else
                {
                    #if DEBUG
                    MelonLogger.Msg($"DEBUG: Found entry before the custom campaign was found / does not exist.");
                    #endif

                    GlobalParsingVariables.PendingCustomCampaignCustomCallers.Add(customCCaller);
                }
            }

            #if DEBUG
            MelonLogger.Msg($"DEBUG: Finished adding this custom caller.");
            #endif
        }

        private static CustomCCaller ParseCustomCaller(ref JObject jObjectParsed, ref string usermodFolderPath,
            ref string jsonFolderPath, ref string customCampaignName, ref bool inMainCampaign,
            ref string customCallerMonsterName, ref string customCallerAudioPath, ref int orderInCampaign,
            int mainCampaignCallAmount, ref Dictionary<int, CustomCCaller> customCallerMainGame)
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

            int customCallerMonsterID =
                -1; // 99% of times should never be used. Scream at the person who uses it in a bad way.

            // Warning Call
            bool isWarningCaller = false;
            int warningCallDay = -1; // If set to -1, it will work for every day if not provided.

            // GameOver Call
            bool isGameOverCaller = false;
            int gameOverCallDay = -1; // If set to -1, it will work for every day if not provided.

            // Accuracy Caller
            bool isAccuracyCaller = false; // If this caller is an accuracy caller.
            float requiredAccuracy = -1; // If this caller is an accuracy caller, this is the required accuracy.
            // If this caller looks for the day accuracy (true) or if the global accuracy (false).
            bool useTotalAccuracy = false; 
            CheckOptions accuracyCheck = CheckOptions.NoneSet; // How it should be checked for.
            
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
                MelonLogger.Error(
                    "ERROR: Provided custom caller is not attached to either custom campaign or main campaign?");
            }

            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_name", ref customCallerName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_transcript", ref customCallerTranscript);

            if (jObjectParsed.TryGetValue("custom_caller_image_name", out var customCallerImageNameValue))
            {
                string customCallerImageLocation = (string)customCallerImageNameValue;

                if (string.IsNullOrEmpty(customCallerImageLocation))
                {
                    MelonLogger.Error(
                        $"ERROR: Invalid file name given for '{usermodFolderPath}'. No image will be shown {((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"for {customCallerName}" : "")}.");
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

            ParsingHelper.TryAssign(jObjectParsed, "order_in_campaign", ref orderInCampaign);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_monster_name", ref customCallerMonsterName);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_monster_id", ref customCallerMonsterID);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_increases_tier", ref increasesTier);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_last_caller_day", ref isLastCallerOfDay);

            if (jObjectParsed.TryGetValue("custom_caller_audio_clip_name", out var customCallerAudioClipNameValue))
            {
                if (!File.Exists(jsonFolderPath + "\\" + customCallerAudioClipNameValue))
                {
                    if (!File.Exists(usermodFolderPath + "\\" + customCallerAudioClipNameValue))
                    {
                        MelonLogger.Warning(
                            $"WARNING: Could not find provided audio file for custom caller at '{jsonFolderPath}' {((customCallerName != null && customCallerName != "NO_CUSTOM_CALLER_NAME") ? $"for {customCallerName}" : "")}.");
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

            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_consequence_caller_id", ref customCallerConsequenceCallerID);
            ParsingHelper.TryAssign(jObjectParsed, "custom_caller_downed_network", ref downedCall);

            // Warning Caller Section

            ParsingHelper.TryAssign(jObjectParsed, "is_warning_caller", ref isWarningCaller);
            ParsingHelper.TryAssign(jObjectParsed, "warning_caller_day", ref warningCallDay);

            // GameOver Caller Section

            ParsingHelper.TryAssign(jObjectParsed, "is_gameover_caller", ref isGameOverCaller);
            ParsingHelper.TryAssign(jObjectParsed, "gameover_caller_day", ref gameOverCallDay);
            
            // Accuracy Caller Section
            
            ParsingHelper.TryAssign(jObjectParsed, "is_accuracy_caller", ref isAccuracyCaller);
            ParsingHelper.TryAssign(jObjectParsed, "accuracy_required", ref requiredAccuracy);
            ParsingHelper.TryAssign(jObjectParsed, "use_total_accuracy", ref useTotalAccuracy);

            if (jObjectParsed.TryGetValue("accuracy_check_type", out var accuracyCheckTypeValue))
            {
                string accuracyCheckTypeString = accuracyCheckTypeValue.Value<string>();

                if (!string.IsNullOrEmpty(accuracyCheckTypeString))
                {
                    switch (accuracyCheckTypeString)
                    {
                        case "eq": // Equal
                            accuracyCheck = CheckOptions.EqualTo;
                            break;
                        
                        case "none": // None
                            accuracyCheck = CheckOptions.NoneSet;
                            break;
                        
                        case "geq": // Greater than or equal to
                            accuracyCheck = CheckOptions.GreaterThanOrEqualTo;
                            break;
                        
                        case "leq": // Less than or equal to
                            accuracyCheck = CheckOptions.LessThanOrEqualTo;
                            break;
                        
                        default:
                            MelonLogger.Warning($"WARNING: Provided accuracy check type '{accuracyCheckTypeString}' is not in any known format. Please double check.");
                            break;
                    }
                }
                else
                {
                    MelonLogger.Warning("WARNING: Unable of parsing accuracy check type. Possible syntax problem?");
                }
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

            return new CustomCCaller(orderInCampaign)
            {
                CallerName = customCallerName,
                CallerImage = customCallerImage,
                CallTranscript = customCallerTranscript,
                MonsterIDAttached = customCallerMonsterID, // Note, this should 99% of times not be set by user!!!
                InCustomCampaign = !inMainCampaign,
                CallerIncreasesTier = increasesTier,
                CallerClipPath = customCallerAudioPath,
                ConsequenceCallerID = customCallerConsequenceCallerID,
                BelongsToCustomCampaign = customCampaignName,
                LastDayCaller = isLastCallerOfDay,
                DownedNetworkCaller = downedCall,

                IsWarningCaller = isWarningCaller,
                WarningCallDay = warningCallDay,

                IsGameOverCaller = isGameOverCaller,
                GameOverCallDay = gameOverCallDay,
                
                IsAccuracyCaller = isAccuracyCaller,
                RequiredAccuracy = requiredAccuracy,
                UseTotalAccuracy = useTotalAccuracy,
                AccuracyCheck = accuracyCheck
            };
        }
    }
}