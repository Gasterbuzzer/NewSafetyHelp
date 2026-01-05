using MelonLoader;
using System;
using System.Reflection;
using NewSafetyHelp.CustomCampaign;
using NewSafetyHelp.CustomCampaign.CustomCampaignModel;
using NewSafetyHelp.ErrorDebugging;
using NewSafetyHelp.JSONParsing;
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
        private static MelonPreferences_Entry<bool> skipComputerScene; // If to skip the initial computer scene.

        public override void OnInitializeMelon()
        {
            // Entries are created when needed.
            PersistantEntrySave = MelonPreferences.CreateCategory("EntryAlreadyCalled");
            
            // Settings
            mainModSettings = MelonPreferences.CreateCategory("MainModSettings");
            skipComputerScene = mainModSettings.CreateEntry("SkipComputerScene", false);
            
            // Subscribe to Unity's logging system
            Application.logMessageReceived += UnityLogHook.HandleUnityLog;
            
            // Check for updates.
            _ = AsyncVersionChecker.CheckForUpdates();
        }

        public override void OnLateInitializeMelon()
        {
            #if DEBUG
                SceneManager.LoadScene("MainMenuScene");
            #endif

            if (skipComputerScene.Value)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            #if DEBUG
            LoggerInstance.Msg($"DEBUG: Scene {sceneName} with build index {buildIndex} has been loaded!");
            #endif

            MelonPreferences.Save(); // Save on scene change.
        }
    }

    // Add new Entries.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "Awake", new Type[] { })]
    public static class MainClassForMonsterEntries
    {
        // If we show the update message again.
        public static bool ShowUpdateMessage = false;
        
        // Check if we already added the entries, if yes, we do not do it again.
        private static bool isInitializedMainOnce = false;

        private static bool addedEntriesToCustomCampaign = false;

        public static MonsterProfile[] CopyMonsterProfiles;
        private static int monsterProfileSize = 0;
        
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
            if (!CustomCampaignGlobal.inCustomCampaign)
            {
                // We left the custom campaign. We reset the custom campaign values / entries.
                if (addedEntriesToCustomCampaign)
                {
                    addedEntriesToCustomCampaign = false;
                    __instance.allEntries.monsterProfiles = copyMonsterProfilesAfterAdding;
                }
                
                // Check if already added monsters at any point.
                if (isInitializedMainOnce)
                {
                    // We already added them once, no need to add them again.
                    MelonLogger.Msg("INFO: Custom Entries were already added. Skipping adding them again. (This happens on scene reload).");
                    return;
                }
            
                // We create copy of the monster profiles. (Before adding all entries)
                CopyMonsterProfiles = __instance.allEntries.monsterProfiles;
                monsterProfileSize = CopyMonsterProfiles.Length;

                MelonLogger.Msg(ConsoleColor.Green, "INFO: Now parsing all '.json' files...");

                // Read all json and add all monsters and campaigns (/Calls)
                ParseJSONFiles.LoadAllJSON(__instance);
                
                // Create copy after adding all custom entries that belong to the main campaign.
                copyMonsterProfilesAfterAdding = __instance.allEntries.monsterProfiles;
                monsterProfileSizeAfterAdding = copyMonsterProfilesAfterAdding.Length;

                isInitializedMainOnce = true;
                MelonLogger.Msg(ConsoleColor.Green, "INFO: Loaded all '.json' files successfully!");
            }
            else if (!addedEntriesToCustomCampaign) // Custom Campaign
            {
                if (CopyMonsterProfiles.Length <= 0 || monsterProfileSize <= 0) // Invalid loading.
                {
                    MelonLogger.Error("CRITICAL ERROR: Loading of old values to add the entries to failed! (Count == 0)");
                    return;
                }

                CustomCampaignExtraInfo customCampaign = CustomCampaignGlobal.getActiveCustomCampaign();

                if (customCampaign == null)
                {
                    MelonLogger.Error("CRITICAL ERROR: Trying to add to empty custom campaign! Custom campaign is enabled but custom campaign was not found?");
                    return;
                }

                if (customCampaign.removeExistingEntries)
                {
                    __instance.allEntries.monsterProfiles = Array.Empty<MonsterProfile>(); // Remove all entries.
                }
                else // Else we replace our current entries with the original copy and add the entries to that.
                {
                    __instance.allEntries.monsterProfiles = CopyMonsterProfiles;
                }
                
                MelonLogger.Msg(ConsoleColor.Green, "INFO: Entries are now being added... (Custom Campaign)");

                // Replace all entries that need replacement.
                CustomCampaignGlobal.replaceAllProvidedCampaignEntries(ref __instance.allEntries);
                
                // Read all JSON and add all monsters and campaigns (/Calls)
                CustomCampaignGlobal.addAllCustomCampaignEntriesToArray(ref __instance.allEntries);

                addedEntriesToCustomCampaign = true;
                MelonLogger.Msg(ConsoleColor.Green, "INFO: Added/Modified all custom entries successfully! (Custom Campaign)");
            }
        }
    }

    // Patches the class when it opens to also update the monster list, since due to our coroutine's problem.
    [HarmonyLib.HarmonyPatch(typeof(OptionsExecutable), "Open", new Type[] { })]
    public static class UpdateListDesktop
    {
        /// <summary>
        /// Update the list when opening.
        /// </summary>
        /// <param name="__instance"> Caller of function. </param>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private static void Prefix(OptionsExecutable __instance)
        {
            if (__instance.myPopup.name == "EntryCanvasStandalone") // We are opening the EntryBrowser, lets update
            {
                // Get the start of the EntryBrowser
                Type optionsExecutable = typeof(EntryCanvasStandaloneBehavior);

                MethodInfo startMethod = optionsExecutable.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);

                if (startMethod != null)
                {
                    startMethod.Invoke(__instance.myPopup.GetComponent<EntryCanvasStandaloneBehavior>(), null);
                }
                else
                {
                    MelonLogger.Error("ERROR: Failed to access the member 'Start' of EntryCanvasStandaloneBehavior.");
                }
            }
        }
    }
}
