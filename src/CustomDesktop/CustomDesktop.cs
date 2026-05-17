using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NewSafetyHelp.Callers.UI.AnimatedEntry;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomCampaignSystem.CustomTextFiles;
using NewSafetyHelp.CustomCampaignSystem.Modifier.Data;
using NewSafetyHelp.CustomDesktop.Utils;
using NewSafetyHelp.CustomVideos;
using NewSafetyHelp.Emails;
using NewSafetyHelp.HelperFunctions;
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

                if (!GlobalVariables.isXmasDLC)
                {
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
                if (!GlobalVariables.isXmasDLC)
                {
                    // Add vsync option
                    InGameSettingHelper.CreateNewToggle(InGameSettingHelper.GetVideoOptionsSection(),
                        ToggleButtonFunctions.OnVsyncToggle,
                        "Enable VSYNC", GlobalPreferences.Vsync.Value);

                    GameObject developerSettings = InGameSettingHelper.CreateNewSettingsSection("Debug Settings",
                        "Mod settings to show more information and also allow skipping the initial load scene.");

                    InGameSettingHelper.CreateNewToggle(developerSettings,
                        ToggleButtonFunctions.OnSkipComputerSceneToggle,
                        "Skip 3D Computer Scene on Startup", GlobalPreferences.SkipComputerScene.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings,
                        ToggleButtonFunctions.OnSkipLoadingScreenToggle,
                        "Skip Desktop Loading Screen", GlobalPreferences.SkipLoadingScreen.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnSkipDayClockInToggle,
                        "Skip Clock In Screen", GlobalPreferences.SkipDayClockIn.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnDebugLogToggle,
                        "Enable Debug Logs", GlobalPreferences.ShowDebugLogs.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings,
                        ToggleButtonFunctions.OnShowSkippedCallerLogToggle,
                        "Enable Skipped Callers Logs", GlobalPreferences.ShowSkippedCallerDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnThemeLogToggle,
                        "Enable Theme Logs", GlobalPreferences.ShowThemeDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnRingtoneLogToggle,
                        "Enable Ringtone Logs", GlobalPreferences.ShowRingtoneDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnEmailLogToggle,
                        "Enable Email Logs", GlobalPreferences.ShowEmailDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnVideoLogToggle,
                        "Enable Video Logs", GlobalPreferences.ShowVideoDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnTextFileLogToggle,
                        "Enable Text File Logs", GlobalPreferences.ShowTextFileDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnEntryLogToggle,
                        "Enable Entry Logs", GlobalPreferences.ShowEntryDebugLog.Value);

                    InGameSettingHelper.CreateNewToggle(developerSettings, ToggleButtonFunctions.OnCutsceneLogToggle,
                        "Enable Cutscene Logs", GlobalPreferences.ShowCutsceneLog.Value);

                    InGameSettingHelper.CreateButton(developerSettings, (e) =>
                    {
                        LoggingHelper.InfoLog("Hot reloading all JSON files. " +
                                              "Please note, this is in beta and may break some features. ",
                            consoleColor: ConsoleColor.Green);
                        ReloadJSONParsing.ReloadAllJSONFiles(e);

                        return e;
                    }, "Reload all JSON files", "Reload all JSON files");

                    InGameSettingHelper.CreateButton(developerSettings, o =>
                        {
                            DebugHelper.CopyLatestLogs();
                            return o;
                        },
                        "Copy Log File", "Copies the log file for debug purposes");
                }

                // Plays beginning segment to desktop.
                __instance.StartCoroutine(StartupRoutine(__instance));

                // Add custom campaign icons and add back to main game buttons:

                // Main Campaign
                if (!CustomCampaignGlobal.InCustomCampaign && !GlobalVariables.isXmasDLC)
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
                // Custom Campaign
                else if (CustomCampaignGlobal.InCustomCampaign && !GlobalVariables.isXmasDLC)
                {
                    CustomCampaignProgramHelper.CreateBackToMainGameButton();

                    // Hide DLC Button
                    CustomDesktopHelper.DisableWinterDlcProgram();
                }

                // Update desktop values via modifiers.
                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return true;
                    }

                    DesktopModifierSnapshot desktopModifierSnapshot = CustomCampaignGlobal.GetModifierDesktopSnapshot();

                    /*
                     * Username Section
                     */

                    string username = null;
                    bool customCampaignUsernameChange = false;

                    // First we apply the campaign value.
                    if (!string.IsNullOrEmpty(customCampaign.DesktopUsernameText))
                    {
                        customCampaignUsernameChange = true;
                        username = customCampaign.DesktopUsernameText;
                    }

                    // Modifier username is provided.
                    if (desktopModifierSnapshot.UsernameText.found)
                    {
                        username = desktopModifierSnapshot.UsernameText.value.Data;
                    }

                    if (desktopModifierSnapshot.UsernameText.found
                        || customCampaignUsernameChange)
                    {
                        CustomDesktopHelper.GetUsernameObject().GetComponent<TextMeshProUGUI>().text = username;
                    }

                    /*
                     * Custom Email Section
                     */

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

                    /*
                     * Main Program Section
                     */

                    string renamedMainGameDesktopIcon = String.Empty;

                    if (!string.IsNullOrEmpty(customCampaign.RenameMainGameDesktopIcon))
                    {
                        renamedMainGameDesktopIcon = customCampaign.RenameMainGameDesktopIcon;
                    }

                    if (desktopModifierSnapshot.RenameMainGameDesktopIcon.found)
                    {
                        renamedMainGameDesktopIcon = desktopModifierSnapshot.RenameMainGameDesktopIcon.value.Data;
                    }

                    if (!string.IsNullOrEmpty(renamedMainGameDesktopIcon))
                    {
                        CustomDesktopHelper.GetMainGameProgram().transform.Find("TextBackground/ExecutableName")
                            .GetComponent<TextMeshProUGUI>().text = renamedMainGameDesktopIcon;
                    }

                    Sprite mainProgramIcon = null;

                    if (customCampaign.ChangeMainGameDesktopIcon != null)
                    {
                        mainProgramIcon = customCampaign.ChangeMainGameDesktopIcon;
                    }

                    if (desktopModifierSnapshot.MainGameDesktopIcon.found)
                    {
                        mainProgramIcon = desktopModifierSnapshot.MainGameDesktopIcon.value.Data;
                    }

                    if (mainProgramIcon != null)
                    {
                        CustomDesktopHelper.GetMainGameProgram().GetComponent<Image>().sprite = mainProgramIcon;
                    }

                    /*
                     * Logo Section
                     */

                    bool disableLogo = false;
                    bool modifierPreventsDisablingOfLogo = false;
                    Sprite desktopLogo = null;

                    if (customCampaign.DisableDesktopLogo)
                    {
                        disableLogo = true;
                    }
                    else if (customCampaign.CustomDesktopLogo != null)
                    {
                        desktopLogo = customCampaign.CustomDesktopLogo;
                    }

                    if (desktopModifierSnapshot.DisableDesktopLogo.found
                        && desktopModifierSnapshot.DisableDesktopLogo.value.Data)
                    {
                        disableLogo = desktopModifierSnapshot.DisableDesktopLogo.value.Data;
                    }
                    else if (desktopModifierSnapshot.CustomBackgroundLogo.found)
                    {
                        modifierPreventsDisablingOfLogo = true;
                        desktopLogo = desktopModifierSnapshot.CustomBackgroundLogo.value.Data;
                    }

                    if (disableLogo
                        && !modifierPreventsDisablingOfLogo)
                    {
                        CustomDesktopHelper.GetLogo().SetActive(false);
                    }
                    else if (desktopLogo != null)
                    {
                        CustomDesktopHelper.GetLogo().GetComponent<Image>().sprite = desktopLogo;
                    }

                    float logoTransparency = 0.2627f;

                    // If we have a Custom Transparency
                    if (!customCampaign.CustomDesktopLogoTransparency.Equals(0.2627f))
                    {
                        logoTransparency = customCampaign.CustomDesktopLogoTransparency;
                    }

                    if (desktopModifierSnapshot.BackgroundLogoTransparency.found)
                    {
                        logoTransparency = desktopModifierSnapshot.BackgroundLogoTransparency.value.Data;
                    }

                    if (!logoTransparency.Equals(0.2627f))
                    {
                        Color tempColorCopy = CustomDesktopHelper.GetLogo().GetComponent<Image>().color;
                        tempColorCopy.a = logoTransparency;

                        CustomDesktopHelper.GetLogo().GetComponent<Image>().color = tempColorCopy;
                    }

                    /*
                     * Video Player Section
                     */

                    if (desktopModifierSnapshot.VideoPlayerDesktopIsWideMode.found
                        && desktopModifierSnapshot.VideoPlayerDesktopIsWideMode.value.Data)
                    {
                        RectTransform videoPlayerRectTransform = CustomDesktopHelper.GetMainMenuCanvas().transform
                            .Find("VideoPopup/WindowsBar/Video").GetComponent<RectTransform>();

                        videoPlayerRectTransform.offsetMax = new Vector2(0, videoPlayerRectTransform.offsetMax.y);
                        videoPlayerRectTransform.offsetMin = new Vector2(0, videoPlayerRectTransform.offsetMin.y);
                    }

                    /*
                     * Email / Mailbox Section
                     */

                    if (desktopModifierSnapshot.MailboxIcon.found)
                    {
                        CustomDesktopHelper.GetMailboxGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.MailboxIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.MailboxRename.found)
                    {
                        CustomDesktopHelper.GetMailboxGameObject().transform.GetChild(0).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = desktopModifierSnapshot.MailboxRename.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationMailboxTitle.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("EmailPopup").GetChild(0).GetChild(3)
                                .GetComponent<TextMeshProUGUI>().text =
                            desktopModifierSnapshot.ApplicationMailboxTitle.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationMailboxIcon.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("EmailPopup").GetChild(0).GetChild(2)
                            .GetComponent<Image>().sprite = desktopModifierSnapshot.ApplicationMailboxIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.DisplayMailboxOnDesktop.found)
                    {
                        if (!desktopModifierSnapshot.DisplayMailboxOnDesktop.value.Data)
                        {
                            CustomDesktopHelper.GetMailboxGameObject().gameObject.SetActive(false);
                        }
                    }


                    /*
                     * Entry Browser Section
                     */

                    if (desktopModifierSnapshot.EntryBrowserIcon.found)
                    {
                        CustomDesktopHelper.GetEntryBrowserGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.EntryBrowserIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.EntryBrowserRename.found)
                    {
                        CustomDesktopHelper.GetEntryBrowserGameObject().transform.GetChild(0).GetChild(0)
                                .GetComponent<TextMeshProUGUI>().text =
                            desktopModifierSnapshot.EntryBrowserRename.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationEntryBrowserTitle.found)
                    {
                        GlobalVariables.entryCanvasScript.gameObject.transform.GetChild(0).GetChild(0).GetChild(2)
                                .GetComponent<TextMeshProUGUI>().text =
                            desktopModifierSnapshot.ApplicationEntryBrowserTitle.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationEntryBrowserIcon.found)
                    {
                        GlobalVariables.entryCanvasScript.gameObject.transform.GetChild(0).GetChild(0).GetChild(1)
                                .GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ApplicationEntryBrowserIcon.value.Data;
                    }

                    /*
                     * Options Section
                     */

                    if (desktopModifierSnapshot.OptionsIcon.found)
                    {
                        CustomDesktopHelper.GetOptionsGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.OptionsIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.OptionsRename.found)
                    {
                        CustomDesktopHelper.GetOptionsGameObject().transform.GetChild(0).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = desktopModifierSnapshot.OptionsRename.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationOptionsTitle.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("OptionsPopup").GetChild(0).GetChild(3)
                                .GetComponent<TextMeshProUGUI>().text =
                            desktopModifierSnapshot.ApplicationOptionsTitle.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationOptionsIcon.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("OptionsPopup").GetChild(0).GetChild(2)
                                .GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ApplicationOptionsIcon.value.Data;
                    }

                    /*
                     * Artbook Section
                     */

                    if (desktopModifierSnapshot.ArtbookIcon.found)
                    {
                        CustomDesktopHelper.GetArtbookGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ArtbookIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.ArtbookRename.found)
                    {
                        CustomDesktopHelper.GetArtbookGameObject().transform.GetChild(0).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = desktopModifierSnapshot.ArtbookRename.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationArtbookTitle.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("ArtbookPopup").GetChild(0).GetChild(3)
                                .GetComponent<TextMeshProUGUI>().text =
                            desktopModifierSnapshot.ApplicationArtbookTitle.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationArtbookIcon.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("ArtbookPopup").GetChild(0).GetChild(2)
                                .GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ApplicationArtbookIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.ArtbookPages.found)
                    {
                        ArtbookPage[] artbookPageArray = desktopModifierSnapshot.ArtbookPages.value.ToArray();

                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("ArtbookPopup")
                            .GetComponent<ArtbookPopupBehavior>().artbookPages = artbookPageArray;
                    }

                    /*
                     * Arcade Section
                     */

                    if (desktopModifierSnapshot.ArcadeIcon.found)
                    {
                        CustomDesktopHelper.GetArcadeGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ArcadeIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.ArcadeRename.found)
                    {
                        CustomDesktopHelper.GetArcadeGameObject().transform.GetChild(0).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = desktopModifierSnapshot.ArcadeRename.value.Data;
                    }

                    /*
                     * Scorecard Section
                     */

                    if (desktopModifierSnapshot.ScorecardIcon.found)
                    {
                        CustomDesktopHelper.GetScorecardGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ScorecardIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.ScorecardRename.found)
                    {
                        CustomDesktopHelper.GetScorecardGameObject().transform.GetChild(0).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = desktopModifierSnapshot.ScorecardRename.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationScorecardTitle.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("ScorecardPopup").GetChild(0).GetChild(3)
                                .GetComponent<TextMeshProUGUI>().text =
                            desktopModifierSnapshot.ApplicationScorecardTitle.value.Data;
                    }

                    if (desktopModifierSnapshot.ApplicationScorecardIcon.found)
                    {
                        CustomDesktopHelper.GetMainMenuCanvas().transform.Find("ScorecardPopup").GetChild(0).GetChild(2)
                                .GetComponent<Image>().sprite =
                            desktopModifierSnapshot.ApplicationScorecardIcon.value.Data;
                    }

                    /*
                     * Credits Section
                     */

                    // Get a copy of the text file icon before we overwrite it.
                    // Since credits are a text file, we need to do it here.
                    if (CustomTextFileHelper.TextFileIcon == null)
                    {
                        CustomTextFileHelper.TextFileIcon =
                            CustomDesktopHelper.GetCreditsGameObject().GetComponent<Image>().sprite;
                    }

                    if (desktopModifierSnapshot.DesktopCredits.found)
                    {
                        CustomDesktopHelper.GetCreditsGameObject().GetComponent<TextFileExecutable>().myContent =
                            desktopModifierSnapshot.DesktopCredits.value.Data;
                    }

                    if (desktopModifierSnapshot.CreditsRename.found)
                    {
                        CustomDesktopHelper.GetCreditsGameObject().transform.GetChild(0).GetChild(0)
                            .GetComponent<TextMeshProUGUI>().text = desktopModifierSnapshot.CreditsRename.value.Data;
                    }

                    if (desktopModifierSnapshot.CreditsIcon.found)
                    {
                        CustomDesktopHelper.GetCreditsGameObject().GetComponent<Image>().sprite =
                            desktopModifierSnapshot.CreditsIcon.value.Data;
                    }

                    if (desktopModifierSnapshot.HideDesktopCredits.found)
                    {
                        CustomDesktopHelper.GetCreditsGameObject().SetActive(
                            !desktopModifierSnapshot.HideDesktopCredits.value.Data);
                    }

                    /*
                     * Discord Section
                     */

                    if (desktopModifierSnapshot.HideDiscordProgram.found)
                    {
                        CustomDesktopHelper.GetNSEDiscordProgram().SetActive(
                            !desktopModifierSnapshot.HideDiscordProgram.value.Data);
                    }


                    /*
                     * Custom Videos Section
                     */

                    if (customCampaign.DisableAllDefaultVideos)
                    {
                        CustomDesktopHelper.DisableDefaultVideos();
                    }

                    if (customCampaign.CustomVideos.Count > 0)
                    {
                        foreach (CustomVideo customVideo in customCampaign.CustomVideos)
                        {
                            VideoHelper.CreateCustomVideoFileProgram(customVideo);
                        }
                    }

                    /*
                     * Custom Text Files Section
                     */

                    if (customCampaign.CustomTextProgramFiles.Count > 0)
                    {
                        foreach (CustomTextFile customTextFile in customCampaign.CustomTextProgramFiles)
                        {
                            CustomTextFileHelper.CreateCustomTextFile(customTextFile);
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


            /// <summary>
            /// A coroutine for the fade in text animation on desktop.
            /// </summary>
            /// <param name="__instance">Instance of MainMenuCanvasBehavior.</param>
            /// <returns>Coroutine to run.</returns>
            private static IEnumerator StartupRoutine(MainMenuCanvasBehavior __instance)
            {
                // We check if null AND if destroyed. Since we might not be initialized.
                // Later the reference might be destroyed, as such we also need to check if destroyed.
                while (GlobalVariables.UISoundControllerScript == null)
                {
                    yield return null;
                }

                if (GlobalPreferences.SkipLoadingScreen.Value)
                {
                    GlobalVariables.fade.FadeOut(0.0001f);
                    yield break;
                }

                if (CustomCampaignGlobal.InCustomCampaign)
                {
                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        yield break;
                    }

                    (bool foundModifier, bool value) disableDesktopLoading =
                        CustomCampaignGlobal.GetActiveModifierValue(c => c.DisableDesktopLoading);

                    if (disableDesktopLoading.foundModifier
                        && disableDesktopLoading.value)
                    {
                        GlobalVariables.fade.FadeOut(0.0001f);
                        yield break;
                    }
                }

                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript
                    .computerStartup);

                yield return new WaitForSeconds(1.3f);

                __instance.loginText.SetActive(true);

                yield return new WaitForSeconds(2f);

                GlobalVariables.UISoundControllerScript.PlayUISoundLooping(
                    GlobalVariables.UISoundControllerScript.computerFanSpin,
                    GlobalVariables.UISoundControllerScript.myFanSpinLoopingSource);

                __instance.loginText2.SetActive(true);

                yield return new WaitForSeconds(3f);

                __instance.loginText.SetActive(false);
                __instance.loginText2.SetActive(false);

                GlobalVariables.fade.FadeOut(0.0001f);

                yield return new WaitForSeconds(0.1f);

                GlobalVariables.UISoundControllerScript.PlayUISound(GlobalVariables.UISoundControllerScript
                    .connectionSuccess);
            }
        }


        [HarmonyLib.HarmonyPatch(typeof(DateTextController), "Start")]
        public static class StartDateTextPatch
        {
            private static readonly FieldInfo MyText = typeof(DateTextController).GetField("myText",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            /// <summary>
            /// Hooks into the Start function of the date function to allow for more robust days in custom campaigns.
            /// </summary>
            /// <param name="__instance"> Caller of function. </param>
            // ReSharper disable once UnusedMember.Local
            private static bool Prefix(DateTextController __instance)
            {
                LoggingHelper.DebugLog("Handling day format.");

                if (MyText == null)
                {
                    LoggingHelper.ErrorLog("'MyText' Field of 'DateTextController' is null! Calling original.");
                    return true;
                }

                // OLD: __instance.myText = __instance.GetComponent<TextMeshProUGUI>();
                MyText.SetValue(__instance, __instance.GetComponent<TextMeshProUGUI>());

                // Main Campaign
                if (!GlobalVariables.isXmasDLC && !CustomCampaignGlobal.InCustomCampaign)
                {
                    // OLD: __instance.myText
                    TextMeshProUGUI text = (TextMeshProUGUI)MyText.GetValue(__instance);

                    string[] strArray = new string[5];

                    int num = 4; // Month

                    strArray[0] = num.ToString();
                    strArray[1] = "/";


                    num = 23 + GlobalVariables.currentDay; // Day


                    strArray[2] = num.ToString();
                    strArray[3] = "/";


                    num = 1996; // Year


                    strArray[4] = num.ToString();

                    string str = string.Concat(strArray);

                    text.text = str;
                }
                else if (!CustomCampaignGlobal.InCustomCampaign) // XMAS DLC
                {
                    // __instance.myText
                    TextMeshProUGUI text = (TextMeshProUGUI)MyText.GetValue(__instance);

                    string[] strArray = new string[5];

                    // Month
                    int num = 12;

                    strArray[0] = num.ToString();
                    strArray[1] = "/";

                    // Day
                    num = 21 + GlobalVariables.currentDay;

                    strArray[2] = num.ToString();
                    strArray[3] = "/";

                    // Year
                    num = 1996;

                    strArray[4] = num.ToString();

                    string str = string.Concat(strArray);

                    text.text = str;
                }
                else // Custom Campaign
                {
                    LoggingHelper.DebugLog("Handling custom day format..");

                    // OLD: __instance.myText
                    TextMeshProUGUI text = (TextMeshProUGUI)MyText.GetValue(__instance);

                    // Get our stored values

                    CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                    if (customCampaign == null)
                    {
                        return false;
                    }

                    // Handle the dates
                    List<int> dateList = new List<int> { 4, 23, 1996 };

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