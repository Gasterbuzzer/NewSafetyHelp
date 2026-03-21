using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.ErrorDebugging;
using NewSafetyHelp.JSONParsing;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using NewSafetyHelp.VersionChecker;

// ReSharper disable RedundantDefaultMemberInitializer

namespace NewSafetyHelp
{
    public class NewSafetyHelpMainClass : MelonMod
    {
        // Category for Entries (So that they can be saved upon quitting the game)
        public static MelonPreferences_Category PersistantEntrySave;

        private static MelonPreferences_Category mainModSettings;

        public static MelonPreferences_Entry<bool> SkipComputerScene; // If to skip the initial computer scene.

        public static MelonPreferences_Entry<bool> SkipLoadingScreen; // If to skip the loading texts part.

        public static MelonPreferences_Entry<bool> ShowDebugLogs; // If to show the debug logs at all.

        // If to show the skipped callers debug log.
        public static MelonPreferences_Entry<bool> ShowSkippedCallerDebugLog; 

        public static MelonPreferences_Entry<bool> ShowThemeDebugLog; // If to show the logs for theme info.
        public static MelonPreferences_Entry<bool> ShowRingtoneDebugLog; // If to show the logs for ringtone info.
        public static MelonPreferences_Entry<bool> ShowEmailDebugLog; // If to show the logs for email info.
        public static MelonPreferences_Entry<bool> ShowVideoDebugLog; // If to show the logs for video info.
        public static MelonPreferences_Entry<bool> ShowEntryDebugLog; // If to show the logs for entry info.

        public override void OnInitializeMelon()
        {
            // Entries are created when needed.
            PersistantEntrySave = MelonPreferences.CreateCategory("EntryAlreadyCalled");

            // Settings
            mainModSettings = MelonPreferences.CreateCategory("MainModSettings");

            SkipComputerScene = mainModSettings.CreateEntry("SkipComputerScene", false);

            SkipLoadingScreen = mainModSettings.CreateEntry("SkipLoadingScreen", false);

            ShowDebugLogs = mainModSettings.CreateEntry("ShowDebugLogs", false);
            ShowSkippedCallerDebugLog = mainModSettings.CreateEntry("ShowSkippedCallerDebugLog", false);
            ShowThemeDebugLog = mainModSettings.CreateEntry("ShowThemeDebugLog", false);
            ShowRingtoneDebugLog = mainModSettings.CreateEntry("ShowRingtoneDebugLog", false);
            ShowEmailDebugLog = mainModSettings.CreateEntry("ShowEmailDebugLog", false);
            ShowVideoDebugLog = mainModSettings.CreateEntry("ShowVideoDebugLog", false);
            ShowEntryDebugLog = mainModSettings.CreateEntry("ShowEntryDebugLog", false);

            // Subscribe to Unity's logging system
            Application.logMessageReceived += UnityLogHook.HandleUnityLog;

            // Check for updates.
            _ = AsyncVersionChecker.CheckForUpdates();
        }

        public override void OnLateInitializeMelon()
        {
            if (SkipComputerScene.Value)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggingHelper.DebugLog(() =>
                $"Scene {sceneName} with build index {buildIndex} has been loaded!");

            MelonPreferences.Save(); // Save on scene change.
        }
    }

    // Add new Entries.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "Awake")]
    public static class MainClassForMonsterEntries
    {
        // If we show the update message again.
        public static bool ShowUpdateMessage = false;

        // Check if we already added the entries, if yes, we do not do it again.
        private static bool isInitializedMainOnce = false;

        public static bool AddedEntriesToCustomCampaign = false;

        public static MonsterProfile[] CopyMonsterProfiles;
        private static int monsterProfileSize = 0;
        
        // Copy of Tiers (6 tiers exist)
        public static readonly List<MonsterProfile[]> CopyTierUnlocks = new List<MonsterProfile[]>();
        public static readonly List<MonsterProfile[]> CopyXmasTier = new List<MonsterProfile[]>();

        private static MonsterProfile[] copyMonsterProfilesAfterAdding;

        // ReSharper disable once NotAccessedField.Local
        private static int monsterProfileSizeAfterAdding = 0; // May be used later. Don't remove.

