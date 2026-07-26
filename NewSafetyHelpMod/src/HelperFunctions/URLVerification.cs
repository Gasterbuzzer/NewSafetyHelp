using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NewSafetyHelp.LoggingSystem;

namespace NewSafetyHelp.HelperFunctions
{
    public static class URLVerification
    {
        /// <summary>
        /// Sets the click url while ensuring it is a safe link.
        /// </summary>
        /// <param name="stringURL">String version of the URL.</param>
        /// <param name="clickURI">Reference to write the URL to if the given url is valid.</param>
        /// <returns>Boolean: If the setting failed or worked.</returns>
        public static bool SetClickURL(string stringURL,
            ref Uri clickURI)
        {
            if (!Uri.TryCreate(stringURL, UriKind.Absolute, out Uri uri))
            {
                int stringMaxPrintLength = Math.Min(10, stringURL.Length);

                LoggingHelper.ErrorLog(
                    $"Given URL '{stringURL.Substring(0, stringMaxPrintLength)}[...]' is not a valid URL. " +
                    "Unable of setting URL. " +
                    "Make sure it is in a valid format: 'http://' or 'https://'.");
                return false;
            }

            if (!IsURLAndNotUnsafe(uri))
            {
                int stringMaxPrintLength = Math.Min(10, stringURL.Length);

                LoggingHelper.ErrorLog(
                    $"Given URL '{stringURL.Substring(0, stringMaxPrintLength)}[...]' is not a accepted URL. " +
                    "Unable of setting URL. " +
                    "Make sure it is in a valid format: 'http://' or 'https://'.");
                return false;
            }

            if (IsADangerousFileUrl(uri))
            {
                int stringMaxPrintLength = Math.Min(10, stringURL.Length);

                LoggingHelper.ErrorLog(
                    $"Given URL '{stringURL.Substring(0, stringMaxPrintLength)}[...]' is not a accepted URL. " +
                    "Unable of setting URL. " +
                    "Please provide an URL that is not a unsafe file type.");
                return false;
            }

            clickURI = uri;

            return true;
        }

        /// <summary>
        /// Opens the URL provided while ensuring it is safe to do so.
        /// </summary>
        /// <returns>Boolean: If the setting failed or worked.</returns>
        public static bool OpenURIInBrowser(Uri clickURL)
        {
            if (clickURL == null)
            {
                return false;
            }

            bool stopOpeningURI = false;

            if (!clickURL.IsAbsoluteUri)
            {
                stopOpeningURI = true;
            }

            if (!IsURLAndNotUnsafe(clickURL))
            {
                stopOpeningURI = true;
            }

            if (IsADangerousFileUrl(clickURL))
            {
                stopOpeningURI = true;
            }

            if (stopOpeningURI)
            {
                LoggingHelper.ErrorLog("Given URL has been marked as unsafe and will not opened.");
                return false;
            }

            int stringMaxPrintLength = Math.Min(10, clickURL.AbsoluteUri.Length);

            LoggingHelper.DebugLog(() => "Opening URI in browser: " +
                                         $"'{clickURL.AbsoluteUri.Substring(0, stringMaxPrintLength)}[...]'.");

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = clickURL.AbsoluteUri,
                    UseShellExecute = true
                });

            return true;
        }

        /// <summary>
        /// A list of file extensions that should be forbidden when accessing a URL.
        /// </summary>
        private static readonly List<string> DangerousExtensions = new List<string>
        {
            // Executables
            ".exe", ".msi", ".bat", ".cmd", ".com", ".ps1", ".vbs", ".js",

            // Archives. Could contain unwanted media.
            ".zip", ".rar", ".7z", ".tar", ".gz",

            // Documents with macro capability
            ".docm", ".xlsm", ".pptm",

            // Unsafe file types.
            ".dll", ".iso", ".dmg", ".sh"
        };

        /// <summary>
        /// Checks if the given URI leads to a file download and prevent that.
        /// </summary>
        /// <param name="uri">URI to be checked.</param>
        /// <returns>(Bool) True: Contains dangerous file extension. False: Is not blatantly unsafe.</returns>
        private static bool IsADangerousFileUrl(Uri uri)
        {
            string extensionPortion = Path.GetExtension(uri.AbsolutePath);

            if (DangerousExtensions.Contains(extensionPortion.ToLowerInvariant()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies that the given URI is not a file or something unwanted.
        /// </summary>
        /// <param name="uri">URI to be checked.</param>
        /// <returns>(Bool) True: Is in a URL scheme. False: Is possibly a file or something else.</returns>
        public static bool IsURLAndNotUnsafe(Uri uri)
        {
            // Avoid malicious urls.
            if (uri.IsFile || uri.IsLoopback)
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp
                && uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            return true;
        }
    }
}