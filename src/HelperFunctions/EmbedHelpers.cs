using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.HelperFunctions
{
    public static class EmbedHelpers
    {
        public const string NewSafetyHelpPrefix = "NewSafetyHelp_";

        /// <summary>
        /// Extracts the embedded resource to a temporary file.
        /// </summary>
        /// <param name="resourceName">Name of the embedded resource in the assembly.</param>
        /// <param name="findResource">If the provided resource name is a file name and not an embedded name, thus we need to find the embedded name first.</param>
        /// <returns>(string) Path to the newly created temporary copy of the embedded resource.</returns>
        [CanBeNull]
        public static string ExtractEmbeddedResourceToTempFile(string resourceName, bool findResource = true)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                LoggingHelper.ErrorLog("Provided resource name is null or empty. " +
                                       "Unable of loading the provided embedded resource.");
                return null;
            }

            // Get Assembly with the embedded resource.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // We try finding the resource via the file name and use that to get the resource name.
            if (findResource)
            {
                string fileName = resourceName;
                resourceName = currentAssembly.GetManifestResourceNames()
                    .SingleOrDefault(str => str.EndsWith(fileName, StringComparison.Ordinal));

                if (resourceName == null)
                {
                    LoggingHelper.ErrorLog($"Could not find embedded resource '{fileName}'. " +
                                           "Unable of loading the provided embedded resource.");
                    return null;
                }
            }

            // Attempt getting the embedded resource (file).
            using (Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    LoggingHelper.ErrorLog($"Could not find embedded resource '{resourceName}'. " +
                                           "Unable of loading the provided embedded resource.");
                    return null;
                }

                // Construct temporary path for the copy of the embedded resource.
                string resourceExtension = Path.GetExtension(resourceName);
                string tempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    $"{NewSafetyHelpPrefix}{Path.GetFileNameWithoutExtension(resourceName)}_{Guid.NewGuid()}{resourceExtension}");

                // Create temporary copy of the embedded resource.
                using (FileStream temporaryFile = File.Create(tempFilePath))
                {
                    resourceStream.CopyTo(temporaryFile);
                }

                // Return path to the copy.
                return tempFilePath;
            }
        }

        /// <summary>
        /// Deletes all temporary files created by this mod.
        /// </summary>
        public static void DeleteTempFiles()
        {
            string tempFilePath = Path.GetTempPath();

            string[] tempFiles = Directory.GetFiles(tempFilePath, $"{NewSafetyHelpPrefix}*");

            if (tempFiles.Length <= 0)
            {
                LoggingHelper.DebugLog("No temporary files found. Thus no deletion required.");
                return;
            }

            foreach (string tempFile in tempFiles)
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempFile))
                    {
                        LoggingHelper.DebugLog($"Deleting temporary file '{tempFile}'.");
                        File.Delete(tempFile);
                    }
                }
                catch (Exception e)
                {
                    LoggingHelper.ErrorLog($"Was unable to delete temporary file '{tempFile}' " +
                                           $"with the following error: '{e.Message}'.");
                }
            }

            LoggingHelper.DebugLog("Finished deleting temporary files.");
        }
    }
}