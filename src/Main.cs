using MelonLoader;
using NewSafetyHelp.src.DebugHelper;
using NewSafetyHelp.src.JSONParsing;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace NewSafetyHelp
{
    public class NewSafetyHelpMainClass : MelonMod
    {

        public static MelonPreferences_Category persistantEntrySave;

        public override void OnInitializeMelon()
        {
            persistantEntrySave = MelonPreferences.CreateCategory("EntryAlreadyCalled");

            // Entries are created when neeeded.
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");

            MelonPreferences.Save(); // Save on scene change.
        }

        public override void OnLateInitializeMelon()
        {
            //SceneManager.LoadScene("MainMenuScene");
            SceneManager.LoadScene("MainScene");
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
                MelonLogger.Msg("Entries were already added. Skipping adding.");
                return;
            }

            MelonLogger.Msg("Entries are now being added...");

            // Add all monsters
            ParseMonster.LoadAllMonsters(__instance);

            isInitialized = true;
            MelonLogger.Msg("INFO: Added all entries successfully!");
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

                var startMethod = _optionsExecutable.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);

                if (startMethod != null)
                {
                    startMethod.Invoke(__instance.myPopup.GetComponent<EntryCanvasStandaloneBehavior>(), null);
                }
                else
                {
                    MelonLogger.Error("Failed to access the member 'Start' of EntryCanvasStandaloneBehavior.");
                }
            }
        }
    }
}
