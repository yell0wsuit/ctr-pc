using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using CutTheRopeDX.Launcher;
using CutTheRopeDX.Launcher.Graphics;

// Picks between the shipped graphics-backend builds and runs one of them. Deliberately references no
// MonoGame assembly: the builds it chooses between are compiled against different ones, so anything that
// loaded a MonoGame type here would tie the launcher to a single backend and defeat the purpose.

bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

GraphicsBackend? forced =
    BackendSelection.ParseOverride(args)
    ?? BackendSelection.ParseOverride(Environment.GetEnvironmentVariable(BackendSelection.OverrideVariable));

// The probe loads and unloads vulkan-1.dll itself, so it is only worth running when the answer can change
// the outcome. Skipping it when a backend was named also gives a way past a driver that crashes on probe.
VulkanProbeResult probe = isWindows && !forced.HasValue
    ? RunProbeSafely()
    : VulkanProbeResult.NoLoader;

GraphicsBackend backend = BackendSelection.Decide(isWindows, probe, forced);

// Say something before handing over, but only when the answer changed. Windows only: it is the sole
// platform that ships both builds, so it is the only one where falling back means anything.
if (isWindows && BackendNotice.ShouldWarn(backend, BackendNotice.ReadLastSeen(), forced.HasValue))
{
    ShowVulkanNotice();
}

if (isWindows && !forced.HasValue)
{
    // Recorded after the probe rather than the launch, so a game that fails to start does not suppress
    // the warning for a machine that has not been told yet.
    BackendNotice.WriteLastSeen(backend);
}

string executable = LocateGameExecutable(backend);

if (executable is null)
{
    Console.Error.WriteLine(
        $"[launcher] No {backend} build found beside the launcher as "
        + $"'{BackendSelection.ExecutableFor(backend)}' or under '{BackendSelection.DirectoryFor(backend)}/'.");
    return 1;
}

ProcessStartInfo startInfo = new()
{
    FileName = executable,
    // Run from the build's own directory so relative paths inside the game resolve as they do when the
    // build is started directly. Shared content sits a level above; ContentPaths looks there for it.
    WorkingDirectory = Path.GetDirectoryName(executable),
    UseShellExecute = false,
};

foreach (string arg in args)
{
    // The backend switches are the launcher's own; passing them on would only puzzle the game.
    if (BackendSelection.ParseOverride(arg) is null)
    {
        startInfo.ArgumentList.Add(arg);
    }
}

if (executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
{
    startInfo.ArgumentList.Insert(0, executable);
    startInfo.FileName = "dotnet";
}

try
{
    using Process game = Process.Start(startInfo)
        ?? throw new InvalidOperationException("the process could not be started");
    // Wait rather than exit immediately, so whatever started the launcher sees the game's real lifetime
    // and exit code. Overlays and shells that track the launched process depend on that.
    game.WaitForExit();
    return game.ExitCode;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"[launcher] Could not start '{executable}': {exception.Message}");
    return 1;
}

// Kept behind a guard so the Windows-only P/Invoke is never reached on another platform, which the
// analyzer checks and a published single-platform build would otherwise trip over.
static void ShowVulkanNotice()
{
    if (OperatingSystem.IsWindows())
    {
        VulkanUnavailableNotice.Show();
    }
}

// Never let a faulting driver inside the probe stop the game from starting; an unusable Vulkan loader and
// a crashing one both mean the same thing here.
static VulkanProbeResult RunProbeSafely()
{
    try
    {
        return OperatingSystem.IsWindows() ? VulkanProbe.Run() : VulkanProbeResult.NoLoader;
    }
    catch (Exception)
    {
        return VulkanProbeResult.NoLoader;
    }
}

// Takes the first layout that is actually present: the flat ahead-of-time one beside the launcher, then
// the per-backend directories a build with loose assemblies needs.
static string LocateGameExecutable(GraphicsBackend backend)
{
    foreach (string candidate in BackendSelection.CandidatePaths(AppContext.BaseDirectory, backend))
    {
        if (File.Exists(candidate))
        {
            return candidate;
        }
    }
    return null;
}
