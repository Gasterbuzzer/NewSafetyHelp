using System.Collections.Generic;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CustomCampaign.CustomRingtone;
using NewSafetyHelp.CustomCampaign.Modifier.Data;
using NewSafetyHelp.CustomCampaign.Themes;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.EntryManager.EntryData;

namespace NewSafetyHelp.JSONParsing
{
    public static class GlobalParsingVariables
    {
        // "Global" Variables for handling extra information such as caller audio. Gets stored as its ID and with its Name.
        public static List<EntryMetadata> EntriesMetadata = new List<EntryMetadata>();
        
        // Main Game (Main Campaign) Lists
        public static List<CustomTheme> MainGameThemes = new List<CustomTheme>();
        public static List<CustomEmail> MainCampaignEmails = new List<CustomEmail>();
        public static Dictionary<int, CallerPatches.CallerModel.CustomCCaller> CustomCallersMainGame = new Dictionary<int, CallerPatches.CallerModel.CustomCCaller>();

        // Custom Campaign Pending Content (Content to be added later because the custom campaign has not been parsed yet)
        public static List<CallerPatches.CallerModel.CustomCCaller> PendingCustomCampaignCustomCallers = new List<CallerPatches.CallerModel.CustomCCaller>();
        public static List<EntryMetadata> PendingCustomCampaignEntries = new List<EntryMetadata>();
        public static List<EntryMetadata> PendingCustomCampaignReplaceEntries = new List<EntryMetadata>();
        public static List<CustomEmail> PendingCustomCampaignEmails = new List<CustomEmail>();
        public static List<CustomMusic> PendingCustomCampaignMusic = new List<CustomMusic>();
        public static List<CustomModifier> PendingCustomCampaignModifiers = new List<CustomModifier>();
        public static List<CustomTheme> PendingCustomCampaignThemes = new List<CustomTheme>();
        public static List<CustomVideo> PendingCustomCampaignVideos = new List<CustomVideo>();
        public static List<CustomRingtone> PendingCustomCampaignRingtones = new List<CustomRingtone>();
        
        // Configuration
        // ID Offset for Entries in the custom campaign.
        public static int CustomCampaignEntryIDOffset = 100000;

        // Constant Campaign Information. (How many callers exist in the main campaign)
        public const int MainCampaignCallAmount = 116;
    }
}