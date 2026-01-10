namespace NewSafetyHelp.CallerPatches.CallerModel
{
    public enum CheckOptions
    {
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        EqualTo,
        NoneSet
    }
    
    public class CustomCaller
    {
        // Base Values
        public RichAudioClip CallerClip = null; // Caller Clip
        public string CallerClipPath = ""; // Caller Clip Path
        public bool IsCallerClipLoaded = false; // Tells if the caller clip is still loading.
        
        public string CallTranscript = "NO_TRANSCRIPT"; // Call Transcript
        public string CallerName = "NO_CALLER_NAME"; // Caller Name
        public UnityEngine.Sprite CallerImage = null; // Caller Image
        
        public int ConsequenceCallerID = -1; // If this caller is a consequence caller, here would be the ID of that original caller.

        public bool CallerIncreasesTier = false;

        public bool LastDayCaller = false; // If this caller will end the day.

        public bool DownedNetworkCaller = false; // If the caller will down the network (Meaning entry information cannot be accessed)
        
        // Monster Profile
        // Monster / Entry name to be attached for. (Used when the caller asks for help and to check if that name is valid).
        public string MonsterNameAttached = "NO_MONSTER_NAME"; 

        public int MonsterIDAttached = -1; // Similar to name but allows also ID to work.
        
        // Call Order
        
        // Order in campaign, used when creating the call list array. If two entries have the same order, the last one will replace it.
        public readonly int OrderInCampaign; 
        
        // Custom Campaign Settings
        public bool InCustomCampaign = false;
        public string BelongsToCustomCampaign = "NO_CUSTOM_CAMPAIGN";
        
        // Special Values
        
        // Warning Call
        public bool IsWarningCaller = false;
        public int WarningCallDay = -1; // If set to -1, it will work for every day if not provided.
        
        // GameOver Call
        public bool IsGameOverCaller = false;
        public int GameOverCallDay = -1; // If set to -1, it will work for every day if not provided.
        
        // Accuracy Caller
        public bool IsAccuracyCaller = false; // If this caller is an accuracy caller.
        public float RequiredAccuracy = -1; // If this caller is an accuracy caller, this is the required accuracy.
        public CheckOptions AccuracyCheck = CheckOptions.NoneSet; // How it should be checked for.
        
        // Constructor
        public CustomCaller(int orderInCampaign) { OrderInCampaign = orderInCampaign;}
    }
}