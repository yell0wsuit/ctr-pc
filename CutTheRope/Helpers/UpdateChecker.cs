using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CutTheRope.GameMain;

namespace CutTheRope.Helpers
{
    internal static class UpdateChecker
    {
        public sealed class UpdateInfo
        {
            public string CurrentVersion { get; init; }
            public string LatestVersion { get; init; }
            public string ReleaseUrl { get; init; }
        }

        public static void StartIfNeeded()
        {
            if (Interlocked.Exchange(ref started, 1) == 1)
            {
                return;
            }

            if (!CTRPreferences.IsUpdateCheckEnabled())
            {
                return;
            }

            string currentVersion = GetCurrentVersionString();
            if (IsDirtyVersion(currentVersion))
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    UpdateInfo info = await FetchLatestReleaseAsync(currentVersion, cts.Token).ConfigureAwait(false);
                    if (info != null)
                    {
                        updateInfo = info;
                    }
                }
                catch (Exception)
                {
                    // Ignore network, cancellation, or parsing failures.
                }
            });
        }

        public static void Cancel()
        {
            cts.Cancel();
        }

        public static bool TryConsumeUpdate(out UpdateInfo info)
        {
            info = null;
            UpdateInfo current = updateInfo;
            if (current == null)
            {
                return false;
            }

            if (Interlocked.Exchange(ref consumed, 1) == 1)
            {
                return false;
            }

            info = current;
            return true;
        }

        public static bool IsDirtyVersion(string version)
        {
            return !string.IsNullOrWhiteSpace(version)
                && version.Contains("dirty", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetCurrentVersionString()
        {
            string version =
                Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "Unknown";
            return version;
        }

        private static async Task<UpdateInfo> FetchLatestReleaseAsync(string currentVersionString, CancellationToken cancellationToken)
        {
            if (!TryParseVersion(currentVersionString, out Version currentVersion))
            {
                return null;
            }

            using HttpRequestMessage request = new(HttpMethod.Get, LatestReleaseUrl);
            request.Headers.UserAgent.ParseAdd("CutTheRopeDX-UpdateChecker/1.0");
            request.Headers.Add("Accept", "application/vnd.github+json");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            using HttpResponseMessage response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            using JsonDocument doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("tag_name", out JsonElement tagElement))
            {
                return null;
            }

            string tag = tagElement.GetString();
            if (string.IsNullOrWhiteSpace(tag) || !TryParseVersion(tag, out Version latestVersion))
            {
                return null;
            }

            if (latestVersion <= currentVersion)
            {
                return null;
            }

            string releaseUrl = null;
            if (doc.RootElement.TryGetProperty("html_url", out JsonElement urlElement))
            {
                releaseUrl = urlElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(releaseUrl))
            {
                releaseUrl = ReleasesPageUrl;
            }

            return new UpdateInfo
            {
                CurrentVersion = currentVersion.ToString(),
                LatestVersion = latestVersion.ToString(),
                ReleaseUrl = releaseUrl
            };
        }

        private static bool TryParseVersion(string input, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            int start = -1;
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    start = i;
                    break;
                }
            }

            if (start < 0)
            {
                return false;
            }

            StringBuilder sb = new();
            for (int i = start; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsDigit(c) || c == '.')
                {
                    _ = sb.Append(c);
                }
                else
                {
                    break;
                }
            }

            string numeric = sb.ToString().TrimEnd('.');
            return Version.TryParse(numeric, out version);
        }

        private const string LatestReleaseUrl = "https://api.github.com/repos/yell0wsuit/cuttherope-dx/releases/latest";
        private const string ReleasesPageUrl = "https://github.com/yell0wsuit/cuttherope-dx/releases";

        private static readonly HttpClient Http = new()
        {
            Timeout = TimeSpan.FromSeconds(6)
        };

        private static readonly CancellationTokenSource cts = new();
        private static int started;
        private static int consumed;
        private static volatile UpdateInfo updateInfo;
    }
}
