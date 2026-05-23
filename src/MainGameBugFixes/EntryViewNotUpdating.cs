using System.Reflection;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class EntryViewNotUpdating
    {
        // Patches the class when it opens to also update the monster list, since due to our coroutine's problem.
        [HarmonyLib.HarmonyPatch(typeof(OptionsExecutable), "Open")]
        public static class UpdateListDesktop
        {
            private static readonly MethodInfo StartMethod =
                typeof(EntryCanvasStandaloneBehavior).GetMethod("Start",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            /// <summary>
            /// This patch (prefix) makes sure when the entry canvas gets opened,
            /// that it updates the entries in the view to the current entry list.
            /// (Or else it would not show added entries)
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
}