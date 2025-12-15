using UnityEngine;
using UnityEngine.UI;

namespace NewSafetyHelp.CustomDesktop.CustomDoubleClickButton
{
    public class DoubleClickButton : MonoBehaviour
    {
        private float lastClickTime = 0f;
        private float doubleClickThreshold = 0.3f;
        
        public void doubleClickCustomCampaign(string customCampaignName)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                CustomDesktopHelper.changeToCustomCampaignSettings(customCampaignName);
            }
            
            lastClickTime = Time.time;
        }
        
        public void doubleClickBackToMainGameAlsoLoad()
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                CustomDesktopHelper.backToMainGame();
            }
            
            lastClickTime = Time.time;
        }
    }
}