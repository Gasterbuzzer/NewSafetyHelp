using System.Collections.Generic;
using NewSafetyHelp.Audio.Music.Data;
using NewSafetyHelp.CallerPatches.CallerModel;
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
        public static List<EntryExtraInfo> entriesExtraInfo = new List<EntryExtraInfo>();
        
        // Main Game Themes
        public static List<ThemesExtraInfo> mainGameThemes = new List<ThemesExtraInfo>();

        // Map for custom callers to replaced in the main game. (ID of the call to replace, Caller for that ID)
        public static Dictionary<int, CustomCallerExtraInfo> customCallerMainGame =
            new Dictionary<int, CustomCallerExtraInfo>();

        // List of custom caller yet to be added to custom campaign. Happens when the custom caller file was found before.
        public static List<CustomCallerExtraInfo> missingCustomCallerCallersCustomCampaign =
            new List<CustomCallerExtraInfo>();

        // List of entries yet to be added to custom campaign. Happens when the entries file was found before.
        public static List<EntryExtraInfo> missingEntriesCustomCampaign = new List<EntryExtraInfo>();

        // List of entries that replace yet to be added to custom campaign. Happens when the replacement entries file was found before.
        public static List<EntryExtraInfo> missingReplaceEntriesCustomCampaign = new List<EntryExtraInfo>();

        // List of emails to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<EmailExtraInfo> missingCustomCampaignEmails = new List<EmailExtraInfo>();

        // List of music to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<MusicExtraInfo> missingCustomCampaignMusic = new List<MusicExtraInfo>();

        // List of modifiers to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<ModifierExtraInfo> missingCustomCampaignModifier = new List<ModifierExtraInfo>();
        
        // List of themes to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<ThemesExtraInfo> missingCustomCampaignTheme = new List<ThemesExtraInfo>();

        // List of videos to be added in a custom campaign when the custom campaign is not parsed yet.
        public static List<CustomVideoExtraInfo> missingCustomCampaignVideo = new List<CustomVideoExtraInfo>();

        // List of emails to be added in the main campaign.
        public static List<EmailExtraInfo> mainCampaignEmails = new List<EmailExtraInfo>();

        // Entry Amount
        public static int amountExtra = 100000;

        // Campaign Information
        public const int mainCampaignCallAmount = 116;
    }
}