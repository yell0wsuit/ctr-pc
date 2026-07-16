using CutTheRopeDX.Content.Commands;

try
{
    ContentCommandLine commandLine = ContentCommandLine.Parse(args);
    return await AssetCommands.RunAsync(commandLine);
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
