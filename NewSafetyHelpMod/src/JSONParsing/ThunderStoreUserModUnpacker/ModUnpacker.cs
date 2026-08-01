using System;
using System.IO;
using MelonLoader.Utils;
using NewSafetyHelp.ImportFiles;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.JSONParsing.ThunderStoreUserModUnpacker
{
    public static class ModUnpacker
    {
        public static void CheckForNotInstalledUserMods()
        {
            string[] foldersDataPathTest = Directory.GetDirectories(MelonEnvironment.UserDataDirectory);

            foreach (string foldersStringName in foldersDataPathTest)
            {
                string folderName = Path.GetFileName(foldersStringName);

                if (!folderName.Equals("NewSafetyHelp"))
                {
                    LoggingHelper.DebugLog($"Found Folder in search for not installed user mods: '{folderName}'.");

                    string[] filesInFolder = Directory.GetFiles(foldersStringName);

                    foreach (string fileName in filesInFolder)
                    {
                        string fileExtension = Path.GetExtension(fileName);

                        if (fileExtension.ToLowerInvariant().Trim().Equals(".zip"))
                        {
                            try
                            {
                                LoggingHelper.InfoLog("Found zip file to be unpacked. " +
                                                      $"Unzipping file '{fileName}' to NewSafetyHelp folder.");

                                System.IO.Compression.ZipFile.ExtractToDirectory(fileName,
                                    FileImporter.GetUserDataFolderPath());

                                LoggingHelper.InfoLog($"Deleting now unpacked and installed zip file '{fileName}'.");
                                File.Delete(fileName);
                            }
                            catch (Exception e)
                            {
                                LoggingHelper.ErrorLog($"Unable to install usermod in zip file '{fileName}'. " +
                                                       $"\nDetailed Error Message: '{e.Message}'.");
                            }
                        }
                    }
                }
            }
        }
    }
}