// Local-dev only: download binary content assets when any are missing.
//
// Invoked from CutTheRopeDX.csproj before content build:
//     dotnet run --project tools/FetchContentAssets -- <contentDir>
//
// Detection is manifest-driven: content.mgcb lists every png/wav source MGCB
// builds (/build: lines), so we resolve each to a path and check it exists.
// The remaining binary types (ogg, mp4, ttf, otf, cur, xnb) ship in the same
// zip but are NOT in the manifest, so we additionally require at least one file
// of each on disk. (wmv is intentionally not required — none ship in the bundle.)
//
// The bundle (ctrdx-assets.zip) is a full content/ snapshot, but we copy ONLY
// the 9 binary extensions into content/ — git-tracked json/xml in the bundle are
// ignored, so a stale bundle can never clobber a tracked text file.

using System.IO.Compression;

const string AssetsUrl =
    "https://github.com/yell0wsuit/ctrdx-assets/releases/latest/download/ctrdx-assets.zip";

// CI sets CI=true and disables MGCB, so it never needs these.
if (Environment.GetEnvironmentVariable("CI") == "true")
{
    return 0;
}

string contentDir = Path.GetFullPath(args.Length > 0 ? args[0] : "../content");
string manifest = Path.Combine(contentDir, "content.mgcb");

if (!File.Exists(manifest))
{
    Console.Error.WriteLine($"content.mgcb not found at {manifest}; skipping asset fetch.");
    return 0;
}

// Expected png/wav sources from the manifest's /build: lines.
// A line is "/build:images/foo.png" or "/build:sounds/a.wav;destName".
List<string> missingManifest = [];
foreach (string line in File.ReadLines(manifest))
{
    if (!line.StartsWith("/build:", StringComparison.Ordinal))
    {
        continue;
    }

    string src = line["/build:".Length..];
    int semi = src.IndexOf(';');
    if (semi >= 0)
    {
        src = src[..semi];
    }

    if (!File.Exists(Path.Combine(contentDir, src)))
    {
        missingManifest.Add(src);
    }
}

// Non-manifest binaries: require at least one file of each type on disk
// (ignoring MGCB's bin/ and obj/ output dirs).
bool HasAny(params string[] exts)
{
    return exts.Any(ext =>
    Directory.EnumerateFiles(contentDir, "*." + ext, SearchOption.AllDirectories)
        .Any(p => !IsBuildArtifact(contentDir, p)));
}

bool missing =
    missingManifest.Count > 0 ||
    !HasAny("ogg") ||
    !HasAny("mp4") ||
    !HasAny("ttf", "otf") ||
    !HasAny("cur") ||
    !HasAny("xnb");

if (!missing)
{
    return 0;
}

Console.WriteLine(
    $"Content assets missing ({missingManifest.Count} manifest file(s) absent) — " +
    $"downloading from {AssetsUrl} (~335 MB, one time)...");

string tmp = Path.Combine(Path.GetTempPath(), "ctrdx-assets-fetch-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(tmp);
try
{
    string zipPath = Path.Combine(tmp, "ctrdx-assets.zip");
    await DownloadWithRetry(AssetsUrl, zipPath, retries: 3);

    string extracted = Path.Combine(tmp, "extracted");
    ZipFile.ExtractToDirectory(zipPath, extracted);

    string[] binaryExts =
        [".png", ".wav", ".ogg", ".mp4", ".ttf", ".otf", ".cur", ".xnb", ".wmv"];
    int copied = 0;
    foreach (string file in Directory.EnumerateFiles(extracted, "*.*", SearchOption.AllDirectories))
    {
        if (!binaryExts.Contains(Path.GetExtension(file).ToLowerInvariant()))
        {
            continue;
        }

        string dest = Path.Combine(contentDir, Path.GetRelativePath(extracted, file));
        _ = Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(file, dest, overwrite: true);
        copied++;
    }
    Console.WriteLine($"Copied {copied} binary assets into {contentDir}.");
}
finally
{
    try
    {
        Directory.Delete(tmp, recursive: true);
    }

    catch
    {
        /* best-effort cleanup */
    }
}
return 0;

// Skip MGCB output directories (content/bin, content/obj).
static bool IsBuildArtifact(string contentDir, string path)
{
    string rel = Path.GetRelativePath(contentDir, path);
    string first = rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
    return first is "bin" or "obj";
}

static async Task DownloadWithRetry(string url, string dest, int retries)
{
    using HttpClient http = new() { Timeout = TimeSpan.FromMinutes(30) };
    for (int attempt = 1; ; attempt++)
    {
        try
        {
            using HttpResponseMessage resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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
