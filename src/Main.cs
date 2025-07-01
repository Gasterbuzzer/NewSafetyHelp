using MelonLoader;
using NewSafetyHelp.src.JSONParsing;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace NewSafetyHelp
{
    public class NewSafetyHelpMainClass : MelonMod
    {
        // Category for Entries (So that they can be saved upon quitting the game)
        public static MelonPreferences_Category persistantEntrySave;

        public override void OnInitializeMelon()
        {
            // Entries are created when neeeded.
            persistantEntrySave = MelonPreferences.CreateCategory("EntryAlreadyCalled");
        }

        public override void OnLateInitializeMelon()
        {
            #if DEBUG
            //SceneManager.LoadScene("MainMenuScene");

            SceneManager.LoadScene("MainScene");

            //SceneManager.LoadScene("MainMenuSceneXmas");

            //SceneManager.LoadScene("Computer3DSceneXmas");
            #endif
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
    public static class MonsterEntries
    {

        // Check if we already added the creatures, if yes, we do not do it again.
        public static bool isInitialized = false;

        /// <summary>
        /// Adds extra Monsters to the list.
        /// We do this preferably here in order to make sure its
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        private static void Postfix(MethodBase __originalMethod, EntryUnlockController __instance)
        {

            if (__instance == null)
            {
                MelonLogger.Error("ERROR: EntryUnlocker is equal to null!");
                return;
            }

            // Check if already added monsters at any point.
            if (isInitialized)
            {
                // We already added them once, no need to add them again.
                MelonLogger.Msg("INFO: Custom Entries were already added. Skipping adding them again. (This happens on scene reload).");
                return;
            }

            MelonLogger.Msg(ConsoleColor.Green, "INFO: Entries are now being added...");

            // Add all monsters
            ParseMonster.LoadAllMonsters(__instance);

            isInitialized = true;
            MelonLogger.Msg(ConsoleColor.Green, "INFO: Added/Modified all custom entries successfully!");
        }
    }

    // Patches the class when it opens to also update the monster list, since due to our coroutines problem.
    [HarmonyLib.HarmonyPatch(typeof(OptionsExecutable), "Open", new Type[] { })]
    public static class UpdateListDesktop
    {

        /// <summary>
        /// Update the list when opening.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        private static void Prefix(MethodBase __originalMethod, OptionsExecutable __instance)
        {
            if (__instance.myPopup.name == "EntryCanvasStandalone") // We are opening the EntryBrowser, lets update
            {
                // Get the start of the EntryBrowser
                Type _optionsExecutable = typeof(EntryCanvasStandaloneBehavior);

                MethodInfo startMethod = _optionsExecutable.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);

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
