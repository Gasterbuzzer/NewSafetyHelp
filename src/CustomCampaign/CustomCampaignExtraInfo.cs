using System.Collections.Generic;
using NewSafetyHelp.CallerPatches;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign
{
    public class CustomCampaignExtraInfo
    {
        public string campaignName = "NO_CAMPAIGN_NAME_PROVIDED";

        public int campaignDays = 7;
        
        // Desktop
        public Sprite campaignIcon = null;
        public string campaignDesktopName = "NO_CAMPAIGN_DESKTOP_NAME";
        
        // In Game
        public List<CustomCallerExtraInfo> customCallersInCampaign = new List<CustomCallerExtraInfo>();
        
        public List<string> campaignDayStrings = new List<string>();
    }
}