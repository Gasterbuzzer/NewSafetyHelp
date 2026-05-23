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
        /// Sets the email click url while 
        /// </summary>
        /// <param name="emailURL">String version of the URL.</param>
        /// <param name="emailClickURL">Reference to write the URL to if the given url is valid.</param>
        /// <returns>Boolean: If the setting failed or worked.</returns>
        public static bool SetEmailClickURL(string emailURL,
            ref Uri emailClickURL)
        {
            if (!Uri.TryCreate(emailURL, UriKind.Absolute, out Uri emailUri))
            {
                LoggingHelper.ErrorLog($"Given URL '{emailURL.Substring(0, 10)}[...]' is not a valid URL." +
                                       " Unable of setting URL. " +
                                       "Make sure it is in a valid format: 'http://' or 'https://'.");
                return false;
            }

            if (!IsURLAndNotUnsafe(emailUri))
            {
                LoggingHelper.ErrorLog($"Given URL '{emailURL.Substring(0, 10)}[...]' is not a accepted URL." +
                                       " Unable of setting URL. " +
                                       "Make sure it is in a valid format: 'http://' or 'https://'.");
                return false;
            }

            if (IsADangerousFileUrl(emailUri))
            {
                LoggingHelper.ErrorLog($"Given URL '{emailURL.Substring(0, 10)}[...]' is not a accepted URL." +
                                       " Unable of setting URL. " +
                                       "Please provide an URL that is not a unsafe file type.");
                return false;
            }

            emailClickURL = emailUri;
            return true;
        }
        
        /// <summary>
        /// Sets the email click url while 
        /// </summary>
        /// <returns>Boolean: If the setting failed or worked.</returns>
        public static bool OpenEmailURI(Uri emailClickURL)
        {
            if (emailClickURL == null)
            {
                return false;
            }

            bool stopOpeningURI = false;

            if (!emailClickURL.IsAbsoluteUri)
            {
                stopOpeningURI = true;
            }

            if (!IsURLAndNotUnsafe(emailClickURL))
            {
                stopOpeningURI = true;
            }

            if (IsADangerousFileUrl(emailClickURL))
            {
                stopOpeningURI = true;
            }

            if (stopOpeningURI)
            {
                LoggingHelper.ErrorLog("Given URL has been marked as unsafe and will not opened.");
                return false;
            }

            LoggingHelper.DebugLog(() => "Opening email URI: " +
                                         $"'{emailClickURL.AbsoluteUri.Substring(0, 10)}[...]'.",
                LoggingHelper.LoggingCategory.EMAIL);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = emailClickURL.AbsoluteUri,
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
        /// <param name="emailUri">URI to be checked.</param>
        /// <returns>(Bool) True: Contains dangerous file extension. False: Is not blatantly unsafe.</returns>
        private static bool IsADangerousFileUrl(Uri emailUri)
        {
            string emailExtensionPortion = Path.GetExtension(emailUri.AbsolutePath);
            
            if (DangerousExtensions.Contains(emailExtensionPortion.ToLowerInvariant()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies that the given URI is not a file or something unwanted.
        /// </summary>
        /// <param name="emailUri">URI to be checked.</param>
        /// <returns>(Bool) True: Is in a URL scheme. False: Is possibly a file or something else.</returns>
        public static bool IsURLAndNotUnsafe(Uri emailUri)
        {
            // Avoid malicious urls.
            if (emailUri.IsFile || emailUri.IsLoopback)
            {
                return false;
            }

            if (emailUri.Scheme != Uri.UriSchemeHttp
                && emailUri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            return true;
        }
    }
}