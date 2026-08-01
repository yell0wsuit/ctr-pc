using System;
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

using Game1 game = new();
game.Run();
return 0;
