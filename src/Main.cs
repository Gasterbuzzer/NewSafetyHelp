using MelonLoader;
using NewSafetyHelp.src.AudioHandler;
using NewSafetyHelp.src.EntryManager;
using NewSafetyHelp.src.ImportFiles;
using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace NewSafetyHelp
{
    public class NewSafetyHelpMainClass : MelonMod
    {
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
        }
    }

    // TODO: Move Audio to its own function

    // Add new Entries.
    [HarmonyLib.HarmonyPatch(typeof(EntryUnlockController), "Awake", new Type[] { })]
    public static class MonsterEntries
    {

        // Some variables needed for persistent calls.
        public static bool isInitialized = false;

        /// <summary>
        /// Adds extra Monsters to the list.
        /// We do this preferably here in order to make sure its
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        private static void Postfix(MethodBase __originalMethod, EntryUnlockController __instance)
        {
            // Get the entryUnlockScript

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
            else
            {
                MelonLogger.Msg("Entries are now being added...");
                isInitialized = true;
            }

            // Get the max Monster ID.
            int maxEntryIDMainCampaing = EntryManager.getlargerID(__instance);
            int maxEntryIDMainDLC = EntryManager.getlargerID(__instance, 1);

            // Attempt to add one

            string userdatapath = FileImporter.getUserDataFolderPath();

            AudioClip monsterSoundClip = null;

            MelonCoroutines.Start(
            AudioImport.LoadAudio(
            (myReturnValue) =>
            {
                monsterSoundClip = myReturnValue;

                if (monsterSoundClip == null)
                {
                    MelonLogger.Error("ERROR: Sound Clip returned null.");
                }

                RichAudioClip monsterSound = AudioImport.CreateRichAudioClip(monsterSoundClip);

                MonsterProfile _newMonster = EntryManager.CreateMonster(_monsterID: maxEntryIDMainCampaing, _monsterAudioClip: monsterSound);

                EntryManager.AddMonsterToTheProfile(ref _newMonster, ref __instance.allEntries.monsterProfiles);
                EntryManager.AddMonsterToTheProfile(ref _newMonster, ref __instance.firstTierUnlocks.monsterProfiles);

                //__instance.allMainCampaignEntries.monsterProfiles.Append<MonsterProfile>(fakeMonster);

                MelonLogger.Msg("Added all entries successfully!");
            },
            userdatapath + "\\test.wav", AudioType.WAV));
        }
    }

    // Patches the class when it opens to also update the monster list, since due to our coroutines problem.
    [HarmonyLib.HarmonyPatch(typeof(OptionsExecutable), "Open", new Type[] { })]
    public static class UpdateListDesktop
    {

        /// <summary>
        /// Update list.
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
