using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NewSafetyHelp.Callers.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomDesktop.Utils;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.InGameSettings;
using NewSafetyHelp.JSONParsing;
using NewSafetyHelp.LoggingSystem;
using NewSafetyHelp.VersionChecker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewSafetyHelp.CustomDesktop
{
    public static class CustomDesktop
    {
        
        [HarmonyLib.HarmonyPatch(typeof(MainMenuCanvasBehavior), "Start")]
        public static class StartPatch
        {
            /// <summary>
            /// Hooks into the Main Menu Canvas Start function to add our own logic after wards.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(MainMenuCanvasBehavior __instance)
            {
                LoggingHelper.DebugLog("Start of Main Menu Canvas Behavior.");
                
                // Credits Double Close Button Fix:
                GameObject mainMenuCanvas = CustomDesktopHelper.GetMainMenuCanvas().gameObject;

                if (mainMenuCanvas != null)
                {
                    GameObject textPopup = mainMenuCanvas.transform.Find("TextPopup").gameObject;

                    if (textPopup != null)
                    {
                        GameObject creditsWindowsBar = textPopup.transform.Find("WindowsBar").gameObject;
                            
                        if (creditsWindowsBar != null)
                        {
                            GameObject closeButton = creditsWindowsBar.transform.Find("CloseButton").gameObject;
                        
                            if (closeButton.GetComponents<Button>().Length >= 2)
                            {
                                Object.Destroy(closeButton.GetComponent<Button>());
                            }
                        }
                    }
                }

                // If in custom campaign, we replace it with custom text.
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();
                    
                    if (customCampaign == null)
                    {
                        return true;
                    }

                    // We initialize all GameObjects required by the email system.
                    EmailHelper.SetAnimatedEmail(
                        AnimatedImageHelper.CreateAnimatedPortrait(EmailHelper.GetEmailImageGameObject(),
                        disableVideoClicking: true));

                    EmailHelper.CreateClickableEmail();

                    // Loading Text replacement.
                    if (customCampaign.LoadingTexts[0].Count > 0 
                        && !string.IsNullOrEmpty(customCampaign.LoadingTexts[0][0]))
                    {
                        __instance.loginText.GetComponent<TextMeshProUGUI>().text = customCampaign.LoadingTexts[0][0];

                        // Set animated texts to provided texts. (Even if just 1)
                        AnimatedText loginText01 = __instance.loginText.GetComponent<AnimatedText>();

                        loginText01.textFrames = new string[customCampaign.LoadingTexts[0].Count];

                        for (int i = 0; i < customCampaign.LoadingTexts[0].Count; i++)
                        {
                            loginText01.textFrames[i] = customCampaign.LoadingTexts[0][i];
                        }
                    }
                    
                    if (customCampaign.LoadingTexts[1].Count > 0 
                        && !string.IsNullOrEmpty(customCampaign.LoadingTexts[1][0]))
                    {
                        __instance.loginText2.GetComponent<TextMeshProUGUI>().text = customCampaign.LoadingTexts[1][0];
                        
                        // Set animated texts to provided texts. (Even if just 1)
                        AnimatedText loginText02 = __instance.loginText2.GetComponent<AnimatedText>();
                        
                        loginText02.textFrames = new string[customCampaign.LoadingTexts[1].Count];

                        for (int i = 0; i < customCampaign.LoadingTexts[1].Count; i++)
                        {
                            loginText02.textFrames[i] = customCampaign.LoadingTexts[1][i];
                        }
                    }

                    if (customCampaign.DisablePickingThemeOption)
                    {
                        ThemeProgramHelper.DisableThemeDropdownDesktop();
                    }
                }
                
                // Add custom settings
                GameObject developerSettings = InGameSettingHelper.CreateNewSettingsSection("Debug Settings", 
                    "Mod settings to show more information and also allow skipping the initial load scene.");
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnSkipComputerSceneToggle,
                    "Skip 3D Computer Scene on Startup", NewSafetyHelpMainClass.SkipComputerScene.Value);
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnSkipLoadingScreenToggle,
                    "Skip Desktop Loading Screen", NewSafetyHelpMainClass.SkipLoadingScreen.Value);
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnDebugLogToggle,
                    "Enable Debug Logs", NewSafetyHelpMainClass.ShowDebugLogs.Value);
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnShowSkippedCallerLogToggle,
                    "Enable Skipped Callers Logs", NewSafetyHelpMainClass.ShowSkippedCallerDebugLog.Value);
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnThemeLogToggle,
                    "Enable Theme Logs", NewSafetyHelpMainClass.ShowThemeDebugLog.Value);
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnRingtoneLogToggle,
                    "Enable Ringtone Logs", NewSafetyHelpMainClass.ShowRingtoneDebugLog.Value);
                    
                InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnEmailLogToggle,
                    "Enable Email Logs", NewSafetyHelpMainClass.ShowEmailDebugLog.Value);

                InGameSettingHelper.CreateButton(developerSettings, (e) =>
                {
                    LoggingHelper.InfoLog("Hot reloading all JSON files. " +
                                          "Please note, this is in beta and may break some features. ",
                        consoleColor: ConsoleColor.Green);
                    ReloadJSONParsing.ReloadAllJSONFiles(e);
                    
                    return e;
                }, "Reload all JSON files", "Reload all JSON files");

                // Plays beginning segment to desktop.
                __instance.StartCoroutine(StartupRoutine(__instance));

                if (!CustomCampaignGlobal.InCustomCampaign && !GlobalVariables.isXmasDLC) // Main Campaign
                {
                    foreach (CustomCampaign customCampaign in CustomCampaignGlobal.CustomCampaignsAvailable)
                    {
                        CustomCampaignProgramHelper.CreateCustomProgramIcon(customCampaign.CampaignDesktopName,
                            customCampaign.CampaignName, customCampaign.CampaignIcon);
                    }
                    
                    // If we have custom emails for the main campaign.
                    if (GlobalParsingVariables.MainCampaignEmails.Count > 0) 
                    {
                        foreach (CustomEmail emailExtra in GlobalParsingVariables.MainCampaignEmails)
                        {
                            if (emailExtra.InMainCampaign)
                            {
                                emailExtra.ReferenceToEmailObject = EmailHelper.CreateEmail(emailExtra);
                            }
                        }
                    }
                    
                    // Enable DLC Button if DLC is installed.
                    // Hide DLC Button
                    CustomDesktopHelper.EnableWinterDlcProgram();
                }
                else if (CustomCampaignGlobal.InCustomCampaign && !GlobalVariables.isXmasDLC) // Custom Campaign
                {
                    CustomCampaignProgramHelper.CreateBackToMainGameButton();
                    
                    // Hide DLC Button
                    CustomDesktopHelper.DisableWinterDlcProgram();
                }
                
                // Change username text if available
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();
                    
                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return true;
                    }
                    
                    // Setting username
                    string username = null;
                    bool customCampaignUsernameChange = false;
                    
                    if (!string.IsNullOrEmpty(customCampaign.DesktopUsernameText)) // First we apply the campaign value.
                    {
                        customCampaignUsernameChange = true;
                        username = customCampaign.DesktopUsernameText;
                    }
                    
                    (bool foundModifier, string value) usernameText = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.UsernameText,
                        v => !string.IsNullOrEmpty(v));
                    
                    if (!string.IsNullOrEmpty(usernameText.value)) // Modifier username is provided.
                    {
                        username = usernameText.value;
                    }
                    
                    if (usernameText.foundModifier || customCampaignUsernameChange)
                    {
                        if (!string.IsNullOrEmpty(username))
                        {
                            CustomDesktopHelper.GetUsernameObject().GetComponent<TextMeshProUGUI>().text = username;
                        }
                    }
                    
                    
                    // Add custom emails.
                    if (customCampaign.Emails.Count > 0) // If we have custom emails.
                    {
                        foreach (CustomEmail emailExtra in customCampaign.Emails)
                        {
                            emailExtra.ReferenceToEmailObject = EmailHelper.CreateEmail(emailExtra);
                        }
                    }
                    
                    // Remove all emails from the main game.
                    if (customCampaign.RemoveDefaultEmails)
                    {
                        EmailHelper.RemoveMainGameEmails();
                    }
                    
                    // Hide Logo

                    bool disableLogo = false;
                    bool modifierPreventsDisablingOfLogo = false;
                    Sprite desktopLogo = null;
                    
                    if (customCampaign.DisableDesktopLogo)
                    {
                        disableLogo = true;
                    }
                    else if (customCampaign.CustomDesktopLogo != null) // We have a desktop logo to show.
                    {
                        desktopLogo = customCampaign.CustomDesktopLogo;
                    }
                    
                    (bool foundModifier, bool value) disableDesktopLogo = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DisableDesktopLogo);

                    (bool foundModifier, Sprite value) customBackgroundLogo =
                        CustomCampaignGlobal.GetActiveModifierValue(
                            c => c.CustomBackgroundLogo,
                            specialPredicate: cM => cM.CustomBackgroundLogoChanged);

                    if (disableDesktopLogo.foundModifier 
                        && disableDesktopLogo.value)
                    {
                        disableLogo = disableDesktopLogo.value;
                    }
                    else if (customBackgroundLogo.foundModifier 
                             && customBackgroundLogo.value != null)
                    {
                        modifierPreventsDisablingOfLogo = true;
                        desktopLogo = customBackgroundLogo.value;
                    }
                    
                    if (disableLogo 
                        && !modifierPreventsDisablingOfLogo)
                    {
                        CustomDesktopHelper.GetLogo().SetActive(false);
                    }
                    else if (desktopLogo != null) // We have a desktop logo to show.
                    {
                        CustomDesktopHelper.GetLogo().GetComponent<Image>().sprite = desktopLogo;
                    }
                    
                    // Adjust Logo

                    float logoTransparency = 0.2627f;
                    
                    // If we have a Custom Transparency
                    if (!customCampaign.CustomDesktopLogoTransparency.Equals(0.2627f)) 
                    {
                        logoTransparency = customCampaign.CustomDesktopLogoTransparency;
                    }
                    
                    (bool foundModifier, float value) backgroundLogoTransparency = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.BackgroundLogoTransparency,
                        v => !v.Equals(0.2627f));

                    if (backgroundLogoTransparency.foundModifier) // Modifier
                    {
                        logoTransparency = backgroundLogoTransparency.value;
                    }
                    
                    if (!logoTransparency.Equals(0.2627f))
                    {
                        Color tempColorCopy = CustomDesktopHelper.GetLogo().GetComponent<Image>().color;
                        tempColorCopy.a = logoTransparency;
                        
                        CustomDesktopHelper.GetLogo().GetComponent<Image>().color = tempColorCopy;
                    }
                    
                    // Rename main program if wanted

                    string renamedMainGameDesktopIcon = String.Empty;
                    
                    if (!string.IsNullOrEmpty(customCampaign.RenameMainGameDesktopIcon))
                    {
                        renamedMainGameDesktopIcon = customCampaign.RenameMainGameDesktopIcon;
                    }
                    
                    (bool foundModifier, string value) renameMainGameDesktopIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.RenameMainGameDesktopIcon,
                        v => !string.IsNullOrEmpty(v));
                    
                    if (renameMainGameDesktopIcon.foundModifier)
                    {
                        renamedMainGameDesktopIcon = renameMainGameDesktopIcon.value;
                    }
                    
                    if (!string.IsNullOrEmpty(renamedMainGameDesktopIcon))
                    {
                        CustomDesktopHelper.GetMainGameProgram().transform.Find("TextBackground").
                            Find("ExecutableName").GetComponent<TextMeshProUGUI>().text = renamedMainGameDesktopIcon;
                    }
                    
                    // Desktop icons
                    
                    (bool foundModifier, Sprite value) entryBrowserIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.EntryBrowserIcon, specialPredicate: cM => cM.EntryBrowserIconChanged);

                    if (entryBrowserIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetEntryBrowserGameObject().GetComponent<Image>().sprite = entryBrowserIcon.value;
                    }
                    
                    (bool foundModifier, Sprite value) mailBoxIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.MailBoxIcon, specialPredicate: cM => cM.MailBoxIconChanged);

                    if (mailBoxIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetMailboxGameObject().GetComponent<Image>().sprite = mailBoxIcon.value;
                    }
                    
                    (bool foundModifier, Sprite value) optionsIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.OptionsIcon, specialPredicate: cM => cM.OptionsIconChanged);

                    if (optionsIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetOptionsGameObject().GetComponent<Image>().sprite = optionsIcon.value;
                    }
                    
                    (bool foundModifier, Sprite value) artbookIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.ArtbookIcon, specialPredicate: cM => cM.ArtbookIconChanged);

                    if (artbookIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetArtbookGameObject().GetComponent<Image>().sprite = artbookIcon.value;
                    }
                    
                    (bool foundModifier, Sprite value) scorecardIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.ScorecardIcon, specialPredicate: cM => cM.ScorecardIconChanged);

                    if (scorecardIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetScorecardGameObject().GetComponent<Image>().sprite = scorecardIcon.value;
                    }
                    
                    (bool foundModifier, Sprite value) arcadeIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.ArcadeIcon, specialPredicate: cM => cM.ArcadeIconChanged);

                    if (arcadeIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetArcadeGameObject().GetComponent<Image>().sprite = arcadeIcon.value;
                    }
                    
                    // Credits
                    
                    (bool foundModifier, string value) desktopCredits = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DesktopCredits,
                        v => !string.IsNullOrEmpty(v));

                    if (desktopCredits.foundModifier)
                    {
                        CustomDesktopHelper.GetCreditsGameObject().GetComponent<TextFileExecutable>().myContent = desktopCredits.value;
                    }
                    
                    (bool foundModifier, Sprite value) desktopCreditsIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.CreditsIcon, specialPredicate:  cM => cM.CreditsIconChanged);

                    if (desktopCreditsIcon.foundModifier)
                    {
                        CustomDesktopHelper.GetCreditsGameObject().GetComponent<Image>().sprite = desktopCreditsIcon.value;
                    }
                    
                    // Discord Icon
                    
                    (bool foundModifier, bool value) hideDiscordProgram = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.HideDiscordProgram, 
                        specialPredicate: v => v.HideDiscordProgramChanged);

                    if (hideDiscordProgram.foundModifier)
                    {
                        CustomDesktopHelper.GetNSEDiscordProgram().SetActive(!hideDiscordProgram.value);
                    }
                    
                    // Change main program icon if wanted.

                    Sprite mainProgramIcon = null;
                    
                    if (customCampaign.ChangeMainGameDesktopIcon != null)
                    {
                        mainProgramIcon = customCampaign.ChangeMainGameDesktopIcon;
                    }
                    
                    (bool foundModifier, Sprite value) mainGameDesktopIcon = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.MainGameDesktopIcon, specialPredicate: cM => cM.MainGameDesktopIconChanged);
                    
                    if (mainGameDesktopIcon.foundModifier)
                    {
                        mainProgramIcon = mainGameDesktopIcon.value;
                    }
                    
                    if (mainProgramIcon != null)
                    {
                        CustomDesktopHelper.GetMainGameProgram().GetComponent<Image>().sprite = mainProgramIcon;
                    }
                    
                    // Disable default videos.
                    if (customCampaign.DisableAllDefaultVideos)
                    {
                        CustomDesktopHelper.DisableDefaultVideos();
                    }

                    if (customCampaign.AllDesktopVideos.Count > 0)
                    {
                        foreach (CustomVideo customVideo in customCampaign.AllDesktopVideos)
                        {
                            VideoHelper.CreateCustomVideoFileProgram(customVideo);
                        }
                    }
                }

                if (MainClassForMonsterEntries.ShowUpdateMessage)
                {
                    MainClassForMonsterEntries.ShowUpdateMessage = false;
                    AsyncVersionChecker.ShowUpdateMessage();
                }
                
                return false; // Skip original function.
            }


            private static IEnumerator StartupRoutine(MainMenuCanvasBehavior __instance)
            {
                if (NewSafetyHelpMainClass.SkipLoadingScreen.Value)
                {
                    __instance.loginText.transform.parent.gameObject.SetActive(false);
                    yield break;
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }
                    
                    (bool foundModifier, bool value) disableDesktopLoading = CustomCampaignGlobal.GetActiveModifierValue(
                        c => c.DisableDesktopLoading);

                    if (disableDesktopLoading.foundModifier 
                        && disableDesktopLoading.value)
                    {
                        __instance.loginText.transform.parent.gameObject.SetActive(false);
                        yield break;
                    }
                }
                
                // We check if null AND if destroyed. Since we might not be initialized.
                // Later the reference might be destroyed, as such we also need to check if destroyed.
                while (GlobalVariables.UISoundControllerScript ==null)
                {
                    yield return null;
                }
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.computerStartup);
                
                yield return new WaitForSeconds(1.3f);
                
                __instance.loginText.SetActive(true);
                
                yield return new WaitForSeconds(2f);
                
                GlobalVariables.UISoundControllerScript.PlayUISoundLooping(GlobalVariables.UISoundControllerScript.computerFanSpin,
                    GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource);
                
                __instance.loginText2.SetActive(true);
                
                yield return new WaitForSeconds(3f);
                
                __instance.loginText.SetActive(false);
                __instance.loginText2.SetActive(false);
                
                GlobalVariables.fade.FadeOut(0.0001f);
                
                yield return new WaitForSeconds(0.1f);
                
                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript.connectionSuccess);
            }
        }
        
        
        [HarmonyLib.HarmonyPatch(typeof(DateTextController), "Start")]
        public static class StartDateTextPatch
        {
            private static FieldInfo myText = typeof(DateTextController).GetField("myText", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            /// <summary>
            /// Hooks into the Start function of the date function to allow for more robust days in custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(DateTextController __instance)
            {
                LoggingHelper.DebugLog("Handling day format.");

                if (myText == null)
                {
                    LoggingHelper.ErrorLog("'MyText' Field of 'DateTextController' is null! Calling original.");
                    return true;
                }
                
                // __instance.myText = __instance.GetComponent<TextMeshProUGUI>();
                myText.SetValue(__instance, __instance.GetComponent<TextMeshProUGUI>()); 
                
                if (!GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign) // Main Campaign
                {
                    TextMeshProUGUI text = (TextMeshProUGUI) myText.GetValue(__instance); // __instance.myText
                    
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
                else if (!CustomCampaignGlobal.InCustomCampaign) // XMAS DLC
                {
                    // __instance.myText
                    TextMeshProUGUI text = (TextMeshProUGUI) myText.GetValue(__instance); 
                    
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
                    LoggingHelper.DebugLog("Handling custom day format..");
                    
                    TextMeshProUGUI text = (TextMeshProUGUI) myText.GetValue(__instance); // __instance.myText
                    
                    // Get our stored values

                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();
                    
                    if (customCampaign == null)
                    {
                        LoggingHelper.CampaignNullError();
                        return false;
                    }
                    
                    // Handle the dates
                    List<int> dateList = new List<int>() {4, 23, 1996};

                    if (customCampaign.DesktopDateStartDay != -1)
                    {
                        dateList[0] = customCampaign.DesktopDateStartDay;
                    }

                    if (customCampaign.DesktopDateStartMonth != -1)
                    {
                        dateList[1] = customCampaign.DesktopDateStartMonth;
                    }

                    if (customCampaign.DesktopDateStartYear != -1)
                    {
                        dateList[2] = customCampaign.DesktopDateStartYear;
                    }
                    
                    LoggingHelper.DebugLog($"Current day format: {dateList[0]} / {dateList[1]} / {dateList[2]}.");

                    dateList = DateUtil.FixDayMonthYear(dateList[0] + GlobalVariables.currentDay,
                        dateList[1], dateList[2]);
                    
                    LoggingHelper.DebugLog($"Day format after fix: {dateList[0]} / {dateList[1]} / {dateList[2]}.");
                    
                    string[] strArray = new string[5];

                    int monthIndex = customCampaign.UseEuropeDateFormat ? 2 : 0;
                    int dayIndex = customCampaign.UseEuropeDateFormat ? 0 : 2;
                    
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