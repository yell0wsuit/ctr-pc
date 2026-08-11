using System;
using System.IO;
using System.Xml.Linq;

using CutTheRopeDX;
using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain;

CommandLineResult cli = CommandLine.Parse(args);

if (cli.IsCustomLevel)
{
    if (cli.ErrorMessage != null)
    {
        Console.Error.WriteLine(cli.ErrorMessage);
        return 1;
    }

    if (!CustomLevelFile.TryLoad(cli.LevelPath, out XElement _, out string loadError))
    {
        Console.Error.WriteLine(loadError);
        return 1;
    }

    CustomLevelSession.Activate(cli.LevelPath);

    // Tell the launcher, before the run loop blocks, that this build understood --level and loaded the
    // level. A build too old for the switch never reaches here, so the line's absence is the signal.
    PlaytestHandshake.Announce(Console.Out);
}

if (cli.IsHeadless)
{
    // A fixed-frame smoke run proving the shipped binary boots and ticks with no window.
    // Scripted scenarios are out of scope; tests drive the engine in-process instead.
    HeadlessHost.Boot(HeadlessHost.DefaultWidth, HeadlessHost.DefaultHeight, LanguageHelper.FromSystemCulture());
    for (int i = 0; i < 600; i++)
    {
        HeadlessHost.Tick(0.016f);
    }

    // Report where the run landed. Without this an "exit 0" would also be printed by a run
    // wedged on the loading screen, which is exactly the failure this smoke test must catch.
    Console.WriteLine($"[headless] ran 600 frames, active controller = {HeadlessHost.ActiveControllerName()}");
    return CustomLevelSession.IsActive && !HeadlessHost.IsInGameplay() ? 1 : 0;
}

InstallAlsoftConfig();

using Game1 game = new();
game.Run();
return 0;

// OpenAL Soft's own config-file discovery depends on the process's current working
// directory, which is unreliable across launch methods - Windows/Linux launchers set it
// to the executable's own folder, but macOS Finder/LaunchServices does not, and does not
// set it to any predictable directory at all. Resolve the bundled alsoft.ini explicitly and
// point OpenAL Soft at it via ALSOFT_CONF, which takes priority over every other config-file
// search path, so playback settings apply the same way regardless of how the game was launched.
static void InstallAlsoftConfig()
{
    string baseDir = AppContext.BaseDirectory;
    string[] candidates =
    [
        Path.Combine(baseDir, "alsoft.ini"),
        // net10.0-macos app bundle: the managed assembly runs from Contents/MonoBundle,
        // but alsoft.ini ships as a BundleResource under the sibling Contents/Resources.
        Path.Combine(baseDir, "..", "Resources", "alsoft.ini"),
    ];

    foreach (string candidate in candidates)
    {
        if (File.Exists(candidate))
        {
            Environment.SetEnvironmentVariable("ALSOFT_CONF", Path.GetFullPath(candidate));
            return;
        }
    }
}
