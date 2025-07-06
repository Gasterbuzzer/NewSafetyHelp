using System.Collections.Generic;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static List<string> customCampaignsAvailable = new List<string>();
        
        public static bool inCustomCampaign = false;

        public static string currentCustomCampaign = "";
    }
}