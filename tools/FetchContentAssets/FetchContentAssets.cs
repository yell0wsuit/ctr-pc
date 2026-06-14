// Local-dev only: fetch/verify binary content assets against a hash manifest.
//
// Invoked from CutTheRopeDX.csproj before content build:
//     dotnet run --project tools/FetchContentAssets -- <contentDir>
// Manual integrity check (re-hashes local files against the manifest):
//     dotnet run --project tools/FetchContentAssets -- <contentDir> --verify
//
// content/file_manifest.json is committed to the repo and lists every binary
// asset (the 9 fetched extensions) and its SHA-256; git-tracked json/xml are
// excluded. Detection checks each listed file exists (fast, offline). When any
// are missing we download the bundle, verify every listed file against the
// manifest's hash, then copy in ONLY the missing ones — present files (including
// a modder's local edits) are left untouched. Run with --verify to hash every
// local file against the manifest and surface a present-but-corrupt asset.

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

const string AssetsUrl =
    "https://github.com/yell0wsuit/ctrdx-assets/releases/latest/download/ctrdx-assets.zip";
const string ManifestName = "file_manifest.json";

// CI sets CI=true and disables MGCB, so it never needs these.
if (Environment.GetEnvironmentVariable("CI") == "true")
{
    return 0;
}

bool verify = args.Contains("--verify");
string[] positional = [.. args.Where(a => !a.StartsWith("--", StringComparison.Ordinal))];
string contentDir = Path.GetFullPath(positional.Length > 0 ? positional[0] : "../content");
string manifestPath = Path.Combine(contentDir, ManifestName);

if (!File.Exists(manifestPath))
{
    Console.Error.WriteLine($"No {ManifestName} in {contentDir}; skipping content-asset check.");
    return 0;
}

Dictionary<string, string> manifest = ReadManifest(manifestPath);

if (verify)
{
    return RunVerify(contentDir, manifest);
}

// Detection: fetch when any listed file is missing on disk.
List<string> missing = MissingFiles(contentDir, manifest);
if (missing.Count == 0)
{
    return 0;
}

Console.WriteLine($"Content assets missing ({missing.Count}/{manifest.Count} listed) — fetching.");
return await Fetch(contentDir, manifest, missing);

static Dictionary<string, string> ReadManifest(string path)
{
    using FileStream fs = File.OpenRead(path);
    using JsonDocument doc = JsonDocument.Parse(fs);
    Dictionary<string, string> result = [];
    if (doc.RootElement.TryGetProperty("files", out JsonElement files))
    {
        foreach (JsonProperty entry in files.EnumerateObject())
        {
            result[entry.Name] = entry.Value.GetString() ?? "";
        }
    }
    return result;
}

static List<string> MissingFiles(string contentDir, Dictionary<string, string> manifest)
{
    List<string> missing = [];
    foreach (string rel in manifest.Keys)
    {
        if (!File.Exists(ToLocalPath(contentDir, rel)))
        {
            missing.Add(rel);
        }
    }
    return missing;
}

static string ToLocalPath(string baseDir, string relPosix)
{
    return Path.Combine(baseDir, relPosix.Replace('/', Path.DirectorySeparatorChar));
}

static string Sha256(string path)
{
    using FileStream fs = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(fs));
}

static int RunVerify(string contentDir, Dictionary<string, string> manifest)
{
    List<string> missing = [];
    List<string> mismatched = [];
    foreach ((string rel, string expected) in manifest)
    {
        string full = ToLocalPath(contentDir, rel);
        if (!File.Exists(full))
        {
            missing.Add(rel);
        }
        else if (!string.Equals(Sha256(full), expected, StringComparison.OrdinalIgnoreCase))
        {
            mismatched.Add(rel);
        }
    }

    foreach (string m in missing)
    {
        Console.Error.WriteLine($"missing:  {m}");
    }
    foreach (string m in mismatched)
    {
        Console.Error.WriteLine($"mismatch: {m}");
    }

    if (missing.Count == 0 && mismatched.Count == 0)
    {
        Console.WriteLine($"All {manifest.Count} content assets verified OK.");
        return 0;
    }

    Console.Error.WriteLine(
        $"Verify failed: {missing.Count} missing, {mismatched.Count} mismatched of {manifest.Count}.");
    return 1;
}

static async Task<int> Fetch(string contentDir, Dictionary<string, string> manifest, List<string> missing)
{
    Console.WriteLine($"Downloading content assets from {AssetsUrl} (~335 MB, one time)...");
    string tmp = Path.Combine(Path.GetTempPath(), "ctrdx-assets-fetch-" + Guid.NewGuid().ToString("N"));
    _ = Directory.CreateDirectory(tmp);
    try
    {
        string zipPath = Path.Combine(tmp, "ctrdx-assets.zip");
        await DownloadWithRetry(AssetsUrl, zipPath, retries: 3);

        string extracted = Path.Combine(tmp, "extracted");
        ZipFile.ExtractToDirectory(zipPath, extracted);

        // Verify the download against the committed manifest before copying anything.
        List<string> bad = [];
        foreach ((string rel, string expected) in manifest)
        {
            string srcFile = ToLocalPath(extracted, rel);
            if (!File.Exists(srcFile) ||
                !string.Equals(Sha256(srcFile), expected, StringComparison.OrdinalIgnoreCase))
            {
                bad.Add(rel);
            }
        }
        if (bad.Count > 0)
        {
            Console.Error.WriteLine(
                $"Downloaded bundle doesn't match {ManifestName} ({bad.Count} file(s) missing/mismatched);");
            Console.Error.WriteLine(
                "the committed manifest may be out of sync with the latest asset release. Aborting.");
            return 1;
        }

        // Copy only the missing files (verified above). Present files — including
        // a modder's local edits — are left untouched; use --verify to catch a
        // present-but-corrupt file.
        foreach (string rel in missing)
        {
            string dest = ToLocalPath(contentDir, rel);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(ToLocalPath(extracted, rel), dest, overwrite: true);
        }

        Console.WriteLine($"Restored {missing.Count} missing binary asset(s) into {contentDir}.");
        return 0;
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or InvalidDataException)
    {
        Console.Error.WriteLine($"Failed to fetch content assets: {ex.Message}");
        return 1;
    }
    finally
    {
        try { Directory.Delete(tmp, recursive: true); }
        catch { /* best-effort cleanup */ }
    }
}

static async Task DownloadWithRetry(string url, string dest, int retries)
{
    using HttpClient http = new() { Timeout = TimeSpan.FromMinutes(30) };
    // GitHub rejects/throttles requests without a User-Agent.
    http.DefaultRequestHeaders.UserAgent.ParseAdd("CutTheRopeDX-FetchContentAssets/1.0");
    for (int attempt = 1; ; attempt++)
    {
        try
        {
            using HttpResponseMessage resp =
                await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            _ = resp.EnsureSuccessStatusCode();
            await using Stream src = await resp.Content.ReadAsStreamAsync();
            await using FileStream fs = File.Create(dest);
            await src.CopyToAsync(fs);
            return;
        }
        catch (Exception ex) when (attempt < retries)
        {
            Console.Error.WriteLine($"Download attempt {attempt} failed ({ex.Message}); retrying...");
        }
    }
}
