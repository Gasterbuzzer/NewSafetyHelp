using System.Collections.Generic;
using MelonLoader;
using NewSafetyHelp.CallerPatches;

namespace NewSafetyHelp.CustomCampaign
{
    public static class CustomCampaignGlobal
    {
        public static List<CustomCampaignExtraInfo> customCampaignsAvailable = new List<CustomCampaignExtraInfo>();
        
        public static bool inCustomCampaign = false;

        public static string currentCustomCampaignName = "";
        
        public static void activateCustomCampaign(string customCampaignName)
        {
            inCustomCampaign = true;
            currentCustomCampaignName = customCampaignName;
        }

        public static void deactivateCustomCampaign()
        {
            inCustomCampaign = false;
            currentCustomCampaignName = "";
        }

        public static CustomCampaignExtraInfo getCustomCampaignExtraInfo()
        {
            return customCampaignsAvailable.Find(scannedCampaign => scannedCampaign.campaignName == currentCustomCampaignName);
        }

        public static CustomCallerExtraInfo getCustomCampaignCustomCallerByOrderID(int orderID)
        {
            return getCustomCampaignExtraInfo().customCallersInCampaign.Find(customCaller => customCaller.orderInCampaign == orderID);
        }

        public static void saveCustomCampaignInfo()
        {
            // Custom Campaigns
            //persistantEntrySave = MelonPreferences.CreateCategory("CustomCampaigns");
        }
    }
}