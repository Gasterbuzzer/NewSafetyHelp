namespace NewSafetyHelp.src.EntryManager
{
    public class EntryExtraInfo
    {
        public int? ID { get; }
        public string Name { get; }

        // Extra Info
        public RichAudioClip callerClip = null; // Caller Clip
        public bool replace = false; // If to replace
        public string callTranscript = "NO_TRANSCRIPT"; // Call Transcript
        public bool inCampaign = false; // If to appear in the campaign

        // Extra Extra
        public bool alreadyCalledOnce = false;
        public bool currentlySelected = false; // Used for finding currently selected, for replacing audio. Please know that the information is updated for the canvas before the audio is played.

        // Constructor
        public EntryExtraInfo(int _id) { ID = _id; Name = null; }
        public EntryExtraInfo(string _name) { Name = _name; ID = null; }
        public EntryExtraInfo(string _name, int _id) { Name = _name; ID = _id; }

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

        public override int GetHashCode()
        {
            unchecked // Prevent overflow exceptions
            {
                int hash = 79; // Start with a prime number
                hash = (hash * 31 + ID.GetHashCode() * 31) + (Name?.GetHashCode() * 31 ?? 0);
                return hash;
            }
        }
    }
}
