using UnityEngine;

namespace NewSafetyHelp.CustomDesktop.CustomDoubleClickButton
{
    public class DoubleClickButton : MonoBehaviour
    {
        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;

        public void DoubleClickCustomCampaign(string customCampaignName)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= DoubleClickThreshold)
            {
                CustomDesktopHelper.ChangeToCustomCampaignSettings(customCampaignName);
            }
            
            lastClickTime = Time.time;
        }
        
        public void DoubleClickBackToMainGameAlsoLoad()
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= DoubleClickThreshold)
            {
                CustomDesktopHelper.BackToMainGame();
            }
            
            lastClickTime = Time.time;
        }
    }
}