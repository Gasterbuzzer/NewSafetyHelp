namespace NewSafetyHelp.CallerPatches.CallerModel
{
    public class CustomCallerExtraInfo
    {
        // Base Values
        public RichAudioClip callerClip = null; // Caller Clip
        public string callerClipPath = ""; // Caller Clip Path
        public bool isCallerClipLoaded = false; // Tells if the caller clip is still loading.
        
        public string callTranscript = "NO_TRANSCRIPT"; // Call Transcript
        public string callerName = "NO_CALLER_NAME"; // Caller Name
        public UnityEngine.Sprite callerImage = null; // Caller Image
        
        public int consequenceCallerID = -1; // If this caller is a consequence caller, here would be the ID of that original caller.

        public bool callerIncreasesTier = false;

        public bool lastDayCaller = false; // If this caller will end the day.

        public bool downedNetworkCaller = false; // If the caller will down the network (Meaning entry information cannot be accessed)
        
        // Monster Profile
        public string monsterNameAttached = "NO_MONSTER_NAME"; // Monster / Entry name to be attached for. (Used when the caller asks for help and to check if that name is valid).

        public int monsterIDAttached = -1; // Similar to name but allows also ID to work.
        
        // Call Order
        public int orderInCampaign; // Order in campaign, used when creating the call list array. If two entries have the same order, the last one will replace it.
        
        // Custom Campaign Settings
        public bool inCustomCampaign = false;
        public string belongsToCustomCampaign = "NO_CUSTOM_CAMPAIGN";
        
        // Special Values
        
        // Warning Call
        public bool isWarningCaller = false;
        public int warningCallDay = -1; // If set to -1, it will work for every day if not provided.
        
        // GameOver Call
        public bool isGameOverCaller = false;
        public int gameOverCallDay = -1; // If set to -1, it will work for every day if not provided.
        
        // Constructor
        public CustomCallerExtraInfo(int _orderInCampaign) { orderInCampaign = _orderInCampaign;}
    }
}