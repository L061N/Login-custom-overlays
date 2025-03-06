using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace benofficial2.Plugin
{
    public class VersionChecker
    {
        public const string CurrentVersion = "2.1";
        private const string VersionUrl = "https://raw.githubusercontent.com/fixfactory/bo2-official-overlays/main/Versions.json";
        private const string DownloadPageUrl = "https://github.com/fixfactory/bo2-official-overlays/releases";

        public async Task CheckForUpdateAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(VersionUrl);
                    JObject jsonObject = JObject.Parse(json);

                    string latestVersion = jsonObject["plugin"]?.ToString();

                    if (!string.IsNullOrEmpty(latestVersion) && IsNewerVersion(latestVersion, CurrentVersion))
                    {
                        DialogResult result = MessageBox.Show(
                            $"A new version {latestVersion} for benofficial2 plugin is available. Do you want to visit the download page?",
                            "Update Available",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information
                        );

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(DownloadPageUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error($"An error occurred while checking for benofficial2 plugin updates:\n{ex.Message}");
            }
        }

        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            Version latest = new Version(latestVersion);
            Version current = new Version(currentVersion);
            return latest > current;
        }
    }
}