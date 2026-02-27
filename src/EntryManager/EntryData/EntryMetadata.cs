using System;
using NewSafetyHelp.CustomCampaignPatches.Abstract;

namespace NewSafetyHelp.EntryManager.EntryData
{
    public class EntryMetadata : CustomCampaignElementBase
    {
        public int? ID;
        public string Name { get; }
        
        // Since it's useful to store values outside
        // Entry Values
        public string EntryDescription;
        
        // Extra Info Caller
        public RichAudioClip CallerClip = null; // Caller Clip
        public string CallTranscript = "NO_TRANSCRIPT"; // Call Transcript
        public string CallerName = "NO_CALLER_NAME"; // Caller Name
        public UnityEngine.Sprite CallerImage = null; // Caller Image
        public float CallerReplaceChance = 0.1f; // Chance that this entry replaces the normal caller.

        // If allowed to ignore the saved value and to allow calling again.
        public bool AllowCallAgainOverRestart = true; 

        // Extra Info
        public bool Replace = false; // If to replace
        public bool InMainCampaign = false; // If to appear in the campaign
        public int PermissionLevel = 0; // Required permission for a call to be replaced.
        public bool OnlyDLC = false; // If only in the DLC.
        public bool IncludeInDlc = false; // If to also include in the DLC

        // Extra Extra
        public bool AlreadyCalledOnce = false;
        // Used for finding currently selected, for replacing audio.
        // Please know that the information is updated for the canvas before the audio is played.
        public bool CurrentlySelected = false; 

        // Copy of Monster Entry
        public MonsterProfile ReferenceCopyEntry = null;

        // Copy of Caller (if called)
        public string ReferenceProfileNameInternal = null;

        // Consequence Values
        public string ConsequenceName = "NO_CONSEQUENCE_CALLER_NAME";
        public string ConsequenceTranscript = "NO_CONSEQUENCE_TRANSCRIPT";
        public RichAudioClip ConsequenceCallerClip = null; // Consequence Caller Clip
        public UnityEngine.Sprite ConsequenceCallerImage = null; // Consequence Caller Image
        
        // Custom Campaign
        public bool OnlyCustomCampaign = false;
        public bool DeleteEntry = false; // If to delete the entry (Only works in replacing mode)
        
        public string VideoUrlPortrait = String.Empty;
        public bool IsVideoPortrait = false;

        // Constructor
        public EntryMetadata(string name, int id) { Name = name; ID = id; }
    }
}
