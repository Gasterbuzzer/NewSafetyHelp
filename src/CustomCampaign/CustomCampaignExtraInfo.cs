using System.Collections.Generic;
using NewSafetyHelp.CallerPatches;
using UnityEngine;

namespace NewSafetyHelp.CustomCampaign
{
    public class CustomCampaignExtraInfo
    {
        public string campaignName = "NO_CAMPAIGN_NAME_PROVIDED";

        public int campaignDays = 5;
        
        public Sprite campaignIcon = null;
        
        public List<CustomCallerExtraInfo> customCallersInCampaign = new List<CustomCallerExtraInfo>();
    }
}