        /// <summary>
        /// Adds extra Monsters to the list.
        /// We do this preferably here in order to make sure its
        /// </summary>
        /// <param name="__instance"> Caller of function. </param>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private static void Postfix(EntryUnlockController __instance)
        {
            if (!CustomCampaignGlobal.InCustomCampaign)
            {
                // We left the custom campaign. We reset the custom campaign values / entries.
                if (AddedEntriesToCustomCampaign)
                {
                    AddedEntriesToCustomCampaign = false;
                    __instance.allEntries.monsterProfiles = copyMonsterProfilesAfterAdding;
                }

                // Check if already added monsters at any point.
                if (isInitializedMainOnce)
                {
                    // We already added them once, no need to add them again.
                    LoggingHelper.InfoLog("Custom Entries were already added. " +
                                          "Skipping adding them again. (This happens on scene reload).");
                    return;
                }

                // We create copy of the monster profiles. (Before adding all entries)
                CopyMonsterProfiles = __instance.allEntries.monsterProfiles;
                monsterProfileSize = CopyMonsterProfiles.Length;
                
                // Copies of tier unlocks.
                CopyTierUnlocks.Add(__instance.firstTierUnlocks.monsterProfiles);
                CopyTierUnlocks.Add(__instance.secondTierUnlocks.monsterProfiles);
                CopyTierUnlocks.Add(__instance.thirdTierUnlocks.monsterProfiles);
                CopyTierUnlocks.Add(__instance.fourthTierUnlocks.monsterProfiles);
                CopyTierUnlocks.Add(__instance.fifthTierUnlocks.monsterProfiles);
                CopyTierUnlocks.Add(__instance.sixthTierUnlocks.monsterProfiles);
                
                CopyXmasTier.Add(__instance.xmastFirstTier.monsterProfiles);
                CopyXmasTier.Add(__instance.xmasSecondTier.monsterProfiles);
                CopyXmasTier.Add(__instance.xmasThirdTier.monsterProfiles);
                CopyXmasTier.Add(__instance.xmasFourthTier.monsterProfiles);

                StartingJSONParsing(__instance);
            }
            else // Custom Campaign
            {
                CustomCampaignInitialization(__instance);
            }
        }

        /// <summary>
        /// Function to initialize the custom campaign values.
        /// </summary>
        /// <param name="__instance">Instance of the entry unlock controller.</param>
        private static void CustomCampaignInitialization(EntryUnlockController __instance)
        {
            if (!AddedEntriesToCustomCampaign)
            {
                // Invalid loading.
                if (CopyMonsterProfiles.Length <= 0 
                    || monsterProfileSize <= 0) 
                {
                    LoggingHelper.CriticalErrorLog("Loading of old values to add the entries to failed! " +
                                                   "(Count == 0)");
                    return;
                }

                CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                if (customCampaign == null)
                {
                    return;
                }

                if (customCampaign.RemoveExistingEntries)
                {
                    // Remove all entries.
                    __instance.allEntries.monsterProfiles = Array.Empty<MonsterProfile>(); 
                }
                else // Else we replace our current entries with the original copy and add the entries to that.
                {
                    __instance.allEntries.monsterProfiles = CopyMonsterProfiles;
                }

                LoggingHelper.InfoLog("Entries are now being added... (Custom Campaign)",
                    consoleColor: ConsoleColor.Green);

                // Replace all entries that need replacement.
                CustomCampaignGlobal.ReplaceAllProvidedCampaignEntries(ref __instance.allEntries);

                // Read all JSON and add all monsters and campaigns (/Calls)
                CustomCampaignGlobal.AddAllCustomCampaignEntriesToArray(ref __instance.allEntries);

                AddedEntriesToCustomCampaign = true;
                LoggingHelper.InfoLog("Added/Modified all custom entries successfully! (Custom Campaign)",
                    consoleColor: ConsoleColor.Green);
            }
        }

        /// <summary>
        /// Small helper function to start the JSON parsing process.
        /// </summary>
        /// <param name="__instance">Instance of the EntryUnlockController.</param>
        public static void StartingJSONParsing(EntryUnlockController __instance)
        {
            LoggingHelper.InfoLog("Now parsing all '.json' files...", consoleColor: ConsoleColor.Green);

            // Read all JSON and add all monsters and campaigns (/Calls)
            ParseJSONFiles.LoadAllJSON(__instance);

            // Create copy after adding all custom entries that belong to the main campaign.
            copyMonsterProfilesAfterAdding = __instance.allEntries.monsterProfiles;
            monsterProfileSizeAfterAdding = copyMonsterProfilesAfterAdding.Length;

            isInitializedMainOnce = true;
            LoggingHelper.InfoLog("Loaded all '.json' files successfully!", consoleColor: ConsoleColor.Green);
        }
    }

    // Patches the class when it opens to also update the monster list, since due to our coroutine's problem.
    [HarmonyLib.HarmonyPatch(typeof(OptionsExecutable), "Open")]
    public static class UpdateListDesktop
    {
        private static readonly MethodInfo StartMethod =
            typeof(EntryCanvasStandaloneBehavior).GetMethod("Start",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Update the entry canvas list when opening.
        /// </summary>
        /// <param name="__instance"> Caller of function. </param>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private static void Prefix(OptionsExecutable __instance)
        {
            // We are opening the EntryBrowser, so we update the list.
            if (__instance.myPopup.name == "EntryCanvasStandalone")
            {
                if (StartMethod == null)
                {
                    LoggingHelper.ReflectionError(nameof(StartMethod));
                    return;
                }

                StartMethod.Invoke(__instance.myPopup.GetComponent<EntryCanvasStandaloneBehavior>(), null);
            }
        }
    }
}