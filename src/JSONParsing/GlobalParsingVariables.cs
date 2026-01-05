using System.Collections.Generic;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CallerPatches.CallerModel;
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
        public static List<EntryExtraInfo> EntriesMetadata = new List<EntryExtraInfo>();
        
        // Main Game (Main Campaign) Lists
        public static List<ThemesExtraInfo> MainGameThemes = new List<ThemesExtraInfo>();
        public static List<EmailExtraInfo> MainCampaignEmails = new List<EmailExtraInfo>();
        public static Dictionary<int, CustomCallerExtraInfo> CustomCallersMainGame = new Dictionary<int, CustomCallerExtraInfo>();

        // Custom Campaign Pending Content (Content to be added later because the custom campaign has not been parsed yet)
        public static List<CustomCallerExtraInfo> PendingCustomCampaignCustomCallers = new List<CustomCallerExtraInfo>();
        public static List<EntryExtraInfo> PendingCustomCampaignEntries = new List<EntryExtraInfo>();
        public static List<EntryExtraInfo> PendingCustomCampaignReplaceEntries = new List<EntryExtraInfo>();
        public static List<EmailExtraInfo> PendingCustomCampaignEmails = new List<EmailExtraInfo>();
        public static List<MusicExtraInfo> PendingCustomCampaignMusic = new List<MusicExtraInfo>();
        public static List<ModifierExtraInfo> PendingCustomCampaignModifiers = new List<ModifierExtraInfo>();
        public static List<ThemesExtraInfo> PendingCustomCampaignThemes = new List<ThemesExtraInfo>();
        public static List<CustomVideoExtraInfo> PendingCustomCampaignVideos = new List<CustomVideoExtraInfo>();
        public static List<RingtoneExtraInfo> PendingCustomCampaignRingtones = new List<RingtoneExtraInfo>();
        
        // Configuration
        // ID Offset for Entries in the custom campaign.
        public static int CustomCampaignEntryIDOffset = 100000;

        // Constant Campaign Information. (How many callers exist in the main campaign)
        public const int MainCampaignCallAmount = 116;
    }
}