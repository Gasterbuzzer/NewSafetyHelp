using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.Emails;
using NewSafetyHelp.JSONParsing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NewSafetyHelp.CustomDesktop
{
    public static class CustomDesktop
    {
        
        [HarmonyLib.HarmonyPatch(typeof(MainMenuCanvasBehavior), "Start", new Type[] { })]
        public static class StartPatch
        {

            /// <summary>
            /// Hooks into the Main Menu Canvas Start function to add our own logic after wards.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, MainMenuCanvasBehavior __instance)
            {
                #if DEBUG
                    MelonLogger.Msg($"DEBUG: Start of Main Menu Canvas Behavior.");
                #endif

                // If in custom campaign, we replace it with custom text.
                if (CustomCampaignGlobal.inCustomCampaign)
                {

                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();
                    
                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null! Unable of replacing loading screen texts. Calling original function.");
                        return true;
                    }

                    if (customCampaign.loadingTexts[0].Count > 0 && !string.IsNullOrEmpty(customCampaign.loadingTexts[0][0]))
                    {
                        __instance.loginText.GetComponent<TextMeshProUGUI>().text = customCampaign.loadingTexts[0][0];

                        // Set animated texts to provided texts. (Even if just 1)
                        AnimatedText loginText01 = __instance.loginText.GetComponent<AnimatedText>();
                            
                        loginText01.textFrames = new string[customCampaign.loadingTexts[0].Count];

                        for (int i = 0; i < customCampaign.loadingTexts[0].Count; i++)
                        {
                            loginText01.textFrames[i] = customCampaign.loadingTexts[0][i];
                        }
                    }
                    
                    if (customCampaign.loadingTexts[1].Count > 0 && !string.IsNullOrEmpty(customCampaign.loadingTexts[1][0]))
                    {
                        __instance.loginText2.GetComponent<TextMeshProUGUI>().text = customCampaign.loadingTexts[1][0];
                        
                        // Set animated texts to provided texts. (Even if just 1)
                        AnimatedText loginText02 = __instance.loginText2.GetComponent<AnimatedText>();
                        
                        loginText02.textFrames = new string[customCampaign.loadingTexts[1].Count];

                        for (int i = 0; i < customCampaign.loadingTexts[1].Count; i++)
                        {
                            loginText02.textFrames[i] = customCampaign.loadingTexts[1][i];
                        }
                    }
                }

                // Plays beginning segment to desktop.
                MelonCoroutines.Start(StartupRoutine(__instance));

                if (!CustomCampaignGlobal.inCustomCampaign && !GlobalVariables.isXmasDLC) // Main Campaign
                {
                    foreach (CustomCampaignExtraInfo customCampaign in CustomCampaignGlobal.customCampaignsAvailable)
                    {
                        CustomDesktopHelper.createCustomProgramIcon(customCampaign.campaignDesktopName, customCampaign.campaignName, customCampaign.campaignIcon);
                    }
                    
                    if (ParseJSONFiles.mainCampaignEmails.Count > 0) // If we have custom emails for the main campaign.
                    {
                        foreach (EmailExtraInfo emailExtra in ParseJSONFiles.mainCampaignEmails)
                        {
                            if (emailExtra.inMainCampaign)
                            {
                                CustomDesktopHelper.createEmail(emailExtra);
                            }
                        }
                    }
                    
                    // Enable DLC Button if DLC is installed.
                    // Hide DLC Button
                    CustomDesktopHelper.enableWinterDLCProgram();
                }
                else if (!GlobalVariables.isXmasDLC) // Custom Campaign
                {
                    CustomDesktopHelper.createBackToMainGameButton();
                    
                    // Hide DLC Button
                    CustomDesktopHelper.disableWinterDLCProgram();
                }
                
                // Change username text if available
                if (CustomCampaignGlobal.inCustomCampaign)
                {
                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();
                    
                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null! Unable of replacing username. Calling original function.");
                        return true;
                    }

                    if (!string.IsNullOrEmpty(customCampaign.desktopUsernameText))
                    {
                        CustomDesktopHelper.getUsernameObject().GetComponent<TextMeshProUGUI>().text = customCampaign.desktopUsernameText;
                    }
                    
                    // Add custom emails.
                    if (customCampaign.emails.Count > 0) // If we have custom emails.
                    {
                        foreach (EmailExtraInfo emailExtra in customCampaign.emails)
                        {
                            CustomDesktopHelper.createEmail(emailExtra);
                        }
                    }
                    
                    // Remove all emails from the main game.
                    if (customCampaign.removeDefaultEmails)
                    {
                        CustomDesktopHelper.removeMainGameEmails();
                    }
                    
                    // Hide Logo
                    if (customCampaign.disableDesktopLogo)
                    {
                        CustomDesktopHelper.getLogo().SetActive(false);
                    }
                    else if (customCampaign.customDesktopLogo != null) // We have a desktop logo to show.
                    {
                        CustomDesktopHelper.getLogo().GetComponent<Image>().sprite = customCampaign.customDesktopLogo;
                    }
                    
                    // Adjust Logo
                    if (!customCampaign.customDesktopLogoTransparency.Equals(0.2627f)) // If we have a Custom Transparency
                    {
                        Color tempColorCopy = CustomDesktopHelper.getLogo().GetComponent<Image>().color;
                        tempColorCopy.a = customCampaign.customDesktopLogoTransparency;

                        CustomDesktopHelper.getLogo().GetComponent<Image>().color = tempColorCopy;
                    }
                }

                return false; // Skip original function.
            }


            public static IEnumerator StartupRoutine(MainMenuCanvasBehavior __instance)
            {
                while (GlobalVariables.UISoundControllerScript ==null)
                {
                    yield return null;
                }
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.computerStartup);
                
                yield return new WaitForSeconds(1.3f);
                
                __instance.loginText.SetActive(true);
                
                yield return new WaitForSeconds(2f);
                
                GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.computerFanSpin, GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource);
                
                __instance.loginText2.SetActive(true);
                
                yield return new WaitForSeconds(3f);
                
                __instance.loginText.SetActive(false);
                __instance.loginText2.SetActive(false);
                
                GlobalVariables.fade.FadeOut(0.0001f);
                
                yield return new WaitForSeconds(0.1f);
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.connectionSuccess);
            }
        }
        
        
        [HarmonyLib.HarmonyPatch(typeof(DateTextController), "Start", new Type[] { })]
        public static class StartDateTextPatch
        {

            /// <summary>
            /// Hooks into the Start function of the date function to allow for more robust days in custom campaigns.
            /// </summary>
            /// <param name="__originalMethod"> Method which was called. </param>
            /// <param name="__instance"> Caller of function. </param>
            private static bool Prefix(MethodBase __originalMethod, DateTextController __instance)
            {
                
                #if DEBUG
                    MelonLogger.Msg("DEBUG: Handling day format.");
                #endif
                
                FieldInfo _myText = typeof(DateTextController).GetField("myText", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (_myText == null)
                {
                    MelonLogger.Error("ERROR: MyText Field of DateTextController is null! Calling original.");
                    return true;
                }
                
                _myText.SetValue(__instance, __instance.GetComponent<TextMeshProUGUI>()); // __instance.myText = __instance.GetComponent<TextMeshProUGUI>();
                
                if (!GlobalVariables.isXmasDLC && !CustomCampaignGlobal.inCustomCampaign) // Main Campaign
                {
                    TextMeshProUGUI text = (TextMeshProUGUI) _myText.GetValue(__instance); // __instance.myText
                    
                    string[] strArray = new string[5];
                    
                    int num = 4;                            // Month
                    
                    strArray[0] = num.ToString();
                    strArray[1] = "/";
                    
                    
                    num = 23 + GlobalVariables.currentDay;  // Day
                    
                    
                    strArray[2] = num.ToString();
                    strArray[3] = "/";
                    
                    
                    num = 1996;                             // Year
                    
                    
                    strArray[4] = num.ToString();
                    
                    string str = string.Concat(strArray);
                    
                    text.text = str;
                }
                else if (!CustomCampaignGlobal.inCustomCampaign) // XMAS DLC
                {
                    TextMeshProUGUI text = (TextMeshProUGUI) _myText.GetValue(__instance); // __instance.myText
                    
                    string[] strArray = new string[5];
                    
                    int num = 12;                           // Month
                    
                    strArray[0] = num.ToString();
                    strArray[1] = "/";
                    
                    
                    num = 21 + GlobalVariables.currentDay;  // Day
                    
                    
                    strArray[2] = num.ToString();
                    strArray[3] = "/";
                    
                    
                    num = 1996;                             // Year
                    
                    
                    strArray[4] = num.ToString();
                    
                    string str = string.Concat(strArray);
                    
                    text.text = str;
                }
                else // Custom Campaign
                {
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Handling custom day format..");
                    #endif
                    
                    TextMeshProUGUI text = (TextMeshProUGUI) _myText.GetValue(__instance); // __instance.myText
                    
                    // Get our stored values

                    CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getCustomCampaignExtraInfo();
                    
                    if (customCampaign == null)
                    {
                        MelonLogger.Error("ERROR: Custom Campaign is null! Calling original function.");
                        return false;
                    }
                    
                    // Handle the dates
                    List<int> dateList = new List<int>() {4, 23, 1996};

                    if (customCampaign.desktopDateStartDay != -1)
                    {
                        dateList[0] = customCampaign.desktopDateStartDay;
                    }

                    if (customCampaign.desktopDateStartMonth != -1)
                    {
                        dateList[1] = customCampaign.desktopDateStartMonth;
                    }

                    if (customCampaign.desktopDateStartYear != -1)
                    {
                        dateList[2] = customCampaign.desktopDateStartYear;
                    }
                    
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Current day format: {dateList[0]} / {dateList[1]} / {dateList[2]}.");
                    #endif
                    
                    dateList = DateUtil.fixDayMonthYear(dateList[0]  + GlobalVariables.currentDay, dateList[1], dateList[2]);
                    
                    #if DEBUG
                        MelonLogger.Msg($"DEBUG: Day format after fix: {dateList[0]} / {dateList[1]} / {dateList[2]}.");
                    #endif
                    
                    string[] strArray = new string[5];

                    int monthIndex = customCampaign.useEuropeDateFormat ? 2 : 0;
                    int dayIndex = customCampaign.useEuropeDateFormat ? 0 : 2;
                    
                    // Month
                    strArray[monthIndex] = dateList[1].ToString();
                    strArray[1] = "/";
                    
                    // Day
                    strArray[dayIndex] = dateList[0].ToString();
                    strArray[3] = "/";
                    
                    // Year
                    strArray[4] = dateList[2].ToString();
                    
                    string str = string.Concat(strArray);
                    
                    text.text = str;
                }
                
                return false; // Skip original function.
            }
            
        }
    }
}