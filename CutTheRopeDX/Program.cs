using System;
using System.Xml.Linq;

using CutTheRopeDX;
using CutTheRopeDX.GameMain;

CustomLevelCommandLineResult cli = CustomLevelCommandLine.Parse(args);

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

using Game1 game = new();
game.Run();
return 0;
