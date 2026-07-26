using JetBrains.Annotations;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.CustomCampaignSystem.CustomComputer3DScreen
{
    public static class Computer3DScreenHelper
    {
        [CanBeNull]
        public static Computer3DScreen Pick3DComputerScreen()
        {
            CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

            if (customCampaign == null)
            {
                LoggingHelper.CampaignNullError();
                return null;
            }

            Computer3DScreen computer3DScreenReturn = null;

            foreach (Computer3DScreen computer3DScreen in customCampaign.CustomComputer3DScreens)
            {
                computer3DScreenReturn = computer3DScreen;
            }

            return computer3DScreenReturn;
        }
    }
}