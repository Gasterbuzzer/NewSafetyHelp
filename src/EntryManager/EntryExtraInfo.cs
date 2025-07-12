namespace NewSafetyHelp.EntryManager
{
    public class EntryExtraInfo
    {
        public int? ID;
        public string Name { get; }
        
        // Since it's useful to store values outside
        // Entry Values
        public string entryDescription;
        
        // Extra Info Caller
        public RichAudioClip callerClip = null; // Caller Clip
        public string callTranscript = "NO_TRANSCRIPT"; // Call Transcript
        public string callerName = "NO_CALLER_NAME"; // Caller Name
        public UnityEngine.Sprite callerImage = null; // Caller Image
        public float callerReplaceChance = 0.1f; // Chance that this entry replaces the normal caller.

        public bool allowCallAgainOverRestart = true; // If allowed to ignore the saved value and to allow calling again.

        // Extra Info
        public bool replace = false; // If to replace
        public bool inMainCampaign = false; // If to appear in the campaign
        public int permissionLevel = 0; // Required permission for a call to be replaced.
        public bool onlyDLC = false; // If only in the DLC.
        public bool includeInDLC = false; // If to also include in the DLC

        // Extra Extra
        public bool alreadyCalledOnce = false;
        public bool currentlySelected = false; // Used for finding currently selected, for replacing audio. Please know that the information is updated for the canvas before the audio is played.

        // Copy of Monster Entry
        public MonsterProfile referenceCopyEntry = null;

        // Copy of Caller (if called)
        public string referenceProfileNameInternal = null;

        // Consequence Values
        public string consequenceName = "NO_CONSEQUENCE_CALLER_NAME";
        public string consequenceTranscript = "NO_CONSEQUENCE_TRANSCRIPT";
        public RichAudioClip consequenceCallerClip = null; // Consequence Caller Clip
        public UnityEngine.Sprite consequenceCallerImage = null; // Consequence Caller Image
        
        // Custom Campaign
        public bool onlyCustomCampaign = false;
        public string customCampaignName = "NO_CUSTOM_CAMPAIGN_NAME";

        // Constructor
        public EntryExtraInfo(int _id) { ID = _id; Name = null; }
        public EntryExtraInfo(string _name) { Name = _name; ID = null; }
        public EntryExtraInfo(string _name, int _id) { Name = _name; ID = _id; }

        /// <summary>
        /// Overloaded equals function for comparing this object to another.
        /// </summary>
        /// <param name="obj"> Object to compare to. </param>
        public override bool Equals(object obj)
        {
            if (obj is EntryExtraInfo other)
            {
                return ID == other.ID && Name == other.Name;
            }

            if (obj is string otherN)
            {
                return otherN == this.Name;
            }

            if (obj is int otherI)
            {
                return otherI == this.ID;
            }

            return false;
        }

        /// <summary>
        /// Gets the hash for optimization.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // Prevent overflow exceptions
            {
                int hash = 79; // Start with a prime number
                hash += (Name?.GetHashCode() * 31 ?? 0);
                return hash;
            }
        }
    }
}
