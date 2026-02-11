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
    /// <summary>
    /// Handles background checks for newer releases on GitHub.
    /// </summary>
    internal static class UpdateChecker
    {
        /// <summary>
        /// Holds resolved version and release metadata for a newer update.
        /// </summary>
        public sealed class UpdateInfo
        {
            /// <summary>
            /// The currently running version string.
            /// </summary>
            public string CurrentVersion { get; init; }
            /// <summary>
            /// The latest available version string.
            /// </summary>
            public string LatestVersion { get; init; }
            /// <summary>
            /// URL to the release page for the latest version.
            /// </summary>
            public string ReleaseUrl { get; init; }
        }

        /// <summary>
        /// Starts the update check in the background if enabled and not already started.
        /// </summary>
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
                        _ = Interlocked.Exchange(ref updateInfo, info);
                    }
                }
                catch (Exception)
                {
                    // Ignore network, cancellation, or parsing failures.
                }
            });
        }

        /// <summary>
        /// Cancels any in-flight update check requests.
        /// </summary>
        public static void Cancel()
        {
            cts.Cancel();
        }

        /// <summary>
        /// Attempts to consume the latest update info once.
        /// </summary>
        /// <param name="info">Receives the update info if available.</param>
        /// <returns>True if update info was available and consumed; otherwise false.</returns>
        public static bool TryConsumeUpdate(out UpdateInfo info)
        {
            info = null;
            UpdateInfo current = Interlocked.CompareExchange(ref updateInfo, null, null);
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

        /// <summary>
        /// Determines if the version string represents a dirty/dev build.
        /// </summary>
        /// <param name="version">The version to check.</param>
        public static bool IsDirtyVersion(string version)
        {
            // Avoids treating missing/blank version strings as "dirty" version
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            // Check if there is "dirty" in the version build
            if (version.Contains("dirty", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // When IncludeSourceRevisionInInformationalVersion is enabled,
            // a build metadata suffix like "+githash" is appended.
            return version.Contains('+', StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the current assembly informational version string.
        /// </summary>
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

        /// <summary>
        /// Fetches the latest GitHub release and returns update info when newer than current.
        /// </summary>
        /// <param name="currentVersionString">The current version string used for comparison.</param>
        /// <param name="cancellationToken">Token used to cancel the HTTP request.</param>
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

        /// <summary>
        /// Attempts to parse a semantic version from a tag or version string.
        /// </summary>
        /// <param name="input">The input tag or version string.</param>
        /// <param name="version">The parsed version when successful.</param>
        /// <returns>True when parsing succeeded; otherwise false.</returns>
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

        /// <summary>
        /// GitHub API endpoint for the latest release.
        /// </summary>
        private const string LatestReleaseUrl = "https://api.github.com/repos/yell0wsuit/cuttherope-dx/releases/latest";
        /// <summary>
        /// Fallback release page when API url is missing.
        /// </summary>
        private const string ReleasesPageUrl = "https://github.com/yell0wsuit/cuttherope-dx/releases";

        /// <summary>
        /// Shared HTTP client for update checks.
        /// </summary>
        private static readonly HttpClient Http = new()
        {
            Timeout = TimeSpan.FromSeconds(6)
        };

        /// <summary>
        /// Cancellation token source for the update request.
        /// </summary>
        private static readonly CancellationTokenSource cts = new();
        /// <summary>
        /// Ensures the update check only starts once per session.
        /// </summary>
        private static int started;
        /// <summary>
        /// Ensures update info is consumed at most once.
        /// </summary>
        private static int consumed;
        /// <summary>
        /// Latest update info fetched from the server.
        /// </summary>
        private static UpdateInfo updateInfo;
    }
}
