using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotnetDumpMonitor.Commons
{
    internal class GithubUpgradeHelper
    {
        private const string LAST_RELEASE_URL = "https://github.com/jim-jiang-github/DotnetDumpMonitor/releases/latest";

        public static Version CurrentVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version == null)
                {
                    return Version.Parse("0.0.0");
                }
                return Version.Parse($"{version.Major}.{version.Minor}.{version.Build}");
            }
        }

        public static async Task<Version?> GetLastReleaseVersion()
        {
            try
            {
                using HttpClient httpClient = new();
                var httpResponseMessage = await httpClient.GetAsync(LAST_RELEASE_URL);
                var requestUri = httpResponseMessage.RequestMessage?.RequestUri;
                if (requestUri == null)
                {
                    return null;
                }
                var match = Regex.Match(requestUri.OriginalString, ".*?\\/v(\\d.\\d.\\d)");
                if (match.Success && match.Groups.Count == 2 && match.Groups[1].Value is string versionString && Version.TryParse(versionString, out Version? version))
                {
                    return version;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void GoToDownloadPage()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "LAST_RELEASE_URL",
                UseShellExecute = true
            });
        }
    }
}
