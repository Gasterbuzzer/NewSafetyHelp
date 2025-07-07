using System.Collections.Generic;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static List<CustomCampaignExtraInfo> customCampaignsAvailable = new List<CustomCampaignExtraInfo>();
        
        public static bool inCustomCampaign = false;

        public static string currentCustomCampaignName = "";
    }
}