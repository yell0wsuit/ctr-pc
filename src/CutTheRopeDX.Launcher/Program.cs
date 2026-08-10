using System;
using System.Diagnostics;
using System.IO;

using CutTheRopeDX.Launcher;
using CutTheRopeDX.Launcher.Graphics;

// Picks between the shipped graphics-backend builds and runs one of them. Deliberately references no
// MonoGame assembly: the builds it chooses between are compiled against different ones, so anything that
// loaded a MonoGame type here would tie the launcher to a single backend and defeat the purpose.

// Everything conditional below keys off this rather than RuntimeInformation, because it is the form the
// platform-compatibility analyzer accepts as a guard for the Windows-only calls it protects.
bool isWindows = OperatingSystem.IsWindows();

GraphicsBackend? forced =
    BackendSelection.ParseOverride(args)
    ?? BackendSelection.ParseOverride(Environment.GetEnvironmentVariable(BackendSelection.OverrideVariable));

string recorded = isWindows ? LauncherState.Read() : null;

// A marker left from last time means that launch never came back from the probe. Assume the driver is at
// fault and skip it, rather than repeat a call that has already proved fatal once on this machine.
bool shouldProbe = isWindows && !forced.HasValue && !LauncherState.ProbeWasFatal(recorded);

if (shouldProbe)
{
    // Must reach disk before the probe runs. A driver that faults inside vkCreateInstance takes the whole
    // process with it, and this file is the only trace the next launch has to go on.
    LauncherState.WriteProbing();
}

VulkanProbeResult probe = shouldProbe && isWindows ? VulkanProbe.Run() : VulkanProbeResult.NoLoader;

GraphicsBackend backend = BackendSelection.Decide(isWindows, probe, forced);

if (isWindows)
{
    if (forced.HasValue)
    {
        // A forced backend says nothing about what this machine manages unaided, so it records nothing. It
        // is also the way out for a machine pinned to OpenGL by a marker its driver left behind.
        LauncherState.Clear();
    }
    else
    {
        // Replaces the probing marker before the game starts. Writing it later would leave the marker in
        // place whenever the game itself failed to launch, and cost the next launch its probe for nothing.
        LauncherState.WriteBackend(backend);
    }

    // Say something before handing over, but only when the answer changed.
    if (BackendSelection.ShouldWarn(backend, LauncherState.LastBackend(recorded), forced.HasValue))
    {
        VulkanUnavailableNotice.Show();
    }
}

string executable = LocateGameExecutable(backend);

if (executable is null)
{
    Console.Error.WriteLine(
        $"[launcher] No {backend} build found beside the launcher as "
        + $"'{BackendSelection.ExecutableFor(backend)}'.");
    return 1;
}

ProcessStartInfo startInfo = new()
{
    FileName = executable,
    // Run from the build's own directory so relative paths inside the game resolve as they do when the
    // build is started directly, content included.
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

// Both builds sit beside the launcher and differ by name, so this is a short list either way.
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
