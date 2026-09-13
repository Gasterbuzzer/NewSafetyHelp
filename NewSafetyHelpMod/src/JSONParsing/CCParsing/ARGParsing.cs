using System.IO;
using NewSafetyHelp.ARG;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.JSONParsing.CCParsing
{
    public static class ARGParsing
    {
        /// <summary>
        /// Gets all the ARG files from the usermod.
        /// </summary>
        /// <param name="usermodFolder">Folder with the correct ARG custom campaign.</param>
        public static void GetAllARGFiles(string usermodFolder)
        {
            string argVideo = "gameoverplaceholder!.mp4";

            // Main Directory:
            if (File.Exists(Path.Combine(usermodFolder, argVideo)))
            {
                ARGLoadedFiles.ARGVideo = Path.Combine(usermodFolder, argVideo);
                LoggingHelper.DebugLog($"Loaded ARG video file: '{ARGLoadedFiles.ARGVideo}'.");
            }

            // Subdirectories:
            string[] foldersDataPath = Directory.GetDirectories(usermodFolder);

            foreach (string folderName in foldersDataPath)
            {
                if (File.Exists(Path.Combine(folderName, argVideo)))
                {
                    ARGLoadedFiles.ARGVideo = Path.Combine(folderName, argVideo);
                    LoggingHelper.DebugLog($"Loaded ARG video file: '{ARGLoadedFiles.ARGVideo}'.");
                }
            }
        }
    }
}